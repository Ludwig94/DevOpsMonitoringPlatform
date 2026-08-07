# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Monitoring.Api.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish Monitoring.Api.csproj -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5158

ENV ASPNETCORE_URLS=http://+:5158

ENTRYPOINT ["dotnet", "Monitoring.Api.dll"]
