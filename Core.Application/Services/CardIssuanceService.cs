using System.Text.Json;
using Core.Application.DTOs;
using Core.Application.Interfaces;
using Core.Application.Options;
using Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Core.Application.Services;

/// <summary>
/// Serviço de aplicação para emissão de cartões
/// Orquestra a geração, tokenização e persistência de cartões
/// Implementa idempotência e outbox pattern
/// </summary>
public sealed class CardIssuanceService
{
    private readonly ICardRepository _cardRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly ITokenVault _tokenVault;
    private readonly IPanGenerator _panGenerator;
    private readonly CardIssuanceOptions _options;
    private readonly ILogger<CardIssuanceService> _logger;

    public CardIssuanceService(
        ICardRepository cardRepository,
        IOutboxRepository outboxRepository,
        ITokenVault tokenVault,
        IPanGenerator panGenerator,
        IOptions<CardIssuanceOptions> options,
        ILogger<CardIssuanceService> logger)
    {
        _cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
        _outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
        _tokenVault = tokenVault ?? throw new ArgumentNullException(nameof(tokenVault));
        _panGenerator = panGenerator ?? throw new ArgumentNullException(nameof(panGenerator));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Emite cartões para um cliente
    /// Implementa idempotência por chave de idempotência
    /// </summary>
    public async Task<List<Card>> EmitirCartõesAsync(CardIssuanceRequestDTO request, CancellationToken ct = default)
    {
        ValidarRequisicao(request);

        // Verificar idempotência
        if (!string.IsNullOrWhiteSpace(request.ChaveIdempotencia))
        {
            if (await _cardRepository.ExisteChaveIdempotenciaAsync(request.ChaveIdempotencia, ct))
            {
                _logger.LogInformation(
                    "Requisição idempotente já processada. ChaveIdempotencia={ChaveIdempotencia}, CorrelacaoId={CorrelacaoId}",
                    request.ChaveIdempotencia, request.CorrelacaoId);

                return await _cardRepository.ObterPorChaveIdempotenciaAsync(request.ChaveIdempotencia, ct);
            }
        }

        var cartoesEmitidos = new List<Card>();

        try
        {
            // Calcular quantidade real de cartões a emitir
            var quantidade = CalcularQuantidadeCartoes(request.QuantidadeCartoesEmitir);

            // Gerar tipos de cartão conforme configuração
            var tipos = DetermirarTiposCartao(request.Entrega, quantidade);

            // Emitir cada cartão
            for (int i = 0; i < quantidade; i++)
            {
                var card = EmitirCartao(request, tipos[i]);
                await _cardRepository.AdicionarAsync(card, ct);
                cartoesEmitidos.Add(card);

                _logger.LogInformation(
                    "Cartão emitido. CardId={CardId}, Tipo={Tipo}, CorrelacaoId={CorrelacaoId}",
                    card.Id, card.Tipo, request.CorrelacaoId);
            }

            // Registrar idempotência
            if (!string.IsNullOrWhiteSpace(request.ChaveIdempotencia))
            {
                await _cardRepository.RegistrarIdempotenciaAsync(
                    request.ChaveIdempotencia,
                    cartoesEmitidos.Select(c => c.Id),
                    ct);
            }

            // Publicar evento de emissão
            await PublicarEventoEmissaoAsync(request, cartoesEmitidos, ct);

            _logger.LogInformation(
                "Cartões emitidos com sucesso. Quantidade={Quantidade}, CorrelacaoId={CorrelacaoId}",
                cartoesEmitidos.Count, request.CorrelacaoId);

            return cartoesEmitidos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro ao emitir cartões. CorrelacaoId={CorrelacaoId}, Mensagem={Mensagem}",
                request.CorrelacaoId, ex.Message);

            throw;
        }
    }

    /// <summary>
    /// Obtém cartões emitidos para um cliente específico
    /// </summary>
    public async Task<List<CartaoClienteDTO>> ObterCartoesPorClienteAsync(Guid clienteId, CancellationToken ct = default)
    {
        if (clienteId == Guid.Empty)
            throw new ArgumentException("ClienteId não pode ser vazio");

        _logger.LogInformation("Consultando cartões para cliente. ClienteId={ClienteId}", clienteId);

        var cards = await _cardRepository.ObterPorClienteAsync(clienteId, ct);

        var result = cards.Select(card => new CartaoClienteDTO
        {
            Id = card.Id,
            NumeroMascarado = MascararPan(card.TokenPan),
            Tipo = card.Tipo,
            Status = card.Status,
            LimiteCredito = card.LimiteCredito,
            DataEmissao = card.DataEmissao,
            DataAtivacao = card.DataAtivacao
        }).ToList();

        _logger.LogInformation(
            "Cartões retornados. ClienteId={ClienteId}, Quantidade={Quantidade}",
            clienteId, result.Count);

        return result;
    }

