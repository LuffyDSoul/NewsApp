@echo off
echo ========================================
echo   NewsApp with Personalized Languages
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
echo ? NEWSAPP WITH LANGUAGE PERSONALIZATION!
echo ========================================
echo.
echo ?? URLs:
echo   Frontend: http://localhost:4200
echo   Backend:  https://localhost:44341
echo   Swagger:  https://localhost:44341/swagger
echo.
echo ?? Authentication:
echo   Username: admin
echo   Password: 1q2w3E*
echo.
echo ?? Language Features:
echo   ? 17 supported languages with flags
echo   ? Personalized news by language preference
echo   ? Automatic language filtering for all searches
echo   ? Visual language indicators throughout the app
echo.
echo ?? Personalized News Experience:
echo   1. Set preferred language in Profile
echo   2. News automatically load in your language
echo   3. Search results filtered by language
echo   4. Category filtering respects language
echo.
echo ?? Try These Languages:
echo   ???? English   ???? Español   ???? Français   ???? Deutsch
echo   ???? Italiano  ???? Português ???? ???????   ???? ??
echo   ???? ???    ???? ???    ???? ???????   ???? Nederlands
echo.
echo ?? Complete Feature Set:
echo   ? User Registration ^& Login
echo   ? Profile Management System  
echo   ? Language Preference Settings
echo   ? Personalized News Feed
echo   ? Multilingual Search ^& Filtering
echo   ? Responsive Modern UI
echo.
echo ?? Quick Test:
echo   1. Login ? Go to Profile ? Set Language to Spanish
echo   2. Return to News ? See "???? News in Español"
echo   3. Search "fútbol" ? Get Spanish sports news
echo   4. Try different languages and categories!
echo.
echo Press any key to close this window...
pause >nul