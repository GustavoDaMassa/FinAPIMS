FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY FinanceApi.sln .
COPY src/FinanceApi.Gateway/FinanceApi.Gateway.csproj src/FinanceApi.Gateway/
COPY src/FinanceApi.Identity/FinanceApi.Identity.csproj src/FinanceApi.Identity/
COPY src/FinanceApi.Finance/FinanceApi.Finance.csproj src/FinanceApi.Finance/
COPY src/FinanceApi.Webhook/FinanceApi.Webhook.csproj src/FinanceApi.Webhook/
COPY shared/FinanceApi.Shared.Contracts/FinanceApi.Shared.Contracts.csproj shared/FinanceApi.Shared.Contracts/
COPY tests/FinanceApi.Identity.Tests/FinanceApi.Identity.Tests.csproj tests/FinanceApi.Identity.Tests/
COPY tests/FinanceApi.Finance.Tests/FinanceApi.Finance.Tests.csproj tests/FinanceApi.Finance.Tests/
COPY tests/FinanceApi.Webhook.Tests/FinanceApi.Webhook.Tests.csproj tests/FinanceApi.Webhook.Tests/
RUN dotnet restore src/FinanceApi.Webhook/FinanceApi.Webhook.csproj

COPY . .
RUN dotnet publish src/FinanceApi.Webhook/FinanceApi.Webhook.csproj -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "FinanceApi.Webhook.dll"]
