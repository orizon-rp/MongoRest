FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

ENV APP_PORT=443 \
    APP_UID=1000 \
    MONGO_URL=mongodb://localhost:27017 \
    MONGO_DB_NAME=Orizon
    
USER $APP_UID
WORKDIR /app
EXPOSE $APP_PORT
EXPOSE $APP_SECONDARY_PORT

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MongoRest/MongoRest.csproj", "MongoRest/"]
RUN dotnet restore "MongoRest/MongoRest.csproj"
COPY . .
WORKDIR "/src/MongoRest"
RUN dotnet build "MongoRest.csproj" -c $BUILD_CONFIGURATION -o /app/build

# We don't need this file when using docker
RUN rm run.bat
RUN rm run.sh

FROM build AS publish

ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MongoRest.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "MongoRest.dll"]