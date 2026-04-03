# FinanceApiMS — Mapa de Classes

<details id="dir-root">
<summary><strong>/ (raiz)</strong></summary>
<blockquote>

- [FinanceApi.sln](../FinanceApi.sln) — solution multi-projeto
- [docker-compose.yml](../docker-compose.yml) — ambiente local (postgres, kafka, zookeeper, 4 serviços)
- [.gitignore](../.gitignore) — dotnet gitignore padrão

</blockquote>
</details>

---

## shared/

<details id="dir-shared-contracts">
<summary><strong>shared/FinanceApi.Shared.Contracts/</strong></summary>
<blockquote>

- [FinanceApi.Shared.Contracts.csproj](../shared/FinanceApi.Shared.Contracts/FinanceApi.Shared.Contracts.csproj) — classlib referenciada por identity, finance e webhook

<details id="dir-shared-events">
<summary><strong>Events/</strong></summary>
<blockquote>

<details id="WebhookEvent">
<summary><strong><a href="../shared/FinanceApi.Shared.Contracts/Events/WebhookEvent.cs">WebhookEvent.cs</a> [record]</strong></summary>
<blockquote>

<details><summary>atributos</summary>

- `ItemId: string` — ID do item Pluggy
- `TransactionsLink: string` — URL para fetch das transações novas

</details>

</blockquote>
</details>

</blockquote>
</details>

</blockquote>
</details>

---

## src/

### FinanceApi.Gateway

<details id="dir-gateway">
<summary><strong>src/FinanceApi.Gateway/</strong></summary>
<blockquote>

- [FinanceApi.Gateway.csproj](../src/FinanceApi.Gateway/FinanceApi.Gateway.csproj) — Yarp.ReverseProxy, JwtBearer 8.x, Serilog
- [Program.cs](../src/FinanceApi.Gateway/Program.cs) — YARP config, JWT validation, roteamento por path
- [appsettings.json](../src/FinanceApi.Gateway/appsettings.json) — rotas YARP, clusters (identity :8081, finance :8082, webhook :8083), JWT
- [Dockerfile](../src/FinanceApi.Gateway/Dockerfile) — multi-stage build a partir da raiz da solution

</blockquote>
</details>

---

### FinanceApi.Identity

<details id="dir-identity">
<summary><strong>src/FinanceApi.Identity/</strong></summary>
<blockquote>

- [FinanceApi.Identity.csproj](../src/FinanceApi.Identity/FinanceApi.Identity.csproj) — JwtBearer, EF Core, Npgsql, FluentValidation, Serilog
- [Program.cs](../src/FinanceApi.Identity/Program.cs) — bootstrap mínimo
- [appsettings.json](../src/FinanceApi.Identity/appsettings.json) — connection string (schema: identity), JWT, MasterKey
- [Dockerfile](../src/FinanceApi.Identity/Dockerfile)

<details id="dir-identity-domain">
<summary><strong>Domain/Models/</strong></summary>
<blockquote>
_(a preencher)_
</blockquote>
</details>

<details id="dir-identity-application">
<summary><strong>Application/</strong></summary>
<blockquote>

- `Interfaces/` _(a preencher)_
- `Services/` _(a preencher)_

</blockquote>
</details>

<details id="dir-identity-infra">
<summary><strong>Infrastructure/</strong></summary>
<blockquote>

- `Persistence/` — DbContext, migrations (schema: identity)
- `Security/` — JwtService, BCrypt

</blockquote>
</details>

<details id="dir-identity-api">
<summary><strong>Api/</strong></summary>
<blockquote>

- `Controllers/` — AuthController (POST /auth/login, /auth/register, /auth/admin)
- `Dtos/` — LoginRequest, RegisterRequest, LoginResponse

</blockquote>
</details>

</blockquote>
</details>

---

### FinanceApi.Finance

<details id="dir-finance">
<summary><strong>src/FinanceApi.Finance/</strong></summary>
<blockquote>

