# syntax=docker/dockerfile:1

# Build stage: full SDK, restore cached separately from the source.
FROM mcr.microsoft.com/dotnet/sdk:10.0.403-noble AS build
WORKDIR /src

COPY global.json Directory.Build.props .editorconfig ./
COPY src/DevopsPlatform.Api/DevopsPlatform.Api.csproj src/DevopsPlatform.Api/
RUN dotnet restore src/DevopsPlatform.Api/DevopsPlatform.Api.csproj

COPY src/ src/
# Not "VERSION": MSBuild reads environment variables as properties, case-insensitively,
# so a VERSION build arg would silently become $(Version) and break the build.
ARG APP_VERSION=0.0.0-local
RUN dotnet publish src/DevopsPlatform.Api/DevopsPlatform.Api.csproj \
        --configuration Release --no-restore --output /app \
        -p:InformationalVersion="${APP_VERSION}" -p:UseAppHost=false

# Runtime stage: chiseled Ubuntu, no shell or package manager, non-root user (app, uid 1654).
FROM mcr.microsoft.com/dotnet/aspnet:10.0.14-noble-chiseled AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
# Numeric uid of the image's "app" user, so Kubernetes runAsNonRoot can verify it.
USER 1654
ENTRYPOINT ["dotnet", "DevopsPlatform.Api.dll"]
