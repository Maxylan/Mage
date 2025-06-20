# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build

WORKDIR /source

COPY --link Reception/*.csproj .
RUN dotnet restore

COPY --link Reception/. .
RUN rm -rf ./obj
RUN rm -rf ./bin

RUN dotnet publish -o /reception

# Runtime..
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine-composite

ENV RECEPTION_ENVIRONMENT=Development

# ENV APP_NAME="${APP_NAME}"
# ENV APP_VERSION="${GUARD_VERSION}.${GARDEN_VERSION}.${RECEPTION_VERSION}.${STORAGE_VERSION}"
# ENV RECEPTION_NAME="${RECEPTION_NAME}"
# ENV RECEPTION_VERSION="${RECEPTION_VERSION}"
# ENV RECEPTION_BASE_PATH="${RECEPTION_BASE_PATH}"
# ENV RECEPTION_ENVIRONMENT="${RECEPTION_ENVIRONMENT}"
# ENV ASPNETCORE_ENVIRONMENT="${RECEPTION_ENVIRONMENT}"
# ENV RECEPTION_BLOB_STORAGE_FOLDER="${RECEPTION_BLOB_STORAGE_FOLDER}"
# ENV RECEPTION_BLOB_STORAGE="${RECEPTION_VOLUME_TARGET}/${RECEPTION_BLOB_STORAGE_FOLDER}"
# ENV RECEPTION_URL="http://${RECEPTION_NAME}:${RECEPTION_PORT}"
# ENV SECRETARY_BASE_PATH="${SECRETARY_BASE_PATH}"
# ENV SECRETARY_URL="http://${SECRETARY_NAME}:${SECRETARY_PORT}"
# ENV GARDEN_URL="http://${GARDEN_NAME}:${GARDEN_PORT}"
# ENV GUARD_URL="http://${GUARD_NAME}:${GUARD_PORT}"
# ENV STORAGE_URL="${STORAGE_NAME}:${STORAGE_PORT}"
# ENV HTTP_PORTS="${RECEPTION_PORT}"

WORKDIR /reception
COPY --link --from=build /reception .