- [FinanceApi.Finance.csproj](../src/FinanceApi.Finance/FinanceApi.Finance.csproj) — JwtBearer, EF Core, Npgsql, HotChocolate 14, Confluent.Kafka, FluentValidation, Serilog, Mapperly
- [Program.cs](../src/FinanceApi.Finance/Program.cs) — bootstrap com GraphQL e Kafka consumer
- [appsettings.json](../src/FinanceApi.Finance/appsettings.json) — connection string (schema: finance), JWT, Kafka
- [Dockerfile](../src/FinanceApi.Finance/Dockerfile)

<details id="dir-finance-domain">
<summary><strong>Domain/</strong></summary>
<blockquote>

- `Models/` — Account, Transaction, Category, FinancialIntegration _(a preencher)_
- `Enums/` — TransactionType (INFLOW/OUTFLOW), AggregatorType (PLUGGY/BELVO) _(a preencher)_

</blockquote>
</details>

<details id="dir-finance-application">
<summary><strong>Application/</strong></summary>
<blockquote>

- `Interfaces/` _(a preencher)_
- `Services/` _(a preencher)_

</blockquote>
</details>

<details id="dir-finance-infra">
<summary><strong>Infrastructure/</strong></summary>
<blockquote>

- `Persistence/` — DbContext, migrations (schema: finance)
- `Kafka/` — WebhookEventConsumer (IHostedService)

</blockquote>
</details>

<details id="dir-finance-api">
<summary><strong>Api/</strong></summary>
<blockquote>

- `GraphQL/Resolvers/` — AccountResolver, TransactionResolver, CategoryResolver, FinancialIntegrationResolver
- `GraphQL/Types/` — tipos de saída Hot Chocolate
- `GraphQL/Inputs/` — inputs de mutação
- `Controllers/` — ImportController (POST /import, OFX)

</blockquote>
</details>

<details id="dir-finance-mappers">
<summary><strong>Mappers/</strong></summary>
<blockquote>
_(Mapperly — source generators, a preencher)_
</blockquote>
</details>

</blockquote>
</details>

---

### FinanceApi.Webhook

<details id="dir-webhook">
<summary><strong>src/FinanceApi.Webhook/</strong></summary>
<blockquote>

- [FinanceApi.Webhook.csproj](../src/FinanceApi.Webhook/FinanceApi.Webhook.csproj) — Confluent.Kafka, Serilog, Shared.Contracts ref
- [Program.cs](../src/FinanceApi.Webhook/Program.cs) — bootstrap mínimo
- [appsettings.json](../src/FinanceApi.Webhook/appsettings.json) — Kafka, Pluggy credentials
- [Dockerfile](../src/FinanceApi.Webhook/Dockerfile)

<details id="dir-webhook-api">
<summary><strong>Api/Controllers/</strong></summary>
<blockquote>
_(a preencher — PluggyWebhookController: POST /webhook/pluggy)_
</blockquote>
</details>

<details id="dir-webhook-application">
<summary><strong>Application/</strong></summary>
<blockquote>

- `Interfaces/` — IPluggyClient, IWebhookEventProducer _(a preencher)_
- `Services/` — PluggyAuthService, RequestService _(a preencher)_

</blockquote>
</details>

<details id="dir-webhook-infra">
<summary><strong>Infrastructure/Kafka/</strong></summary>
<blockquote>
_(a preencher — WebhookEventProducer)_
</blockquote>
</details>

<details id="dir-webhook-datatransfer">
<summary><strong>DataTransfer/</strong></summary>
<blockquote>
_(a preencher — DTOs da API Pluggy)_
</blockquote>
</details>

</blockquote>
</details>

---

## tests/

<details id="dir-tests">
<summary><strong>tests/</strong></summary>
<blockquote>

- `FinanceApi.Identity.Tests/` — xUnit + NSubstitute
- `FinanceApi.Finance.Tests/` — xUnit + NSubstitute + EF InMemory
- `FinanceApi.Webhook.Tests/` — xUnit + NSubstitute

</blockquote>
</details>
