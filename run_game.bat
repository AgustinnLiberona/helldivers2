@echo off
title Helldivers 2 - Defenders of Democracy (Native C# OpenGL)
cd /d "%~dp0"
if exist "bin\Release\net10.0\HelldiversDesktop.exe" (
    start "" "bin\Release\net10.0\HelldiversDesktop.exe"
) else if exist "bin\Debug\net10.0\HelldiversDesktop.exe" (
    start "" "bin\Debug\net10.0\HelldiversDesktop.exe"
) else (
    dotnet run -c Release
)
