FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/Identity/IdentityService.API/IdentityService.API.csproj", "Services/Identity/IdentityService.API/"]
COPY ["Services/Identity/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj", "Services/Identity/IdentityService.Infrastructure/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]
COPY ["Shared/Shared.Application/Shared.Application.csproj", "Shared/Shared.Application/"]
COPY ["Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["Shared/Shared.Logging/Shared.Logging.csproj", "Shared/Shared.Logging/"]
COPY ["Shared/Shared.Security/Shared.Security.csproj", "Shared/Shared.Security/"]
COPY ["Services/Identity/IdentityService.Application/IdentityService.Application.csproj", "Services/Identity/IdentityService.Application/"]
COPY ["Services/Identity/IdentityService.Domain/IdentityService.Domain.csproj", "Services/Identity/IdentityService.Domain/"]
RUN dotnet restore "./Services/Identity/IdentityService.API/IdentityService.API.csproj"
COPY . .
WORKDIR "/src/Services/Identity/IdentityService.API"
RUN dotnet build "./IdentityService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

ENV ASPNETCORE_ENVIRONMENT=Docker

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IdentityService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["Services/Identity/IdentityService.API/appsettings.Docker.json", "."]
ENTRYPOINT ["dotnet", "IdentityService.API.dll"]