# FinanceApiMS — Mapa de Classes

Mono-repo C# / .NET 8. Solução com 4 projetos `src/`, 1 `shared/` e 3 `tests/`.

---

<details id="dir-root">
<summary><strong>/ (raiz)</strong></summary>
<blockquote>

- [FinanceApi.sln](../FinanceApi.sln) — solution com 8 projetos
- [docker-compose.yml](../docker-compose.yml) — ambiente local (build das imagens)
- [docker-compose.prod.yml](../docker-compose.prod.yml) — produção (imagens Docker Hub + Watchtower + Nginx)
- [.env.example](../.env.example) — variáveis necessárias (DATABASE_PASSWORD, JWT_SECRET, MASTER_KEY, PLUGGY_*)

</blockquote>
</details>

<details id="dir-nginx">
<summary><strong>nginx/</strong></summary>
<blockquote>

- [nginx/nginx.prod.conf](../nginx/nginx.prod.conf) — reverse proxy HTTP→gateway:8080 (SSL via Cloudflare)

</blockquote>
</details>

---

## shared/

<details id="dir-shared-contracts">
<summary><strong>shared/FinanceApi.Shared.Contracts/Events/</strong></summary>
<blockquote>

<details id="WebhookEvent">
<summary><strong><a href="../shared/FinanceApi.Shared.Contracts/Events/WebhookEvent.cs">WebhookEvent.cs</a> [record]</strong></summary>
<blockquote>

<details><summary>tipos</summary>

- `record WebhookEvent(string LinkId, IReadOnlyList<ExternalTransaction> Transactions)` — evento publicado pelo webhook-service após buscar as transações no Pluggy
- `record ExternalTransaction(string ExternalId, decimal Amount, string PluggyType, string? Description, DateOnly Date, string PluggyAccountId)` — transação crua do Pluggy; `PluggyType` é `"CREDIT"` ou `"DEBIT"`

</details>

</blockquote>
</details>

</blockquote>
</details>

---

## src/FinanceApi.Gateway

<details id="dir-gateway-infra">
<summary><strong>src/FinanceApi.Gateway/Infrastructure/</strong></summary>
<blockquote>

<details id="UserHeaderTransformProvider">
<summary><strong><a href="../src/FinanceApi.Gateway/Infrastructure/UserHeaderTransform.cs">UserHeaderTransform.cs</a> [ITransformProvider]</strong></summary>
<blockquote>

<details><summary>implements</summary>

- `ITransformProvider` (YARP)

</details>
<details><summary>funcao</summary>

Injetado no pipeline YARP. Para cada request autenticado, extrai `sub`/`ClaimTypes.NameIdentifier` e `role`/`ClaimTypes.Role` do JWT e os propaga como headers `X-User-Id` e `X-User-Role` para os serviços downstream.

</details>
<details><summary>metodos</summary>

- `Apply(TransformBuilderContext)` — registra transform que injeta headers por request

</details>

</blockquote>
</details>

</blockquote>
</details>

- [src/FinanceApi.Gateway/Program.cs](../src/FinanceApi.Gateway/Program.cs) — configura YARP com JWT Bearer validation + `.AddTransforms<UserHeaderTransformProvider>()`
- [src/FinanceApi.Gateway/Dockerfile](../src/FinanceApi.Gateway/Dockerfile)
- [src/FinanceApi.Gateway/appsettings.json](../src/FinanceApi.Gateway/appsettings.json) — `ReverseProxy` routes + clusters

---

## src/FinanceApi.Identity

<details id="dir-identity-domain">
<summary><strong>src/FinanceApi.Identity/Domain/</strong></summary>
<blockquote>

<details id="Role">
<summary><strong><a href="../src/FinanceApi.Identity/Domain/Enums/Role.cs">Role.cs</a> [enum]</strong></summary>
<blockquote>

- `User`, `Admin`

</blockquote>
</details>

<details id="IdentityUser">
<summary><strong><a href="../src/FinanceApi.Identity/Domain/Models/User.cs">User.cs</a> [entity]</strong></summary>
<blockquote>

<details><summary>atributos</summary>

