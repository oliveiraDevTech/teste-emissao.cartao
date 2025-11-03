namespace Core.Application.Interfaces;

/// <summary>
/// Interface para gerador de PAN com validação Luhn
/// </summary>
public interface IPanGenerator
{
    /// <summary>
    /// Gera um PAN válido com algoritmo Luhn
    /// </summary>
    /// <param name="bin">Bank Identification Number (6 dígitos)</param>
    /// <param name="tamanho">Tamanho total do PAN (padrão 16)</param>
    /// <returns>PAN válido com dígito de verificação</returns>
    string GerarPan(string bin, int tamanho = 16);

    /// <summary>
    /// Gera um CVV aleatório (3 dígitos)
    /// </summary>
    /// <returns>CVV com 3 dígitos</returns>
    string GerarCvv();
}
