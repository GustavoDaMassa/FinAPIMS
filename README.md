# MSFinanceApi

Reimplementação da [financeApi](https://github.com/GustavoDaMassa/financeApi) como sistema de microsserviços em **.NET 9 / ASP.NET Core / C#**.

O objetivo é aplicar os conceitos de Sistemas Distribuídos na prática — cada serviço com responsabilidade isolada, banco de dados próprio, comunicação síncrona via HTTP e assíncrona via Kafka.

---

## Stack

| Componente | Tecnologia |
|---|---|
| Runtime | .NET 9 |
| Framework | ASP.NET Core |
| Banco de dados | PostgreSQL 15 (schemas isolados por serviço) |
| ORM | Entity Framework Core |
| Migrations | EF Core Migrations |
| Gateway | YARP (Yet Another Reverse Proxy) |
| Auth | JWT HMAC-SHA256 |
| Mensageria | Apache Kafka (Confluent 7.0.1) |
| API | GraphQL (Hot Chocolate) + REST |
| Testes | xUnit + Moq |
| Logs | Serilog |
| Build | .NET CLI / Solution (.sln) |

---

## Arquitetura

```
Cliente
   │
   ▼
┌─────────────────┐
│     Gateway     │  :8080 — valida JWT, roteia, injeta X-User-Id / X-User-Role
└────────┬────────┘
         │ HTTP interno
    ┌────┴──────────────────────────┐
    │                               │
    ▼                               ▼
┌──────────┐                ┌──────────────┐
│ Identity │                │   Finance    │  GraphQL + REST (OFX import)
│ :5001    │                │   :5002      │
└──────────┘                └──────┬───────┘
                                   │ Kafka consumer
                                   │ (topic: webhook.events)
                            ┌──────┴───────┐
                            │   Webhook    │  :5003 — recebe POST do Pluggy
                            └──────────────┘
                                   │ Kafka producer
                                   ▼
                              [kafka:9092]
```

### Propagação de identidade

O **Gateway** valida o JWT e injeta os claims do usuário como headers HTTP nos serviços internos:

```
X-User-Id:   <userId>
X-User-Role: <role>
```

Os serviços downstream confiam nesses headers sem revalidar o token. O `UserContext` no Finance lê esses headers diretamente:

```csharp
// O gateway valida o JWT e injeta X-User-Id e X-User-Role nos headers.
// O finance-service confia nesses headers sem revalidar o token.
public class UserContext(IHttpContextAccessor accessor) : IUserContext
{
    public Guid UserId
    {
        get
        {
            var value = accessor.HttpContext?.Request.Headers["X-User-Id"].ToString();
            if (string.IsNullOrEmpty(value) || !Guid.TryParse(value, out var id))
                throw new UnauthorizedAccessException("Missing or invalid X-User-Id header.");
            return id;
        }
    }
}
```

---

## Serviços

### FinanceApi.Gateway

Ponto de entrada único do sistema. Responsabilidades:

- Validar o JWT em todas as requisições autenticadas
- Rotear para o serviço correto via YARP
- Injetar `X-User-Id` e `X-User-Role` como headers nas requisições downstream

Não contém lógica de negócio.

---

### FinanceApi.Identity

Gerencia usuários e autenticação.

**Endpoints REST:**
| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/auth/register` | Criar conta |
| POST | `/api/auth/login` | Autenticar e receber JWT |
| POST | `/api/auth/admin` | Criar conta admin (requer Master Key) |

**Banco:** schema `identity` no PostgreSQL compartilhado.

---

### FinanceApi.Finance

Serviço principal — toda a lógica financeira.

**GraphQL** (via Hot Chocolate) em `/graphql`:

| Operação | Descrição |
|---|---|
| `accounts` | Listar contas do usuário |
| `createAccount` | Criar conta |
| `transactions` | Listar transações com filtros e paginação |
| `createTransaction` | Criar transação manual |
| `categorizeTransaction` | Categorizar transação |
| `categories` | Listar categorias do usuário |
| `createCategory` | Criar categoria/subcategoria |
| `financialIntegrations` | Listar integrações bancárias |
| `createFinancialIntegration` | Criar integração com Pluggy |

**REST:**
| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/import/{accountId}` | Importar extrato OFX |

**Banco:** schema `finance` no PostgreSQL compartilhado.

**Modelo de dados:**
```
User (via X-User-Id)
 └── Account (N)
      └── Transaction (N)
 └── Category (N)
      └── Transaction (N) — via category/subcategory
 └── FinancialIntegration (N)
      └── Account (N)
```

**Importação OFX:**
- Suporta OFX 1.x (SGML) e OFX 2.x (XML)
- Detecção automática de encoding (ISO-8859-1 / UTF-8)
- Deduplicação via `externalId` — reimportar o mesmo extrato é seguro

**Comunicação com Webhook:** consome eventos do tópico Kafka `webhook.events` via `BackgroundService`. O contrato compartilhado está em `shared/FinanceApi.Shared.Contracts`.

---

### FinanceApi.Webhook

Recebe notificações do Pluggy (Open Finance) e publica transações no Kafka.

**Fluxo:**
```
POST /webhook/pluggy  (Pluggy notifica novo item)
         │
         ▼
PluggyClient busca transações via API Pluggy
         │
         ▼
WebhookEventProducer publica WebhookEvent no tópico webhook.events
         │
         ▼
Finance consumer persiste as transações
```

Não tem banco de dados próprio — é stateless.

---

## Contrato compartilhado

O projeto `shared/FinanceApi.Shared.Contracts` contém os eventos trocados via Kafka entre Webhook e Finance:

```csharp
public record WebhookEvent(
    string LinkId,
    IReadOnlyList<ExternalTransaction> Transactions
);

public record ExternalTransaction(
    string ExternalId,
    decimal Amount,
    string PluggyType,   // "CREDIT" | "DEBIT"
    string? Description,
    DateOnly Date,
    string PluggyAccountId
);
```

---

## Banco de dados

Um único servidor PostgreSQL com schemas isolados por serviço:

| Serviço | Schema |
|---|---|
| Identity | `identity` |
| Finance | `finance` |

O isolamento por schema mantém os dados separados logicamente sem a necessidade de múltiplos servidores em ambiente local.

---

## Testes

| Projeto de teste | Cobertura |
|---|---|
| `FinanceApi.Finance.Tests` | AccountService, CategoryService, TransactionService, FinancialIntegrationService, OfxImportService, OfxParser, WebhookEventConsumer |
| `FinanceApi.Identity.Tests` | AuthService |
| `FinanceApi.Webhook.Tests` | PluggyWebhookController |

Todos os testes de serviço usam mocks — sem banco de dados real.

---

## Como rodar localmente

### Pré-requisitos

- .NET 9 SDK
- Docker e Docker Compose

### Variáveis de ambiente

Crie um `.env` na raiz do projeto:

```env
DATABASE_PASSWORD=sua-senha
JWT_SECRET=sua-chave-secreta-de-pelo-menos-32-caracteres
MASTER_KEY=chave-para-criar-admins
PLUGGY_CLIENT_ID=seu-client-id-pluggy
PLUGGY_CLIENT_SECRET=seu-client-secret-pluggy
```

### Subir tudo

```bash
docker compose up --build -d
```

O sistema estará disponível em `http://localhost:8080`.

### Rodar testes

```bash
dotnet test
```

### Rodar um serviço individualmente (sem Docker)

```bash
cd src/FinanceApi.Finance
dotnet run
```

---

## Comparação com a versão anterior (financeApi)

| | financeApi (monolito) | MSFinanceApi (microsserviços) |
|---|---|---|
| Linguagem | Java 21 / Spring Boot | C# / .NET 9 / ASP.NET Core |
| Auth | JWT RSA no próprio serviço | JWT HMAC no Gateway, propagado via header |
| Banco | PostgreSQL único (tabelas) | PostgreSQL único (schemas isolados) |
| Deploy | Um container | Quatro containers de serviço |
| Escalabilidade | Vertical | Horizontal por serviço |
| Complexidade | Menor | Maior — justificada por exercício de sistemas distribuídos |

A versão em microsserviços não é necessariamente "melhor" para o problema em questão — o monolito é mais simples de operar. O objetivo foi praticar os padrões de sistemas distribuídos: isolamento de serviços, comunicação assíncrona, propagação de identidade e contrato compartilhado.
