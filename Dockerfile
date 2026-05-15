# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy solution and project files
COPY ["src/ninx.Api/ninx.Api.csproj", "src/ninx.Api/"]
COPY ["src/ninx.Ioc/ninx.Ioc.csproj", "src/ninx.Ioc/"]

# Restore dependencies
RUN dotnet restore "src/ninx.Api/ninx.Api.csproj"

# Copy the entire source code
COPY . .

# Build the project
RUN dotnet build "src/ninx.Api/ninx.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish

RUN dotnet publish "src/ninx.Api/ninx.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=publish /app/publish .

# Expose port (Render.com default is 10000 or 8080)
EXPOSE 8080

# Set environment for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ninx.Api.dll"]
