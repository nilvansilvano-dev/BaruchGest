@echo off
echo Iniciando BaruchGest...

start "BaruchGest - API" cmd /k "cd /d C:\git-trabalhos\FinanceiroAPI && dotnet run"
timeout /t 4 /nobreak > nul
start "BaruchGest - Frontend" cmd /k "cd /d C:\git-trabalhos\FinanceiroAPI\frontend && npm run dev"

echo.
echo Aguarde alguns segundos e acesse:
echo   Frontend: http://localhost:5173
echo   API/Swagger: http://localhost:5000/swagger
echo.
pause

echo.
echo Aguarde alguns segundos e acesse:
echo   Frontend: http://localhost:5173
echo   API/Swagger: http://localhost:5000/swagger
echo.