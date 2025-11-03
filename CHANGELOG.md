# Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/),
e este projeto adere a [Semantic Versioning](https://semver.org/lang/pt-BR/).

## [Unreleased]

### Planejado
- Suporte a cartões multi-moeda
- Integração com serviço de crédito real
- Dashboard administrativo
- Webhooks para eventos de cartão
- Rate limiting por cliente
- Autenticação mTLS

---

## [1.0.0] - 2024-11-03

### Added

#### Funcionalidades Principais
- ✅ **Emissão de Cartões**: Endpoint completo para emissão de cartões de crédito
  - Validação robusta de dados de entrada
  - Geração segura de tokens PAN e CVV
  - Suporte a múltiplos tipos de cartão (CREDIT, DEBIT)
  - Armazenamento seguro no banco de dados

- ✅ **Ativação de Cartões**: Endpoint para ativar cartões após aprovação
  - Validação de OTP (One-Time Password)
  - Validação de CVV
  - Rastreamento do canal de ativação
  - Estados de cartão (REQUESTED, ACTIVE, BLOCKED)

- ✅ **Padrão Outbox**: Implementação do Outbox Pattern para mensageria confiável
  - Transações atômicas (ACID)
  - Garantia de entrega de eventos
  - `OutboxDispatcher` para publicação automática
  - Suporte a retry com backoff exponencial

- ✅ **Idempotência**: Suporte completo a idempotência de requisições
  - Header `Idempotency-Key` para deduplicação
  - Armazenamento de chaves e respostas
  - Prevenção de duplicatas em retries

- ✅ **Integração RabbitMQ**: Publicação de eventos assíncronos
  - Conexão com fallback automático
  - Retry logic com circuit breaker
  - Configuração via environment variables
  - Logs detalhados de conexão

- ✅ **Autenticação JWT**: Endpoints protegidos com tokens JWT
  - Login com usuário/senha
  - Geração de tokens com expiração configurável
  - Validação de assinatura e expiração
  - Issuer e Audience customizáveis

- ✅ **Swagger/OpenAPI**: Documentação automática da API
  - UI interativa em `/swagger`
  - Schemas JSON completos
  - Exemplos de requisição/resposta
  - Autenticação JWT integrada

#### Infraestrutura & Arquitetura
- ✅ **Clean Architecture**: Separação clara de responsabilidades
  - Domain Layer com lógica de negócio
  - Application Layer com serviços
  - Presentation Layer com controllers
  - Infrastructure Layer com repositórios

- ✅ **Entity Framework Core 8**: ORM moderno e performático
  - Migrations automáticas
  - LINQ type-safe queries
  - Lazy loading e eager loading
  - Change tracking automático

- ✅ **Dependency Injection**: Injeção de dependências nativa
  - Registro de serviços em Program.cs
  - Lifetime management (Scoped, Singleton, Transient)
  - Factory patterns para serviços complexos

- ✅ **Logging Estruturado**: Serilog com múltiplos sinks
  - Console output formatado
  - File rolling por dia
  - Structured logging com campos customizados
  - Integration com Seq (opcional)

- ✅ **Validação Fluente**: FluentValidation para regras declarativas
  - Validações customizadas
  - Mensagens de erro localizáveis
  - Validação de cascata

- ✅ **Mapeamento de Objetos**: AutoMapper para DTO mapping
  - Profiles de mapeamento customizados
  - Projeções eficientes
  - Flatten/Unflatten automático

#### Testing
- ✅ **xUnit**: Framework moderno de testes
  - Testes unitários completos
  - Suporte a [Theory] para múltiplos cenários
  - Fixtures reutilizáveis

- ✅ **Moq**: Mocking de dependências
  - Mock de repositórios
  - Verificação de chamadas (Verify)
  - Setup condicional

- ✅ **FluentAssertions**: Assertions legíveis
  - Should() pattern
  - Mensagens customizadas
  - Comparação de objetos complexos

#### Documentação
- ✅ **README Completo**: Documentação detalhada
  - Instruções de setup
  - Exemplos de API
  - Troubleshooting
  - Recursos adicionais

- ✅ **CONTRIBUTING.md**: Guia para contribuidores
  - Workflow de desenvolvimento
  - Padrões de código
  - Processo de review

- ✅ **Swagger Docs**: Documentação inline da API
  - Descrições de endpoints
  - Exemplos de payload
  - Status codes documentados

#### Segurança
- ✅ **JWT com HS256**: Tokens assinados e verificados
- ✅ **Validação de Entrada**: FluentValidation em todos endpoints
- ✅ **CORS Configurável**: Controle de origem de requisições
- ✅ **HTTPS Recomendado**: Redirecionamento automático em produção

### Changed
- Removido código legado de "Cadastro de Clientes"
- Refatorado database initialization (EnsureCreated → Migrate)
- Swagger repositionado de `/` para `/swagger`
- Nomenclatura interna atualizada para "Card Issuance"

### Fixed
- ❌ Banco de dados não era criado automaticamente → ✅ Migrations aplicadas em startup
- ❌ Swagger retornava 404 → ✅ RoutePrefix configurado corretamente
- ❌ OutboxEvents table missing → ✅ Nova migration adicionada

### Removed
- ❌ ClientesController (fora do escopo)
- ❌ ClienteService e interfaces relacionadas
- ❌ DTOs de cliente (ClienteCreateDto, etc)
- ❌ Testes relacionados a clientes

---

## [0.9.0] - 2024-10-20 (Pre-release)

### Added (Experimental)
- Estrutura inicial de projeto
- Controllers básicos
- DbContext configuration
- RabbitMQ connection
- Logging setup

### Known Issues
- Database initialization issues
- OutboxEvents table missing
- Swagger documentation incomplete
- Customer management code not removed

---

## Notas de Versão

### Semver Versioning

Este projeto segue Semantic Versioning: **MAJOR.MINOR.PATCH**

- **MAJOR**: Breaking changes (ex: 2.0.0)
- **MINOR**: Novas features compatíveis (ex: 1.1.0)
- **PATCH**: Bug fixes (ex: 1.0.1)

### Compatibilidade com Versões

- **1.0.0**: Versão estável inicial
- Compatível com .NET 8.0+
- RabbitMQ 3.12+ (opcional)
- SQLite 3.40+

### Migration Guide

#### De 0.9.0 para 1.0.0

1. **Backup do Banco**: Se possui dados, faça backup
   ```bash
   cp card_issuance.db card_issuance.db.backup
   ```

2. **Deletar Banco Antigo** (se necessário):
   ```bash
   rm card_issuance.db card_issuance.db-*
   ```

3. **Aplicar Migrations**:
   ```bash
   dotnet ef database update --project Driven.SqlLite
   ```

4. **Atualizar Configurações**:
   - Jwt issuer: `CadastroClientesApi` → `CardIssuanceApi`
   - Jwt audience: `CadastroClientesApp` → `CardIssuanceApp`

---

## Roadmap

### Q4 2024
- ✅ v1.0.0 - Emissão e ativação de cartões

### Q1 2025
- 🔄 Multi-currency support
- 🔄 Real credit service integration
- 🔄 Admin dashboard

### Q2 2025
- 🔄 Webhooks
- 🔄 Rate limiting
- 🔄 Advanced analytics

---

## Contribuindo

Para sugestões de melhorias ou bugs, veja [CONTRIBUTING.md](CONTRIBUTING.md).

---

## Autores

- **Projeto**: Card Issuance API
- **Arquitetura**: Clean Architecture + DDD
- **Versão**: 1.0.0
- **Última Atualização**: 03 de Novembro de 2024

---

## Links Úteis

- [GitHub Repository](https://github.com/usuario/card-issuance-api)
- [README](README.md)
- [Contributing Guidelines](CONTRIBUTING.md)
- [Issues](https://github.com/usuario/card-issuance-api/issues)
- [Discussions](https://github.com/usuario/card-issuance-api/discussions)

---

## Formato Changelog

Este changelog usa o formato:
- **Added**: Novas funcionalidades
- **Changed**: Alterações em funcionalidades existentes
- **Deprecated**: Funcionalidades que serão removidas
- **Removed**: Funcionalidades removidas
- **Fixed**: Bugs corrigidos
- **Security**: Correções de segurança

Veja [Keep a Changelog](https://keepachangelog.com/) para mais detalhes.
