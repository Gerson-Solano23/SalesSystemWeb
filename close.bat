@echo off
echo "Stopping API..."
taskkill /F /IM dotnet.exe

echo "Stopping Web App..."
taskkill /f /im node.exe