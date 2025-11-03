using System.Text;
using Core.Application.Interfaces;

namespace Core.Infra.CardIssuance;

/// <summary>
/// Gerador de números de cartão com validação Luhn
/// </summary>
public sealed class PanGenerator : IPanGenerator
{
    private readonly Random _random = new();

    /// <summary>
    /// Gera um PAN válido com algoritmo Luhn
    /// </summary>
    /// <param name="bin">Bank Identification Number (6 dígitos)</param>
    /// <param name="tamanho">Tamanho total do PAN (padrão 16)</param>
    /// <returns>PAN válido com dígito de verificação</returns>
    public string GerarPan(string bin, int tamanho = 16)
    {
        if (string.IsNullOrWhiteSpace(bin) || bin.Length < 6)
            throw new ArgumentException("BIN deve ter no mínimo 6 dígitos");

        if (tamanho < 13 || tamanho > 19)
            throw new ArgumentException("Tamanho do PAN deve estar entre 13 e 19");

        // Gerar números aleatórios entre BIN e dígito de verificação
        var bodyLength = tamanho - bin.Length - 1;
        var body = GenerarNumeroAleatorio(bodyLength);
        var partial = bin + body;

        // Calcular dígito de verificação Luhn
        var checkDigit = CalcularDigitoLuhn(partial);
        return partial + checkDigit;
    }

    /// <summary>
    /// Gera um CVV aleatório (3 dígitos)
    /// </summary>
    /// <returns>CVV com 3 dígitos</returns>
    public string GerarCvv()
    {
        return _random.Next(100, 999).ToString();
    }

    private string GenerarNumeroAleatorio(int length)
    {
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < length; i++)
        {
            result.Append(_random.Next(0, 10));
        }

        return result.ToString();
    }

    private int CalcularDigitoLuhn(string partial)
    {
        int sum = 0;
        bool alternado = true;

        for (int i = partial.Length - 1; i >= 0; i--)
        {
            int digito = int.Parse(partial[i].ToString());

            if (alternado)
            {
                digito *= 2;
                if (digito > 9)
                    digito -= 9;
            }

            sum += digito;
            alternado = !alternado;
        }

        return (10 - (sum % 10)) % 10;
    }
}
