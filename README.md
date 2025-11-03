# Card Issuance API

Uma API robusta e escalável para **emissão e ativação de cartões de crédito**, desenvolvida com arquitetura Clean Architecture e padrões de design enterprise.

## 📋 Sumário

- [Visão Geral](#visão-geral)
- [Arquitetura](#arquitetura)
- [Tecnologias](#tecnologias)
- [Pré-requisitos](#pré-requisitos)
- [Instalação e Setup](#instalação-e-setup)
- [Executando a Aplicação](#executando-a-aplicação)
- [Endpoints da API](#endpoints-da-api)
- [Autenticação](#autenticação)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Padrões de Design Implementados](#padrões-de-design-implementados)
- [Fluxo de Emissão de Cartões](#fluxo-de-emissão-de-cartões)
- [Configuração e Variáveis de Ambiente](#configuração-e-variáveis-de-ambiente)
- [Banco de Dados](#banco-de-dados)
- [Logging](#logging)
- [Tratamento de Erros](#tratamento-de-erros)
- [Testes](#testes)
- [Contribuindo](#contribuindo)
- [Troubleshooting](#troubleshooting)

---

## 🎯 Visão Geral

A **Card Issuance API** é um serviço especializado em:

✅ **Emissão de Cartões**: Processa solicitações de emissão de cartões com validações robustas
✅ **Ativação de Cartões**: Ativa cartões através de validação OTP/CVV
✅ **Idempotência**: Suporta chave de idempotência para garantir operações seguras
✅ **Padrão Outbox**: Implementa o padrão Outbox para mensageria confiável
✅ **Integração RabbitMQ**: Publica eventos para sistemas downstream
✅ **Autenticação JWT**: Endpoints protegidos com tokens JWT

### Contexto de Negócio

A API foi desenvolvida seguindo o padrão de **Domain-Driven Design** com foco em:
- **Emissão de Cartões de Crédito**: Recebe requisições pós-aprovação de propostas
- **Ativação de Cartões**: Permite ativar cartões após validações de segurança
- **Publicação de Eventos**: Publica eventos de emissão/ativação para sistemas downstream
- **Audit Trail**: Mantém histórico completo de operações no banco de dados

---

## 🏗️ Arquitetura

A aplicação segue **Clean Architecture** com separação clara de responsabilidades:

```
┌─────────────────────────────────────────────────────────┐
│                  Driving.Api (API Layer)                │
│              Controllers & HTTP Handlers                │
└─────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────┐
│          Core.Application (Application Layer)           │
│     Services, DTOs, Mappers, Validators, Interfaces     │
└─────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────┐
│            Core.Domain (Domain Layer)                   │
│         Entities, Value Objects, Business Logic         │
└─────────────────────────────────────────────────────────┘
                           ↓
┌──────────────────────────────────────────────────────────┐
│          Driven.* (Infrastructure Layer)                │
│  SQLite DB, RabbitMQ, Credit Mock, Cache, Logging       │
└──────────────────────────────────────────────────────────┘
```

### Camadas da Arquitetura

| Camada | Responsabilidade | Exemplos |
|--------|-----------------|----------|
| **Driving.Api** | Controllers, HTTP handling, Swagger | `CardsController`, `AuthController` |
| **Core.Application** | Services, DTOs, Mappers, Validators | `CardIssuanceService`, `CardActivationService` |
| **Core.Domain** | Entidades, lógica de negócio | `Card`, `OutboxEvent`, `Cliente` |
| **Driven.SqlLite** | Persistência de dados | `CardRepository`, `OutboxRepository` |
| **Core.Infra** | Logging, Cache, Serviços genéricos | `OutboxDispatcher` |
| **Driven.RabbitMQ** | Mensageria e eventos | `RabbitMQConnection`, `MessageBus` |
| **Driven.CreditMock** | Mock do serviço de crédito | `CreditMockService` |

---

## 🛠️ Tecnologias

### Framework & Platform
- **.NET 8.0**: Framework moderno e performance optimizada
- **C# 12**: Linguagem com features modernas (nullable reference types, records, etc)
- **ASP.NET Core 8**: Web framework para APIs REST

### Banco de Dados & ORM
- **SQLite**: Banco de dados embarcado, ideal para desenvolvimento
- **Entity Framework Core 8**: ORM com migrations automáticas
- **LINQ**: Queries type-safe

### Autenticação & Segurança
- **JWT (JSON Web Tokens)**: Autenticação stateless
- **SymmetricSecurityKey**: Criptografia de tokens
- **Bearer Authentication**: Padrão de autenticação HTTP

### Logging & Observabilidade
- **Serilog**: Logging estruturado e configurável
- **Seq**: Agregação de logs (opcional)
- **Console/File Sinks**: Destinos de logging

### Mensageria
- **RabbitMQ**: Message broker para eventos assíncronos
- **Outbox Pattern**: Transação 2-phase distribuída

### Validação & Mapeamento
- **FluentValidation**: Validação fluent e declarativa
- **AutoMapper**: Mapeamento de objetos DTO ↔ Domain

### Testing
- **xUnit**: Framework de testes
- **Moq**: Mock objects para testes
- **FluentAssertions**: Assertions fluentes e legíveis

### Outras Libraries
- **Newtonsoft.Json**: Serialização JSON
- **System.Security.Cryptography**: Operações criptográficas

---

## 📋 Pré-requisitos

Antes de começar, certifique-se de ter instalado:

### Obrigatório
- **.NET SDK 8.0+**: [Download](https://dotnet.microsoft.com/download)
- **Git**: Para clonar o repositório

### Opcional (para funcionalidade completa)
- **RabbitMQ 3.12+**: Para mensageria
  - Windows: [RabbitMQ Windows Installer](https://www.rabbitmq.com/install-windows.html)
  - Docker: `docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.12-management`

- **Seq**: Para agregação de logs (opcional)
  - Docker: `docker run -d --name seq -e ACCEPT_EULA=Y -p 5341:80 datalust/seq`

- **Visual Studio 2022**: IDE recomendada
- **Visual Studio Code**: Alternativa leve

### Verificar Instalação
```bash
dotnet --version
```

---

## 🚀 Instalação e Setup

### 1. Clonar o Repositório
```bash
git clone https://github.com/seu-usuario/card-issuance-api.git
cd card-issuance-api
```

### 2. Restaurar Dependências
```bash
dotnet restore
```

### 3. Aplicar Migrations do Banco de Dados
```bash
dotnet ef database update --project Driven.SqlLite --startup-project Driving.Api
```

Ou deixe a aplicação aplicar automaticamente na inicialização (já configurado).

### 4. Configurar Variáveis de Ambiente (Opcional)
Crie um arquivo `.env` ou `appsettings.Development.json`:

```json
{
  "Jwt": {
    "Secret": "sua-chave-secreta-com-minimo-32-caracteres"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=card_issuance.db;"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  },
  "Serilog": {
    "MinimumLevel": "Debug"
  }
}
```

---

## ▶️ Executando a Aplicação

### Desenvolvimento Local
```bash
dotnet run --project Driving.Api
```

Ou use Visual Studio:
1. Abra a solução `Emissao.Cartao.sln`
2. Defina `Driving.Api` como projeto de inicialização
3. Pressione `F5` para executar

### Acessar Swagger UI
```
https://localhost:7215/swagger
```

### Acessar API
```
https://localhost:7215/api/v1/cards/issue
http://localhost:5202/api/v1/cards/issue (HTTP)
```

### Executar Testes
```bash
dotnet test
```

### Build para Produção
```bash
dotnet publish -c Release -o ./publish
```

---

## 🔌 Endpoints da API

### Authentication

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "usuario": "user",
  "senha": "password"
}
```

**Response** (200 OK):
```json
{
  "sucesso": true,
  "mensagem": "Login realizado com sucesso",
  "dados": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "tipo": "Bearer",
    "expiracaoEm": "2024-11-04T10:30:00Z"
  }
}
```

#### Validar Token
```http
POST /api/auth/validar-token
Content-Type: application/json

"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Response** (200 OK):
```json
{
  "sucesso": true,
  "mensagem": "Token é válido"
}
```

---

### Card Issuance (Emissão de Cartões)

#### Emitir Cartões
```http
POST /api/v1/cards/issue
Authorization: Bearer {token}
Content-Type: application/json
Idempotency-Key: unique-request-id-123

{
  "clienteId": "550e8400-e29b-41d4-a716-446655440000",
  "propostaId": "660e8400-e29b-41d4-a716-446655440000",
  "contaId": "770e8400-e29b-41d4-a716-446655440000",
  "codigoProduto": "GOLD",
  "correlacaoId": "corr-123456"
}
```

**Parameters**:
| Campo | Tipo | Descrição |
|-------|------|-----------|
| `clienteId` | UUID | ID do cliente (obrigatório) |
| `propostaId` | UUID | ID da proposta de crédito (obrigatório) |
| `contaId` | UUID | ID da conta bancária (obrigatório) |
| `codigoProduto` | string | Código do produto (ex: GOLD, PLATINUM) (obrigatório) |
| `correlacaoId` | string | ID único para rastreamento (obrigatório) |

**Response** (202 Accepted):
```json
{
  "cartoes": [
    {
      "idCartao": "550e8400-e29b-41d4-a716-446655440000",
      "tokenPan": "****1234",
      "validade": "12/26",
      "tipo": "CREDIT",
      "status": "REQUESTED"
    }
  ],
  "correlacaoId": "corr-123456",
  "dataEmissao": "2024-11-03T10:15:00Z"
}
```

**Status Codes**:
- `202 Accepted`: Emissão aceita e em processamento
- `400 Bad Request`: Validação falhou (ex: IDs inválidos)
- `401 Unauthorized`: Token inválido/expirado
- `500 Internal Server Error`: Erro no servidor

**Headers Importantes**:
- `Idempotency-Key`: Chave única para idempotência (evita duplicatas em retry)

---

#### Ativar Cartão
```http
POST /api/v1/cards/{cardId}/activate
Authorization: Bearer {token}
Content-Type: application/json

{
  "otp": "123456",
  "cvv": "123",
  "canal": "MOBILE_APP"
}
```

**Parameters**:
| Campo | Tipo | Descrição |
|-------|------|-----------|
| `cardId` | UUID (URL) | ID do cartão a ativar |
| `otp` | string | One-Time Password (obrigatório) |
| `cvv` | string | Card Verification Value (obrigatório) |
| `canal` | string | Canal de ativação (MOBILE_APP, ATM, etc) |

**Response** (200 OK):
```json
{
  "sucesso": true,
  "mensagem": "Cartão ativado com sucesso",
  "dados": {
    "cartaoId": "550e8400-e29b-41d4-a716-446655440000",
    "status": "ACTIVE",
    "dataAtivacao": "2024-11-03T10:20:00Z",
    "canalAtivacao": "MOBILE_APP"
  }
}
```

**Status Codes**:
- `200 OK`: Ativação realizada com sucesso
- `400 Bad Request`: Validação falhou (OTP/CVV inválidos)
- `404 Not Found`: Cartão não encontrado
- `500 Internal Server Error`: Erro no servidor

---

## 🔐 Autenticação

### Fluxo de Autenticação

```
┌─────────────────────────────────────────┐
│   1. Fazer Login                        │
│   POST /api/auth/login                  │
└─────────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────┐
│   2. Receber JWT Token                  │
│   { "token": "eyJ...", ... }            │
└─────────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────┐
│   3. Usar Token em Requisições          │
│   Authorization: Bearer eyJ...          │
└─────────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────┐
│   4. Token Validado                     │
│   Requisição Processada                 │
└─────────────────────────────────────────┘
```

### Credentials de Desenvolvimento

**Usuário Padrão:**
- Login: `user`
- Senha: `password`

> ⚠️ **IMPORTANTE**: Altere essas credenciais em produção!

### Configuração JWT

O JWT é configurado em `Program.cs`:

```csharp
// Issuer: CardIssuanceApi
// Audience: CardIssuanceApp
// Expiração: 60 minutos (configurável)
// Algoritmo: HS256 (HMAC SHA256)
```

### Exemplo de Uso com cURL

```bash
# 1. Obter Token
curl -X POST https://localhost:7215/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"usuario":"user","senha":"password"}'

# Resposta contém o token JWT

# 2. Usar Token em Requisição Autenticada
curl -X POST https://localhost:7215/api/v1/cards/issue \
  -H "Authorization: Bearer eyJ..." \
  -H "Content-Type: application/json" \
  -d '{
    "clienteId": "550e8400-e29b-41d4-a716-446655440000",
    "propostaId": "660e8400-e29b-41d4-a716-446655440000",
    "contaId": "770e8400-e29b-41d4-a716-446655440000",
    "codigoProduto": "GOLD",
    "correlacaoId": "corr-123456"
  }'
```

---

## 📁 Estrutura do Projeto

```
card-issuance-api/
├── Driving.Api/                          # Camada de Apresentação (Controllers, Startup)
│   ├── Controllers/
│   │   ├── CardsController.cs           # Endpoints de cartões
│   │   └── AuthController.cs            # Endpoints de autenticação
│   ├── Extensions/
│   │   └── SerilogExtensions.cs         # Configuração de logging
│   ├── Program.cs                        # Configuração da aplicação
│   └── appsettings.*.json               # Configurações por ambiente
│
├── Core.Application/                     # Camada de Aplicação (Serviços, DTOs)
│   ├── DTOs/
│   │   ├── CardIssuanceRequestDTO.cs
│   │   ├── CardIssuanceResponseDTO.cs
│   │   ├── CardActivationRequestDTO.cs
│   │   ├── LoginDto.cs
│   │   └── ...
│   ├── Services/
│   │   ├── CardIssuanceService.cs       # Lógica de emissão de cartões
│   │   ├── CardActivationService.cs     # Lógica de ativação de cartões
│   │   └── AuthenticationService.cs     # Lógica de autenticação JWT
│   ├── Interfaces/
│   │   ├── Services/
│   │   │   └── IAuthenticationService.cs
│   │   └── Repositories/
│   │       ├── ICardRepository.cs
│   │       └── IOutboxRepository.cs
│   ├── Mappers/
│   │   └── CardMapper.cs               # Mapeamento de DTOs
│   ├── Validators/                      # Validações FluentValidation
│   └── Core.Application.DependencyInjection.cs
│
├── Core.Domain/                          # Camada de Domínio (Entidades, Lógica)
│   ├── Entities/
│   │   ├── Card.cs                     # Entidade de Cartão
│   │   ├── OutboxEvent.cs              # Eventos do padrão Outbox
│   │   ├── CardIdempotencyKey.cs       # Chaves de idempotência
│   │   └── Cliente.cs                  # Entidade de Cliente
│   ├── Common/
│   │   └── BaseEntity.cs               # Classe base para entidades
│   └── Value Objects/                   # Value Objects (se houver)
│
├── Driven.SqlLite/                       # Camada de Dados (SQLite)
│   ├── Data/
│   │   └── ApplicationDbContext.cs      # DbContext do EF Core
│   ├── Repositories/
│   │   ├── CardRepository.cs
│   │   ├── OutboxRepository.cs
│   │   └── BaseRepository.cs
│   ├── Migrations/
│   │   ├── 20250101000000_InitialCreate.cs
│   │   ├── 20250101000001_AddInformacoesFinanceirasAndUsuario.cs
│   │   ├── 20250101000002_AddCardsAndOutboxEvents.cs
│   │   └── ApplicationDbContextModelSnapshot.cs
│   └── Driven.SqlLite.DependencyInjection.cs
│
├── Core.Infra/                           # Camada de Infraestrutura (Serviços Genéricos)
│   ├── CardIssuance/
│   │   └── OutboxDispatcher.cs          # Dispatcher para publicar eventos
│   └── Core.Infra.DependencyInjection.cs
│
├── Driven.RabbitMQ/                      # Mensageria (RabbitMQ)
│   ├── Events/
│   │   └── CardIssuanceEvents.cs        # Definição de eventos
│   ├── Interfaces/
│   │   └── IMessageBus.cs
│   ├── RabbitMQConnection.cs            # Conexão RabbitMQ
│   └── Driven.RabbitMQ.DependencyInjection.cs
│
├── Driven.CreditMock/                    # Serviço Mock de Crédito
│   ├── Services/
│   │   └── CreditMockService.cs         # Mock do serviço de análise de crédito
│   └── Driven.CreditMock.DependencyInjection.cs
│
├── Test.XUnit/                           # Testes Unitários e Integração
│   ├── Application/
│   │   └── CardIssuanceServiceTests.cs
│   ├── Infrastructure/
│   │   ├── CreditMockServiceTests.cs
│   │   └── OutboxRepositoryTests.cs
│   ├── Domain/
│   │   └── CardTests.cs
│   └── GlobalUsings.cs
│
├── Emissao.Cartao.sln                   # Solução Visual Studio
├── README.md                             # Este arquivo
└── .gitignore
```

---

## 🎨 Padrões de Design Implementados

### 1. **Clean Architecture**
Separação clara de responsabilidades entre camadas:
- Controllers (Driving)
- Services (Application)
- Entities (Domain)
- Repositories (Infrastructure)

### 2. **Dependency Injection (DI)**
Todas as dependências são registradas no `Program.cs`:
```csharp
builder.Services.AddApplicationServices(...);
builder.Services.AddSqlLiteDatabase(...);
builder.Services.AddCardIssuanceServices();
```

### 3. **Repository Pattern**
Abstração de dados através de repositórios:
```csharp
ICardRepository
IOutboxRepository
```

### 4. **Service Layer Pattern**
Serviços encapsulam lógica de negócio:
```csharp
CardIssuanceService
CardActivationService
AuthenticationService
```

### 5. **DTO (Data Transfer Object)**
Separação entre objetos de transferência e entidades:
```csharp
CardIssuanceRequestDTO
CardIssuanceResponseDTO
CardActivationRequestDTO
```

### 6. **Outbox Pattern**
Implementa transações distribuídas seguras:
1. Salva operação + evento na mesma transação
2. `OutboxDispatcher` publica eventos para RabbitMQ
3. Garante entrega confiável de mensagens

### 7. **Idempotency Pattern**
Evita duplicatas em operações:
- Usa `Idempotency-Key` HTTP header
- Armazena requisições em `CardIdempotencyKeys`
- Retorna resultado anterior em retry

### 8. **Value Objects**
(Implementação futura com tipos específicos do domínio)

### 9. **Domain Events**
Eventos de domínio publicados através do Outbox:
```csharp
CardIssuedEvent
CardActivatedEvent
```

### 10. **Circuit Breaker Pattern**
(Implementado em RabbitMQ connection com retry logic)

---

## 🔄 Fluxo de Emissão de Cartões

```
┌─────────────────────────────────────┐
│  1. Cliente envia requisição        │
│  POST /api/v1/cards/issue           │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  2. Autenticação JWT Validada       │
│  Bearer Token verificado            │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  3. Validação de Entrada            │
│  - ClienteId, PropostaId, etc       │
│  - FluentValidation                 │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  4. Verificação de Idempotência     │
│  - Idempotency-Key já processada?   │
│  - Se sim, retorna resultado cache  │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  5. Criar Cartões (Entidade)        │
│  - Card.cs gera tokens PAN/CVV      │
│  - Status: REQUESTED                │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  6. Persistir em Transação          │
│  - Salvar Cards no banco            │
│  - Salvar CardIdempotencyKey        │
│  - Salvar OutboxEvent (CardIssued)  │
│  - Commit atômico (ACID)            │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  7. Retornar 202 Accepted           │
│  - Requisição aceita em processamento│
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  8. OutboxDispatcher                │
│  - Lê eventos não publicados        │
│  - Publica em RabbitMQ              │
│  - Marca como enviado (DataEnvio)   │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  9. RabbitMQ Delivery               │
│  - Sistemas downstream recebem evento│
│  - Ex: Notificação, Auditoria, etc  │
└─────────────────────────────────────┘
```

### Fluxo de Ativação de Cartão

```
┌─────────────────────────────────────┐
│  1. Cliente envia requisição        │
│  POST /api/v1/cards/{id}/activate   │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  2. Autenticação JWT Validada       │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  3. Localizar Cartão                │
│  - Buscar por CardId no repositório │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  4. Validar OTP/CVV                 │
│  - Verificar credenciais de segurança│
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  5. Atualizar Status                │
│  - Status: ACTIVE                   │
│  - CanalAtivacao: MOBILE_APP, etc   │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  6. Persistir + Evento Outbox       │
│  - Salvar cardão ativado            │
│  - Salvar CardActivatedEvent        │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  7. Retornar 200 OK                 │
│  - Cartão ativado com sucesso       │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  8. Publicar Evento (Outbox)        │
│  - CardActivated para downstream    │
└─────────────────────────────────────┘
```

---

## ⚙️ Configuração e Variáveis de Ambiente

### appsettings.json (Geral)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### appsettings.Development.json
```json
{
  "Jwt": {
    "Secret": "sua-chave-super-secreta-com-minimo-32-caracteres-para-desenvolvimento"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=card_issuance.db;"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "MaxRetries": 3,
    "RetryDelayMs": 1000
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  },
  "Serilog": {
    "MinimumLevel": "Debug",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/app-.txt" } }
    ]
  }
}
```

### appsettings.Production.json
```json
{
  "Jwt": {
    "Secret": "USE_ENVIRONMENT_VARIABLE_IN_PRODUCTION"
  },
  "ConnectionStrings": {
    "DefaultConnection": "USE_ENVIRONMENT_VARIABLE_IN_PRODUCTION"
  },
  "RabbitMQ": {
    "HostName": "rabbitmq.production.com",
    "Port": 5672,
    "Username": "rabbitmq-user",
    "Password": "rabbitmq-secure-password",
    "MaxRetries": 5,
    "RetryDelayMs": 2000
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

### Variáveis de Ambiente (Recomendado)
```bash
# Segurança
export JWT_SECRET="sua-chave-super-secreta-com-minimo-32-caracteres"

# Banco de Dados
export DB_CONNECTION_STRING="Data Source=/var/data/card_issuance.db;"

# RabbitMQ
export RABBITMQ_HOST="rabbitmq.internal"
export RABBITMQ_PORT="5672"
export RABBITMQ_USER="cardissuance_user"
export RABBITMQ_PASSWORD="secure_password"

# Logging
export LOG_LEVEL="Information"
export SEQ_SERVER_URL="https://logs.production.com"
```

---

## 🗄️ Banco de Dados

### Schema do Banco

#### Tabela: Clientes
```sql
CREATE TABLE Clientes (
  Id TEXT PRIMARY KEY,
  Nome TEXT NOT NULL,
  Email TEXT NOT NULL UNIQUE,
  Telefone TEXT NOT NULL,
  Cpf TEXT NOT NULL UNIQUE,
  Endereco TEXT NOT NULL,
  Cidade TEXT NOT NULL,
  Estado TEXT NOT NULL,
  Cep TEXT NOT NULL,
  DataCriacao TEXT NOT NULL,
  DataAtualizacao TEXT,
  CriadoPor TEXT,
  AtualizadoPor TEXT,
  Ativo INTEGER NOT NULL
);
```

#### Tabela: Cards
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
  Status TEXT NOT NULL DEFAULT 'REQUESTED',
  CanalAtivacao TEXT,
  CorrelacaoId TEXT NOT NULL,
  MesValidade INTEGER NOT NULL,
  AnoValidade INTEGER NOT NULL,
  DataCriacao TEXT NOT NULL,
  DataAtualizacao TEXT,
  CriadoPor TEXT,
  AtualizadoPor TEXT,
  Ativo INTEGER NOT NULL,
  FOREIGN KEY (ClienteId) REFERENCES Clientes(Id)
);
```

#### Tabela: CardIdempotencyKeys
```sql
CREATE TABLE CardIdempotencyKeys (
  Id TEXT PRIMARY KEY,
  ChaveIdempotencia TEXT NOT NULL UNIQUE,
  CartoesIds TEXT NOT NULL,
  DataCriacao TEXT NOT NULL,
  DataAtualizacao TEXT,
  CriadoPor TEXT,
  AtualizadoPor TEXT,
  Ativo INTEGER NOT NULL
);
```

#### Tabela: OutboxEvents
```sql
CREATE TABLE OutboxEvents (
  Id TEXT PRIMARY KEY,
  Topico TEXT NOT NULL,
  Payload TEXT NOT NULL,
  DataEnvio TEXT,
  DataCriacao TEXT NOT NULL,
  DataAtualizacao TEXT,
  CriadoPor TEXT,
  AtualizadoPor TEXT,
  Ativo INTEGER NOT NULL
);
```

### Migrations

As migrations estão em `Driven.SqlLite/Migrations/`:

1. **InitialCreate** - Cria tabela Clientes
2. **AddInformacoesFinanceirasAndUsuario** - Adiciona tabelas de dados financeiros
3. **AddCardsAndOutboxEvents** - Adiciona tabelas de cartões e eventos

Para aplicar todas as migrations:
```bash
dotnet ef database update --project Driven.SqlLite --startup-project Driving.Api
```

Para criar uma nova migration:
```bash
dotnet ef migrations add NomeDaMigracao --project Driven.SqlLite --startup-project Driving.Api
```

---

## 📊 Logging

### Serilog Configuration

O sistema usa **Serilog** para logging estruturado:

```csharp
// Program.cs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

### Log Levels

| Level | Uso |
|-------|-----|
| **Verbose** | Informações muito detalhadas |
| **Debug** | Informações de debugging |
| **Information** | Informações gerais (default) |
| **Warning** | Avisos importantes |
| **Error** | Erros da aplicação |
| **Fatal** | Erros críticos que afetam funcionalidade |

### Exemplos de Log

```csharp
// Informativo
_logger.LogInformation(
    "Requisição de emissão recebida. CorrelacaoId={CorrelacaoId}",
    request.CorrelacaoId);

// Aviso
_logger.LogWarning(
    "RabbitMQ não disponível. Usando fallback em memória");

// Erro
_logger.LogError(ex,
    "Erro ao emitir cartões. PropostaId={PropostaId}",
    request.PropostaId);
```

### Visualizar Logs

**Arquivo**: `logs/app-YYYY-MM-DD.txt`

```bash
# Seguir logs em tempo real (Linux/Mac)
tail -f logs/app-*.txt

# No Windows
Get-Content logs/app-*.txt -Tail 50 -Wait
```

### Com Seq (Opcional)

Para centralizar logs, use Seq:

```csharp
.WriteTo.Seq("https://logs.seu-server.com")
```

---

## ⚠️ Tratamento de Erros

### Exceções Custom

```csharp
// Domain exception
public class CartaoJaAtivadoException : DomainException
{
    public CartaoJaAtivadoException(Guid cartaoId)
        : base($"Cartão {cartaoId} já foi ativado")
    {
    }
}
```

### Global Exception Handler

Implementado via middleware (recomendado):

```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features
            .Get<IExceptionHandlerPathFeature>();
        var ex = exceptionHandlerPathFeature?.Error;

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = ex switch
        {
            ArgumentException => 400,
            KeyNotFoundException => 404,
            _ => 500
        };

        await context.Response.WriteAsJsonAsync(new { error = ex?.Message });
    });
});
```

### Response Padrão de Erro

```json
{
  "sucesso": false,
  "mensagem": "Validação falhou",
  "erros": [
    "ClienteId não pode estar vazio",
    "PropostaId inválido"
  ]
}
```

---

## 🧪 Testes

### Estrutura de Testes

```
Test.XUnit/
├── Application/
│   ├── CardIssuanceServiceTests.cs
│   └── AuthenticationServiceTests.cs
├── Infrastructure/
│   ├── CreditMockServiceTests.cs
│   ├── OutboxRepositoryTests.cs
│   └── CardRepositoryTests.cs
├── Domain/
│   └── CardTests.cs
└── GlobalUsings.cs
```

### Executar Testes

```bash
# Todos os testes
dotnet test

# Projeto específico
dotnet test Test.XUnit

# Teste específico
dotnet test --filter "FullyQualifiedName~CardIssuanceServiceTests"

# Com cobertura
dotnet test /p:CollectCoverage=true
```

### Exemplo de Teste Unitário

```csharp
[Fact]
public async Task EmitirCartoes_ComDadosValidos_DeveRetornarCartoes()
{
    // Arrange
    var request = new CardIssuanceRequestDTO
    {
        ClienteId = Guid.NewGuid(),
        PropostaId = Guid.NewGuid(),
        ContaId = Guid.NewGuid(),
        CodigoProduto = "GOLD",
        CorrelacaoId = "corr-123"
    };

    var repositoryMock = new Mock<ICardRepository>();
    var service = new CardIssuanceService(repositoryMock.Object);

    // Act
    var resultado = await service.EmitirCartõesAsync(request, CancellationToken.None);

    // Assert
    resultado.Should().NotBeEmpty();
    resultado.Count().Should().Be(1);
    resultado.First().Status.Should().Be("REQUESTED");
    repositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<Card>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

### Cobertura de Código

Objetivo: > 80% de cobertura

```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=lcov
```

---

## 🤝 Contribuindo

### Processo de Desenvolvimento

1. **Crie uma Branch**
```bash
git checkout -b feature/nova-funcionalidade
```

2. **Faça Commits Atômicos**
```bash
git commit -m "feat: adicionar suporte a cartões multi-moeda"
```

3. **Siga o Padrão de Commits**
- `feat:` Nova funcionalidade
- `fix:` Correção de bug
- `docs:` Documentação
- `style:` Formatação de código
- `refactor:` Refatoração
- `test:` Testes
- `chore:` Manutenção

4. **Push e Pull Request**
```bash
git push origin feature/nova-funcionalidade
```

5. **Code Review**
- Mínimo 2 aprovações
- Testes devem passar
- Cobertura deve ser mantida

### Padrões de Código

- **C# Naming**: PascalCase para public, camelCase para private
- **Async/Await**: Use async para operações I/O
- **Null Coalescing**: Use `??` e `?.`
- **LINQ**: Prefira method syntax (`.Where()`) a query syntax
- **Comments**: Documente lógica complexa com XML comments

```csharp
/// <summary>
/// Emite cartões para um cliente após aprovação de proposta
/// </summary>
/// <param name="request">Dados para emissão</param>
/// <param name="cancellationToken">Token de cancelamento</param>
/// <returns>Lista de cartões emitidos</returns>
public async Task<List<Card>> EmitirCartõesAsync(
    CardIssuanceRequestDTO request,
    CancellationToken cancellationToken = default)
{
    // implementação
}
```

---

## 🔧 Troubleshooting

### Problema: "Database is locked"

**Causa**: SQLite está sendo acessado por múltiplos processos
**Solução**:
```bash
# Feche outras instâncias
# Ou use connection string com timeout
Data Source=card_issuance.db;Connection Timeout=30;
```

### Problema: "RabbitMQ connection refused"

**Causa**: RabbitMQ não está rodando
**Solução**:
```bash
# Docker
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.12-management

# Windows Service
rabbitmq-service start

# Verificar
docker logs rabbitmq
```

### Problema: "JWT token invalid"

**Causa**: Chave secreta diferente entre geração e validação
**Solução**:
- Verifique se `Jwt:Secret` é igual em `Login` e `TokenValidationParameters`
- Em produção, use variáveis de ambiente

### Problema: "OutboxDispatcher: no such table OutboxEvents"

**Causa**: Migrations não foram aplicadas
**Solução**:
```bash
# Aplicar migrations
dotnet ef database update --project Driven.SqlLite --startup-project Driving.Api

# Ou deixar aplicar automaticamente (já configurado em Program.cs)
```

### Problema: "Migration pending"

**Causa**: Código alterado mas migration não criada
**Solução**:
```bash
# Criar migration
dotnet ef migrations add DescricaoDaAlteracao --project Driven.SqlLite

# Aplicar
dotnet ef database update --project Driven.SqlLite
```

### Problema: "Port 7215 already in use"

**Causa**: Outra aplicação usando a porta
**Solução**:
```bash
# Windows
netstat -ano | findstr :7215

# Linux/Mac
lsof -i :7215

# Matar processo
taskkill /PID <processo_id> /F
```

### Problema: "Build falha com CS0234"

**Causa**: Namespace removido mas ainda referenciado
**Solução**: Procure por `using` statements antigos e remova-os

---

## 📚 Recursos Adicionais

### Documentação Oficial
- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)

### Artigos Recomendados
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Outbox Pattern for Reliable Event Publishing](https://microservices.io/patterns/data/transactional-outbox.html)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8949)

### Ferramentas Úteis
- [Postman](https://www.postman.com/) - API Testing
- [Swagger/OpenAPI](https://swagger.io/) - API Documentation
- [DBeaver](https://dbeaver.io/) - Database Management
- [RabbitMQ Management UI](http://localhost:15672) - Message Broker Management

---

## 📝 Changelog

### v1.0.0 (2024-11-03)
✅ Emissão de cartões com suporte a idempotência
✅ Ativação de cartões com validação OTP/CVV
✅ Padrão Outbox para eventos confiáveis
✅ Integração com RabbitMQ
✅ Autenticação JWT
✅ Logging estruturado com Serilog
✅ Documentação Swagger completa
✅ Testes unitários com xUnit

---

## 📄 Licença

Este projeto está licenciado sob a [MIT License](LICENSE).

---

## 👥 Autores

- **Desenvolvimento Backend**: Equipe de Desenvolvimento
- **Arquitetura**: Domain-Driven Design (DDD) & Clean Architecture

---

## 📞 Suporte

Para questões e problemas:
1. Verifique o [Troubleshooting](#troubleshooting)
2. Abra uma [Issue](https://github.com/seu-usuario/card-issuance-api/issues)
3. Envie um email: suporte@seu-dominio.com

---

**Última atualização**: 03 de Novembro de 2024
**Versão**: 1.0.0
