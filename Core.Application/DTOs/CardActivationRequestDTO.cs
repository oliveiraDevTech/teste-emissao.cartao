namespace Core.Application.DTOs;

/// <summary>
/// DTO para requisição de ativação de cartão
/// </summary>
public class CardActivationRequestDTO
{
    /// <summary>
    /// OTP ou CVV para ativação
    /// </summary>
    public string OtpOuCvv { get; set; } = string.Empty;

    /// <summary>
    /// Canal de ativação: APP, OTP
    /// </summary>
    public string Canal { get; set; } = "APP";

    /// <summary>
    /// ID de correlação para rastreamento
    /// </summary>
    public string CorrelacaoId { get; set; } = string.Empty;
}

/// <summary>
/// DTO para resposta de ativação
/// </summary>
public class CardActivationResponseDTO
{
    /// <summary>
    /// ID do cartão ativado
    /// </summary>
    public Guid IdCartao { get; set; }

    /// <summary>
    /// ID do cliente
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Status após ativação
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Data de ativação
    /// </summary>
    public DateTime DataAtivacao { get; set; }

    /// <summary>
    /// Canal utilizado
    /// </summary>
    public string CanalAtivacao { get; set; } = string.Empty;

    /// <summary>
    /// ID de correlação
    /// </summary>
    public string CorrelacaoId { get; set; } = string.Empty;
}
