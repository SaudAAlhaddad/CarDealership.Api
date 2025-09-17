FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore CarDealership.Api.csproj

COPY . .
RUN dotnet publish CarDealership.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN mkdir -p /app/data

COPY --from=build /app/publish ./

EXPOSE 8080

# run the app on http://+:8080 
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Development

# Start the app
ENTRYPOINT ["dotnet", "CarDealership.Api.dll"]
