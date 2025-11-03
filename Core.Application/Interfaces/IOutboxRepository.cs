namespace Core.Application.Interfaces;

/// <summary>
/// Interface para repositório Outbox
/// Implementa o padrão Outbox para garantir consistência distribuída
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Adiciona um evento ao outbox
    /// </summary>
    /// <param name="topico">Tópico/tipo do evento</param>
    /// <param name="payload">Conteúdo do evento (JSON)</param>
    /// <param name="ct">Token de cancelamento</param>
    Task AdicionarAsync(string topico, string payload, CancellationToken ct = default);

    /// <summary>
    /// Obtém eventos não enviados
    /// </summary>
    /// <param name="limite">Limite de eventos a retornar</param>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>Lista de eventos pendentes</returns>
    Task<List<OutboxEventoDTO>> ObterPendentesAsync(int limite = 100, CancellationToken ct = default);

    /// <summary>
    /// Marca um evento como enviado
    /// </summary>
    /// <param name="id">ID do evento</param>
    /// <param name="ct">Token de cancelamento</param>
    Task MarcarComoEnviadoAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Remove eventos antigos já enviados
    /// </summary>
    /// <param name="diasRetencao">Quantos dias manter</param>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>Quantidade de registros deletados</returns>
    Task<int> LimparAntigosAsync(int diasRetencao = 7, CancellationToken ct = default);
}

/// <summary>
/// DTO para leitura de eventos do outbox
/// </summary>
public class OutboxEventoDTO
{
    public Guid Id { get; set; }
    public string Topico { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public DateTime? DataEnvio { get; set; }
}
