FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0.102-ca-patch-buster-slim AS build
WORKDIR /src
COPY NuGet.config .
COPY dependencies.props .
COPY Directory.Build.props .
COPY . .

RUN dotnet restore src/Platform.Migrations/Platform.Migrations.csproj

COPY . .
WORKDIR /src/src/Platform.Migrations
RUN dotnet build Platform.Migrations.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Platform.Migrations.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Platform.Migrations.dll"]
