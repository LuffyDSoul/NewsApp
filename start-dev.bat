@echo off
echo Starting NewsApp Backend and Frontend...

echo Starting .NET Backend...
cd "src\NewsApp.HttpApi.Host"
start "Backend" cmd /c "dotnet run"

echo Waiting for backend to start...
timeout /t 5

echo Starting Angular Frontend...
cd "..\..\NewsApp.Angular"
start "Frontend" cmd /c "npm install && ng serve"

echo.
echo Backend running at: https://localhost:44341
echo Frontend running at: http://localhost:4200
echo.
echo Press any key to exit...
pause