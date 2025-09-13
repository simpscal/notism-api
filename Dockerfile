FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY ["src/Notism.Api/Notism.Api.csproj", "Notism.Api/"]
COPY ["src/Notism.Application/Notism.Application.csproj", "Notism.Application/"]
COPY ["src/Notism.Domain/Notism.Domain.csproj", "Notism.Domain/"]
COPY ["src/Notism.Infrastructure/Notism.Infrastructure.csproj", "Notism.Infrastructure/"]
COPY ["src/Notism.Shared/Notism.Shared.csproj", "Notism.Shared/"]

COPY ["Directory.Packages.props", "./"]
COPY ["Directory.Build.props", "./"]

RUN dotnet restore "Notism.Api/Notism.Api.csproj"

COPY . ../

WORKDIR /src/Notism.Api

RUN dotnet build "Notism.Api.csproj" -c Release -o /app/build

FROM build AS publish

RUN dotnet publish --no-restore -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0

ENV ASPNETCORE_HTTP_PORTS=5000

EXPOSE 5000

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Notism.Api.dll"]
