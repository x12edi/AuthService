# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and restore dependencies
COPY ../AuthService.sln ./
COPY ../AuthService.API/*.csproj ./AuthService.API/
COPY ../AuthService.Application/*.csproj ./AuthService.Application/
COPY ../AuthService.Domain/*.csproj ./AuthService.Domain/
COPY ../AuthService.Infrastructure/*.csproj ./AuthService.Infrastructure/

RUN dotnet restore ./AuthService.API/AuthService.API.csproj

# Copy the rest of the source
COPY ../AuthService.API ./AuthService.API
COPY ../AuthService.Application ./AuthService.Application
COPY ../AuthService.Domain ./AuthService.Domain
COPY ../AuthService.Infrastructure ./AuthService.Infrastructure

WORKDIR /src/AuthService.API
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AuthService.API.dll"]
#Replace MyApi.dll with your actual output DLL name (should match your main project name).