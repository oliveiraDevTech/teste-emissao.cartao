namespace Driven.CreditMock.Interfaces;

/// <summary>
/// Interface para serviço mock de validação de crédito
/// Simula um serviço externo de análise de crédito que retorna um score aleatório
/// </summary>
public interface ICreditMockService
{
    /// <summary>
    /// Gera um score de crédito aleatório para um cliente
    /// </summary>
    /// <param name="clientId">ID do cliente</param>
    /// <returns>Score de crédito entre 0 e 1000</returns>
    int GerarScoreAleatorio(Guid clientId);

    /// <summary>
    /// Gera um score de crédito com distribuição mais realista
    /// Tendência a gerar scores mais altos (simula aprovações mais comuns)
    /// </summary>
    /// <param name="clientId">ID do cliente</param>
    /// <returns>Score de crédito entre 0 e 1000</returns>
    int GerarScoreComDistribuicaoNormal(Guid clientId);

    /// <summary>
    /// Obtém a descrição textual da avaliação de crédito baseada no score
    /// </summary>
    /// <param name="score">Score de crédito</param>
    /// <returns>Descrição textual da avaliação</returns>
    string ObterDescricaoAvaliacao(int score);
}
