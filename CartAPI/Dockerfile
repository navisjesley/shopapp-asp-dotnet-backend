# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file first for better layer caching
COPY ["CartAPI.csproj", "./"]
RUN dotnet restore "CartAPI.csproj"

# Copy everything else and publish
COPY . .
RUN dotnet publish "CartAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Optional: expose container port
EXPOSE 8080

# Configure ASP.NET Core to listen on 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CartAPI.dll"]