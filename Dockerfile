# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first so Docker can cache the packages
COPY RaceDayMVC.csproj ./
RUN dotnet restore RaceDayMVC.csproj

COPY . ./
RUN dotnet publish RaceDayMVC.csproj -c Release -o /app/publish --no-restore

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish ./

# Do not run as root
USER app

ENTRYPOINT ["dotnet", "RaceDayMVC.dll"]
