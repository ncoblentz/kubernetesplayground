#mcr.microsoft.com/dotnet/sdk:6.0-alpine-amd64
#mcr.microsoft.com/dotnet/aspnet:6.0-alpine-amd64

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine-amd64 as base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine-amd64 as build
WORKDIR /src
COPY . .
RUN dotnet restore "diagnosticapp.csproj" && dotnet build "diagnosticapp.csproj" -o /app/build && dotnet publish "diagnosticapp.csproj" -o /app/publish /p:UseAppHost=false

FROM base as final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet","diagnosticapp.dll"]
