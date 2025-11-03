namespace Core.Application.Interfaces;

/// <summary>
/// Interface abstrata para publicação de mensagens
/// Desacopla Core.Application da implementação específica (RabbitMQ, etc)
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publica uma mensagem em um tópico
    /// </summary>
    /// <param name="topic">Tópico/fila de destino</param>
    /// <param name="message">Conteúdo da mensagem</param>
    void Publish(string topic, string message);

    /// <summary>
    /// Publica uma mensagem de forma assíncrona
    /// </summary>
    Task PublishAsync(string topic, string message, CancellationToken cancellationToken = default);
}
