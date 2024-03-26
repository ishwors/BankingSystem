FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["BankingSystem.API/BankingSystem.API.csproj", "BankingSystem.API/"]
COPY ["BankingSystem.Test/BankingSystem.Test.csproj", "BankingSystem.Test/"]
RUN dotnet restore "BankingSystem.API/BankingSystem.API.csproj"
COPY . .
WORKDIR "/src/BankingSystem.API"
RUN dotnet build "BankingSystem.API.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "BankingSystem.API.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BankingSystem.API.dll"]
