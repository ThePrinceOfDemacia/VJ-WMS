FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Server/VjWms.Server.API/VjWms.Server.API.csproj", "src/Server/VjWms.Server.API/"]
COPY ["src/Server/VjWms.Server.Application/VjWms.Server.Application.csproj", "src/Server/VjWms.Server.Application/"]
COPY ["src/Server/VjWms.Server.Domain/VjWms.Server.Domain.csproj", "src/Server/VjWms.Server.Domain/"]
COPY ["src/Server/VjWms.Server.Infrastructure/VjWms.Server.Infrastructure.csproj", "src/Server/VjWms.Server.Infrastructure/"]
COPY ["src/Shared/VjWms.Shared.csproj", "src/Shared/"]
RUN dotnet restore "src/Server/VjWms.Server.API/VjWms.Server.API.csproj"
COPY . .
WORKDIR "/src/src/Server/VjWms.Server.API"
RUN dotnet build "VjWms.Server.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VjWms.Server.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VjWms.Server.API.dll"]
