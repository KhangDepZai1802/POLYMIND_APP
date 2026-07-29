# syntax=docker/dockerfile:1

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src

COPY Polymind.slnx ./
COPY src/Polymind.Domain/Polymind.Domain.csproj src/Polymind.Domain/
COPY src/Polymind.Application/Polymind.Application.csproj src/Polymind.Application/
COPY src/Polymind.Infrastructure/Polymind.Infrastructure.csproj src/Polymind.Infrastructure/
COPY src/Polymind.Web/Polymind.Web.csproj src/Polymind.Web/
RUN target_arch="$TARGETARCH"; \
    if [ "$target_arch" = "amd64" ]; then target_arch="x64"; fi; \
    dotnet restore Polymind.slnx --arch "$target_arch"

COPY . .
RUN target_arch="$TARGETARCH"; \
    if [ "$target_arch" = "amd64" ]; then target_arch="x64"; fi; \
    dotnet publish src/Polymind.Web/Polymind.Web.csproj \
        --configuration Release \
        --arch "$target_arch" \
        --output /app/publish \
        --no-restore \
        /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

# Chạy web bằng user không đặc quyền. Tạo sẵn các thư mục được mount bằng named volume
# để volume mới kế thừa đúng owner và app vẫn ghi được log/Data Protection keys.
RUN mkdir -p /app/logs /app/data-protection-keys \
    && chown -R $APP_UID:$APP_UID /app
USER $APP_UID

ENTRYPOINT ["dotnet", "Polymind.Web.dll"]
