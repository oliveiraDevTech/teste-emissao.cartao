using Driven.RabbitMQ.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Core.Application.Interfaces;
using RabbitMQPublisher = Driven.RabbitMQ.Interfaces.IMessagePublisher;
using RabbitMQSettings = Driven.RabbitMQ.Settings.RabbitMQSettings;

namespace Driving.Api.Handlers;

/// <summary>
/// Handler para processar eventos de pedido de emissão de cartão
/// Consome PedidoEmissaoCartaoIntegrationEvent do serviço de Clientes
/// Realiza a emissão do cartão e publica resultado
/// </summary>
public class PedidoEmissaoCartaoEventHandler
{
    private readonly ICardRepository _cardRepository;
    private readonly RabbitMQPublisher _messagePublisher;
    private readonly ILogger<PedidoEmissaoCartaoEventHandler> _logger;
    private readonly RabbitMQSettings _rabbitMQSettings;

    public PedidoEmissaoCartaoEventHandler(
        ICardRepository cardRepository,
        RabbitMQPublisher messagePublisher,
        ILogger<PedidoEmissaoCartaoEventHandler> logger,
        IOptions<RabbitMQSettings> rabbitMQSettings)
    {
        _cardRepository = cardRepository;
        _messagePublisher = messagePublisher;
        _logger = logger;
        _rabbitMQSettings = rabbitMQSettings.Value;
    }

    /// <summary>
    /// Processa o evento de pedido de emissão de cartão
    /// Emite um novo cartão e publica evento de conclusão
    /// </summary>
    public async Task HandleAsync(PedidoEmissaoCartaoIntegrationEvent evento)
    {
        try
        {
            _logger.LogInformation(
                "Processando pedido de emissão de cartão para cliente {ClienteId} - {Nome}",
                evento.ClienteId, evento.Nome);

            // Validar dados do evento
            ValidarEvento(evento);

            // Verificar elegibilidade (validação de segurança)
            if (evento.ScoreCredito < 600)
            {
                _logger.LogWarning(
                    "Cliente {ClienteId} rejeitado: score insuficiente ({Score})",
                    evento.ClienteId, evento.ScoreCredito);

                await PublicarFalha(evento.ClienteId, "Score de crédito insuficiente para emissão de cartão");
                return;
            }

            // TODO: Implementar emissão completa de cartão com tokens PAN/CVV
            _logger.LogInformation(
                "Pedido de emissão de cartão recebido para cliente {ClienteId}",
                evento.ClienteId);

            // Publicar evento de sucesso (simplificado por enquanto)
            await _messagePublisher.PublishAsync(_rabbitMQSettings.Queues.CartaoEmitido, new
            {
                ClienteId = evento.ClienteId,
                Status = "PROCESSANDO",
                DataEmissao = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Evento de emissão publicado para cliente {ClienteId}",
                evento.ClienteId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao processar pedido de emissão de cartão para cliente {ClienteId}",
                evento.ClienteId);

            // Publicar evento de falha
            await PublicarFalha(evento.ClienteId, $"Erro ao emitir cartão: {ex.Message}");
        }
    }

    /// <summary>
    /// Valida os dados do evento
    /// </summary>
    private void ValidarEvento(PedidoEmissaoCartaoIntegrationEvent evento)
    {
        if (evento.ClienteId == Guid.Empty)
            throw new ArgumentException("ClienteId não pode ser vazio");

        if (string.IsNullOrWhiteSpace(evento.Nome))
            throw new ArgumentException("Nome do cliente não pode ser vazio");

        if (string.IsNullOrWhiteSpace(evento.CPF))
            throw new ArgumentException("CPF do cliente não pode ser vazio");

        if (string.IsNullOrWhiteSpace(evento.Email))
            throw new ArgumentException("Email do cliente não pode ser vazio");

        if (evento.ScoreCredito < 0 || evento.ScoreCredito > 1000)
            throw new ArgumentException("Score de crédito deve estar entre 0 e 1000");
    }

    /// <summary>
    /// Publica um evento de falha na emissão de cartão
    /// </summary>
    private async Task PublicarFalha(Guid clienteId, string motivo)
    {
        try
        {
            await _messagePublisher.PublishAsync(_rabbitMQSettings.Queues.CartaoEmissaoFalha, new
            {
                ClienteId = clienteId,
                Motivo = motivo,
                DataTentativa = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Evento de falha publicado para cliente {ClienteId}",
                clienteId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao publicar evento de falha para cliente {ClienteId}",
                clienteId);
        }
    }
}
