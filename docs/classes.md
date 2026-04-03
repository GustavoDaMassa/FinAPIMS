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

- [FinanceApi.Finance.csproj](../src/FinanceApi.Finance/FinanceApi.Finance.csproj) — EF Core, Npgsql, HotChocolate 14, Confluent.Kafka, Serilog
- [Program.cs](../src/FinanceApi.Finance/Program.cs) — DbContext, serviços, UserContext, Hot Chocolate + extensions, Kafka consumer; aplica migrations no startup
- [appsettings.json](../src/FinanceApi.Finance/appsettings.json) — connection string (schema: finance), Kafka
- [Dockerfile](../src/FinanceApi.Finance/Dockerfile)

<details id="dir-finance-domain">
<summary><strong>Domain/</strong></summary>
<blockquote>

<details id="Account">
<summary><strong><a href="../src/FinanceApi.Finance/Domain/Models/Account.cs">Account.cs</a></strong></summary>
<blockquote>

<details><summary>atributos</summary>

- `Id: Guid`, `AccountName: string`, `Institution: string`, `Description: string?`
- `Balance: decimal` (atualizado por UpdateBalance)
- `PluggyAccountId: string?` (unique, nullable)
- `UserId: Guid`, `IntegrationId: Guid?`

</details>
<details><summary>metodos</summary>

- `static Create(...)`: Account — factory
- `Update(accountName, institution, description)`
- `UpdateBalance(balance)`
- `LinkToPluggy(pluggyAccountId, integrationId)`

</details>

</blockquote>
</details>

<details id="Category">
<summary><strong><a href="../src/FinanceApi.Finance/Domain/Models/Category.cs">Category.cs</a></strong></summary>
<blockquote>

<details><summary>atributos</summary>

- `Id: Guid`, `Name: string`, `UserId: Guid`
- Unique constraint: (Name, UserId)

</details>
<details><summary>metodos</summary>

- `static Create(name, userId)`, `Update(name)`

</details>

</blockquote>
</details>

<details id="Transaction">
<summary><strong><a href="../src/FinanceApi.Finance/Domain/Models/Transaction.cs">Transaction.cs</a></strong></summary>
<blockquote>

<details><summary>atributos</summary>

- `Id: Guid`, `Amount: decimal`, `Type: TransactionType`, `Description: string?`
- `Source: string?`, `Destination: string?`, `TransactionDate: DateOnly`
- `ExternalId: string?` (unique, para deduplicação Pluggy/OFX)
- `AccountId: Guid`, `CategoryId: Guid?`

</details>
<details><summary>metodos</summary>

- `static Create(...)`: Transaction — factory
- `Categorize(categoryId: Guid?)`

</details>

</blockquote>
</details>

<details id="FinancialIntegration">
<summary><strong><a href="../src/FinanceApi.Finance/Domain/Models/FinancialIntegration.cs">FinancialIntegration.cs</a></strong></summary>
<blockquote>

<details><summary>atributos</summary>

- `Id: Guid`, `Aggregator: AggregatorType`, `LinkId: string` (unique)
- `Status: string?`, `CreatedAt: DateTime`, `ExpiresAt: DateTime?`
- `UserId: Guid`

</details>
<details><summary>metodos</summary>

- `static Create(aggregator, linkId, userId)` — ExpiresAt = +12 meses, Status = "UPDATED"
- `UpdateStatus(status)`, `Renew()`

</details>

</blockquote>
</details>

<details id="TransactionType">
<summary><strong><a href="../src/FinanceApi.Finance/Domain/Enums/TransactionType.cs">TransactionType.cs</a> [enum + extension]</strong></summary>
<blockquote>

- `Inflow`, `Outflow`
- `Apply(amount): decimal` — Inflow: +amount, Outflow: -amount (enum strategy via extension)
- `FromPluggy(string): TransactionType` — "CREDIT" → Inflow, demais → Outflow

</blockquote>
</details>

<details id="AggregatorType">
<summary><strong><a href="../src/FinanceApi.Finance/Domain/Enums/AggregatorType.cs">AggregatorType.cs</a> [enum]</strong></summary>
<blockquote>

- `Pluggy`, `Belvo`

</blockquote>
</details>

</blockquote>
</details>

