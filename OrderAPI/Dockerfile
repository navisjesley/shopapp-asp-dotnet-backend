# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file first for better layer caching
COPY ["OrderAPI.csproj", "./"]
RUN dotnet restore "OrderAPI.csproj"

# Copy everything else and publish
COPY . .
RUN dotnet publish "OrderAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Optional: expose container port
EXPOSE 8080

# Configure ASP.NET Core to listen on 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OrderAPI.dll"]