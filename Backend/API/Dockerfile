# syntax=docker/dockerfile:1.7
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY API.csproj ./
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore API.csproj

COPY . ./
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish API.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /src/prompts ./prompts
ENTRYPOINT ["dotnet", "API.dll"]
