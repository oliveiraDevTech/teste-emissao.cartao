using Core.Domain.Common;

namespace Core.Domain.Entities;

/// <summary>
/// Entidade de domínio para Cliente
/// Representa um cliente no sistema
/// </summary>
public class Cliente : BaseEntity
{
    /// <summary>
    /// Nome completo do cliente
    /// </summary>
    public string Nome { get; private set; } = string.Empty;

    /// <summary>
    /// Email do cliente
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Telefone do cliente
    /// </summary>
    public string Telefone { get; private set; } = string.Empty;

    /// <summary>
    /// CPF do cliente
    /// </summary>
    public string Cpf { get; private set; } = string.Empty;

    /// <summary>
    /// Endereço do cliente
    /// </summary>
    public string Endereco { get; private set; } = string.Empty;

    /// <summary>
    /// Cidade do cliente
    /// </summary>
    public string Cidade { get; private set; } = string.Empty;

    /// <summary>
    /// Estado/UF do cliente
    /// </summary>
    public string Estado { get; private set; } = string.Empty;

    /// <summary>
    /// CEP do cliente
    /// </summary>
    public string Cep { get; private set; } = string.Empty;

    /// <summary>
    /// Score de crédito do cliente (0-1000)
    /// Determina quantidade de cartões e limite de crédito
    /// </summary>
    public int ScoreCredito { get; private set; } = 0;

    /// <summary>
    /// Limite de crédito aprovado por cartão em reais
    /// </summary>
    public decimal LimiteCreditoPorCartao { get; private set; } = 0;

    /// <summary>
    /// Número máximo de cartões permitidos
    /// </summary>
    public int NumeroMaximoCartoes { get; private set; } = 0;

    /// <summary>
    /// Data da última avaliação de crédito
    /// </summary>
    public DateTime? DataUltimaAvaliacaoCredito { get; private set; }

    /// <summary>
    /// Construtor privado para criar cliente vazio
    /// </summary>
    private Cliente() { }

