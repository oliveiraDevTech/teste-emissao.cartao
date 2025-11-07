using Driven.RabbitMQ.Events;
using Core.Application.Services;
using Core.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace Core.Application.Handlers;

/// <summary>
/// Handler para processar pedidos de emissão de cartão vindos da fila
/// Consome PedidoEmissaoCartaoIntegrationEvent do serviço de Clientes
/// </summary>
public class PedidoEmissaoCartaoEventHandler
{
    private readonly CardIssuanceService _cardIssuanceService;
    private readonly ILogger<PedidoEmissaoCartaoEventHandler> _logger;

    public PedidoEmissaoCartaoEventHandler(
        CardIssuanceService cardIssuanceService,
        ILogger<PedidoEmissaoCartaoEventHandler> logger)
    {
        _cardIssuanceService = cardIssuanceService;
        _logger = logger;
    }

    /// <summary>
    /// Processa o pedido de emissão de cartão
    /// </summary>
    public async Task HandleAsync(PedidoEmissaoCartaoIntegrationEvent evento)
    {
        try
        {
            _logger.LogInformation(
                "Processando pedido de emissão de cartão. ClienteId={ClienteId}, Quantidade={Quantidade}, CorrelacaoId={CorrelacaoId}",
                evento.ClienteId, evento.QuantidadeCartoesEmitir, evento.CorrelacaoId
            );

            // Validar dados do evento
            ValidarEvento(evento);

            // Criar request para o serviço de emissão
            var request = new CardIssuanceRequestDTO
            {
                ClienteId = evento.ClienteId,
                PropostaId = evento.PropostaId,
                ContaId = evento.ContaId,
                CodigoProduto = evento.CodigoProduto,
                QuantidadeCartoesEmitir = evento.QuantidadeCartoesEmitir,
                LimiteCreditoPorCartao = evento.LimiteCreditoPorCartao,
                CorrelacaoId = evento.CorrelacaoId,
                ChaveIdempotencia = evento.ChaveIdempotencia,
                Entrega = new DeliveryConfigDTO
                {
                    // Mapear TipoEntrega do evento para flags Fisico/Virtual
                    Fisico = evento.Entrega.TipoEntrega != "VIRTUAL",
                    Virtual = evento.Entrega.TipoEntrega == "VIRTUAL" || evento.Entrega.TipoEntrega == "AMBOS"
                }
            };

            // Emitir cartões
            var cartoes = await _cardIssuanceService.EmitirCartõesAsync(request);

            _logger.LogInformation(
                "Cartões emitidos com sucesso. ClienteId={ClienteId}, Quantidade={Quantidade}, CartaoIds={CartaoIds}",
                evento.ClienteId, 
                cartoes.Count, 
                string.Join(", ", cartoes.Select(c => c.Id))
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao processar pedido de emissão de cartão. ClienteId={ClienteId}, CorrelacaoId={CorrelacaoId}",
                evento.ClienteId, evento.CorrelacaoId
            );
            throw;
        }
    }

    /// <summary>
    /// Valida os dados do evento
    /// </summary>
    private void ValidarEvento(PedidoEmissaoCartaoIntegrationEvent evento)
    {
        if (evento.ClienteId == Guid.Empty)
            throw new ArgumentException("ClienteId não pode ser vazio");

        if (evento.PropostaId == Guid.Empty)
            throw new ArgumentException("PropostaId não pode ser vazio");

        if (evento.ContaId == Guid.Empty)
            throw new ArgumentException("ContaId não pode ser vazio");

        if (string.IsNullOrWhiteSpace(evento.CodigoProduto))
            throw new ArgumentException("CodigoProduto não pode estar vazio");

        if (evento.QuantidadeCartoesEmitir <= 0 || evento.QuantidadeCartoesEmitir > 2)
            throw new ArgumentException("QuantidadeCartoesEmitir deve estar entre 1 e 2");

        if (evento.LimiteCreditoPorCartao <= 0)
            throw new ArgumentException("LimiteCreditoPorCartao deve ser maior que zero");

        if (string.IsNullOrWhiteSpace(evento.CorrelacaoId))
            throw new ArgumentException("CorrelacaoId não pode estar vazio");
    }
}
