using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Core.Application.Interfaces;

namespace Core.Infra.CardIssuance;

/// <summary>
/// Implementação em memória do vault de tokenização
/// Para produção, substituir por HSM real ou vault externo
/// </summary>
public sealed class InMemoryTokenVault : ITokenVault
{
    private readonly ConcurrentDictionary<string, string> _store = new();
    private readonly string _encryptionKey;

    public InMemoryTokenVault(string encryptionKey = "default-dev-key-32-chars-min!!!")
    {
        // Em produção, carregar chave de variável de ambiente segura
        if (string.IsNullOrEmpty(encryptionKey))
            throw new ArgumentException("Encryption key não pode estar vazio");

        _encryptionKey = encryptionKey.PadRight(32).Substring(0, 32); // Garantir 32 bytes para AES
    }

    public string ArmazenarPan(string pan)
    {
        if (string.IsNullOrWhiteSpace(pan) || pan.Length != 16)
            throw new ArgumentException("PAN deve ter exatamente 16 dígitos");

        var token = GerarToken("pan");
        var encrypted = EncriptarAes(pan);
        _store[token] = encrypted;

        return token;
    }

    public string ArmazenarCvv(string cvv)
    {
        if (string.IsNullOrWhiteSpace(cvv) || (cvv.Length != 3 && cvv.Length != 4))
            throw new ArgumentException("CVV deve ter 3 ou 4 dígitos");

        var token = GerarToken("cvv");
        var encrypted = EncriptarAes(cvv);
        _store[token] = encrypted;

        return token;
    }

    private static string GerarToken(string prefix)
    {
        return $"tok_{prefix}_{Guid.NewGuid():N}";
    }

    private string EncriptarAes(string plaintext)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);
            aes.GenerateIV();

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream())
            {
                ms.Write(aes.IV, 0, aes.IV.Length);
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plaintext);
                }

                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    private string DescriptarAes(string ciphertext)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);

            var buffer = Convert.FromBase64String(ciphertext);
            aes.IV = new byte[aes.IV.Length];
            Array.Copy(buffer, 0, aes.IV, 0, aes.IV.Length);

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream(buffer, aes.IV.Length, buffer.Length - aes.IV.Length))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
