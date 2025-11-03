namespace Core.Application.Options;

/// <summary>
/// Configurações do serviço de emissão de cartões
/// </summary>
public sealed class CardIssuanceOptions
{
    public string BinVisa { get; set; } = "516233";
    public string BinMastercard { get; set; } = "453912";
    public int AnosValidade { get; set; } = 3;
}
