# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the project files required for restore
COPY ["src/ReportingService/ReportingService.csproj", "ReportingService/"]
COPY ["src/Shared.Auth/Shared.Auth.csproj", "Shared.Auth/"]
COPY ["src/Shared.Logging/Shared.Logging.csproj", "Shared.Logging/"]
COPY ["src/Shared.Middleware/Shared.Middleware.csproj", "Shared.Middleware/"]

# Restore dependencies
RUN dotnet restore "ReportingService/ReportingService.csproj"

# Copy the full source code
COPY src/ .

# Build the service
WORKDIR "/src/ReportingService"
RUN dotnet build "ReportingService.csproj" -c Release -o /app/build

# Publish the service
FROM build AS publish
RUN dotnet publish "ReportingService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Add entrypoint and make it executable
COPY src/CategoriesService/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Set which DLL to execute
ENV DLL_NAME=CategoriesService.dll

# Entrypoint waits for Terraform before launching
EXPOSE 80
ENTRYPOINT ["/entrypoint.sh"]
