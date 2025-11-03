using System.Text.Json;
using Core.Application.Interfaces;
using Core.Domain.Entities;
using Driven.SqlLite.Data;
using Microsoft.EntityFrameworkCore;

namespace Driven.SqlLite.Repositories;

/// <summary>
/// Repositório de Cartões com suporte a idempotência
/// </summary>
public sealed class CardRepository : ICardRepository
{
    private readonly ApplicationDbContext _context;

    public CardRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AdicionarAsync(Card card, CancellationToken ct = default)
    {
        if (card == null)
            throw new ArgumentNullException(nameof(card));

        await _context.Cards.AddAsync(card, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Card?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Cards.FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<List<Card>> ObterPorClienteAsync(Guid clienteId, CancellationToken ct = default)
    {
        return await _context.Cards
            .Where(c => c.ClienteId == clienteId)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync(ct);
    }

    public async Task<List<Card>> ObterPorPropostaAsync(Guid propostaId, CancellationToken ct = default)
    {
        return await _context.Cards
            .Where(c => c.PropostaId == propostaId)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync(ct);
    }

    public async Task AtualizarAsync(Card card, CancellationToken ct = default)
    {
        if (card == null)
            throw new ArgumentNullException(nameof(card));

        _context.Cards.Update(card);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExisteChaveIdempotenciaAsync(string chaveIdempotencia, CancellationToken ct = default)
    {
        return await _context.CardIdempotencyKeys
            .AnyAsync(k => k.ChaveIdempotencia == chaveIdempotencia, ct);
    }

    public async Task<List<Card>> ObterPorChaveIdempotenciaAsync(string chaveIdempotencia, CancellationToken ct = default)
    {
        var key = await _context.CardIdempotencyKeys
            .FirstOrDefaultAsync(k => k.ChaveIdempotencia == chaveIdempotencia, ct);

        if (key == null)
            return new List<Card>();

        try
        {
            var cartoesIds = JsonSerializer.Deserialize<List<Guid>>(key.CartoesIds) ?? new List<Guid>();
            return await _context.Cards
                .Where(c => cartoesIds.Contains(c.Id))
                .ToListAsync(ct);
        }
        catch
        {
            return new List<Card>();
        }
    }

    public async Task RegistrarIdempotenciaAsync(string chaveIdempotencia, IEnumerable<Guid> cartoesIds, CancellationToken ct = default)
    {
        var key = CardIdempotencyKey.Criar(chaveIdempotencia, cartoesIds);
        await _context.CardIdempotencyKeys.AddAsync(key, ct);
        await _context.SaveChangesAsync(ct);
    }
}
