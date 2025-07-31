# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file
COPY CrudApi.sln ./

# Copy project files
COPY src/CrudApi.Api/CrudApi.Api.csproj src/CrudApi.Api/
COPY src/CrudApi.Application/CrudApi.Application.csproj src/CrudApi.Application/
COPY src/CrudApi.Core/CrudApi.Core.csproj src/CrudApi.Core/
COPY src/CrudApi.Infrastructure/CrudApi.Infrastructure.csproj src/CrudApi.Infrastructure/

# Restore packages
RUN dotnet restore CrudApi.sln

# Copy all source files
COPY . .

# Build and publish
WORKDIR /src/src/CrudApi.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

# Create logs directory and set permissions
RUN mkdir -p /app/logs && chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080;https://+:8081
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "CrudApi.Api.dll"]
