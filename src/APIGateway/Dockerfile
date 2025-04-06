# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the project files required for restore
COPY ["src/APIGateway/APIGateway.csproj", "APIGateway/"]
COPY ["src/Shared.Auth/Shared.Auth.csproj", "Shared.Auth/"]
COPY ["src/Shared.Logging/Shared.Logging.csproj", "Shared.Logging/"]

# Restore dependencies
RUN dotnet restore "APIGateway/APIGateway.csproj"

# Copy the full source code
COPY src/ .

# Build the service
WORKDIR "/src/APIGateway"
RUN dotnet build "APIGateway.csproj" -c Release -o /app/build

# Publish the service
FROM build AS publish
RUN dotnet publish "APIGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Optional: if ocelot.Docker.json is not included via .csproj, add this:
COPY src/APIGateway/ocelot.Docker.json .

ENTRYPOINT ["dotnet", "APIGateway.dll"]
