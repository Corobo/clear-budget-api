# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the project files required for restore
COPY ["src/TransactionsService/TransactionsService.csproj", "TransactionsService/"]
COPY ["src/Shared.Auth/Shared.Auth.csproj", "Shared.Auth/"]
COPY ["src/Shared.Logging/Shared.Logging.csproj", "Shared.Logging/"]
COPY ["src/Shared.Messaging/Shared.Messaging.csproj", "Shared.Messaging/"]
COPY ["src/Shared.Middleware/Shared.Middleware.csproj", "Shared.Middleware/"]

# Restore dependencies
RUN dotnet restore "TransactionsService/TransactionsService.csproj"

# Copy the full source code
COPY src/ .

# Build the service
WORKDIR "/src/TransactionsService"
RUN dotnet build "TransactionsService.csproj" -c Release -o /app/build

# Publish the service
FROM build AS publish
RUN dotnet publish "TransactionsService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app

RUN apt-get update && apt-get install -y jq && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Add entrypoint and make it executable
COPY src/TransactionsService/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Set which DLL to execute
ENV DLL_NAME=TransactionsService.dll

# Entrypoint waits for Terraform before launching
EXPOSE 80
ENTRYPOINT ["/entrypoint.sh"]
