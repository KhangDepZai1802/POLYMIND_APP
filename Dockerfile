# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Polymind.slnx ./
COPY src/Polymind.Domain/Polymind.Domain.csproj src/Polymind.Domain/
COPY src/Polymind.Application/Polymind.Application.csproj src/Polymind.Application/
COPY src/Polymind.Infrastructure/Polymind.Infrastructure.csproj src/Polymind.Infrastructure/
COPY src/Polymind.Web/Polymind.Web.csproj src/Polymind.Web/
RUN dotnet restore Polymind.slnx

COPY . .
RUN dotnet publish src/Polymind.Web/Polymind.Web.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Polymind.Web.dll"]
