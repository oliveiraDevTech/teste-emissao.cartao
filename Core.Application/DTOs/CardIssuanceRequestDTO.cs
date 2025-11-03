namespace Core.Application.DTOs;

/// <summary>
/// DTO para requisição de emissão de cartões
/// Corresponde ao contrato de entrada do serviço
/// </summary>
public class CardIssuanceRequestDTO
{
    /// <summary>
    /// ID da proposta aprovada
    /// </summary>
    public Guid PropostaId { get; set; }

    /// <summary>
    /// ID do cliente
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// ID da conta
    /// </summary>
    public Guid ContaId { get; set; }

    /// <summary>
    /// Quantidade de cartões a emitir
    /// </summary>
    public int QuantidadeCartoesEmitir { get; set; }

    /// <summary>
    /// Limite de crédito por cartão
    /// </summary>
    public decimal LimiteCreditoPorCartao { get; set; }

    /// <summary>
    /// Código do produto
    /// </summary>
    public string CodigoProduto { get; set; } = string.Empty;

    /// <summary>
    /// Configurações de entrega
    /// </summary>
    public DeliveryConfigDTO Entrega { get; set; } = new();

    /// <summary>
    /// ID de correlação para rastreamento
    /// </summary>
    public string CorrelacaoId { get; set; } = string.Empty;

    /// <summary>
    /// Chave de idempotência (header HTTP)
    /// </summary>
    public string ChaveIdempotencia { get; set; } = string.Empty;
}

/// <summary>
/// Configurações de entrega do cartão
/// </summary>
public class DeliveryConfigDTO
{
    /// <summary>
    /// Emitir cartão físico
    /// </summary>
    public bool Fisico { get; set; }

    /// <summary>
    /// Emitir cartão virtual
    /// </summary>
    public bool Virtual { get; set; }
}
