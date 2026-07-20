# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FamilyTreeApp/FamilyTreeApp.Api/FamilyTreeApp.Api.csproj", "FamilyTreeApp/FamilyTreeApp.Api/"]
COPY ["FamilyTreeApp/FamilyTreeApp.Application/FamilyTreeApp.Application.csproj", "FamilyTreeApp/FamilyTreeApp.Application/"]
COPY ["FamilyTreeApp/FamilyTreeApp.Domain/FamilyTreeApp.Domain.csproj", "FamilyTreeApp/FamilyTreeApp.Domain/"]
COPY ["FamilyTreeApp/FamilyTreeApp.Infrastructure/FamilyTreeApp.Infrastructure.csproj", "FamilyTreeApp/FamilyTreeApp.Infrastructure/"]
RUN dotnet restore "./FamilyTreeApp/FamilyTreeApp.Api/FamilyTreeApp.Api.csproj"
COPY . .
WORKDIR "/src/FamilyTreeApp/FamilyTreeApp.Api"
RUN dotnet build "./FamilyTreeApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FamilyTreeApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FamilyTreeApp.Api.dll"]