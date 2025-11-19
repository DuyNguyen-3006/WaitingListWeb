# ==========================
# 1. BUILD STAGE
# ==========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files individually
COPY WaitingListWeb.Api/*.csproj WaitingListWeb.Api/
COPY WaitingListWeb.Application/*.csproj WaitingListWeb.Application/
COPY WaitingListWeb.Domain/*.csproj WaitingListWeb.Domain/
COPY WaitingListWeb.Infrastructure/*.csproj WaitingListWeb.Infrastructure/
COPY Project.SharedLibrary/*.csproj Project.SharedLibrary/

# Restore dependencies
RUN dotnet restore WaitingListWeb.Api/WaitingListWeb.Api.csproj

# Copy the entire source code
COPY . .

# Build the API
RUN dotnet publish WaitingListWeb.Api/WaitingListWeb.Api.csproj -c Release -o /app/out


# ==========================
# 2. RUNTIME STAGE
# ==========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy published output
COPY --from=build /app/out .

# Expose port (optional, docker-compose will map ports)
EXPOSE 80

ENTRYPOINT ["dotnet", "WaitingListWeb.Api.dll"]
