FROM mcr.microsoft.com/dotnet/sdk:8.0 as build

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_HTTPS_PORT=8080

WORKDIR /app
COPY . ./

RUN dotnet restore  ./src/Survey.Api
RUN dotnet publish ./src/Survey.Api -c Release -o build

FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN apt-get update && apt-get --yes install tzdata

WORKDIR /app
COPY --from=build /app/build ./

CMD dotnet Survey.Api.dll