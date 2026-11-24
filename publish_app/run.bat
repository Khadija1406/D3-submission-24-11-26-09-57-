@echo off
title Pulse App - Local Deployment Server
echo ========================================================
echo   Pulse MVC - Local Production Server
echo   Listening on: http://localhost:5000
echo ========================================================
cd /d "%~dp0"
PulseMvc.exe --urls "http://localhost:5000"
pause
