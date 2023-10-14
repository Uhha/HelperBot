#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0.12-bookworm-slim-arm64v8 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:9005
#ENV ASPNETCORE_HTTPS_PORT=9006
EXPOSE 9005

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["BotApi.csproj", "BotApi/"]
RUN dotnet restore "BotApi/BotApi.csproj"
WORKDIR "/src/BotApi"
COPY . .
RUN dotnet build "BotApi.csproj" -c Release -r linux-arm64 -o /app/build --self-contained=false

FROM build AS publish
RUN dotnet publish "BotApi.csproj" -c Release -r linux-arm64 -o /app/publish --self-contained=false
#/p:UseAppHost=false 

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BotApi.dll"]