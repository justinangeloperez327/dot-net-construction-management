# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Web/Web.csproj src/Web/

RUN dotnet restore src/Web/Web.csproj

COPY src/ src/

RUN dotnet publish src/Web/Web.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_EnableDiagnostics=0

EXPOSE 8080

RUN mkdir -p /app/data/uploads /app/data/keys \
    && chown -R "$APP_UID":"$APP_UID" /app/data

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "Web.dll"]
