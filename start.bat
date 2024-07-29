@echo off
echo "Starting API of Spring Boot..."
cd %~dp0..\SalesSystemWeb\backend\SalesSystem
start "" dotnet run --project SalesSystem.API\SalesSystem.API.csproj

echo "Waiting..."
timeout /t 10

echo "Starting App Web..."
cd %~dp0..\SalesSystemWeb\frontend\SalesSystemWeb
start "" ng serve