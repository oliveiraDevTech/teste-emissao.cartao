using Driven.CreditMock.Interfaces;

namespace Driven.CreditMock.Services;

/// <summary>
/// Serviço mock que simula um serviço externo de validação de crédito
/// Gera scores aleatórios para testes e desenvolvimento
/// </summary>
public class CreditMockService : ICreditMockService
{
    private static readonly Random _random = new();

    /// <summary>
    /// Gera um score de crédito aleatório (distribuição uniforme 0-1000)
    /// </summary>
    /// <param name="clientId">ID do cliente (utilizado para seed no futuro)</param>
    /// <returns>Score entre 0 e 1000</returns>
    public int GerarScoreAleatorio(Guid clientId)
    {
        // Score aleatório entre 0 e 1000
        return _random.Next(0, 1001);
    }

    /// <summary>
    /// Gera um score com distribuição mais realista
    /// Tendência a gerar scores na faixa 500-800 (aprovações comuns)
    /// </summary>
    /// <param name="clientId">ID do cliente</param>
    /// <returns>Score entre 0 e 1000 com distribuição normal</returns>
    public int GerarScoreComDistribuicaoNormal(Guid clientId)
    {
        // Simula distribuição normal centrada em 650
        // Com mais chance de scores altos que baixos
        double gaussian = GerarGaussiana(650, 200);

        // Clamp entre 0 e 1000
        int score = (int)Math.Max(0, Math.Min(1000, gaussian));

        return score;
    }

    /// <summary>
    /// Obtém descrição textual da avaliação baseada no score
    /// </summary>
    public string ObterDescricaoAvaliacao(int score)
    {
        return score switch
        {
            // 0 a 100: Sem aprovação
            <= 100 => "Reprovado - Score muito baixo. Sem liberação de cartão de crédito.",

            // 101 a 500: Aprovação básica
            <= 500 => "Aprovado com restrições - 1 cartão com limite R$ 1.000,00. Recomenda-se melhorar o histórico de crédito.",

            // 501 a 750: Aprovação padrão
            <= 750 => "Aprovado - Até 2 cartões com limite R$ 5.000,00 cada. Histórico de crédito satisfatório.",

            // 751 a 900: Aprovação boa
            <= 900 => "Aprovado com vantagens - Até 2 cartões com limite R$ 5.000,00 cada. Histórico de crédito bom.",

            // 901 a 1000: Aprovação premium
            _ => "Aprovado premium - Até 2 cartões com limite R$ 5.000,00 cada. Excelente histórico de crédito."
        };
    }

    /// <summary>
    /// Gera um número aleatório com distribuição normal (Box-Muller Transform)
    /// </summary>
    private static double GerarGaussiana(double media, double desvio)
    {
        // Box-Muller Transform
        double u1 = _random.NextDouble();
        double u2 = _random.NextDouble();

        double z0 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);

        return media + z0 * desvio;
    }
}
