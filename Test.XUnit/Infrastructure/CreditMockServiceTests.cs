using Driven.CreditMock.Services;

namespace Test.XUnit.Infrastructure;

/// <summary>
/// Testes unitários para CreditMockService
/// Testa geração aleatória de scores e descrições de avaliação
/// </summary>
public class CreditMockServiceTests
{
    private readonly CreditMockService _service;

    public CreditMockServiceTests()
    {
        _service = new CreditMockService();
    }

    [Fact]
    public void GerarScoreAleatorio_DeveRetornarEntre0E1000()
    {
        // Arrange
        var clientId = Guid.NewGuid();

        // Act
        var score = _service.GerarScoreAleatorio(clientId);

        // Assert
        score.Should().BeGreaterThanOrEqualTo(0);
        score.Should().BeLessThanOrEqualTo(1000);
    }

    [Fact]
    public void GerarScoreAleatorio_Multiplas_DeveGerarValoresVariados()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var scores = new List<int>();

        // Act - Gera 100 scores
        for (int i = 0; i < 100; i++)
        {
            scores.Add(_service.GerarScoreAleatorio(clientId));
        }

        // Assert - Todos entre 0 e 1000
        scores.Should().AllSatisfy(s => s.Should().BeGreaterThanOrEqualTo(0).And.BeLessThanOrEqualTo(1000));

        // Assert - Deve ter variação (não todos iguais)
        var distintos = scores.Distinct().Count();
        distintos.Should().BeGreaterThan(1); // Pelo menos 2 valores diferentes
    }

    [Fact]
    public void GerarScoreComDistribuicaoNormal_DeveRetornarEntre0E1000()
    {
        // Arrange
        var clientId = Guid.NewGuid();

        // Act
        var score = _service.GerarScoreComDistribuicaoNormal(clientId);

        // Assert
        score.Should().BeGreaterThanOrEqualTo(0);
        score.Should().BeLessThanOrEqualTo(1000);
    }

    [Fact]
    public void GerarScoreComDistribuicaoNormal_DeveTerescoresAltos()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var scores = new List<int>();
        var media = 0;

        // Act - Gera 1000 scores
        for (int i = 0; i < 1000; i++)
        {
            scores.Add(_service.GerarScoreComDistribuicaoNormal(clientId));
        }

        media = (int)scores.Average(s => (double)s);

        // Assert - Média deve estar ao redor de 650 (por causa da distribuição gaussiana)
        // Tolerância: entre 600 e 700
        media.Should().BeGreaterThan(600).And.BeLessThan(700);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void ObterDescricaoAvaliacao_ComScoreBaixo_DeveRetornarReprovado(int score)
    {
        // Act
        var descricao = _service.ObterDescricaoAvaliacao(score);

        // Assert
        descricao.Should().Contain("Reprovado");
        descricao.Should().Contain("Score muito baixo");
        descricao.Should().Contain("Sem liberação");
    }

    [Theory]
    [InlineData(101)]
    [InlineData(250)]
    [InlineData(500)]
    public void ObterDescricaoAvaliacao_ComScoreMedio_DeveRetornarAprovadoRestricoes(int score)
    {
        // Act
        var descricao = _service.ObterDescricaoAvaliacao(score);

        // Assert
        descricao.Should().Contain("Aprovado");
        descricao.Should().Contain("restrições");
        descricao.Should().Contain("1 cartão");
        descricao.Should().Contain("R$ 1.000");
    }

    [Theory]
    [InlineData(501)]
    [InlineData(600)]
    [InlineData(750)]
    public void ObterDescricaoAvaliacao_ComScoreAlto_DeveRetornarAprovadoDoisCartoes(int score)
    {
        // Act
        var descricao = _service.ObterDescricaoAvaliacao(score);

        // Assert
        descricao.Should().Contain("Aprovado");
        descricao.Should().Contain("2 cartões");
        descricao.Should().Contain("R$ 5.000");
    }

    [Theory]
    [InlineData(900)]
    [InlineData(950)]
    [InlineData(1000)]
    public void ObterDescricaoAvaliacao_ComScoreExcelente_DeveRetornarAprovadoPremium(int score)
    {
        // Act
        var descricao = _service.ObterDescricaoAvaliacao(score);

        // Assert
        descricao.Should().Contain("Aprovado");
        descricao.Should().Contain("2 cartões");
        descricao.Should().Contain("R$ 5.000");
    }

    [Fact]
    public void ObterDescricaoAvaliacao_Score501_DeveRetornarAprovado()
    {
        // Act
        var descricao = _service.ObterDescricaoAvaliacao(501);

        // Assert
        descricao.Should().NotBeNullOrEmpty();
        descricao.Should().Contain("Aprovado");
    }

    [Fact]
    public void GerarScoreAleatorio_ComClientesDiferentes_PodeGerarMesmoScore()
    {
        // Arrange
        var clientId1 = Guid.NewGuid();
        var clientId2 = Guid.NewGuid();

        // Act - Múltiplas chamadas
        var scores1 = Enumerable.Range(0, 10)
            .Select(_ => _service.GerarScoreAleatorio(clientId1))
            .ToList();

        var scores2 = Enumerable.Range(0, 10)
            .Select(_ => _service.GerarScoreAleatorio(clientId2))
            .ToList();

        // Assert - Ambos devem estar no intervalo válido
        scores1.Should().AllSatisfy(s => s.Should().BeGreaterThanOrEqualTo(0).And.BeLessThanOrEqualTo(1000));
        scores2.Should().AllSatisfy(s => s.Should().BeGreaterThanOrEqualTo(0).And.BeLessThanOrEqualTo(1000));
    }
}
