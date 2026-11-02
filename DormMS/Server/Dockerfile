# 1. Stage: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Build context je root projekta (docker-compose: context: "."), pa je .csproj u root-u konteksta.
COPY ["DormMS.csproj", "./"]
RUN dotnet restore "DormMS.csproj"

# Kopiramo ceo projekat iz root konteksta
COPY . .
RUN dotnet publish "DormMS.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Stage: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DormMS.dll"]