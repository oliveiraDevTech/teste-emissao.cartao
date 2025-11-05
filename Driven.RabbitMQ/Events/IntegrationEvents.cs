namespace Driven.RabbitMQ.Events;

/// <summary>
/// Evento de integração: Pedido de emissão de cartão (consumido de Clientes)
/// </summary>
public class PedidoEmissaoCartaoIntegrationEvent : DomainEvent
{
    /// <summary>
    /// ID do cliente que solicitou o cartão
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Nome completo do cliente
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// CPF do cliente
    /// </summary>
    public string CPF { get; set; } = string.Empty;

    /// <summary>
    /// Email do cliente
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Score de crédito atual do cliente
    /// </summary>
    public int ScoreCredito { get; set; }

    /// <summary>
    /// Data do pedido
    /// </summary>
    public DateTime DataPedido { get; set; }
}

/// <summary>
/// Evento de integração: Cartão foi emitido com sucesso
/// Publicado para o serviço de Clientes
/// </summary>
public class CartaoEmitidoIntegrationEvent : DomainEvent
{
    /// <summary>
    /// ID do cliente que recebeu o cartão
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// ID do cartão emitido
    /// </summary>
    public Guid CartaoId { get; set; }

    /// <summary>
    /// Número do cartão (mascarado/parcial por segurança)
    /// </summary>
    public string NumeroCartao { get; set; } = string.Empty;

    /// <summary>
    /// Status do cartão
    /// </summary>
    public string Status { get; set; } = "EMITIDO";

    /// <summary>
    /// Data de emissão
    /// </summary>
    public DateTime DataEmissao { get; set; }
}

/// <summary>
/// Evento de integração: Emissão de cartão falhou
/// Publicado para o serviço de Clientes quando há erro
/// </summary>
public class CartaoEmissaoFalhouIntegrationEvent : DomainEvent
{
    /// <summary>
    /// ID do cliente que solicitou o cartão
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Motivo da falha
    /// </summary>
    public string Motivo { get; set; } = string.Empty;

    /// <summary>
    /// Data da tentativa
    /// </summary>
    public DateTime DataTentativa { get; set; }
}
