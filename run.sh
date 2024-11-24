#!/bin/bash

export APP_PORT=443
export MONGO_URL="mongodb://localhost:27017"
export MONGO_DB_NAME="Orizon"

dotnet run -c Release
