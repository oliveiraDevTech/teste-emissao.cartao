namespace Core.Application.Interfaces;

/// <summary>
/// Interface para vault de tokenização de dados sensíveis
/// Simula um HSM/PCI-compliant vault
/// </summary>
public interface ITokenVault
{
    /// <summary>
    /// Armazena um PAN e retorna um token
    /// </summary>
    /// <param name="pan">PAN em claro (16 dígitos)</param>
    /// <returns>Token do PAN (nunca expõe o PAN)</returns>
    string ArmazenarPan(string pan);

    /// <summary>
    /// Armazena um CVV e retorna um token
    /// </summary>
    /// <param name="cvv">CVV em claro (3-4 dígitos)</param>
    /// <returns>Token do CVV (nunca expõe o CVV)</returns>
    string ArmazenarCvv(string cvv);
}
