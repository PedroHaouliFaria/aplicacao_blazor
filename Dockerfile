# ============================================
# Multi-stage Dockerfile for BlazorWeatherApp
# ============================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first (for layer caching)
COPY BlazorWeatherApp.sln .
COPY BlazorWeatherApp/BlazorWeatherApp.csproj BlazorWeatherApp/
COPY BlazorWeatherApp.Tests/BlazorWeatherApp.Tests.csproj BlazorWeatherApp.Tests/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Run tests during build to ensure quality
RUN dotnet test BlazorWeatherApp.Tests/ --no-restore --verbosity normal

# Publish the application
RUN dotnet publish BlazorWeatherApp/ -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "BlazorWeatherApp.dll"]
