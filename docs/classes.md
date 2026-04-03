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

- [FinanceApi.Identity.csproj](../src/FinanceApi.Identity/FinanceApi.Identity.csproj) — JwtBearer, EF Core, Npgsql, BCrypt.Net-Next, FluentValidation, Serilog
- [Program.cs](../src/FinanceApi.Identity/Program.cs) — registra DbContext, JwtService, AuthService; aplica migrations no startup
- [appsettings.json](../src/FinanceApi.Identity/appsettings.json) — connection string (schema: identity), JWT, MasterKey
- [Dockerfile](../src/FinanceApi.Identity/Dockerfile)

<details id="dir-identity-domain">
<summary><strong>Domain/</strong></summary>
<blockquote>

<details id="User">
<summary><strong><a href="../src/FinanceApi.Identity/Domain/Models/User.cs">User.cs</a></strong></summary>
<blockquote>

<details><summary>atributos</summary>

- `Id: Guid` (gerado no Create)
- `Name: string` (max 100)
- `Email: string` (max 100, unique)
- `PasswordHash: string`
- `Role: Role` (enum, stored as string)
- `CreatedAt: DateTime` (UTC)

</details>
<details><summary>metodos</summary>

- `static Create(name, email, passwordHash, role): User` — factory method; construtor privado

</details>

</blockquote>
</details>

<details id="Role">
<summary><strong><a href="../src/FinanceApi.Identity/Domain/Enums/Role.cs">Role.cs</a> [enum]</strong></summary>
<blockquote>

- `User`, `Admin`

</blockquote>
</details>

</blockquote>
</details>

<details id="dir-identity-application">
<summary><strong>Application/</strong></summary>
<blockquote>

<details id="IAuthService">
<summary><strong><a href="../src/FinanceApi.Identity/Application/Interfaces/IAuthService.cs">IAuthService.cs</a> [interface]</strong></summary>
<blockquote>

<details><summary>metodos</summary>

- `RegisterAsync(RegisterRequest): Task<LoginResponse>`
- `LoginAsync(LoginRequest): Task<LoginResponse>`
- `CreateAdminAsync(CreateAdminRequest): Task<LoginResponse>`

</details>

</blockquote>
</details>

<details id="AuthService">
<summary><strong><a href="../src/FinanceApi.Identity/Application/Services/AuthService.cs">AuthService.cs</a> [implements [IAuthService](#IAuthService)]</strong></summary>
<blockquote>

<details><summary>dependencias</summary>

- [IdentityDbContext](#IdentityDbContext)
- [JwtService](#JwtService)
- `IConfiguration`

</details>
<details><summary>metodos</summary>

- `RegisterAsync` — verifica email duplicado, BCrypt.HashPassword, User.Create, salva, BuildResponse
- `LoginAsync` — busca por email, BCrypt.Verify, BuildResponse; lança UnauthorizedAccessException se inválido
- `CreateAdminAsync` — valida MasterKey, cria com Role.Admin
- `BuildResponse(User): LoginResponse` — private

</details>

</blockquote>
</details>

</blockquote>
</details>

<details id="dir-identity-infra">
<summary><strong>Infrastructure/</strong></summary>
<blockquote>

<details id="IdentityDbContext">
<summary><strong><a href="../src/FinanceApi.Identity/Infrastructure/Persistence/IdentityDbContext.cs">IdentityDbContext.cs</a> [DbContext]</strong></summary>
<blockquote>

<details><summary>atributos</summary>

- `Users: DbSet<User>`

</details>
<details><summary>configuracao</summary>

- Schema padrão: `identity`
- Email com índice unique
- Role armazenado como string

</details>

</blockquote>
</details>

- [IdentityDbContextFactory.cs](../src/FinanceApi.Identity/Infrastructure/Persistence/IdentityDbContextFactory.cs) — `IDesignTimeDbContextFactory` para migrations em design-time
- `Migrations/` — migration `InitialCreate` (tabela `identity.users`)

<details id="JwtService">
<summary><strong><a href="../src/FinanceApi.Identity/Infrastructure/Security/JwtService.cs">JwtService.cs</a></strong></summary>
<blockquote>

<details><summary>metodos</summary>

- `GenerateToken(User): string` — HS256; claims: sub (Guid), email, role, jti

</details>

</blockquote>
</details>

</blockquote>
</details>

<details id="dir-identity-api">
<summary><strong>Api/</strong></summary>
<blockquote>

<details id="AuthController">
<summary><strong><a href="../src/FinanceApi.Identity/Api/Controllers/AuthController.cs">AuthController.cs</a> [ApiController, Route("auth")]</strong></summary>
<blockquote>

<details><summary>metodos</summary>

- `POST /auth/register` → `RegisterAsync` — 200 OK / 409 Conflict
- `POST /auth/login` → `LoginAsync` — 200 OK / 401 Unauthorized
- `POST /auth/admin` → `CreateAdminAsync` — 200 OK / 401 / 409

</details>

</blockquote>
</details>

<details id="AuthDtos">
<summary><strong><a href="../src/FinanceApi.Identity/Api/Dtos/AuthDtos.cs">AuthDtos.cs</a> [records]</strong></summary>
<blockquote>

- `RegisterRequest(Name, Email, Password)`
- `LoginRequest(Email, Password)`
- `CreateAdminRequest(Name, Email, Password, MasterKey)`
- `LoginResponse(Token, UserId, Email, Name, Role)`

</blockquote>
</details>

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
