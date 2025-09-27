@echo off
echo ========================================
echo   NewsApp with Login & Register
echo ========================================
echo.

echo ?? Starting Backend (ABP Framework)...
cd "src\NewsApp.HttpApi.Host"
start "NewsApp Backend" cmd /c "echo Starting backend... && timeout /t 3 && dotnet run"

echo.
echo ? Waiting for backend to initialize...
timeout /t 10

echo.
echo ?? Starting Frontend (Angular)...
cd "..\..\NewsApp.Angular"
start "NewsApp Frontend" cmd /c "echo Starting frontend... && timeout /t 3 && ng serve --open"

echo.
echo ========================================
echo ? NEWSAPP WITH FULL AUTH IS STARTING!
echo ========================================
echo.
echo ?? URLs:
echo   Frontend: http://localhost:4200
echo   Backend:  https://localhost:44341
echo   Swagger:  https://localhost:44341/swagger
echo.
echo ?? Authentication Options:
echo   1. Login with existing admin account:
echo      Username: admin
echo      Password: 1q2w3E*
echo.
echo   2. Register new account:
echo      Click "Sign Up" in the app
echo      Use any email/username/password
echo.
echo ?? What you can do:
echo   1. Open http://localhost:4200
echo   2. Create new account OR sign in
echo   3. Browse authenticated news
echo   4. Try logout and login again
echo   5. Test with multiple user accounts
echo.
echo Press any key to close this window...
pause >nul