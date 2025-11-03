namespace Core.Application.Options;

/// <summary>
/// Configurações do OutboxDispatcher
/// </summary>
public sealed class OutboxDispatcherOptions
{
    public int IntervalMilisegundos { get; set; } = 1000;
    public int LoteTamanho { get; set; } = 100;
    public int DelayRetryMilisegundos { get; set; } = 500;
    public int DelayMaximoMilisegundos { get; set; } = 30000;
    public int MaximoTentativas { get; set; } = 5;
    public double FatorExponencial { get; set; } = 2.0;
    public int DiasRetencao { get; set; } = 7;
}
