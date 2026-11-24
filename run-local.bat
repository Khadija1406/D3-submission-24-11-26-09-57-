@echo off
title Pulse App - Local Deployment Server
echo ========================================================
echo   Pulse MVC - Local Production Deployment
echo   Listening on: http://localhost:5000
echo ========================================================
if exist "%~dp0publish_app\PulseMvc.exe" (
    cd /d "%~dp0publish_app"
) else (
    cd /d "%~dp0publish"
)
echo Starting PulseMvc.exe on http://localhost:5000...
PulseMvc.exe --urls "http://localhost:5000"
pause
