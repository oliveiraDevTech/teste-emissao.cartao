using Core.Application.DTOs;
using Core.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Driving.Api.Controllers;

/// <summary>
/// Controller para operações de emissão e ativação de cartões
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class CardsController : ControllerBase
{
    private readonly CardIssuanceService _issuanceService;
    private readonly CardActivationService _activationService;
    private readonly ILogger<CardsController> _logger;

    public CardsController(
        CardIssuanceService issuanceService,
        CardActivationService activationService,
        ILogger<CardsController> logger)
    {
        _issuanceService = issuanceService ?? throw new ArgumentNullException(nameof(issuanceService));
        _activationService = activationService ?? throw new ArgumentNullException(nameof(activationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Emite cartões para um cliente após aprovação de proposta
    /// Suporta idempotência via header Idempotency-Key
    /// </summary>
    /// <param name="request">Dados para emissão</param>
    /// <returns>Cartões emitidos</returns>
    /// <response code="202">Cartões emitidos com sucesso</response>
    /// <response code="400">Validação falhou</response>
    /// <response code="500">Erro interno</response>
    [HttpPost("issue")]
    [ProducesResponseType(typeof(CardIssuanceResponseDTO), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EmitirCartoes(
        [FromBody] CardIssuanceRequestDTO request,
        [FromHeader(Name = "Idempotency-Key")] string? chaveIdempotencia,
        CancellationToken ct)
    {
        try
        {
            // Adicionar chave de idempotência à requisição
            if (!string.IsNullOrWhiteSpace(chaveIdempotencia))
                request.ChaveIdempotencia = chaveIdempotencia;

            _logger.LogInformation(
                "Requisição de emissão recebida. CorrelacaoId={CorrelacaoId}, ChaveIdempotencia={ChaveIdempotencia}",
                request.CorrelacaoId, request.ChaveIdempotencia);

            var cartoes = await _issuanceService.EmitirCartõesAsync(request, ct);

            var response = new CardIssuanceResponseDTO
            {
                Cartoes = cartoes.Select(c => new CartaoEmitidoDTO
                {
                    IdCartao = c.Id,
                    TokenPan = c.TokenPan,
                    Validade = $"{c.MesValidade:D2}/{c.AnoValidade % 100:D2}",
                    Tipo = c.Tipo,
                    Status = c.Status
                }).ToList(),
                CorrelacaoId = request.CorrelacaoId,
                DataEmissao = DateTime.UtcNow
            };

            return Accepted(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validação falhou na emissão");
            return BadRequest(new ProblemDetails
            {
                Title = "Erro de validação",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao emitir cartões");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Erro interno",
                    Detail = "Falha ao emitir cartões",
                    Status = StatusCodes.Status500InternalServerError
                });
        }
    }

    /// <summary>
    /// Ativa um cartão após validação de OTP/CVV
    /// </summary>
    /// <param name="cardId">ID do cartão a ativar</param>
    /// <param name="request">Dados para ativação (OTP/CVV)</param>
    /// <returns>Status da ativação</returns>
    /// <response code="200">Cartão ativado com sucesso</response>
    /// <response code="400">Validação falhou</response>
    /// <response code="404">Cartão não encontrado</response>
    /// <response code="500">Erro interno</response>
    [HttpPost("{cardId:guid}/activate")]
    [ProducesResponseType(typeof(CardActivationResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtivarCartao(
        [FromRoute] Guid cardId,
        [FromBody] CardActivationRequestDTO request,
        CancellationToken ct)
    {
        try
        {
            _logger.LogInformation(
                "Requisição de ativação recebida. CardId={CardId}, Canal={Canal}",
                cardId, request.Canal);

            var response = await _activationService.AtivarCartãoAsync(cardId, request, ct);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cartão não encontrado");
            return NotFound(new ProblemDetails
            {
                Title = "Não encontrado",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validação falhou na ativação");
            return BadRequest(new ProblemDetails
            {
                Title = "Erro de validação",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operação inválida na ativação");
            return BadRequest(new ProblemDetails
            {
                Title = "Operação inválida",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ativar cartão");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Erro interno",
                    Detail = "Falha ao ativar cartão",
                    Status = StatusCodes.Status500InternalServerError
                });
        }
    }

}

/// <summary>
/// DTO para resposta de emissão de cartões
/// </summary>
public class CardIssuanceResponseDTO
{
    public List<CartaoEmitidoDTO> Cartoes { get; set; } = new();
    public string CorrelacaoId { get; set; } = string.Empty;
    public DateTime DataEmissao { get; set; }
}