<details id="dir-finance-application">
<summary><strong>Application/</strong></summary>
<blockquote>

<details id="NotFoundException">
<summary><strong><a href="../src/FinanceApi.Finance/Application/Exceptions/NotFoundException.cs">NotFoundException.cs</a></strong></summary>
<blockquote>

- `NotFoundException` (abstract) → `AccountNotFoundException(Guid)`, `TransactionNotFoundException(Guid)`, `CategoryNotFoundException(Guid)`, `FinancialIntegrationNotFoundException(Guid | string)`

</blockquote>
</details>

- [FinanceDtos.cs](../src/FinanceApi.Finance/Application/Dtos/FinanceDtos.cs) — records: `AccountDto`, `CategoryDto`, `TransactionDto`, `TransactionListWithBalanceDto`, `FinancialIntegrationDto` + requests

<details id="IAccountService">
<summary><strong><a href="../src/FinanceApi.Finance/Application/Interfaces/IAccountService.cs">IAccountService.cs</a> [interface]</strong></summary>
<blockquote>

`CreateAsync` · `FindByIdAsync` · `ListByUserAsync` · `UpdateAsync` · `DeleteAsync` · `LinkToPluggyAsync`

</blockquote>
</details>

<details id="AccountService">
<summary><strong><a href="../src/FinanceApi.Finance/Application/Services/AccountService.cs">AccountService.cs</a> [implements [IAccountService](#IAccountService)]</strong></summary>
<blockquote>

<details><summary>dependencias</summary>

- [FinanceDbContext](#FinanceDbContext)

</details>

</blockquote>
</details>

<details id="ICategoryService">
<summary><strong><a href="../src/FinanceApi.Finance/Application/Interfaces/ICategoryService.cs">ICategoryService.cs</a> [interface]</strong></summary>
<blockquote>

`CreateAsync` (lança InvalidOperationException se duplicata) · `FindByIdAsync` · `ListByUserAsync` · `DeleteAsync`

</blockquote>
</details>

<details id="CategoryService">
<summary><strong><a href="../src/FinanceApi.Finance/Application/Services/CategoryService.cs">CategoryService.cs</a> [implements [ICategoryService](#ICategoryService)]</strong></summary>
<blockquote>

<details><summary>dependencias</summary>

- [FinanceDbContext](#FinanceDbContext)

</details>

</blockquote>
</details>

<details id="ITransactionService">
<summary><strong><a href="../src/FinanceApi.Finance/Application/Interfaces/ITransactionService.cs">ITransactionService.cs</a> [interface]</strong></summary>
<blockquote>

`CreateAsync` · `FindByIdAsync` · `ListByAccountAsync` · `ListByPeriodAsync` · `ListByTypeAsync` · `ListByCategoriesAsync` · `CategorizeAsync` · `ExistsByExternalIdAsync` · `DeleteAsync`

</blockquote>
</details>

<details id="TransactionService">
<summary><strong><a href="../src/FinanceApi.Finance/Application/Services/TransactionService.cs">TransactionService.cs</a> [implements [ITransactionService](#ITransactionService)]</strong></summary>
<blockquote>

<details><summary>dependencias</summary>

- [FinanceDbContext](#FinanceDbContext)

</details>
<details><summary>metodos notaveis</summary>

- `BuildResult(List<Transaction>)` — private; calcula balanço via `TransactionType.Apply()`

</details>

</blockquote>
</details>

<details id="IFinancialIntegrationService">
<summary><strong><a href="../src/FinanceApi.Finance/Application/Interfaces/IFinancialIntegrationService.cs">IFinancialIntegrationService.cs</a> [interface]</strong></summary>
<blockquote>

`CreateAsync` · `FindByIdAsync` · `FindByLinkIdAsync` · `ListByUserAsync` · `DeleteAsync`

</blockquote>
</details>

<details id="FinancialIntegrationService">
<summary><strong><a href="../src/FinanceApi.Finance/Application/Services/FinancialIntegrationService.cs">FinancialIntegrationService.cs</a> [implements [IFinancialIntegrationService](#IFinancialIntegrationService)]</strong></summary>
<blockquote>

<details><summary>dependencias</summary>

- [FinanceDbContext](#FinanceDbContext)

</details>

</blockquote>
</details>

</blockquote>
</details>

<details id="dir-finance-infra">
<summary><strong>Infrastructure/</strong></summary>
<blockquote>

<details id="FinanceDbContext">
<summary><strong><a href="../src/FinanceApi.Finance/Infrastructure/Persistence/FinanceDbContext.cs">FinanceDbContext.cs</a> [DbContext]</strong></summary>
<blockquote>

<details><summary>atributos</summary>

- `Accounts`, `Categories`, `Transactions`, `FinancialIntegrations` — DbSets

</details>
<details><summary>configuracao</summary>

- Schema padrão: `finance`
- `PluggyAccountId` unique (filtrado para não nulos)
- `ExternalId` unique (filtrado para não nulos)
- `TransactionType` e `AggregatorType` armazenados como string
- Category: unique constraint (Name, UserId)

</details>

</blockquote>
</details>

- [FinanceDbContextFactory.cs](../src/FinanceApi.Finance/Infrastructure/Persistence/FinanceDbContextFactory.cs) — design-time factory para migrations
- `Migrations/` — migration `InitialCreate` (schema: finance)

<details id="IUserContext">
<summary><strong><a href="../src/FinanceApi.Finance/Infrastructure/Http/IUserContext.cs">IUserContext.cs</a> [interface]</strong></summary>
<blockquote>

- `UserId: Guid`, `IsAdmin: bool`

</blockquote>
</details>

<details id="UserContext">
<summary><strong><a href="../src/FinanceApi.Finance/Infrastructure/Http/UserContext.cs">UserContext.cs</a> [implements [IUserContext](#IUserContext)]</strong></summary>
<blockquote>

Lê `X-User-Id` e `X-User-Role` dos headers HTTP injetados pelo gateway. Lança `UnauthorizedAccessException` se header ausente.

</blockquote>
</details>

</blockquote>
</details>

<details id="dir-finance-api">
<summary><strong>Api/</strong></summary>
<blockquote>

<details id="FinanceErrorFilter">
<summary><strong><a href="../src/FinanceApi.Finance/Api/GraphQL/Errors/FinanceErrorFilter.cs">FinanceErrorFilter.cs</a> [IErrorFilter]</strong></summary>
<blockquote>

Converte exceções em erros GraphQL tipados: `NotFoundException` → NOT_FOUND, `InvalidOperationException` → CONFLICT, `UnauthorizedAccessException` → UNAUTHORIZED.

</blockquote>
</details>

**Queries** (`[ExtendObjectType(Query)]`):
- [AccountQueries.cs](../src/FinanceApi.Finance/Api/GraphQL/Queries/AccountQueries.cs) — `account(id)`, `accounts` (scoped ao user via header)
- [CategoryQueries.cs](../src/FinanceApi.Finance/Api/GraphQL/Queries/CategoryQueries.cs) — `category(id)`, `categories`
- [TransactionQueries.cs](../src/FinanceApi.Finance/Api/GraphQL/Queries/TransactionQueries.cs) — `transaction(id)`, `transactions(accountId)`, `transactionsByPeriod`, `transactionsByType`, `transactionsByCategories`
- [FinancialIntegrationQueries.cs](../src/FinanceApi.Finance/Api/GraphQL/Queries/FinancialIntegrationQueries.cs) — `financialIntegration(id)`, `financialIntegrations`

**Mutations** (`[ExtendObjectType(Mutation)]`):
- [AccountMutations.cs](../src/FinanceApi.Finance/Api/GraphQL/Mutations/AccountMutations.cs) — `createAccount`, `updateAccount`, `deleteAccount`, `linkAccount`
- [CategoryMutations.cs](../src/FinanceApi.Finance/Api/GraphQL/Mutations/CategoryMutations.cs) — `createCategory`, `deleteCategory`
- [TransactionMutations.cs](../src/FinanceApi.Finance/Api/GraphQL/Mutations/TransactionMutations.cs) — `createTransaction`, `updateTransaction`, `categorizeTransaction`, `deleteTransaction`
- [FinancialIntegrationMutations.cs](../src/FinanceApi.Finance/Api/GraphQL/Mutations/FinancialIntegrationMutations.cs) — `createFinancialIntegration`, `deleteFinancialIntegration`

- `Controllers/` — ImportController (POST /import, OFX) _(a implementar)_

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
