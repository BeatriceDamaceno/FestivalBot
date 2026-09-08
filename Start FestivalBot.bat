@echo off
title FestivalBot
set "BOT_ROOT=C:\Users\sophie.mendonca\FestivalBot"
set "BOT_EXE=%BOT_ROOT%\publish\FestivalBot.exe"

if not exist "%BOT_EXE%" (
  echo FestivalBot.exe not found at:
  echo %BOT_EXE%
  echo.
  echo Rebuild/publish the project first.
  pause
  exit /b 1
)

REM Stop any previous FestivalBot instances to avoid double replies
powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-CimInstance Win32_Process | Where-Object { $_.Name -eq 'FestivalBot.exe' -or ($_.Name -eq 'dotnet.exe' -and $_.CommandLine -match 'FestivalBot') } | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }"

echo Starting FestivalBot...
cd /d "%BOT_ROOT%\publish"
"%BOT_EXE%"
echo.
echo FestivalBot stopped.
pause
