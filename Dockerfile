# syntax=docker/dockerfile:1

# ---- Stage 1: Build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project files first (preserving folder structure) so Docker
# caches the restore layer across builds that don't change dependencies.
COPY ["src/ninx.Api/ninx.Api.csproj", "src/ninx.Api/"]
COPY ["src/ninx.Ioc/ninx.Ioc.csproj", "src/ninx.Ioc/"]
COPY ["src/ninx.Application/ninx.Application.csproj", "src/ninx.Application/"]
COPY ["src/ninx.Communication/ninx.Communication.csproj", "src/ninx.Communication/"]
COPY ["src/ninx.Data/ninx.Data.csproj", "src/ninx.Data/"]
COPY ["src/ninx.Domain/ninx.Domain.csproj", "src/ninx.Domain/"]
COPY ["src/ninx.Infra/ninx.Infra.csproj", "src/ninx.Infra/"]

RUN dotnet restore "src/ninx.Api/ninx.Api.csproj"

COPY . .

RUN dotnet publish "src/ninx.Api/ninx.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- Stage 2: Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ninx.Api.dll"]
