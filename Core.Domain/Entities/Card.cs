using Core.Domain.Common;

namespace Core.Domain.Entities;

/// <summary>
/// Entidade de domínio para Cartão de Crédito
/// Representa um cartão emitido para um cliente
/// Nunca armazena PAN/CVV em claro (utiliza tokens)
/// </summary>
public class Card : BaseEntity
{
    /// <summary>
    /// ID do cliente proprietário do cartão
    /// </summary>
    public Guid ClienteId { get; private set; }

    /// <summary>
    /// ID da proposta associada (origem da emissão)
    /// </summary>
    public Guid PropostaId { get; private set; }

    /// <summary>
    /// ID da conta associada ao cartão
    /// </summary>
    public Guid ContaId { get; private set; }

    /// <summary>
    /// Código do produto (ex: VISA_GOLD, MASTERCARD_PLATINUM)
    /// </summary>
    public string CodigoProduto { get; private set; } = string.Empty;

    /// <summary>
    /// Tipo de cartão: VIRTUAL ou PHYSICAL
    /// </summary>
    public string Tipo { get; private set; } = "VIRTUAL";

    /// <summary>
    /// Token do PAN (nunca armazenar PAN em claro)
    /// </summary>
    public string TokenPan { get; private set; } = string.Empty;

    /// <summary>
    /// Token do CVV (nunca armazenar CVV em claro)
    /// </summary>
    public string TokenCvv { get; private set; } = string.Empty;

    /// <summary>
    /// Mês de expiração (1-12)
    /// </summary>
    public int MesValidade { get; private set; }

    /// <summary>
    /// Ano de expiração (ex: 2028)
    /// </summary>
    public int AnoValidade { get; private set; }

    /// <summary>
    /// Limite de crédito do cartão
    /// </summary>
    public decimal LimiteCreditoAprovado { get; private set; }

    /// <summary>
    /// Status do cartão: REQUESTED, ISSUED, ACTIVATION_PENDING, ACTIVE, BLOCKED
    /// </summary>
    public string Status { get; private set; } = "REQUESTED";

    /// <summary>
    /// Data de quando o cartão foi ativado
    /// </summary>
    public DateTime? DataAtivacao { get; private set; }

    /// <summary>
    /// Canal de ativação utilizado: APP, OTP, FIRST_PURCHASE
    /// </summary>
    public string? CanalAtivacao { get; private set; }

    /// <summary>
    /// ID de correlação para rastreamento distribuído
    /// </summary>
    public string CorrelacaoId { get; private set; } = string.Empty;

    /// <summary>
    /// Construtor privado para EF Core
    /// </summary>
    private Card() { }

    /// <summary>
    /// Factory method para criar um novo cartão
    /// </summary>
    public static Card Criar(
        Guid clienteId,
        Guid propostaId,
        Guid contaId,
        string codigoProduto,
        string tipo,
        string tokenPan,
        string tokenCvv,
        int mesValidade,
        int anoValidade,
        decimal limiteCreditoAprovado,
        string correlacaoId)
    {
        // Validações básicas
        if (clienteId == Guid.Empty)
            throw new ArgumentException("ClienteId não pode ser vazio");
        if (propostaId == Guid.Empty)
            throw new ArgumentException("PropostaId não pode ser vazio");
        if (contaId == Guid.Empty)
            throw new ArgumentException("ContaId não pode ser vazio");
        if (string.IsNullOrWhiteSpace(codigoProduto))
            throw new ArgumentException("CodigoProduto não pode estar vazio");
        if (!new[] { "VIRTUAL", "PHYSICAL" }.Contains(tipo))
            throw new ArgumentException("Tipo deve ser VIRTUAL ou PHYSICAL");
        if (string.IsNullOrWhiteSpace(tokenPan))
            throw new ArgumentException("TokenPan não pode estar vazio");
        if (string.IsNullOrWhiteSpace(tokenCvv))
            throw new ArgumentException("TokenCvv não pode estar vazio");
        if (mesValidade < 1 || mesValidade > 12)
            throw new ArgumentException("MesValidade deve estar entre 1 e 12");
        if (anoValidade < DateTime.UtcNow.Year)
            throw new ArgumentException("AnoValidade deve ser no futuro");
        if (limiteCreditoAprovado <= 0)
            throw new ArgumentException("LimiteCreditoAprovado deve ser maior que zero");
        if (string.IsNullOrWhiteSpace(correlacaoId))
            throw new ArgumentException("CorrelacaoId não pode estar vazio");

        return new Card
        {
            Id = Guid.NewGuid(),
            ClienteId = clienteId,
            PropostaId = propostaId,
            ContaId = contaId,
            CodigoProduto = codigoProduto,
            Tipo = tipo,
            TokenPan = tokenPan,
            TokenCvv = tokenCvv,
            MesValidade = mesValidade,
            AnoValidade = anoValidade,
            LimiteCreditoAprovado = limiteCreditoAprovado,
            Status = "REQUESTED",
            CorrelacaoId = correlacaoId,
            DataCriacao = DateTime.UtcNow,
            Ativo = true
        };
    }

    /// <summary>
    /// Marca o cartão como emitido
    /// </summary>
    public void MarcarComoEmitido()
    {
        if (Status != "REQUESTED")
            throw new InvalidOperationException($"Cartão não pode ser marcado como ISSUED do status {Status}");

        Status = "ISSUED";
        MarcarComoAtualizada();
    }

    /// <summary>
    /// Ativa o cartão após validação de OTP/CVV
    /// </summary>
    /// <param name="canalAtivacao">Canal utilizado: APP, OTP, FIRST_PURCHASE</param>
    public new void Ativar(string canalAtivacao)
    {
        if (Status != "ACTIVATION_PENDING" && Status != "ISSUED")
            throw new InvalidOperationException($"Cartão não pode ser ativado do status {Status}");

        if (!new[] { "APP", "OTP", "FIRST_PURCHASE" }.Contains(canalAtivacao))
            throw new ArgumentException("CanalAtivacao inválido");

        Status = "ACTIVE";
        CanalAtivacao = canalAtivacao;
        DataAtivacao = DateTime.UtcNow;
        MarcarComoAtualizada();
    }

    /// <summary>
    /// Verifica se o cartão está expirado
    /// </summary>
    public bool EstaExpirado()
    {
        var hoje = DateTime.UtcNow;
        var proximoMesDiaUm = new DateTime(AnoValidade, MesValidade, 1).AddMonths(1);
        return hoje >= proximoMesDiaUm;
    }

    /// <summary>
    /// Verifica se o cartão pode ser ativado
    /// </summary>
    public bool PodeSerAtivado()
    {
        return (Status == "ISSUED" || Status == "ACTIVATION_PENDING") && !EstaExpirado();
    }
}
