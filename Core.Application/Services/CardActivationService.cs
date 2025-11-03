using System.Text.Json;
using Core.Application.DTOs;
using Core.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Core.Application.Services;

/// <summary>
/// Serviço de ativação de cartões
/// Valida credenciais e marca cartão como ativo
/// Publica evento de ativação
/// </summary>
public sealed class CardActivationService
{
    private readonly ICardRepository _cardRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly ILogger<CardActivationService> _logger;

    public CardActivationService(
        ICardRepository cardRepository,
        IOutboxRepository outboxRepository,
        ILogger<CardActivationService> logger)
    {
        _cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
        _outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Ativa um cartão após validação de OTP/CVV
    /// </summary>
    public async Task<CardActivationResponseDTO> AtivarCartãoAsync(
        Guid idCartao,
        CardActivationRequestDTO request,
        CancellationToken ct = default)
    {
        ValidarRequisicao(request);

        // Obter cartão
        var card = await _cardRepository.ObterPorIdAsync(idCartao, ct);
        if (card == null)
        {
            _logger.LogWarning("Cartão não encontrado para ativação. CardId={CardId}", idCartao);
            throw new KeyNotFoundException($"Cartão {idCartao} não encontrado");
        }

        // Validar se cartão pode ser ativado
        if (!card.PodeSerAtivado())
        {
            _logger.LogWarning(
                "Cartão não pode ser ativado. CardId={CardId}, Status={Status}",
                idCartao, card.Status);

            throw new InvalidOperationException($"Cartão não pode ser ativado. Status atual: {card.Status}");
        }

        // Validar credenciais (simulado)
        ValidarCredenciais(card, request.OtpOuCvv);

        // Ativar cartão
        card.Ativar(request.Canal);
        await _cardRepository.AtualizarAsync(card, ct);

        _logger.LogInformation(
            "Cartão ativado com sucesso. CardId={CardId}, Canal={Canal}, CorrelacaoId={CorrelacaoId}",
            idCartao, request.Canal, request.CorrelacaoId);

        // Publicar evento de ativação
        await PublicarEventoAtivacaoAsync(card, request, ct);

        return new CardActivationResponseDTO
        {
            IdCartao = card.Id,
            ClienteId = card.ClienteId,
            Status = card.Status,
            DataAtivacao = card.DataAtivacao ?? DateTime.UtcNow,
            CanalAtivacao = card.CanalAtivacao ?? request.Canal,
            CorrelacaoId = request.CorrelacaoId
        };
    }

    private void ValidarRequisicao(CardActivationRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.OtpOuCvv))
            throw new ArgumentException("OtpOuCvv não pode estar vazio");

        if (string.IsNullOrWhiteSpace(request.Canal))
            throw new ArgumentException("Canal não pode estar vazio");

        if (!new[] { "APP", "OTP" }.Contains(request.Canal))
            throw new ArgumentException("Canal inválido. Deve ser APP ou OTP");

        if (string.IsNullOrWhiteSpace(request.CorrelacaoId))
            throw new ArgumentException("CorrelacaoId não pode estar vazio");
    }

    private void ValidarCredenciais(Core.Domain.Entities.Card card, string otpOuCvv)
    {
        // Simulação: aceitar qualquer valor não vazio
        // Em produção, validar contra HSM/2FA service

        if (string.IsNullOrWhiteSpace(otpOuCvv))
            throw new InvalidOperationException("OTP/CVV inválido ou expirado");

        if (otpOuCvv.Length < 3 || otpOuCvv.Length > 6 || !otpOuCvv.All(char.IsDigit))
            throw new InvalidOperationException("OTP/CVV em formato inválido");
    }

    private async Task PublicarEventoAtivacaoAsync(
        Core.Domain.Entities.Card card,
        CardActivationRequestDTO request,
        CancellationToken ct)
    {
        var @event = new
        {
            CardId = card.Id,
            ClienteId = card.ClienteId,
            Status = card.Status,
            AtivoEm = card.DataAtivacao,
            CanalAtivacao = request.Canal,
            CorrelacaoId = request.CorrelacaoId
        };

        var payload = JsonSerializer.Serialize(@event);
        await _outboxRepository.AdicionarAsync("card.activated", payload, ct);
    }
}