    private string MascararPan(string tokenPan)
    {
        // Tentar recuperar PAN real do vault
        try
        {
            var pan = _tokenVault.RecuperarPan(tokenPan);
            if (pan.Length >= 16)
            {
                return $"{pan.Substring(0, 4)} **** **** {pan.Substring(12, 4)}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao recuperar PAN do vault. Token={Token}", tokenPan);
        }

        // Fallback: mascarar o token
        return $"**** **** **** {tokenPan.Substring(Math.Max(0, tokenPan.Length - 4))}";
    }

    private void ValidarRequisicao(CardIssuanceRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.ClienteId == Guid.Empty)
            throw new ArgumentException("ClienteId não pode ser vazio");

        if (request.ContaId == Guid.Empty)
            throw new ArgumentException("ContaId não pode ser vazio");

        if (request.PropostaId == Guid.Empty)
            throw new ArgumentException("PropostaId não pode ser vazio");

        if (string.IsNullOrWhiteSpace(request.CodigoProduto))
            throw new ArgumentException("CodigoProduto não pode estar vazio");

        if (request.LimiteCreditoPorCartao <= 0)
            throw new ArgumentException("LimiteCreditoPorCartao deve ser maior que zero");

        if (request.QuantidadeCartoesEmitir < 1 || request.QuantidadeCartoesEmitir > 2)
            throw new ArgumentException("QuantidadeCartoesEmitir deve ser 1 ou 2");

        if (!request.Entrega.Fisico && !request.Entrega.Virtual)
            throw new ArgumentException("Ao menos um tipo de entrega deve estar habilitado");

        if (string.IsNullOrWhiteSpace(request.CorrelacaoId))
            throw new ArgumentException("CorrelacaoId não pode estar vazio");
    }

    private int CalcularQuantidadeCartoes(int solicitado)
    {
        // Limitar a máximo 2 cartões
        return Math.Min(2, Math.Max(1, solicitado));
    }

    private List<string> DetermirarTiposCartao(DeliveryConfigDTO entrega, int quantidade)
    {
        var tipos = new List<string>();

        if (quantidade == 1)
        {
            // Se apenas 1 cartão, preferir virtual
            tipos.Add(entrega.Virtual ? "VIRTUAL" : "PHYSICAL");
        }
        else if (quantidade == 2)
        {
            // Se 2 cartões, emitir virtual e físico
            if (entrega.Virtual) tipos.Add("VIRTUAL");
            if (entrega.Fisico) tipos.Add("PHYSICAL");

            // Se configuração não permite um dos tipos, duplicar o disponível
            if (tipos.Count == 1)
            {
                tipos.Add(tipos[0]);
            }
        }

        return tipos;
    }

    private Card EmitirCartao(CardIssuanceRequestDTO request, string tipo)
    {
        // Selecionar BIN conforme produto
        var bin = SelecionarBin(request.CodigoProduto);

        // Gerar PAN com validação Luhn
        var pan = _panGenerator.GerarPan(bin);

        // Gerar CVV aleatório
        var cvv = _panGenerator.GerarCvv();

        // Tokenizar dados sensíveis
        var tokenPan = _tokenVault.ArmazenarPan(pan);
        var tokenCvv = _tokenVault.ArmazenarCvv(cvv);

        // Calcular validade
        var validadeAté = DateTime.UtcNow.AddYears(_options.AnosValidade);

        // Criar cartão
        var card = Card.Criar(
            request.ClienteId,
            request.PropostaId,
            request.ContaId,
            request.CodigoProduto,
            tipo,
            tokenPan,
            tokenCvv,
            validadeAté.Month,
            validadeAté.Year,
            request.LimiteCreditoPorCartao,
            request.CorrelacaoId);

        card.MarcarComoEmitido();
        return card;
    }

    private string SelecionarBin(string codigoProduto)
    {
        return codigoProduto switch
        {
            "VISA_GOLD" => _options.BinVisa,
            "VISA_PLATINUM" => _options.BinVisa,
            "MASTERCARD_GOLD" => _options.BinMastercard,
            "MASTERCARD_PLATINUM" => _options.BinMastercard,
            _ => _options.BinVisa
        };
    }

    private async Task PublicarEventoEmissaoAsync(
        CardIssuanceRequestDTO request,
        List<Card> cartoesEmitidos,
        CancellationToken ct)
    {
        var @event = new CardIssuedEventDTO
        {
            PropostaId = request.PropostaId,
            ClienteId = request.ClienteId,
            ContaId = request.ContaId,
            CorrelacaoId = request.CorrelacaoId,
            DataEmissao = DateTime.UtcNow,
            Cartoes = cartoesEmitidos.Select(c => new CartaoEmitidoDTO
            {
                IdCartao = c.Id,
                TokenPan = c.TokenPan,
                Validade = $"{c.MesValidade:D2}/{c.AnoValidade % 100:D2}",
                Tipo = c.Tipo,
                Status = c.Status
            }).ToList()
        };

        var payload = JsonSerializer.Serialize(@event);
        await _outboxRepository.AdicionarAsync("card.issued", payload, ct);
    }
}

