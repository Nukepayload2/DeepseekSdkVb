@echo off
cd /d "%~dp0"
for %%I in ("..\..\docfx\src\docfx\bin\Release\net9.0") do set DOCFX_PATH=%%~fI
set PATH=%PATH%;%DOCFX_PATH%

cmd /k
