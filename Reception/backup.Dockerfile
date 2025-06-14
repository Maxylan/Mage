# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-alpine

ARG RECEPTION_NAME
ARG RECEPTION_MOUNT_POINT
ARG RECEPTION_BUILD_TARGET
ARG RECEPTION_APP_TARGET

WORKDIR $RECEPTION_BUILD_TARGET

COPY --link $RECEPTION_MOUNT_POINT/*.csproj .
RUN dotnet restore

COPY --link . .
RUN dotnet publish --no-restore -o $RECEPTION_APP_TARGET

# Runtime..
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine-composite

WORKDIR $RECEPTION_APP_TARGET
COPY --link --from=build $RECEPTION_APP_TARGET .

ENTRYPOINT ["./${RECEPTION_NAME}"]
