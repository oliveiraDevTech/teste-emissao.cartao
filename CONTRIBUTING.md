# Guia de Contribuição

Obrigado por considerar contribuir para a **Card Issuance API**! Este documento fornece diretrizes e instruções para contribuidores.

## 📋 Índice

- [Código de Conduta](#código-de-conduta)
- [Como Começar](#como-começar)
- [Workflow de Desenvolvimento](#workflow-de-desenvolvimento)
- [Padrões de Código](#padrões-de-código)
- [Commits e Pull Requests](#commits-e-pull-requests)
- [Testes](#testes)
- [Documentação](#documentação)
- [Reportando Bugs](#reportando-bugs)
- [Sugestões de Melhorias](#sugestões-de-melhorias)

---

## 🤝 Código de Conduta

Este projeto adota um Código de Conduta inclusivo:

- **Respeito**: Trate todos com respeito e dignidade
- **Inclusão**: Bem-vindo a pessoas de todas as origens
- **Profissionalismo**: Mantenha comunicação profissional e construtiva
- **Integridade**: Seja honesto e transparente

Comportamentos inaceitáveis incluem:
- Assédio ou discriminação
- Ataques pessoais
- Linguagem ofensiva
- Intimidação

Denuncie violações para: suporte@seu-dominio.com

---

## 🚀 Como Começar

### 1. Fork o Repositório

```bash
# Via GitHub UI ou CLI
gh repo fork seu-usuario/card-issuance-api
```

### 2. Clone seu Fork Localmente

```bash
git clone https://github.com/SEU-USUARIO/card-issuance-api.git
cd card-issuance-api

# Adicione upstream remote
git remote add upstream https://github.com/original-usuario/card-issuance-api.git
```

### 3. Configure o Ambiente

```bash
# Restaurar dependências
dotnet restore

# Aplicar migrations
dotnet ef database update --project Driven.SqlLite --startup-project Driving.Api

# Executar testes para verificar setup
dotnet test
```

### 4. Crie uma Branch para sua Feature

```bash
git checkout -b feature/sua-feature-incrivel
```

---

## 🔄 Workflow de Desenvolvimento

### 1. Atualize sua Branch com upstream

```bash
git fetch upstream
git rebase upstream/main
```

### 2. Faça suas Alterações

```bash
# Edite arquivos, adicione código, etc

# Verifique o que foi alterado
git status
git diff
```

### 3. Commits Atômicos

Cada commit deve ser pequeno e focado:

```bash
# ❌ Evite commits grandes
git commit -am "Vários fixes e features"

# ✅ Prefira commits específicos
git commit -am "feat: adicionar suporte a cartões crédito"
git commit -am "fix: corrigir validação de CVV"
git commit -am "test: adicionar testes para novos endpoints"
```

### 4. Sync com upstream antes de Push

```bash
git fetch upstream
git rebase upstream/main

# Se houver conflitos, resolva-os e continue
git rebase --continue
```

### 5. Push para seu Fork

```bash
git push origin feature/sua-feature-incrivel
```

### 6. Crie um Pull Request

- Vá para GitHub
- Clique em "Compare & pull request"
- Preencha o template de PR com descrição clara
- Aguarde review

---

## 📝 Padrões de Código

### Convenções de Nomenclatura

```csharp
// Classes: PascalCase
public class CardIssuanceService { }
public interface ICardRepository { }

// Métodos: PascalCase
public async Task EmitirCartõesAsync() { }

// Propriedades: PascalCase
public string CorrelacaoId { get; set; }

// Variáveis locais: camelCase
var clienteId = Guid.NewGuid();
int tentativas = 0;

// Constantes: UPPER_SNAKE_CASE
const int MAX_RETRIES = 3;
const string DEFAULT_TIMEOUT = "30";

// Private fields: _camelCase
private readonly ICardRepository _repository;
private string _token;
```

### Estrutura de Classes

```csharp
public class CardIssuanceService
{
    // 1. Campos privados
    private readonly ICardRepository _cardRepository;
    private readonly ILogger<CardIssuanceService> _logger;

    // 2. Construtor
    public CardIssuanceService(
        ICardRepository cardRepository,
        ILogger<CardIssuanceService> logger)
    {
        _cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // 3. Propriedades públicas
    public string ServiceName => "Card Issuance";

    // 4. Métodos públicos
    public async Task<List<Card>> EmitirCartõesAsync(CardIssuanceRequestDTO request, CancellationToken ct)
    {
        // Implementação
    }

    // 5. Métodos privados/auxiliares
    private void ValidarRequisicao(CardIssuanceRequestDTO request)
    {
        // Implementação
    }
}
```

### Async/Await

```csharp
// ✅ BOM: Sempre use async para I/O
public async Task<Card> ObterCartãoAsync(Guid id)
{
    return await _repository.GetByIdAsync(id);
}

// ❌ EVITAR: Sync over async
var card = ObterCartãoAsync(id).Result; // Pode causar deadlock!

// ✅ BOM: Use ConfigureAwait(false) em libraries
await SomeAsyncMethod().ConfigureAwait(false);
```

### Null Safety

```csharp
// ✅ BOM: Use null coalescing
var valor = input ?? defaultValue;

// ✅ BOM: Use null conditional
var nome = cliente?.Nome;

// ✅ BOM: Use null coalescing assignment
cliente ??= new Cliente();

// ✅ BOM: Validação em construtor
_repository = repository ?? throw new ArgumentNullException(nameof(repository));
```

### LINQ

```csharp
// ✅ BOM: Method syntax (geralmente mais legível)
var cartoes = await _repository
    .GetAll()
    .Where(c => c.Status == CardStatus.Active)
    .OrderBy(c => c.DataCriacao)
    .ToListAsync();

// ✅ OK: Query syntax para queries complexas
var resultado = from card in _context.Cards
                where card.ClienteId == clienteId && card.Ativo
                select new CardDto { Id = card.Id, Status = card.Status };
```

### Exception Handling

```csharp
// ✅ BOM: Específico e informativo
try
{
    await ProcessarCartãoAsync(card);
}
catch (CartãoNãoEncontradoException ex)
{
    _logger.LogWarning(ex, "Cartão {CartaoId} não encontrado", card.Id);
    throw new ApiException("Cartão não existe", 404);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Erro inesperado ao processar cartão");
    throw;
}

// ❌ EVITAR: Capturar Exception genérica
try { /* code */ }
catch (Exception) { /* swallow */ }
```

### Comments e Documentação

```csharp
// ✅ BOM: XML Comments para métodos públicos
/// <summary>
/// Emite um novo cartão para o cliente
/// </summary>
/// <param name="request">Dados da requisição de emissão</param>
/// <param name="cancellationToken">Token de cancelamento</param>
/// <returns>Cartão emitido</returns>
/// <exception cref="ArgumentNullException">Se request for null</exception>
public async Task<Card> EmitirCartãoAsync(
    CardIssuanceRequestDTO request,
    CancellationToken cancellationToken = default)

// ✅ BOM: Comentários para lógica complexa
// Usa distribuição normal (Gaussian) para gerar scores
// com média 650 e desvio padrão 100
var score = GerarScoreComDistribuicaoNormal();

// ❌ EVITAR: Comentários óbvios
i++; // Incrementar i
```

---

## 💬 Commits e Pull Requests

### Formato de Commit

Siga o [Conventional Commits](https://www.conventionalcommits.org/):

```
<tipo>[escopo opcional]: <descrição breve>

[corpo opcional]

[rodapé opcional]
```

### Tipos de Commit

```
feat:     Nova funcionalidade
fix:      Correção de bug
docs:     Mudanças em documentação
style:    Formatação, sem mudança de lógica
refactor: Refatoração de código
perf:     Melhorias de performance
test:     Adicionar/modificar testes
chore:    Dependências, build, CI
ci:       Configuração CI/CD
```

### Exemplos de Commits

```bash
# Nova funcionalidade
git commit -m "feat: adicionar suporte a cartões multi-moeda"

# Com escopo
git commit -m "feat(cards): adicionar filtro por status"

# Com descrição detalhada
git commit -m "fix(idempotency): garantir chave única

- Adiciona constraint UNIQUE em CardIdempotencyKeys
- Evita duplicatas em retries
- Melhora rastreamento de requisições

Fixes #123"

# Commit que fecha issue
git commit -m "fix: resolver bug de ativação

Closes #456"
```

### Template de Pull Request

```markdown
## Descrição
Breve descrição do que foi implementado/corrigido

## Tipo de Mudança
- [ ] Bug fix
- [ ] Nova feature
- [ ] Breaking change
- [ ] Documentação

## Como Testar
Passo a passo para reproduzir/testar

1. ...
2. ...

## Checklist
- [ ] Código segue padrões do projeto
- [ ] Testes foram adicionados/atualizados
- [ ] Cobertura de testes > 80%
- [ ] Documentação foi atualizada
- [ ] Sem breaking changes (ou documentado)
- [ ] Build passa localmente
- [ ] Sem conflitos com main

## Screenshots (se aplicável)
Adicione screenshots para mudanças UI

## Issues Relacionadas
Closes #123
Related to #456
```

---

## 🧪 Testes

### Requisitos de Teste

- ✅ Todos os novos features devem ter testes
- ✅ Nenhum bug fix sem teste
- ✅ Mínimo 80% de cobertura
- ✅ Testes devem passar localmente

### Executar Testes

```bash
# Todos os testes
dotnet test

# Projeto específico
dotnet test Test.XUnit

# Com cobertura
dotnet test /p:CollectCoverage=true

# Testes específicos
dotnet test --filter "ClassName"
```

### Exemplo de Teste

```csharp
[Fact]
public async Task EmitirCartoes_ComDadosValidos_DeveRetornarSucesso()
{
    // Arrange
    var request = new CardIssuanceRequestDTO
    {
        ClienteId = Guid.NewGuid(),
        PropostaId = Guid.NewGuid(),
        ContaId = Guid.NewGuid(),
        CodigoProduto = "GOLD"
    };

    var repositoryMock = new Mock<ICardRepository>();
    repositoryMock
        .Setup(x => x.AdicionarAsync(It.IsAny<Card>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    var service = new CardIssuanceService(repositoryMock.Object, LoggerMock);

    // Act
    var resultado = await service.EmitirCartõesAsync(request, CancellationToken.None);

    // Assert
    resultado.Should().NotBeEmpty();
    repositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<Card>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

### Naming Convention para Testes

```csharp
// Padrão: [MethodName]_[Scenario]_[ExpectedResult]

[Fact]
public void Método_Cenário_ResultadoEsperado()

// Exemplos
[Fact]
public void EmitirCartoes_ComDadosValidos_RetornaCartões()

[Fact]
public void ValidarCVV_ComCVVInvalido_LançaException()

[Fact]
public void ObterCartão_CartãoNãoExiste_RetornaNull()
```

---

## 📚 Documentação

### Atualizar README

Se sua feature afeta usuários finais, atualize o README:

```markdown
## Sua Nova Feature

Descrição breve da feature

### Exemplo de Uso

```csharp
var resultado = await service.SuaNovaFeatureAsync();
```
```

### Atualizar CHANGELOG

```markdown
## [Unreleased]

### Added
- Nova feature X

### Fixed
- Bug Y

### Changed
- Comportamento Z alterado
```

### Code Comments

Use XML comments em APIs públicas:

```csharp
/// <summary>
/// Descrição breve
/// </summary>
/// <param name="parametro">Descrição do parâmetro</param>
/// <returns>Descrição do retorno</returns>
/// <exception cref="ExceptionType">Quando lançada</exception>
public async Task<T> MinhaFuncaoAsync(string parametro)
```

---

## 🐛 Reportando Bugs

### Antes de Reportar

1. Verifique se o bug já foi reportado
2. Atualize para a versão mais recente
3. Verifique a documentação
4. Tente reproduzir o problema

### Template de Bug Report

```markdown
## Descrição
Descrição clara e concisa do bug

## Passos para Reproduzir
1. Passo 1
2. Passo 2
3. Passo 3

## Comportamento Esperado
O que deveria acontecer

## Comportamento Observado
O que está acontecendo

## Informações do Ambiente
- OS: Windows 10 / macOS / Linux
- .NET Version: 8.0
- Versão da App: 1.0.0
- RabbitMQ: Sim/Não

## Logs Relevantes
```
[Copie logs relevantes aqui]
```

## Screenshots
[Se aplicável]
```

---

## 💡 Sugestões de Melhorias

### Antes de Sugerir

1. Verifique se já foi sugerido
2. Considere escopo e compatibilidade
3. Pense em casos de uso reais

### Template de Feature Request

```markdown
## Descrição
Descrição clara da feature sugerida

## Motivação
Por que essa feature seria útil?

## Caso de Uso
Exemplo real de como seria usada

## Possível Implementação
(Opcional) Sua ideia de como implementar

## Alternativas Consideradas
Outras abordagens possíveis
```

---

## 🔍 Processo de Review

### O que Esperar

1. **Code Review**: Um ou mais maintainers revisarão
2. **CI Checks**: Testes e linting devem passar
3. **Feedback**: Podem ser solicitadas mudanças
4. **Aprovação**: Quando OK, PR será merged

### Dicas para Passar no Review

- ✅ Commits pequenos e focados
- ✅ Descrição clara do PR
- ✅ Testes incluídos
- ✅ Sem mudanças não relacionadas
- ✅ Seguir padrões do projeto
- ✅ Documentação atualizada

### Respondendo a Feedback

```bash
# Faça as mudanças solicitadas
# Commite as alterações
git commit -am "refactor: endereçar feedback do review"

# Não faça force push (a menos que solicitado)
git push origin feature/sua-feature
```

---

## 📖 Recursos Úteis

- [Git Flow Cheatsheet](https://danielkummer.github.io/git-flow-cheatsheet/)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style)
- [Clean Code in C#](https://www.pluralsight.com/courses/clean-code-in-csharp)

---

## 🙏 Obrigado!

Sua contribuição faz diferença na comunidade. Obrigado por ajudar a melhorar a Card Issuance API!

Se tiver dúvidas, abra uma discussion ou entre em contato.

---

**Last Updated**: 03 de Novembro de 2024
