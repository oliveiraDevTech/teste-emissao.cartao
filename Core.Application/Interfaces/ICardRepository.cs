using Core.Domain.Entities;

namespace Core.Application.Interfaces;

/// <summary>
/// Interface para repositório de Cartões
/// </summary>
public interface ICardRepository
{
    /// <summary>
    /// Adiciona um novo cartão ao repositório
    /// </summary>
    /// <param name="card">Cartão a adicionar</param>
    /// <param name="ct">Token de cancelamento</param>
    Task AdicionarAsync(Card card, CancellationToken ct = default);

    /// <summary>
    /// Obtém um cartão pelo ID
    /// </summary>
    /// <param name="id">ID do cartão</param>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>Cartão ou null se não encontrado</returns>
    Task<Card?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Obtém todos os cartões de um cliente
    /// </summary>
    /// <param name="clienteId">ID do cliente</param>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>Lista de cartões</returns>
    Task<List<Card>> ObterPorClienteAsync(Guid clienteId, CancellationToken ct = default);

    /// <summary>
    /// Obtém cartões por ID de proposta
    /// </summary>
    /// <param name="propostaId">ID da proposta</param>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>Lista de cartões</returns>
    Task<List<Card>> ObterPorPropostaAsync(Guid propostaId, CancellationToken ct = default);

    /// <summary>
    /// Atualiza um cartão existente
    /// </summary>
    /// <param name="card">Cartão a atualizar</param>
    /// <param name="ct">Token de cancelamento</param>
    Task AtualizarAsync(Card card, CancellationToken ct = default);

    /// <summary>
    /// Verifica se uma chave de idempotência já foi processada
    /// </summary>
    /// <param name="chaveIdempotencia">Chave única da requisição</param>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>True se já foi processada</returns>
    Task<bool> ExisteChaveIdempotenciaAsync(string chaveIdempotencia, CancellationToken ct = default);

    /// <summary>
    /// Obtém cartões previamente emitidos para uma chave de idempotência
    /// </summary>
    /// <param name="chaveIdempotencia">Chave de idempotência</param>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>Lista de cartões se encontrados</returns>
    Task<List<Card>> ObterPorChaveIdempotenciaAsync(string chaveIdempotencia, CancellationToken ct = default);

    /// <summary>
    /// Registra uma chave de idempotência como processada
    /// </summary>
    /// <param name="chaveIdempotencia">Chave de idempotência</param>
    /// <param name="cartoesIds">IDs dos cartões emitidos</param>
    /// <param name="ct">Token de cancelamento</param>
    Task RegistrarIdempotenciaAsync(string chaveIdempotencia, IEnumerable<Guid> cartoesIds, CancellationToken ct = default);
}
