namespace Core.Application.DTOs;

/// <summary>
/// DTO para evento de cartão emitido
/// Publicado após emissão bem-sucedida
/// </summary>
public class CardIssuedEventDTO
{
    /// <summary>
    /// ID da proposta
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
    /// Lista de cartões emitidos
    /// </summary>
    public List<CartaoEmitidoDTO> Cartoes { get; set; } = new();

    /// <summary>
    /// ID de correlação
    /// </summary>
    public string CorrelacaoId { get; set; } = string.Empty;

    /// <summary>
    /// Data de emissão
    /// </summary>
    public DateTime DataEmissao { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Dados do cartão no evento de emissão
/// </summary>
public class CartaoEmitidoDTO
{
    /// <summary>
    /// ID do cartão
    /// </summary>
    public Guid IdCartao { get; set; }

    /// <summary>
    /// Token do PAN (nunca expor PAN real)
    /// </summary>
    public string TokenPan { get; set; } = string.Empty;

    /// <summary>
    /// Validade formatada (MM/YY)
    /// </summary>
    public string Validade { get; set; } = string.Empty;

    /// <summary>
    /// Tipo: VIRTUAL ou PHYSICAL
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Status do cartão
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
