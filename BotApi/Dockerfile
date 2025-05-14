#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Use architecture-neutral base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0.5-bookworm-slim AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:9005
EXPOSE 9005

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["BotApi.csproj", "BotApi/"]
WORKDIR /src/BotApi
RUN dotnet restore "../BotApi/BotApi.csproj"
COPY . .

# Remove hardcoded runtime identifier (use portable build)
RUN dotnet build "BotApi.csproj" -c Release -o /app/build --no-self-contained

# Publish stage
FROM build AS publish
RUN dotnet publish "BotApi.csproj" -c Release -o /app/publish --no-self-contained

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BotApi.dll"]