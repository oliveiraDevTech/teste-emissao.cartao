# Documentação Técnica - Card Issuance API (Emissão de Cartões)

## 📋 Índice

1. [Visão Geral](#visão-geral)
2. [Arquitetura](#arquitetura)
3. [Stack Tecnológica](#stack-tecnológica)
4. [Entidades e Modelo de Domínio](#entidades-e-modelo-de-domínio)
5. [Regras de Negócio](#regras-de-negócio)
6. [APIs e Endpoints](#apis-e-endpoints)
7. [Fluxos de Processo](#fluxos-de-processo)
8. [Integração e Mensageria](#integração-e-mensageria)
9. [Segurança](#segurança)
10. [Persistência de Dados](#persistência-de-dados)
11. [Padrões e Práticas](#padrões-e-práticas)
12. [Configurações](#configurações)

---

## 📊 Visão Geral

### Propósito do Sistema
A **Card Issuance API** é um microsserviço especializado responsável por:
- **Emissão de cartões de crédito** (físicos e virtuais)
- **Ativação de cartões** através de validação segura
- **Publicação de eventos** para sistemas downstream via RabbitMQ
- **Garantia de idempotência** em operações críticas

### Contexto de Negócio
O serviço atua no fluxo de pós-aprovação de propostas de crédito:
1. Recebe requisição de emissão após aprovação de proposta
2. Gera PAN e CVV tokenizados (nunca em claro)
3. Persiste cartões com status apropriado
4. Publica eventos para downstream (notificação, fulfillment)
5. Permite ativação via OTP/CVV

### Características Principais
- ✅ **Clean Architecture** com separação de camadas
- ✅ **Domain-Driven Design** com entidades ricas
- ✅ **Outbox Pattern** para mensageria confiável
- ✅ **Idempotência** por chave em headers HTTP
- ✅ **PCI-DSS Compliant** (tokenização de PAN/CVV)
- ✅ **Event-Driven** com RabbitMQ
- ✅ **API-First** com OpenAPI/Swagger

---

## 🏗️ Arquitetura

### Diagrama de Camadas

```
┌───────────────────────────────────────────────────────────────┐
│                    Driving.Api Layer                          │
│  Controllers, Middleware, Filters, HTTP Request Handling      │
│  - CardsController: POST /api/v1/cards/issue, activate        │
│  - AuthController: POST /api/v1/auth/login                    │
└─────────────────────────────┬─────────────────────────────────┘
                              │
┌─────────────────────────────▼─────────────────────────────────┐
│                  Core.Application Layer                       │
│  Services, DTOs, Mappers, Validators, Use Cases               │
│  - CardIssuanceService: Orquestra emissão                     │
│  - CardActivationService: Gerencia ativação                   │
│  - AuthenticationService: JWT authentication                  │
└─────────────────────────────┬─────────────────────────────────┘
                              │
┌─────────────────────────────▼─────────────────────────────────┐
│                     Core.Domain Layer                         │
│  Entities, Value Objects, Business Rules                      │
│  - Card: Entidade principal de cartão                         │
│  - OutboxEvent: Padrão Outbox para eventos                    │
│  - CardIdempotencyKey: Suporte a idempotência                 │
└─────────────────────────────┬─────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
┌───────▼───────┐  ┌──────────▼─────────┐  ┌───────▼──────────┐
│ Driven.SqlLite│  │  Core.Infra        │  │ Driven.RabbitMQ  │
│ Repositories  │  │  PanGenerator      │  │ MessageBus       │
│ EF Core       │  │  TokenVault        │  │ Events           │
│ Migrations    │  │  OutboxDispatcher  │  │ Publishers       │
└───────────────┘  └────────────────────┘  └──────────────────┘
```

### Arquitetura Hexagonal (Ports & Adapters)

**Ports (Interfaces)**
- `ICardRepository`: Persistência de cartões
- `IOutboxRepository`: Persistência de eventos
- `ITokenVault`: Tokenização de dados sensíveis
- `IPanGenerator`: Geração de PAN
- `IMessagePublisher`: Publicação de mensagens

**Adapters**
- `CardRepository`: Implementação SQLite
- `OutboxRepository`: Implementação SQLite
- `TokenVault`: Mock de vault (produção: HSM)
- `PanGenerator`: Algoritmo Luhn
- `RabbitMQConnection`: Implementação RabbitMQ

---

## 🛠️ Stack Tecnológica

### Framework & Runtime
| Tecnologia | Versão | Propósito |
|-----------|--------|-----------|
| **.NET** | 8.0 | Runtime e Framework base |
| **ASP.NET Core** | 8.0 | Web API framework |
| **C#** | 12 | Linguagem de programação |

### Persistência
| Tecnologia | Versão | Propósito |
|-----------|--------|-----------|
| **Entity Framework Core** | 8.0 | ORM para acesso a dados |
| **SQLite** | 3.x | Banco de dados embarcado |
| **EF Core Migrations** | 8.0 | Versionamento de schema |

### Mensageria
| Tecnologia | Versão | Propósito |
|-----------|--------|-----------|
| **RabbitMQ** | 3.12+ | Message broker |
| **RabbitMQ.Client** | 6.x | Client library .NET |

### Segurança
| Tecnologia | Versão | Propósito |
|-----------|--------|-----------|
| **JWT Bearer** | - | Autenticação stateless |
| **BCrypt.Net** | - | Hashing de senhas |

### Observabilidade
| Tecnologia | Versão | Propósito |
|-----------|--------|-----------|
| **Serilog** | 3.x | Logging estruturado |
| **Serilog.Sinks.Console** | - | Output para console |
| **Serilog.Sinks.File** | - | Output para arquivos |

### Qualidade & Testes
| Tecnologia | Versão | Propósito |
|-----------|--------|-----------|
| **xUnit** | 2.5+ | Framework de testes |
| **Moq** | 4.x | Mocking library |
| **FluentAssertions** | 6.x | Assertions fluentes |
| **FluentValidation** | 11.x | Validação de DTOs |

### Documentação & Contratos
| Tecnologia | Versão | Propósito |
|-----------|--------|-----------|
| **Swashbuckle** | 6.5+ | Swagger/OpenAPI |
| **OpenAPI** | 3.0 | Especificação de API |

### Infraestrutura
| Tecnologia | Versão | Propósito |
|-----------|--------|-----------|
| **Docker** | 24+ | Containerização |
| **Docker Compose** | 2.x | Orquestração local |

---

## 📦 Entidades e Modelo de Domínio

### 1. Card (Cartão)

**Responsabilidade**: Representa um cartão de crédito emitido

```csharp
public class Card : BaseEntity
{
    // Identificadores
    public Guid ClienteId { get; private set; }
    public Guid PropostaId { get; private set; }
    public Guid ContaId { get; private set; }
    
    // Produto
    public string CodigoProduto { get; private set; }  // VISA_GOLD, MC_PLATINUM
    public string Tipo { get; private set; }           // VIRTUAL, PHYSICAL
    
    // Dados Sensíveis (Tokenizados)
    public string TokenPan { get; private set; }       // Token do PAN (16 dígitos)
    public string TokenCvv { get; private set; }       // Token do CVV (3-4 dígitos)
    
    // Validade
    public int MesValidade { get; private set; }       // 1-12
    public int AnoValidade { get; private set; }       // Ex: 2028
    
    // Crédito
    public decimal LimiteCreditoAprovado { get; private set; }
    
    // Status e Lifecycle
    public string Status { get; private set; }         // REQUESTED, ISSUED, ACTIVATION_PENDING, ACTIVE, BLOCKED
    public DateTime? DataAtivacao { get; private set; }
    public string? CanalAtivacao { get; private set; } // APP, OTP, FIRST_PURCHASE
    
    // Rastreabilidade
    public string CorrelacaoId { get; private set; }
}
```

**Estados do Cartão**
```
REQUESTED → ISSUED → ACTIVATION_PENDING → ACTIVE
                                       ↓
                                    BLOCKED
```

**Factory Methods**
- `Card.Criar()`: Cria novo cartão com validações
- `Card.Ativar()`: Transiciona para ACTIVE
- `Card.Bloquear()`: Transiciona para BLOCKED

### 2. OutboxEvent

**Responsabilidade**: Implementa Outbox Pattern para mensageria confiável

```csharp
public class OutboxEvent : BaseEntity
{
    public string Topico { get; private set; }         // Ex: card.issued, card.activated
    public string Payload { get; private set; }        // JSON serializado
    public DateTime? DataEnvio { get; private set; }   // null = não enviado
    public int TentativasEnvio { get; private set; }   // Contador de retries
    public string? ErroEnvio { get; private set; }     // Último erro
}
```

**Fluxo do Outbox**
1. Evento criado na mesma transação do cartão
2. `OutboxDispatcher` busca eventos pendentes
3. Publica no RabbitMQ
4. Marca como enviado com timestamp

### 3. CardIdempotencyKey

**Responsabilidade**: Garante idempotência de operações

```csharp
public class CardIdempotencyKey : BaseEntity
{
    public string ChaveIdempotencia { get; private set; }  // UUID único
    public string CartoesIds { get; private set; }         // JSON: ["guid1", "guid2"]
}
```

### 4. BaseEntity (Herança)

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime DataCriacao { get; protected set; }
    public DateTime? DataAtualizacao { get; protected set; }
    public bool Ativo { get; protected set; }
}
```

---

## ⚖️ Regras de Negócio

### Emissão de Cartões

#### RN-001: Quantidade de Cartões
- **Regra**: Cliente pode receber 1 ou 2 cartões por emissão
- **Critério**: Definido pela proposta aprovada
- **Implementação**: `CalcularQuantidadeCartoes()`
- **Validação**: Min=1, Max=2

#### RN-002: Tipos de Cartão
- **Regra**: Cartões podem ser VIRTUAL e/ou PHYSICAL
- **Critérios**:
  - Se `Entrega.Virtual=true` e `Entrega.Fisico=false`: 1 virtual
  - Se `Entrega.Virtual=false` e `Entrega.Fisico=true`: 1 físico
  - Se ambos true e quantidade=2: 1 virtual + 1 físico
  - Se ambos true e quantidade=1: preferência físico
- **Implementação**: `DeterminarTiposCartao()`

#### RN-003: Geração de PAN
- **Regra**: PAN deve ser único, válido e seguir padrões de bandeira
- **Algoritmo**: Luhn (checksum validation)
- **Formato**: 
  - Visa: `4xxx xxxx xxxx xxxx` (16 dígitos)
  - Mastercard: `5xxx xxxx xxxx xxxx` (16 dígitos)
- **BIN**: Primeiros 6 dígitos definem emissor
- **Implementação**: `PanGenerator.GerarPan()`

#### RN-004: Tokenização Obrigatória
- **Regra**: PAN e CVV NUNCA podem ser armazenados em claro
- **Compliance**: PCI-DSS Level 1
- **Processo**:
  1. Gera PAN/CVV
  2. Envia para TokenVault
  3. Recebe token
  4. Armazena apenas token
- **Implementação**: `TokenVault.TokenizarPan()`, `TokenVault.TokenizarCvv()`

#### RN-005: Validade do Cartão
- **Regra**: Cartão válido por período configurável
- **Padrão**: 5 anos a partir da emissão
- **Formato**: MM/AAAA
- **Cálculo**: `DataEmissao + CardIssuanceOptions.AnosValidade`

#### RN-006: Status Inicial
- **Regra**: Cartão emitido inicia com status apropriado
- **Virtual**: `ISSUED` (já disponível para uso)
- **Físico**: `ACTIVATION_PENDING` (requer ativação)

#### RN-007: Idempotência
- **Regra**: Mesma requisição não deve criar cartões duplicados
- **Chave**: Header `Idempotency-Key` (UUID)
- **Comportamento**:
  - Se chave existe: retorna cartões já criados
  - Se chave nova: processa normalmente
  - Se sem chave: processa sempre (não idempotente)
- **TTL**: Chave válida por 24 horas
- **Implementação**: `CardRepository.ExisteChaveIdempotenciaAsync()`

### Ativação de Cartões

#### RN-008: Elegibilidade para Ativação
- **Regra**: Apenas cartões com status `ACTIVATION_PENDING` podem ser ativados
- **Validação**: `if (card.Status != "ACTIVATION_PENDING") throw InvalidOperationException`

#### RN-009: Validação de OTP/CVV
- **Regra**: Ativação requer validação de segurança
- **Opções**:
  - **OTP**: Código temporário enviado via SMS/Email
  - **CVV**: Código de segurança do cartão
- **Tentativas**: Máximo 3 tentativas incorretas → bloqueia cartão

#### RN-010: Canal de Ativação
- **Regra**: Canal de ativação deve ser registrado
- **Opções**:
  - `APP`: Aplicativo mobile
  - `OTP`: One-Time Password
  - `FIRST_PURCHASE`: Primeira compra
  - `CALL_CENTER`: Central de atendimento

#### RN-011: Transição de Status
- **Regra**: Ativação bem-sucedida transiciona status
- **Fluxo**: `ACTIVATION_PENDING` → `ACTIVE`
- **Timestamp**: `DataAtivacao` = DateTime.UtcNow

### Eventos e Mensageria

#### RN-012: Publicação de Eventos
- **Regra**: Operações críticas devem gerar eventos
- **Eventos**:
  - `card.issued`: Cartão emitido com sucesso
  - `card.activated`: Cartão ativado
  - `card.blocked`: Cartão bloqueado
- **Payload**: JSON com dados relevantes
- **Garantia**: Transacional via Outbox Pattern

#### RN-013: Outbox Pattern
- **Regra**: Eventos devem ser persistidos na mesma transação
- **Processo**:
  1. Begin Transaction
  2. Salva Card
  3. Salva OutboxEvent
  4. Commit Transaction
  5. Background job publica eventos pendentes
- **Retry**: Até 5 tentativas com backoff exponencial

---

## 🌐 APIs e Endpoints

### Base URL
```
http://localhost:5001/api/v1
```

### Autenticação

#### POST /auth/login
Autentica usuário e retorna token JWT

**Request**
```json
{
  "email": "admin@sistema.com",
  "password": "Admin@123"
}
```

**Response 200**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "type": "Bearer",
  "expiresIn": 3600,
  "usuario": {
    "id": "guid",
    "nome": "Admin Sistema",
    "email": "admin@sistema.com"
  }
}
```

### Emissão de Cartões

#### POST /cards/issue
Emite cartões para um cliente

**Headers**
```
Authorization: Bearer {token}
Idempotency-Key: {uuid}  (opcional)
```

**Request**
```json
{
  "propostaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "clienteId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "contaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "quantidadeCartoesEmitir": 2,
  "limiteCreditoPorCartao": 5000.00,
  "codigoProduto": "VISA_GOLD",
  "entrega": {
    "virtual": true,
    "fisico": true
  },
  "correlacaoId": "req-123456"
}
```

**Response 202 Accepted**
```json
{
  "cartoes": [
    {
      "idCartao": "guid",
      "tokenPan": "tok_pan_abc123",
      "validade": "12/28",
      "tipo": "VIRTUAL",
      "status": "ISSUED"
    },
    {
      "idCartao": "guid",
      "tokenPan": "tok_pan_def456",
      "validade": "12/28",
      "tipo": "PHYSICAL",
      "status": "ACTIVATION_PENDING"
    }
  ],
  "correlacaoId": "req-123456",
  "dataEmissao": "2024-11-03T10:30:00Z"
}
```

**Response 400 Bad Request**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Erro de validação",
  "status": 400,
  "detail": "LimiteCreditoPorCartao deve ser maior que zero"
}
```

### Ativação de Cartões

#### POST /cards/{cardId}/activate
Ativa um cartão pendente

**Headers**
```
Authorization: Bearer {token}
```

**Request**
```json
{
  "otp": "123456",
  "cvv": "123",
  "canal": "APP"
}
```

**Response 200 OK**
```json
{
  "cardId": "guid",
  "status": "ACTIVE",
  "dataAtivacao": "2024-11-03T10:35:00Z",
  "canal": "APP",
  "mensagem": "Cartão ativado com sucesso"
}
```

**Response 404 Not Found**
```json
{
  "title": "Não encontrado",
  "status": 404,
  "detail": "Cartão não encontrado"
}
```

**Response 400 Bad Request**
```json
{
  "title": "Operação inválida",
  "status": 400,
  "detail": "Cartão não está elegível para ativação. Status atual: ACTIVE"
}
```

---

## 🔄 Fluxos de Processo

### Fluxo 1: Emissão de Cartões

```
[Cliente API] → [POST /cards/issue] → [CardsController]
                                            ↓
                                   [CardIssuanceService]
                                            ↓
                    ┌───────────────────────┴───────────────────────┐
                    ↓                                               ↓
            [ValidarRequisicao]                            [VerificarIdempotencia]
                    ↓                                               ↓
         [CalcularQuantidade]                                  [Existe?]
                    ↓                                               ↓
         [DeterminarTipos]                               Sim → [RetornarExistentes]
                    ↓                                               ↓
              Loop: Para cada cartão                                Não
                    ↓                                               ↓
           [PanGenerator.GerarPan()]                      [ContinuarFluxo]
                    ↓
           [TokenVault.TokenizarPan()]
                    ↓
           [TokenVault.TokenizarCvv()]
                    ↓
           [Card.Criar()]
                    ↓
           [CardRepository.AdicionarAsync()]
                    ↓
              Fim Loop
                    ↓
         [RegistrarIdempotencia]
                    ↓
         [PublicarEventoEmissao] → [OutboxRepository.AdicionarAsync()]
                    ↓
         [Commit Transaction]
                    ↓
         [RetornarCartoes] ← HTTP 202 Accepted
```

### Fluxo 2: Ativação de Cartões

```
[Mobile App] → [POST /cards/{id}/activate] → [CardsController]
                                                    ↓
                                         [CardActivationService]
                                                    ↓
                              [CardRepository.ObterPorIdAsync()]
                                                    ↓
                                      [ValidarStatus(ACTIVATION_PENDING)]
                                                    ↓
                                            [ValidarOTP/CVV]
                                                    ↓
                                            [Card.Ativar()]
                                                    ↓
                              [CardRepository.AtualizarAsync()]
                                                    ↓
                         [PublicarEventoAtivacao] → [OutboxRepository]
                                                    ↓
                                         [Commit Transaction]
                                                    ↓
                                   [RetornarConfirmacao] ← HTTP 200 OK
```

### Fluxo 3: Outbox Dispatcher (Background)

```
[Hosted Service] → [Timer: 30s]
                        ↓
            [OutboxRepository.ObterEventosPendentes()]
                        ↓
                  Loop: Para cada evento
                        ↓
              [MessagePublisher.PublishAsync()]
                        ↓
                [RabbitMQ Exchange] → [Fila: card.issued]
                        ↓
              [MarcarComoEnviado(evento)]
                        ↓
                   Fim Loop
```

---

## 📨 Integração e Mensageria

### RabbitMQ

**Configuração**
```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest",
    "AutomaticRecovery": true,
    "NetworkRecoveryInterval": 5,
    "RequestedHeartbeat": 60
  }
}
```

**Exchanges e Filas**

| Exchange | Tipo | Routing Key | Fila | Consumer |
|----------|------|-------------|------|----------|
| `card-events` | Topic | `card.issued` | `card-issued-queue` | Notificação |
| `card-events` | Topic | `card.activated` | `card-activated-queue` | Fulfillment |
| `card-events` | Topic | `card.blocked` | `card-blocked-queue` | Segurança |

**Evento: card.issued**
```json
{
  "eventId": "guid",
  "eventType": "card.issued",
  "timestamp": "2024-11-03T10:30:00Z",
  "correlationId": "req-123456",
  "data": {
    "cardId": "guid",
    "clienteId": "guid",
    "propostaId": "guid",
    "tipo": "VIRTUAL",
    "status": "ISSUED",
    "codigoProduto": "VISA_GOLD",
    "limiteCredito": 5000.00
  }
}
```

**Evento: card.activated**
```json
{
  "eventId": "guid",
  "eventType": "card.activated",
  "timestamp": "2024-11-03T10:35:00Z",
  "correlationId": "req-123456",
  "data": {
    "cardId": "guid",
    "clienteId": "guid",
    "dataAtivacao": "2024-11-03T10:35:00Z",
    "canal": "APP"
  }
}
```

---

## 🔒 Segurança

### Autenticação JWT

**Configuração**
```csharp
{
  "Jwt": {
    "Secret": "chave-secreta-minimo-32-chars",
    "Issuer": "CardIssuanceApi",
    "Audience": "CardIssuanceApp",
    "ExpirationMinutes": 60
  }
}
```

**Claims no Token**
- `sub`: User ID
- `email`: Email do usuário
- `name`: Nome completo
- `role`: Perfil (Admin, Operator)
- `iat`: Issued at
- `exp`: Expiration

### PCI-DSS Compliance

**Dados Sensíveis**
- ❌ **NUNCA** armazenar PAN em claro
- ❌ **NUNCA** armazenar CVV em claro
- ✅ **SEMPRE** usar tokenização
- ✅ **SEMPRE** criptografar em trânsito (HTTPS)
- ✅ **SEMPRE** logar sem dados sensíveis

**Tokenização**
```csharp
// PAN: 4111111111111111
// Token: tok_pan_abc123xyz789

// CVV: 123
// Token: tok_cvv_def456uvw321
```

**Logs Seguros**
```csharp
// ❌ ERRADO
_logger.LogInformation($"Cartão criado: {pan}");

// ✅ CORRETO
_logger.LogInformation($"Cartão criado: {tokenPan}");
```

---

## 💾 Persistência de Dados

### Schema do Banco de Dados

**Tabela: Cards**
```sql
CREATE TABLE Cards (
    Id TEXT PRIMARY KEY,
    ClienteId TEXT NOT NULL,
    PropostaId TEXT NOT NULL,
    ContaId TEXT NOT NULL,
    CodigoProduto TEXT NOT NULL,
    Tipo TEXT NOT NULL,
    TokenPan TEXT NOT NULL,
    TokenCvv TEXT NOT NULL,
    MesValidade INTEGER NOT NULL,
    AnoValidade INTEGER NOT NULL,
    LimiteCreditoAprovado REAL NOT NULL,
    Status TEXT NOT NULL,
    DataAtivacao TEXT,
    CanalAtivacao TEXT,
    CorrelacaoId TEXT NOT NULL,
    DataCriacao TEXT NOT NULL,
    DataAtualizacao TEXT,
    Ativo INTEGER NOT NULL,
    
    CONSTRAINT CK_Cards_Tipo CHECK (Tipo IN ('VIRTUAL', 'PHYSICAL')),
    CONSTRAINT CK_Cards_Status CHECK (Status IN ('REQUESTED', 'ISSUED', 'ACTIVATION_PENDING', 'ACTIVE', 'BLOCKED'))
);

CREATE INDEX IX_Cards_ClienteId ON Cards(ClienteId);
CREATE INDEX IX_Cards_PropostaId ON Cards(PropostaId);
CREATE INDEX IX_Cards_Status ON Cards(Status);
```

**Tabela: OutboxEvents**
```sql
CREATE TABLE OutboxEvents (
    Id TEXT PRIMARY KEY,
    Topico TEXT NOT NULL,
    Payload TEXT NOT NULL,
    DataEnvio TEXT,
    TentativasEnvio INTEGER DEFAULT 0,
    ErroEnvio TEXT,
    DataCriacao TEXT NOT NULL,
    DataAtualizacao TEXT,
    Ativo INTEGER NOT NULL
);

CREATE INDEX IX_OutboxEvents_Topico ON OutboxEvents(Topico);
CREATE INDEX IX_OutboxEvents_DataEnvio ON OutboxEvents(DataEnvio);
```

**Tabela: CardIdempotencyKeys**
```sql
CREATE TABLE CardIdempotencyKeys (
    Id TEXT PRIMARY KEY,
    ChaveIdempotencia TEXT UNIQUE NOT NULL,
    CartoesIds TEXT NOT NULL,
    DataCriacao TEXT NOT NULL,
    DataAtualizacao TEXT,
    Ativo INTEGER NOT NULL
);

CREATE UNIQUE INDEX IX_CardIdempotencyKeys_Chave 
    ON CardIdempotencyKeys(ChaveIdempotencia);
```

### Migrations

**Lista de Migrations**
1. `20250101000000_InitialCreate.cs`: Estrutura base
2. `20250101000002_AddCardsAndOutboxEvents.cs`: Cards + Outbox + Idempotency

**Aplicar Migrations**
```bash
dotnet ef database update --project Driven.SqlLite
```

---

## 📐 Padrões e Práticas

### Design Patterns Implementados

#### 1. Repository Pattern
```csharp
public interface ICardRepository
{
    Task<Card?> ObterPorIdAsync(Guid id);
    Task AdicionarAsync(Card card);
    Task AtualizarAsync(Card card);
    Task<bool> ExisteChaveIdempotenciaAsync(string chave);
}
```

#### 2. Outbox Pattern
- Garante mensageria confiável
- Eventos persistidos na mesma transação
- Background job processa outbox

#### 3. Factory Pattern
```csharp
Card.Criar(clienteId, propostaId, ...);  // Factory method
```

#### 4. Dependency Injection
- Todos os serviços registrados no DI Container
- Interfaces para inversão de dependência
- Scoped lifetime para repositórios

#### 5. DTO Pattern
- Separação entre domínio e API
- Validação com FluentValidation
- Mapeamento explícito

### Princípios SOLID

✅ **Single Responsibility**: Cada classe tem uma responsabilidade
✅ **Open/Closed**: Extensível via interfaces
✅ **Liskov Substitution**: Herança apropriada (BaseEntity)
✅ **Interface Segregation**: Interfaces específicas
✅ **Dependency Inversion**: Dependência de abstrações

---

## ⚙️ Configurações

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=card_issuance.db;"
  },
  "Jwt": {
    "Secret": "sua_chave_super_secreta_com_minimo_32_caracteres_para_producao",
    "Issuer": "CardIssuanceApi",
    "Audience": "CardIssuanceApp",
    "ExpirationMinutes": 60
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest"
  },
  "CardIssuance": {
    "AnosValidade": 5,
    "BinPadrao": "411111",
    "TamanhoOtp": 6,
    "TentativasMaximasOtp": 3
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Variáveis de Ambiente (Docker)

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5001
ConnectionStrings__DefaultConnection=Data Source=/app/data/cards.db;
JWT_SECRET=producao-secret-key-32-chars
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672
```

---

## 📊 Métricas e KPIs

### Performance
- **Latência P95**: < 200ms para emissão
- **Latência P95**: < 100ms para ativação
- **Throughput**: 1000 req/s

### Disponibilidade
- **Uptime**: > 99.9%
- **RTO**: < 15 minutos
- **RPO**: < 5 minutos

### Negócio
- **Taxa de Emissão**: Cartões emitidos/hora
- **Taxa de Ativação**: % cartões ativados em 24h
- **Taxa de Erro**: < 0.1%

---

## 📚 Referências

- [Clean Architecture - Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [PCI-DSS Requirements](https://www.pcisecuritystandards.org/)
- [RabbitMQ Best Practices](https://www.rabbitmq.com/tutorials/tutorial-one-dotnet.html)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Best Practices](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/best-practices)

---

**Última Atualização**: 03 de Novembro de 2024
**Versão**: 1.0.0
**Mantenedor**: Equipe de Desenvolvimento Backend