    /// <summary>
    /// Cria uma nova instância de Cliente com os dados fornecidos
    /// Aplica validações de domínio antes de criar
    /// </summary>
    /// <exception cref="ArgumentException">Lançado quando os dados são inválidos</exception>
    public static Cliente Criar(string nome, string email, string telefone, string cpf, string endereco, string cidade, string estado, string cep)
    {
        // Validações rápidas (fast-fail)
        ValidarDados(nome, email, telefone, cpf, endereco, cidade, estado, cep);

        return new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Email = email,
            Telefone = telefone,
            Cpf = cpf,
            Endereco = endereco,
            Cidade = cidade,
            Estado = estado,
            Cep = cep,
            DataCriacao = DateTime.UtcNow,
            Ativo = true
        };
    }

    /// <summary>
    /// Valida os dados do cliente antes da criação
    /// Realiza validações rápidas (fast-fail) para falhar no primeiro erro
    /// </summary>
    private static void ValidarDados(string nome, string email, string telefone, string cpf, string endereco, string cidade, string estado, string cep)
    {
        var erros = new List<string>();

        // Validações de Nome
        if (string.IsNullOrWhiteSpace(nome))
            erros.Add("Nome é obrigatório");
        else if (nome.Length < 3)
            erros.Add("Nome deve ter no mínimo 3 caracteres");
        else if (nome.Length > 150)
            erros.Add("Nome não pode ter mais de 150 caracteres");

        // Validações de Email
        if (string.IsNullOrWhiteSpace(email))
            erros.Add("Email é obrigatório");
        else if (!ValidarEmailFormatoBrasil(email))
            erros.Add("Email inválido");

        // Validações de Telefone
        if (string.IsNullOrWhiteSpace(telefone))
            erros.Add("Telefone é obrigatório");
        else if (!ValidarTelefoneBrasil(telefone))
            erros.Add("Telefone deve ter entre 10 e 11 dígitos");

        // Validações de CPF
        if (string.IsNullOrWhiteSpace(cpf))
            erros.Add("CPF é obrigatório");
        else if (!ValidarCpfComDigitos(cpf))
            erros.Add("CPF inválido");

        // Validações de Endereço
        if (string.IsNullOrWhiteSpace(endereco))
            erros.Add("Endereço é obrigatório");
        else if (endereco.Length < 3)
            erros.Add("Endereço deve ter no mínimo 3 caracteres");
        else if (endereco.Length > 200)
            erros.Add("Endereço não pode ter mais de 200 caracteres");

        // Validações de Cidade
        if (string.IsNullOrWhiteSpace(cidade))
            erros.Add("Cidade é obrigatória");
        else if (cidade.Length < 2)
            erros.Add("Cidade deve ter no mínimo 2 caracteres");
        else if (cidade.Length > 100)
            erros.Add("Cidade não pode ter mais de 100 caracteres");

        // Validações de Estado
        if (string.IsNullOrWhiteSpace(estado))
            erros.Add("Estado é obrigatório");
        else if (estado.Length != 2)
            erros.Add("Estado deve ter 2 caracteres");

        // Validações de CEP
        if (string.IsNullOrWhiteSpace(cep))
            erros.Add("CEP é obrigatório");
        else if (!ValidarCepBrasil(cep))
            erros.Add("CEP deve ter 8 dígitos");

        // Fast-fail: lança exceção na primeira validação
        if (erros.Any())
        {
            throw new ArgumentException($"Erro ao criar cliente: {string.Join("; ", erros)}");
        }
    }

    /// <summary>
    /// Atualiza os dados do cliente com validações
    /// </summary>
    /// <exception cref="ArgumentException">Lançado quando os dados são inválidos</exception>
    public void Atualizar(string nome, string email, string telefone, string endereco, string cidade, string estado, string cep, string? atualizadoPor = null)
    {
        // Valida os novos dados antes de atualizar
        ValidarDados(nome, email, telefone, Cpf, endereco, cidade, estado, cep);

        Nome = nome;
        Email = email;
        Telefone = telefone;
        Endereco = endereco;
        Cidade = cidade;
        Estado = estado;
        Cep = cep;
        MarcarComoAtualizada(atualizadoPor);
    }

    /// <summary>
    /// Valida o formato de um email
    /// </summary>
    private static bool ValidarEmailFormatoBrasil(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 150)
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Valida o CPF com algoritmo de dígitos verificadores
    /// </summary>
    private static bool ValidarCpfComDigitos(string cpf)
    {
        // Remove caracteres não numéricos
        cpf = System.Text.RegularExpressions.Regex.Replace(cpf, @"\D", "");

        // Deve ter exatamente 11 dígitos
        if (cpf.Length != 11)
            return false;

        // Não pode ser sequência repetida
        if (cpf == new string(cpf[0], 11))
            return false;

        // Valida primeiro dígito verificador
        int soma = 0;
        for (int i = 0; i < 9; i++)
            soma += int.Parse(cpf[i].ToString()) * (10 - i);

        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;

        if (int.Parse(cpf[9].ToString()) != digito1)
            return false;

        // Valida segundo dígito verificador
        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += int.Parse(cpf[i].ToString()) * (11 - i);

        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;

        return int.Parse(cpf[10].ToString()) == digito2;
    }

    /// <summary>
    /// Valida o formato de um telefone brasileiro
    /// </summary>
    private static bool ValidarTelefoneBrasil(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            return false;

        var limpo = System.Text.RegularExpressions.Regex.Replace(telefone, @"\D", "");
        return limpo.Length >= 10 && limpo.Length <= 11;
    }

    /// <summary>
    /// Valida o formato de um CEP brasileiro
    /// </summary>
    private static bool ValidarCepBrasil(string cep)
    {
        if (string.IsNullOrWhiteSpace(cep))
            return false;

        var limpo = System.Text.RegularExpressions.Regex.Replace(cep, @"\D", "");
        return limpo.Length == 8;
    }

    /// <summary>
    /// Atualiza o score e limite de crédito do cliente
    /// Baseado no score, determina automaticamente:
    /// - 0 a 100: Sem cartão
    /// - 101 a 500: 1 cartão com limite R$ 1.000,00
    /// - 501 a 1000: Até 2 cartões com limite R$ 5.000,00 cada
    /// </summary>
    /// <param name="scoreCredito">Score de 0-1000</param>
    /// <param name="atualizadoPor">Usuário que atualizou</param>
    public void AtualizarCredito(int scoreCredito, string? atualizadoPor = null)
    {
        // Validação
        if (scoreCredito < 0 || scoreCredito > 1000)
            throw new ArgumentException("Score de crédito deve estar entre 0 e 1000");

        ScoreCredito = scoreCredito;
        DataUltimaAvaliacaoCredito = DateTime.UtcNow;

        // Aplicar regras de crédito baseado no score
        (NumeroMaximoCartoes, LimiteCreditoPorCartao) = CalcularLimites(scoreCredito);

        MarcarComoAtualizada(atualizadoPor);
    }

    /// <summary>
    /// Calcula o número máximo de cartões e limite por cartão baseado no score
    /// </summary>
    /// <returns>Tupla (NumeroMaximoCartoes, LimitePorCartao)</returns>
    private static (int maxCartoes, decimal limitePorCartao) CalcularLimites(int score)
    {
        return score switch
        {
            // 0 a 100: Sem cartão
            <= 100 => (0, 0),

            // 101 a 500: 1 cartão com limite R$ 1.000,00
            <= 500 => (1, 1000),

            // 501 a 1000: Até 2 cartões com limite R$ 5.000,00 cada
            _ => (2, 5000)
        };
    }
}
