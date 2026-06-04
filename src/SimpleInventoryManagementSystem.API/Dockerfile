# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SimpleInventoryManagementSystem.Domain/SimpleInventoryManagementSystem.Domain.csproj", "src/SimpleInventoryManagementSystem.Domain/"]
COPY ["src/SimpleInventoryManagementSystem.Application/SimpleInventoryManagementSystem.Application.csproj", "src/SimpleInventoryManagementSystem.Application/"]
COPY ["src/SimpleInventoryManagementSystem.Infrastructure/SimpleInventoryManagementSystem.Infrastructure.csproj", "src/SimpleInventoryManagementSystem.Infrastructure/"]
COPY ["src/SimpleInventoryManagementSystem.API/SimpleInventoryManagementSystem.API.csproj", "src/SimpleInventoryManagementSystem.API/"]
RUN dotnet restore "./src/SimpleInventoryManagementSystem.API/SimpleInventoryManagementSystem.API.csproj"
COPY . .
WORKDIR "/src/src/SimpleInventoryManagementSystem.API"
RUN dotnet build "./SimpleInventoryManagementSystem.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SimpleInventoryManagementSystem.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleInventoryManagementSystem.API.dll"]
