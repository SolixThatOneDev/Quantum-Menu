@echo off
set "GAME_PATH=C:\Program Files (x86)\Steam\steamapps\common\Gorilla Tag"
set "PLUGINS_PATH=%GAME_PATH%\BepInEx\plugins"
set "DLL_PATH=bin\Debug\netstandard2.1\Quantum Menu.dll"

echo ========================================
echo     QUANTUM MENU - AUTO UPDATE
echo ========================================

echo [1/3] Building Quantum Menu...
dotnet build "QuantumMenu.sln" -c Debug

if not exist "%DLL_PATH%" (
    echo ERROR: Build failed. Check the errors above.
    pause
    exit /b
)

echo [2/3] Deploying to Gorilla Tag Plugins...
if not exist "%PLUGINS_PATH%" mkdir "%PLUGINS_PATH%"
copy /Y "%DLL_PATH%" "%PLUGINS_PATH%"
echo Successfully updated game folder!

echo [3/3] Pushing to GitHub...
git add .
git commit -m "Automated build and release update"
git push origin main

echo ========================================
echo   DONE! Check GitHub for your Release.
echo ========================================
pause
