# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# ═══════════════════════════════════════════════════════════════
# ✅ BASE KATMANI: VS FAST MODE İÇİN VERİ TABANI ALANINI BURADA HAZIRLIYORUZ
# ═══════════════════════════════════════════════════════════════
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Linux ortamında SQLite dosya oluşturma ve kilitleme yetki sorunlarını root ile aşıyoruz
USER root
RUN mkdir -p /app/database && chmod -R 777 /app/database
# ═══════════════════════════════════════════════════════════════

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["UI.Web/UI.Web.csproj", "UI.Web/"]
COPY ["Business/Business.csproj", "Business/"]
COPY ["Data/Data.csproj", "Data/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["Utils/Utils.csproj", "Utils/"]
RUN dotnet restore "./UI.Web/UI.Web.csproj"
COPY . .
WORKDIR "/src/UI.Web"
RUN dotnet build "./UI.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish katmanı (Projenin derlenmiş çıktılarını toplar)
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UI.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ═══════════════════════════════════════════════════════════════
# ✅ FINAL KATMANI: CANLIYA ÇIKARKEN KULLANILACAK SON SÜRÜM
# ═══════════════════════════════════════════════════════════════
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root

# Yerelde tıkır tıkır çalışan, içi 107 otel dolu StayHub.db dosyanı production imajına paketliyoruz
COPY ["UI.Web/StayHub.db", "/app/database/StayHub.db"]
RUN chmod -R 777 /app/database

# Uygulamanın ayağa kalkacağı portu ve bağlantı dizesini sisteme fısıldıyoruz
ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__DefaultConnection="Data Source=/app/database/StayHub.db;"

USER root
ENTRYPOINT ["dotnet", "UI.Web.dll"]