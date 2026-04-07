FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY syncify.sln ./
COPY src/Syncify.Api/Syncify.Api.csproj                                             src/Syncify.Api/
COPY src/Syncify.Shared/Syncify.Shared.csproj                                       src/Syncify.Shared/
COPY src/Syncify.Connections.Domain/Syncify.Connections.Domain.csproj               src/Syncify.Connections.Domain/
COPY src/Syncify.Connections.Application/Syncify.Connections.Application.csproj     src/Syncify.Connections.Application/
COPY src/Syncify.Connections.Infrastructure/Syncify.Connections.Infrastructure.csproj src/Syncify.Connections.Infrastructure/
COPY src/Syncify.Sync.Domain/Syncify.Sync.Domain.csproj                             src/Syncify.Sync.Domain/
COPY src/Syncify.Sync.Application/Syncify.Sync.Application.csproj                   src/Syncify.Sync.Application/
COPY src/Syncify.Sync.Infrastructure/Syncify.Sync.Infrastructure.csproj             src/Syncify.Sync.Infrastructure/
COPY tests/Syncify.Connections.Application.Tests/Syncify.Connections.Application.Tests.csproj tests/Syncify.Connections.Application.Tests/
COPY tests/Syncify.Connections.Domain.Tests/Syncify.Connections.Domain.Tests.csproj       tests/Syncify.Connections.Domain.Tests/
COPY tests/Syncify.Sync.Domain.Tests/Syncify.Sync.Domain.Tests.csproj                     tests/Syncify.Sync.Domain.Tests/
COPY tests/Syncify.Sync.Application.Tests/Syncify.Sync.Application.Tests.csproj           tests/Syncify.Sync.Application.Tests/
COPY tests/Syncify.Sync.Infrastructure.Tests/Syncify.Sync.Infrastructure.Tests.csproj     tests/Syncify.Sync.Infrastructure.Tests/
COPY tests/Syncify.Api.Tests/Syncify.Api.Tests.csproj                                     tests/Syncify.Api.Tests/

RUN dotnet restore

COPY src/ src/
COPY tests/ tests/

RUN dotnet publish src/Syncify.Api/Syncify.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Syncify.Api.dll"]
