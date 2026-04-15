@echo off
set "GAME_PATH=C:\Program Files (x86)\Steam\steamapps\common\Gorilla Tag"
set "PLUGINS_PATH=%GAME_PATH%\BepInEx\plugins"
set "DLL_PATH=bin\Debug\netstandard2.1\Quantum Menu.dll"

echo Building Quantum Menu...
dotnet build "QuantumMenu.sln" -c Debug

if exist "%DLL_PATH%" (
    echo Building successful! Copying to plugins...
    if not exist "%PLUGINS_PATH%" mkdir "%PLUGINS_PATH%"
    copy /Y "%DLL_PATH%" "%PLUGINS_PATH%"
    echo Successfully deployed to: %PLUGINS_PATH%
) else (
    echo ERROR: Build failed or DLL not found at %DLL_PATH%
)

pause