- `Guid Id` — private set
- `string Name` — private set
- `string Email` — private set
- `string PasswordHash` — private set
- `Role Role` — private set
- `DateTime CreatedAt` — private set

</details>
<details><summary>metodos</summary>

- `static Create(name, email, passwordHash, role) : User` — factory method; `Id = Guid.NewGuid()`

</details>

</blockquote>
</details>

</blockquote>
</details>

<details id="dir-identity-app">
<summary><strong>src/FinanceApi.Identity/Application/</strong></summary>
<blockquote>

<details id="IAuthService">
<summary><strong><a href="../src/FinanceApi.Identity/Application/Interfaces/IAuthService.cs">IAuthService.cs</a> [interface]</strong></summary>
<blockquote>

- `RegisterAsync(RegisterRequest) : Task<AuthResponse>`
- `LoginAsync(LoginRequest) : Task<AuthResponse>`
- `CreateAdminAsync(CreateAdminRequest) : Task<AuthResponse>`

</blockquote>
</details>

<details id="AuthService">
<summary><strong><a href="../src/FinanceApi.Identity/Application/Services/AuthService.cs">AuthService.cs</a></strong></summary>
<blockquote>

<details><summary>implements</summary>

- [IAuthService](#IAuthService)

</details>
<details><summary>dependencias</summary>

- `IdentityDbContext`
- `JwtService`
- `BCrypt` (BCrypt.Net-Next)

</details>
<details><summary>metodos</summary>

- `RegisterAsync` — verifica e-mail duplicado (409), cria [User](#IdentityUser), persiste, emite JWT
- `LoginAsync` — verifica e-mail + hash (401), emite JWT
- `CreateAdminAsync` — requer `MasterKey` (401), cria usuário com `Role.Admin`
- `BuildResponse(User) : AuthResponse` — helper privado

</details>

</blockquote>
</details>

- [src/FinanceApi.Identity/Api/Dtos/AuthDtos.cs](../src/FinanceApi.Identity/Api/Dtos/AuthDtos.cs) — `RegisterRequest`, `LoginRequest`, `CreateAdminRequest`, `AuthResponse`

</blockquote>
</details>

<details id="dir-identity-infra">
<summary><strong>src/FinanceApi.Identity/Infrastructure/</strong></summary>
<blockquote>

<details id="JwtService">
<summary><strong><a href="../src/FinanceApi.Identity/Infrastructure/Security/JwtService.cs">JwtService.cs</a></strong></summary>
<blockquote>

<details><summary>funcao</summary>

Gera tokens JWT (HS256). Claims: `sub` (Guid), `email`, `ClaimTypes.Role`, `jti` (Guid).

</details>

</blockquote>
</details>

<details id="IdentityDbContext">
<summary><strong><a href="../src/FinanceApi.Identity/Infrastructure/Persistence/IdentityDbContext.cs">IdentityDbContext.cs</a> [DbContext]</strong></summary>
<blockquote>

- Schema: `identity`
- `DbSet<User> Users`

</blockquote>
</details>

- [IdentityDbContextFactory.cs](../src/FinanceApi.Identity/Infrastructure/Persistence/IdentityDbContextFactory.cs) — `IDesignTimeDbContextFactory` para migrations
- [Migrations/20260403210246_InitialCreate.cs](../src/FinanceApi.Identity/Infrastructure/Persistence/Migrations/20260403210246_InitialCreate.cs)

</blockquote>
</details>

<details id="AuthController">
<summary><strong><a href="../src/FinanceApi.Identity/Api/Controllers/AuthController.cs">AuthController.cs</a> [ApiController]</strong></summary>
<blockquote>

<details><summary>dependencias</summary>

- [IAuthService](#IAuthService)

</details>
<details><summary>endpoints</summary>

- `POST /auth/register` → 200 / 409
- `POST /auth/login` → 200 / 401
- `POST /auth/admin` → 200 / 401 / 409

</details>

</blockquote>
</details>

- [src/FinanceApi.Identity/Program.cs](../src/FinanceApi.Identity/Program.cs)
- [src/FinanceApi.Identity/Dockerfile](../src/FinanceApi.Identity/Dockerfile)

---

## src/FinanceApi.Finance

<details id="dir-finance-domain">
<summary><strong>src/FinanceApi.Finance/Domain/</strong></summary>
<blockquote>

<details id="TransactionType">
<summary><strong><a href="../src/FinanceApi.Finance/Domain/Enums/TransactionType.cs">TransactionType.cs</a> [enum + extension]</strong></summary>
<blockquote>

- `enum TransactionType { Inflow, Outflow }`
- `static class TransactionTypeExtensions`
  - `Apply(this TransactionType, decimal) : decimal` — Enum Strategy: `Inflow → +amount`, `Outflow → -amount`
  - `FromPluggy(string pluggyType) : TransactionType` — `"CREDIT"→Inflow`, outros→`Outflow`

</blockquote>
</details>

<details id="AggregatorType">
<summary><strong><a href="../src/FinanceApi.Finance/Domain/Enums/AggregatorType.cs">AggregatorType.cs</a> [enum]</strong></summary>
<blockquote>

- `Pluggy`

</blockquote>
</details>

<details id="Account">
<summary><strong><a href="../src/FinanceApi.Finance/Domain/Models/Account.cs">Account.cs</a> [entity]</strong></summary>
<blockquote>

<details><summary>atributos</summary>

- `Guid Id`, `string AccountName`, `string Institution`, `string? Description`
- `decimal Balance`, `string? PluggyAccountId`
- `Guid UserId`, `Guid? IntegrationId`
- `ICollection<Transaction> Transactions`

</details>
<details><summary>metodos</summary>

- `static Create(...) : Account` — factory method
- `Update(accountName, institution, description)` — campos editáveis
- `UpdateBalance(decimal)` — chamado pelo service ao recalcular saldo
- `LinkToPluggy(pluggyAccountId, integrationId)` — vincula à integração Pluggy

</details>

</blockquote>
</details>

<details id="Transaction">
<summary><strong><a href="../src/FinanceApi.Finance/Domain/Models/Transaction.cs">Transaction.cs</a> [entity]</strong></summary>
<blockquote>

<details><summary>atributos</summary>

- `Guid Id`, `decimal Amount`, `TransactionType Type`
- `string? Description`, `string? Source`, `string? Destination`
- `DateOnly TransactionDate`, `string? ExternalId`
- `Guid AccountId`, `Guid? CategoryId`
- `Account Account`, `Category? Category` (nav)

</details>
<details><summary>metodos</summary>

- `static Create(...) : Transaction` — factory method; `ExternalId` opcional (OFX/Pluggy)
- `Update(amount, type, description, source, destination, transactionDate, categoryId)` — mutação in-place; `AccountId` e `ExternalId` imutáveis
- `Categorize(Guid?)` — atribui ou limpa categoria

</details>

</blockquote>
</details>

<details id="Category">
<summary><strong><a href="../src/FinanceApi.Finance/Domain/Models/Category.cs">Category.cs</a> [entity]</strong></summary>
<blockquote>

<details><summary>atributos</summary>

- `Guid Id`, `string Name`, `Guid UserId`

</details>
<details><summary>metodos</summary>

- `static Create(name, userId) : Category`
- `Update(name)`

</details>

</blockquote>
</details>

<details id="FinancialIntegration">
<summary><strong><a href="../src/FinanceApi.Finance/Domain/Models/FinancialIntegration.cs">FinancialIntegration.cs</a> [entity]</strong></summary>
<blockquote>

<details><summary>atributos</summary>

- `Guid Id`, `AggregatorType Aggregator`, `string LinkId`
- `string? Status`, `DateTime CreatedAt`, `DateTime? ExpiresAt`
- `Guid UserId`, `ICollection<Account> Accounts`

</details>
<details><summary>metodos</summary>

- `static Create(aggregator, linkId, userId) : FinancialIntegration`
- `UpdateStatus(string)`, `Renew()` — renova ExpiresAt +12 meses

</details>

</blockquote>
</details>

</blockquote>
</details>

<details id="dir-finance-exceptions">
<summary><strong>src/FinanceApi.Finance/Application/Exceptions/</strong></summary>
<blockquote>

<details id="NotFoundException">
<summary><strong><a href="../src/FinanceApi.Finance/Application/Exceptions/NotFoundException.cs">NotFoundException.cs</a></strong></summary>
<blockquote>

Hierarquia:
- `abstract class NotFoundException : Exception` — base
  - `AccountNotFoundException(Guid id)`
  - `TransactionNotFoundException(Guid id)`
  - `CategoryNotFoundException(Guid id)`
  - `FinancialIntegrationNotFoundException` — 2 construtores: `(Guid id)` e `(string linkId)`

</blockquote>
</details>

</blockquote>
</details>

<details id="dir-finance-interfaces">
<summary><strong>src/FinanceApi.Finance/Application/Interfaces/</strong></summary>
<blockquote>

<details id="IAccountService">
<summary><strong><a href="../src/FinanceApi.Finance/Application/Interfaces/IAccountService.cs">IAccountService.cs</a> [interface]</strong></summary>
<blockquote>

- `CreateAsync`, `FindByIdAsync`, `ListByUserAsync`, `UpdateAsync`, `DeleteAsync`
- `LinkToPluggyAsync(LinkAccountRequest)`

</blockquote>
</details>

<details id="ICategoryService">
<summary><strong><a href="../src/FinanceApi.Finance/Application/Interfaces/ICategoryService.cs">ICategoryService.cs</a> [interface]</strong></summary>
<blockquote>

- `CreateAsync`, `FindByIdAsync`, `ListByUserAsync`, `UpdateAsync`, `DeleteAsync`

</blockquote>
</details>

<details id="ITransactionService">
<summary><strong><a href="../src/FinanceApi.Finance/Application/Interfaces/ITransactionService.cs">ITransactionService.cs</a> [interface]</strong></summary>
<blockquote>

- `CreateAsync(CreateTransactionRequest)`
- `FindByIdAsync(Guid)`, `DeleteAsync(Guid)`
- `UpdateAsync(Guid, UpdateTransactionRequest)`
- `CategorizeAsync(Guid, Guid?)`
- `ExistsByExternalIdAsync(string)` — usado por deduplicação OFX e Kafka consumer
- `ListByAccountAsync`, `ListByPeriodAsync`, `ListByTypeAsync`, `ListByCategoriesAsync`

</blockquote>
</details>

<details id="IFinancialIntegrationService">
<summary><strong><a href="../src/FinanceApi.Finance/Application/Interfaces/IFinancialIntegrationService.cs">IFinancialIntegrationService.cs</a> [interface]</strong></summary>
<blockquote>

- `CreateAsync`, `FindByIdAsync`, `ListByUserAsync`, `DeleteAsync`

</blockquote>
</details>

<details id="IOfxImportService">
<summary><strong><a href="../src/FinanceApi.Finance/Application/Interfaces/IOfxImportService.cs">IOfxImportService.cs</a> [interface]</strong></summary>
<blockquote>

- `ImportAsync(Stream ofxStream, Guid accountId) : Task<OfxImportResult>`

</blockquote>
</details>

</blockquote>
</details>

<details id="dir-finance-services">
<summary><strong>src/FinanceApi.Finance/Application/Services/</strong></summary>
<blockquote>

<details id="AccountService">
<summary><strong><a href="../src/FinanceApi.Finance/Application/Services/AccountService.cs">AccountService.cs</a></strong></summary>
<blockquote>

<details><summary>implements</summary>[IAccountService](#IAccountService)</details>
<details><summary>dependencias</summary>

- `FinanceDbContext`, `IUserContext`

</details>

</blockquote>
</details>

<details id="CategoryService">
<summary><strong><a href="../src/FinanceApi.Finance/Application/Services/CategoryService.cs">CategoryService.cs</a></strong></summary>
<blockquote>

<details><summary>implements</summary>[ICategoryService](#ICategoryService)</details>
<details><summary>dependencias</summary>

- `FinanceDbContext`, `IUserContext`

</details>

</blockquote>
</details>

<details id="TransactionService">
<summary><strong><a href="../src/FinanceApi.Finance/Application/Services/TransactionService.cs">TransactionService.cs</a></strong></summary>
<blockquote>

<details><summary>implements</summary>[ITransactionService](#ITransactionService)</details>
<details><summary>dependencias</summary>

- `FinanceDbContext`

</details>
<details><summary>funcao</summary>

`UpdateAsync` chama `transaction.Update()` in-place — preserva `Id`, `AccountId` e `ExternalId`. `BuildResult` calcula balance via `TransactionType.Apply()` (Enum Strategy).

</details>

</blockquote>
</details>

<details id="FinancialIntegrationService">
<summary><strong><a href="../src/FinanceApi.Finance/Application/Services/FinancialIntegrationService.cs">FinancialIntegrationService.cs</a></strong></summary>
<blockquote>

<details><summary>implements</summary>[IFinancialIntegrationService](#IFinancialIntegrationService)</details>
<details><summary>dependencias</summary>

- `FinanceDbContext`, `IUserContext`

</details>

</blockquote>
</details>

<details id="OfxImportService">
<summary><strong><a href="../src/FinanceApi.Finance/Application/Services/OfxImportService.cs">OfxImportService.cs</a></strong></summary>
<blockquote>

<details><summary>implements</summary>[IOfxImportService](#IOfxImportService)</details>
<details><summary>dependencias</summary>

- [ITransactionService](#ITransactionService), `FinanceDbContext`, [OfxParser](#OfxParser)

</details>
<details><summary>funcao</summary>

Lê o stream OFX, parseia via `OfxParser`, itera as linhas. Para cada `FITID` verifica `ExistsByExternalIdAsync` — se já existe, incrementa `skipped`. Caso contrário cria via `ITransactionService.CreateAsync` com `ExternalId = FITID`. Retorna `OfxImportResult(imported, skipped, transactions)`.

</details>

</blockquote>
</details>

- [src/FinanceApi.Finance/Application/Dtos/FinanceDtos.cs](../src/FinanceApi.Finance/Application/Dtos/FinanceDtos.cs) — `AccountDto`, `CategoryDto`, `TransactionDto`, `TransactionListWithBalanceDto`, `FinancialIntegrationDto`, todos os `*Request`
- [src/FinanceApi.Finance/Application/Dtos/OfxDtos.cs](../src/FinanceApi.Finance/Application/Dtos/OfxDtos.cs) — `record OfxImportResult(int Imported, int Skipped, IReadOnlyList<TransactionDto> Transactions)`

</blockquote>
</details>

<details id="dir-finance-infra">
<summary><strong>src/FinanceApi.Finance/Infrastructure/</strong></summary>
<blockquote>

<details id="IUserContext">
<summary><strong><a href="../src/FinanceApi.Finance/Infrastructure/Http/IUserContext.cs">IUserContext.cs</a> [interface]</strong></summary>
<blockquote>

- `Guid UserId { get; }` — lança `UnauthorizedAccessException` se header ausente/inválido
- `bool IsAdmin { get; }`

</blockquote>
</details>

<details id="UserContext">
<summary><strong><a href="../src/FinanceApi.Finance/Infrastructure/Http/UserContext.cs">UserContext.cs</a></strong></summary>
<blockquote>

<details><summary>implements</summary>[IUserContext](#IUserContext)</details>
<details><summary>dependencias</summary>

- `IHttpContextAccessor`

</details>
<details><summary>funcao</summary>

Lê `X-User-Id` e `X-User-Role` dos headers HTTP injetados pelo gateway. Não valida JWT — confia no gateway.

</details>

</blockquote>
</details>

<details id="WebhookEventConsumer">
<summary><strong><a href="../src/FinanceApi.Finance/Infrastructure/Kafka/WebhookEventConsumer.cs">WebhookEventConsumer.cs</a> [BackgroundService]</strong></summary>
<blockquote>

<details><summary>extends</summary>

- `BackgroundService`

</details>
<details><summary>dependencias</summary>

- `IConfiguration`, `IServiceScopeFactory`, `ILogger`

</details>
<details><summary>funcao</summary>

Consumer Kafka singleton. Assina tópico `webhook.events`. Por mensagem: abre novo scope DI via `IServiceScopeFactory`, resolve [WebhookEventHandler](#WebhookEventHandler), chama `HandleAsync`. Commit manual após processamento.

</details>

</blockquote>
</details>

<details id="WebhookEventHandler">
<summary><strong><a href="../src/FinanceApi.Finance/Infrastructure/Kafka/WebhookEventHandler.cs">WebhookEventHandler.cs</a></strong></summary>
<blockquote>

<details><summary>dependencias</summary>

- `FinanceDbContext`, [ITransactionService](#ITransactionService)

</details>
<details><summary>funcao</summary>

Para cada [WebhookEvent](#WebhookEvent): resolve `FinancialIntegration` pelo `LinkId`, itera `ExternalTransaction`. Pula se `ExistsByExternalIdAsync`. Busca `Account` pelo `PluggyAccountId + UserId`. Converte `PluggyType` via `TransactionTypeExtensions.FromPluggy`. Cria transação via `ITransactionService.CreateAsync`.

</details>

</blockquote>
</details>

<details id="OfxParser">
<summary><strong><a href="../src/FinanceApi.Finance/Infrastructure/Ofx/OfxParser.cs">OfxParser.cs</a></strong></summary>
<blockquote>

<details><summary>funcao</summary>

Parseia OFX V1 (SGML, tags sem fechamento) e V2 (XML com PI `<?OFX ... ?>`). Regex sobre blocos `<STMTTRN>`. Extrai `FITID`, `TRNAMT`, `DTPOSTED`, `MEMO`/`NAME`. Valor negativo → `Outflow`; positivo → `Inflow`. Data: primeiros 8 chars de `YYYYMMDD[...]`. Registrado como `Singleton`.

</details>
<details><summary>metodos</summary>

- `Parse(string content) : IReadOnlyList<OfxTransactionRow>`

</details>

</blockquote>
</details>

<details id="OfxTransactionRow">
<summary><strong><a href="../src/FinanceApi.Finance/Infrastructure/Ofx/OfxTransactionRow.cs">OfxTransactionRow.cs</a> [record]</strong></summary>
<blockquote>

- `record OfxTransactionRow(string FitId, decimal Amount, TransactionType Type, DateOnly Date, string? Description)`

</blockquote>
</details>

<details id="FinanceDbContext">
<summary><strong><a href="../src/FinanceApi.Finance/Infrastructure/Persistence/FinanceDbContext.cs">FinanceDbContext.cs</a> [DbContext]</strong></summary>
<blockquote>

- Schema: `finance`
- `DbSet<Account>`, `DbSet<Transaction>`, `DbSet<Category>`, `DbSet<FinancialIntegration>`
- Indexes parciais únicos: `PluggyAccountId` (nullable) e `ExternalId` (nullable)

</blockquote>
</details>

- [FinanceDbContextFactory.cs](../src/FinanceApi.Finance/Infrastructure/Persistence/FinanceDbContextFactory.cs) — `IDesignTimeDbContextFactory`
- [Migrations/20260403212405_InitialCreate.cs](../src/FinanceApi.Finance/Infrastructure/Persistence/Migrations/20260403212405_InitialCreate.cs)

</blockquote>
</details>

<details id="dir-finance-graphql">
<summary><strong>src/FinanceApi.Finance/Api/GraphQL/</strong></summary>
<blockquote>

<details><summary><strong>Queries/</strong></summary>
<blockquote>

- [AccountQueries.cs](../src/FinanceApi.Finance/Api/GraphQL/Queries/AccountQueries.cs) — `[ExtendObjectType(Query)]`: `account(id)`, `accounts`
- [CategoryQueries.cs](../src/FinanceApi.Finance/Api/GraphQL/Queries/CategoryQueries.cs) — `category(id)`, `categories`
- [TransactionQueries.cs](../src/FinanceApi.Finance/Api/GraphQL/Queries/TransactionQueries.cs) — `transaction(id)`, `transactionsByAccount`, `transactionsByPeriod`, `transactionsByType`, `transactionsByCategories`
- [FinancialIntegrationQueries.cs](../src/FinanceApi.Finance/Api/GraphQL/Queries/FinancialIntegrationQueries.cs) — `financialIntegration(id)`, `financialIntegrations`

</blockquote>
</details>

<details><summary><strong>Mutations/</strong></summary>
<blockquote>

- [AccountMutations.cs](../src/FinanceApi.Finance/Api/GraphQL/Mutations/AccountMutations.cs) — `createAccount`, `updateAccount`, `deleteAccount`, `linkAccountToPluggy`
- [CategoryMutations.cs](../src/FinanceApi.Finance/Api/GraphQL/Mutations/CategoryMutations.cs) — `createCategory`, `updateCategory`, `deleteCategory`
- [TransactionMutations.cs](../src/FinanceApi.Finance/Api/GraphQL/Mutations/TransactionMutations.cs) — `createTransaction`, `updateTransaction` (usa `UpdateAsync`), `categorizeTransaction`, `deleteTransaction`
- [FinancialIntegrationMutations.cs](../src/FinanceApi.Finance/Api/GraphQL/Mutations/FinancialIntegrationMutations.cs) — `createFinancialIntegration`, `deleteFinancialIntegration`

</blockquote>
</details>

- [Errors/FinanceErrorFilter.cs](../src/FinanceApi.Finance/Api/GraphQL/Errors/FinanceErrorFilter.cs) — mapeia `NotFoundException` → GraphQL error com code `NOT_FOUND`

</blockquote>
</details>

<details id="ImportController">
<summary><strong><a href="../src/FinanceApi.Finance/Api/Controllers/ImportController.cs">ImportController.cs</a> [ApiController]</strong></summary>
<blockquote>

<details><summary>dependencias</summary>

- [IOfxImportService](#IOfxImportService)

</details>
<details><summary>endpoints</summary>

- `POST /import/ofx?accountId={guid}` — `multipart/form-data`, campo `file`
  - 200: `OfxImportResult`
  - 400: sem arquivo
  - 404: conta não encontrada

</details>

</blockquote>
</details>

- [src/FinanceApi.Finance/Program.cs](../src/FinanceApi.Finance/Program.cs)
- [src/FinanceApi.Finance/Dockerfile](../src/FinanceApi.Finance/Dockerfile)

---

## src/FinanceApi.Webhook

<details id="dir-webhook-app">
<summary><strong>src/FinanceApi.Webhook/Application/</strong></summary>
<blockquote>

<details id="IPluggyClient">
<summary><strong><a href="../src/FinanceApi.Webhook/Application/Interfaces/IPluggyClient.cs">IPluggyClient.cs</a> [interface]</strong></summary>
<blockquote>

- `GetApiKeyAsync() : Task<string>`
- `FetchTransactionsAsync(string transactionsUrl) : Task<IReadOnlyList<ExternalTransaction>>`

</blockquote>
</details>

<details id="IWebhookEventProducer">
<summary><strong><a href="../src/FinanceApi.Webhook/Application/Interfaces/IWebhookEventProducer.cs">IWebhookEventProducer.cs</a> [interface]</strong></summary>
<blockquote>

- `PublishAsync(WebhookEvent) : Task`

</blockquote>
</details>

<details id="PluggyClient">
<summary><strong><a href="../src/FinanceApi.Webhook/Application/Services/PluggyClient.cs">PluggyClient.cs</a></strong></summary>
<blockquote>

<details><summary>implements</summary>[IPluggyClient](#IPluggyClient)</details>
<details><summary>funcao</summary>

`GetApiKeyAsync` — POST `/auth` na Pluggy API com `clientId`+`clientSecret`, retorna `apiKey`. `FetchTransactionsAsync` — GET com header `X-API-KEY`, desserializa lista de `ExternalTransaction`.

</details>

</blockquote>
</details>

- [src/FinanceApi.Webhook/Application/Dtos/WebhookDtos.cs](../src/FinanceApi.Webhook/Application/Dtos/WebhookDtos.cs) — DTOs de request/response Pluggy

</blockquote>
</details>

<details id="dir-webhook-infra">
<summary><strong>src/FinanceApi.Webhook/Infrastructure/Kafka/</strong></summary>
<blockquote>

<details id="WebhookEventProducer">
<summary><strong><a href="../src/FinanceApi.Webhook/Infrastructure/Kafka/WebhookEventProducer.cs">WebhookEventProducer.cs</a></strong></summary>
<blockquote>

<details><summary>implements</summary>

- [IWebhookEventProducer](#IWebhookEventProducer), `IDisposable`

</details>
<details><summary>funcao</summary>

Producer Kafka. Serializa [WebhookEvent](#WebhookEvent) como JSON e publica no tópico `webhook.events`. `IProducer` criado no construtor e descartado no `Dispose`.

</details>

</blockquote>
</details>

</blockquote>
</details>

<details id="PluggyWebhookController">
<summary><strong><a href="../src/FinanceApi.Webhook/Api/Controllers/PluggyWebhookController.cs">PluggyWebhookController.cs</a> [ApiController]</strong></summary>
<blockquote>

<details><summary>dependencias</summary>

- [IPluggyClient](#IPluggyClient), [IWebhookEventProducer](#IWebhookEventProducer)

</details>
<details><summary>endpoints</summary>

- `GET /webhook/pluggy` — health check
- `POST /webhook/pluggy` — recebe payload Pluggy, busca transações via `IPluggyClient`, publica [WebhookEvent](#WebhookEvent) enriquecido via `IWebhookEventProducer`

</details>

</blockquote>
</details>

- [src/FinanceApi.Webhook/Program.cs](../src/FinanceApi.Webhook/Program.cs)
- [src/FinanceApi.Webhook/Dockerfile](../src/FinanceApi.Webhook/Dockerfile)

---

## tests/

<details id="dir-finance-tests">
<summary><strong>tests/FinanceApi.Finance.Tests/</strong></summary>
<blockquote>

- [TestHelpers.cs](../tests/FinanceApi.Finance.Tests/TestHelpers.cs) — `CreateDb()` (InMemory), `UserId`, `OtherUserId`
- [AccountServiceTests.cs](../tests/FinanceApi.Finance.Tests/AccountServiceTests.cs)
- [CategoryServiceTests.cs](../tests/FinanceApi.Finance.Tests/CategoryServiceTests.cs)
- [TransactionServiceTests.cs](../tests/FinanceApi.Finance.Tests/TransactionServiceTests.cs) — inclui `UpdateAsync` (TDD Red→Green)
- [FinancialIntegrationServiceTests.cs](../tests/FinanceApi.Finance.Tests/FinancialIntegrationServiceTests.cs)
- [WebhookEventConsumerTests.cs](../tests/FinanceApi.Finance.Tests/WebhookEventConsumerTests.cs)
- [OfxParserTests.cs](../tests/FinanceApi.Finance.Tests/OfxParserTests.cs) — V1 SGML, V2 XML, tipos, datas, MEMO/NAME
- [OfxImportServiceTests.cs](../tests/FinanceApi.Finance.Tests/OfxImportServiceTests.cs) — import, deduplicação, conta inexistente
- [GlobalUsings.cs](../tests/FinanceApi.Finance.Tests/GlobalUsings.cs)

**Total: 54 testes**

</blockquote>
</details>

<details id="dir-identity-tests">
<summary><strong>tests/FinanceApi.Identity.Tests/</strong></summary>
<blockquote>

- [AuthServiceTests.cs](../tests/FinanceApi.Identity.Tests/AuthServiceTests.cs) — register, login, admin, duplicates

**Total: 7 testes**

</blockquote>
</details>

<details id="dir-webhook-tests">
<summary><strong>tests/FinanceApi.Webhook.Tests/</strong></summary>
<blockquote>

- [PluggyWebhookControllerTests.cs](../tests/FinanceApi.Webhook.Tests/PluggyWebhookControllerTests.cs) — health check, publish flow, Pluggy error handling

**Total: 3 testes**

</blockquote>
</details>

---

## Fluxos principais

### Auth (REST)
`Client → Nginx → Gateway (valida JWT) → identity-service`

### Finance (GraphQL)
`Client → Nginx → Gateway (valida JWT, injeta X-User-Id/X-User-Role) → finance-service → FinanceDbContext`

### OFX Import (REST)
`Client → POST /import/ofx → ImportController → OfxImportService → OfxParser → ITransactionService`

### Webhook Pluggy (async)
`Pluggy → POST /webhook/pluggy → PluggyWebhookController → IPluggyClient (busca txs) → IWebhookEventProducer → Kafka`
`Kafka → WebhookEventConsumer (BackgroundService) → WebhookEventHandler → ITransactionService`
