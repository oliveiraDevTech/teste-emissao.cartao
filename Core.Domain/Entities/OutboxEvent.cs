using Core.Domain.Common;

namespace Core.Domain.Entities;

/// <summary>
/// Entidade que implementa o padrão Outbox
/// Garante consistência distribuída através de persistência antes da publicação
/// </summary>
public class OutboxEvent : BaseEntity
{
    /// <summary>
    /// Tópico/tipo do evento
    /// </summary>
    public string Topico { get; set; } = string.Empty;

    /// <summary>
    /// Conteúdo do evento em JSON
    /// </summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// Data em que o evento foi enviado (null enquanto pendente)
    /// </summary>
    public DateTime? DataEnvio { get; set; }

    /// <summary>
    /// Factory method
    /// </summary>
    public static OutboxEvent Criar(string topico, string payload)
    {
        if (string.IsNullOrWhiteSpace(topico))
            throw new ArgumentException("Topico não pode estar vazio");

        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload não pode estar vazio");

        return new OutboxEvent
        {
            Id = Guid.NewGuid(),
            Topico = topico,
            Payload = payload,
            DataCriacao = DateTime.UtcNow,
            DataEnvio = null,
            Ativo = true
        };
    }

    /// <summary>
    /// Marca o evento como enviado
    /// </summary>
    public void MarcarComoEnviado()
    {
        DataEnvio = DateTime.UtcNow;
        MarcarComoAtualizada();
    }
}
