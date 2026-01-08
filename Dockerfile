FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ./*.props ./

COPY ["src/UserService/UserService.csproj", "src/UserService/"]
COPY ["src/Application/UserService.Application/UserService.Application.csproj", "src/Application/UserService.Application/"]
COPY ["src/Application/UserService.Application.Abstractions/UserService.Application.Abstractions.csproj", "src/Application/UserService.Application.Abstractions/"]
COPY ["src/Application/UserService.Application.Contracts/UserService.Application.Contracts.csproj", "src/Application/UserService.Application.Contracts/"]
COPY ["src/Application/UserService.Application.Models/UserService.Application.Models.csproj", "src/Application/UserService.Application.Models/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Presentation/Grpc/Grpc.csproj", "src/Presentation/Grpc/"]
COPY ["src/Presentation/Kafka/Kafka.csproj", "src/Presentation/Kafka/"]

RUN dotnet restore "src/UserService/UserService.csproj"

COPY . .
WORKDIR "/src/src/UserService"
RUN dotnet build "UserService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UserService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "UserService.dll"]
