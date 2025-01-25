FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Order.Api/Order.Api.csproj", "Order.Api/"]
COPY ["src/Order.Application/Order.Application.csproj", "Order.Application/"]
COPY ["src/Order.Domain/Order.Domain.csproj", "Order.Domain/"]
COPY ["src/Order.Infrastructure/Order.Infrastructure.csproj", "Order.Infrastructure/"]
COPY ["Directory.Packages.props", "./"]
COPY ["Directory.Build.props", "./"]
RUN dotnet restore "Order.Api/Order.Api.csproj"
COPY . ../
WORKDIR /src/Order.Api
RUN dotnet build "Order.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish --no-restore -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
ENV ASPNETCORE_HTTP_PORTS=5001
EXPOSE 5001
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Order.Api.dll"]
