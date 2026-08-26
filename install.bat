@echo off
title PeakMod Installer
echo.
echo  PeakMod V0.2.0 Installer
echo  ========================
echo.

:: Check for BepInEx in current directory
if not exist "BepInEx\plugins" (
    echo  This installer should be run from inside the PEAK game folder.
    echo.
    echo  Instructions:
    echo  1. Extract PeakMod-Release.zip into your PEAK game folder
    echo  2. Run install.bat from that folder
    echo.
    echo  Or manually copy:
    echo    PeakMod.dll       -^> BepInEx\plugins\
    echo    DearImGuiInjection.dll -^> BepInEx\plugins\
    echo.
    pause
    exit /b 1
)

:: Check for PEAK.exe
if not exist "PEAK.exe" (
    echo  PEAK.exe not found in current directory!
    echo  Please run this from your PEAK game folder.
    echo.
    pause
    exit /b 1
)

echo  Installing PeakMod...
if exist "BepInEx\plugins\PeakMod.dll" (
    echo  PeakMod.dll installed successfully.
) else (
    echo  ERROR: PeakMod.dll not found. Make sure the release zip was extracted properly.
    pause
    exit /b 1
)

echo.
echo  Installation complete!
echo  Launch PEAK and press Z to open the mod menu.
echo  Press M to toggle the coordinate overlay.
echo.
pause
