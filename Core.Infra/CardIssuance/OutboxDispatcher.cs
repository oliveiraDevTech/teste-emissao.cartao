using Core.Application.Interfaces;
using Core.Application.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Core.Infra.CardIssuance;

/// <summary>
/// BackgroundService que processa eventos do Outbox
/// Publica eventos para RabbitMQ com retry automático
/// Implementa resiliência com exponential backoff
/// </summary>
public sealed class OutboxDispatcher : BackgroundService
{
    private readonly ILogger<OutboxDispatcher> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly OutboxDispatcherOptions _options;

    public OutboxDispatcher(
        ILogger<OutboxDispatcher> logger,
        IServiceProvider serviceProvider,
        OutboxDispatcherOptions options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxDispatcher iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessarEventosPendentesAsync(stoppingToken);
                await LimparEventosAntigosAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar eventos do Outbox");
            }

            await Task.Delay(_options.IntervalMilisegundos, stoppingToken);
        }

        _logger.LogInformation("OutboxDispatcher parado");
    }

    private async Task ProcessarEventosPendentesAsync(CancellationToken ct)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var messagePublisher = scope.ServiceProvider.GetService<IMessagePublisher>();

            // Se MessagePublisher não está disponível, apenas retornar
            if (messagePublisher == null)
            {
                _logger.LogDebug("MessagePublisher não disponível, pulando dispatcher");
                return;
            }

            var eventos = await outboxRepo.ObterPendentesAsync(_options.LoteTamanho, ct);

            if (eventos.Count == 0)
                return;

            _logger.LogInformation("Processando {Count} eventos do Outbox", eventos.Count);

            foreach (var evento in eventos)
            {
                try
                {
                    // Publicar para MessagePublisher
                    await PublicarComRetryAsync(messagePublisher, evento, ct);

                    // Marcar como enviado
                    await outboxRepo.MarcarComoEnviadoAsync(evento.Id, ct);

                    _logger.LogInformation(
                        "Evento publicado. Id={EventoId}, Topico={Topico}",
                        evento.Id, evento.Topico);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Falha ao publicar evento. Id={EventoId}, Topico={Topico}",
                        evento.Id, evento.Topico);

                    // Continuar com próximo evento ao invés de falhar toda a lote
                }
            }
        }
    }

    private async Task PublicarComRetryAsync(
        IMessagePublisher messagePublisher,
        Core.Application.Interfaces.OutboxEventoDTO evento,
        CancellationToken ct)
    {
        int tentativas = 0;
        int delayMs = _options.DelayRetryMilisegundos;

        while (tentativas < _options.MaximoTentativas)
        {
            try
            {
                // Publicar o evento
                await messagePublisher.PublishAsync(evento.Topico, evento.Payload, ct);
                return; // Sucesso
            }
            catch (Exception ex) when (tentativas < _options.MaximoTentativas - 1)
            {
                tentativas++;
                _logger.LogWarning(ex,
                    "Falha ao publicar evento (tentativa {Tentativa}/{Max}). Aguardando {DelayMs}ms",
                    tentativas, _options.MaximoTentativas, delayMs);

                await Task.Delay(delayMs, ct);
                delayMs = (int)Math.Min(delayMs * _options.FatorExponencial, _options.DelayMaximoMilisegundos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha permanente ao publicar evento após {Max} tentativas", _options.MaximoTentativas);
                throw;
            }
        }
    }

    private async Task LimparEventosAntigosAsync(CancellationToken ct)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var deletados = await outboxRepo.LimparAntigosAsync(_options.DiasRetencao, ct);

            if (deletados > 0)
                _logger.LogInformation("Limpeza do Outbox: {Count} eventos antigos removidos", deletados);
        }
    }
}

