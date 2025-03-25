FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnet-build

WORKDIR /app
COPY ./Rendezvous.API .
RUN dotnet restore
RUN dotnet publish -c Release -o /publish

FROM node:latest AS ng-build
WORKDIR /app
COPY ./Rendezvous.Client/package.json /app
RUN npm install
COPY ./Rendezvous.Client /app
RUN npm run build

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /publish
COPY --from=dotnet-build /publish .
ENTRYPOINT [ "dotnet", "Rendezvous.API.dll" ]
