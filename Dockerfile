FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Packages.props .
COPY PremierBankTesting.sln .
COPY src/PremierBankTesting.Domain/PremierBankTesting.Domain.csproj src/PremierBankTesting.Domain/
COPY src/PremierBankTesting.Contracts/PremierBankTesting.Contracts.csproj src/PremierBankTesting.Contracts/
COPY src/PremierBankTesting.Application/PremierBankTesting.Application.csproj src/PremierBankTesting.Application/
COPY src/PremierBankTesting.Infrastructure/PremierBankTesting.Infrastructure.csproj src/PremierBankTesting.Infrastructure/
COPY src/PremierBankTesting.Api/PremierBankTesting.Api.csproj src/PremierBankTesting.Api/

RUN dotnet restore src/PremierBankTesting.Api/PremierBankTesting.Api.csproj

COPY src/ src/

RUN dotnet publish src/PremierBankTesting.Api/PremierBankTesting.Api.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS migrator
WORKDIR /src

COPY --from=build /src .

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

ENTRYPOINT ["dotnet", "ef", "database", "update", \
    "--project", "src/PremierBankTesting.Infrastructure", \
    "--startup-project", "src/PremierBankTesting.Api"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PremierBankTesting.Api.dll"]
