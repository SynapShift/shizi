@echo off
setlocal
cd /d "%~dp0"

if exist ".dotnet\dotnet.exe" (
  ".dotnet\dotnet.exe" run --project ".\src\Shizi\Shizi.csproj" --configuration Release --no-build
) else (
  dotnet run --project ".\src\Shizi\Shizi.csproj" --configuration Release
)

endlocal

