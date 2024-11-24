@echo off

set APP_PORT=443
set MONGO_URL=mongodb://localhost:27017
set MONGO_DB_NAME=Orizon

dotnet run -c Release

pause
