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
    /// ID da proposta de crédito
    /// </summary>
    public Guid PropostaId { get; set; }

    /// <summary>
    /// ID da conta bancária do cliente
    /// </summary>
    public Guid ContaId { get; set; }

    /// <summary>
    /// Código do produto (ex: CREDIT_CARD_PLATINUM)
    /// </summary>
    public string CodigoProduto { get; set; } = string.Empty;

    /// <summary>
    /// Quantidade de cartões a emitir
    /// </summary>
    public int QuantidadeCartoesEmitir { get; set; }

    /// <summary>
    /// Limite de crédito por cartão
    /// </summary>
    public decimal LimiteCreditoPorCartao { get; set; }

    /// <summary>
    /// ID de correlação para rastreamento
    /// </summary>
    public string CorrelacaoId { get; set; } = string.Empty;

    /// <summary>
    /// Chave de idempotência para evitar duplicatas
    /// </summary>
    public string ChaveIdempotencia { get; set; } = string.Empty;

    /// <summary>
    /// Informações de entrega
    /// </summary>
    public EntregaInfo Entrega { get; set; } = new();

    /// <summary>
    /// Data da solicitação
    /// </summary>
    public DateTime DataSolicitacao { get; set; }
}

/// <summary>
/// Informações de entrega do cartão
/// </summary>
public class EntregaInfo
{
    public string TipoEntrega { get; set; } = string.Empty;
    public EnderecoEntregaInfo EnderecoEntrega { get; set; } = new();
}

/// <summary>
/// Endereço de entrega
/// </summary>
public class EnderecoEntregaInfo
{
    public string Logradouro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
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
