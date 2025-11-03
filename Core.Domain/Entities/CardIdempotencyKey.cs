using Core.Domain.Common;

namespace Core.Domain.Entities;

/// <summary>
/// Entidade para rastrear chaves de idempotência
/// Previne emissão duplicada de cartões
/// </summary>
public class CardIdempotencyKey : BaseEntity
{
    /// <summary>
    /// Chave única de idempotência (hash ou valor fornecido)
    /// </summary>
    public string ChaveIdempotencia { get; set; } = string.Empty;

    /// <summary>
    /// IDs dos cartões já emitidos para esta chave (JSON array)
    /// </summary>
    public string CartoesIds { get; set; } = string.Empty;

    /// <summary>
    /// Factory method
    /// </summary>
    public static CardIdempotencyKey Criar(string chaveIdempotencia, IEnumerable<Guid> cartoesIds)
    {
        if (string.IsNullOrWhiteSpace(chaveIdempotencia))
            throw new ArgumentException("ChaveIdempotencia não pode estar vazia");

        return new CardIdempotencyKey
        {
            Id = Guid.NewGuid(),
            ChaveIdempotencia = chaveIdempotencia,
            CartoesIds = System.Text.Json.JsonSerializer.Serialize(cartoesIds.ToList()),
            DataCriacao = DateTime.UtcNow,
            Ativo = true
        };
    }
}
