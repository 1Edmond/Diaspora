# Consultez https://aka.ms/customizecontainer pour savoir comment personnaliser votre conteneur de débogage et comment Visual Studio utilise ce Dockerfile pour générer vos images afin d’accélérer le débogage.

# Cet index est utilisé lors de l’exécution à partir de VS en mode rapide (par défaut pour la configuration de débogage)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8082
EXPOSE 8445


# Cette phase est utilisée pour générer le projet de service
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/KYC/KYC.Api/KYC.Api.csproj", "Services/KYC/KYC.Api/"]
COPY ["Services/KYC/KYC.Infrastructure/KYC.Infrastructure.csproj", "Services/KYC/KYC.Infrastructure/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]
COPY ["Shared/Shared.Application/Shared.Application.csproj", "Shared/Shared.Application/"]
COPY ["Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["Shared/Shared.Logging/Shared.Logging.csproj", "Shared/Shared.Logging/"]
COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]
COPY ["Shared/Shared.Security/Shared.Security.csproj", "Shared/Shared.Security/"]
COPY ["Services/KYC/KYC.Application/KYC.Application.csproj", "Services/KYC/KYC.Application/"]
COPY ["Services/KYC/KYC.Domain/KYC.Domain.csproj", "Services/KYC/KYC.Domain/"]
RUN dotnet restore "./Services/KYC/KYC.Api/KYC.Api.csproj"
COPY . .
WORKDIR "/src/Services/KYC/KYC.Api"
RUN dotnet build "./KYC.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Cette étape permet de publier le projet de service à copier dans la phase finale
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./KYC.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Cette phase est utilisée en production ou lors de l’exécution à partir de VS en mode normal (par défaut quand la configuration de débogage n’est pas utilisée)
FROM base AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Docker

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KYC.Api.dll"]