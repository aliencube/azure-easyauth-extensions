# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:9.0-noble AS build

COPY . /source

WORKDIR /source/samples/Aliencube.EasyAuth.ContainerApp

RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled AS final

WORKDIR /app

COPY --from=build /app .

USER $APP_UID

ENTRYPOINT ["dotnet", "Aliencube.EasyAuth.ContainerApp.dll"]
