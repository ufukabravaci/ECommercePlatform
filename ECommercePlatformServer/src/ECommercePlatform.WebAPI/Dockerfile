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
COPY ["src/ECommercePlatform.WebAPI/ECommercePlatform.WebAPI.csproj", "src/ECommercePlatform.WebAPI/"]
COPY ["src/ECommercePlatform.Application/ECommercePlatform.Application.csproj", "src/ECommercePlatform.Application/"]
COPY ["src/ECommercePlatform.Core/ECommercePlatform.Domain.csproj", "src/ECommercePlatform.Core/"] 
COPY ["src/ECommercePlatform.Infrastructure/ECommercePlatform.Infrastructure.csproj", "src/ECommercePlatform.Infrastructure/"]
RUN dotnet restore "./src/ECommercePlatform.WebAPI/ECommercePlatform.WebAPI.csproj"
COPY . .
WORKDIR "/src/src/ECommercePlatform.WebAPI"
RUN dotnet build "./ECommercePlatform.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ECommercePlatform.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ECommercePlatform.WebAPI.dll"]