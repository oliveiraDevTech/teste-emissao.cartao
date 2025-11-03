using Core.Application.Interfaces;
using Core.Domain.Entities;
using Driven.SqlLite.Data;
using Microsoft.EntityFrameworkCore;

namespace Driven.SqlLite.Repositories;

/// <summary>
/// Repositório para padrão Outbox
/// Persiste eventos para publicação confiável
/// </summary>
public sealed class OutboxRepository : IOutboxRepository
{
    private readonly ApplicationDbContext _context;

    public OutboxRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AdicionarAsync(string topico, string payload, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(topico))
            throw new ArgumentException("Topico não pode estar vazio");

        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload não pode estar vazio");

        var @event = OutboxEvent.Criar(topico, payload);
        await _context.OutboxEvents.AddAsync(@event, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<OutboxEventoDTO>> ObterPendentesAsync(int limite = 100, CancellationToken ct = default)
    {
        return await _context.OutboxEvents
            .Where(e => e.DataEnvio == null)
            .OrderBy(e => e.DataCriacao)
            .Take(limite)
            .Select(e => new OutboxEventoDTO
            {
                Id = e.Id,
                Topico = e.Topico,
                Payload = e.Payload,
                DataCriacao = e.DataCriacao,
                DataEnvio = e.DataEnvio
            })
            .ToListAsync(ct);
    }

    public async Task MarcarComoEnviadoAsync(Guid id, CancellationToken ct = default)
    {
        var @event = await _context.OutboxEvents.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (@event != null)
        {
            @event.MarcarComoEnviado();
            _context.OutboxEvents.Update(@event);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<int> LimparAntigosAsync(int diasRetencao = 7, CancellationToken ct = default)
    {
        var data = DateTime.UtcNow.AddDays(-diasRetencao);
        var eventos = await _context.OutboxEvents
            .Where(e => e.DataEnvio != null && e.DataCriacao < data)
            .ToListAsync(ct);

        _context.OutboxEvents.RemoveRange(eventos);
        await _context.SaveChangesAsync(ct);

        return eventos.Count;
    }
}
