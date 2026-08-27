@echo off
setlocal
echo ===================================================
echo  Building MedTRx EPD v1.0.2 (Windows Forms App)
echo ===================================================

set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

if not exist "%CSC%" (
    echo Error: .NET Framework 4.x CSC compiler not found at %CSC%
    pause
    exit /b 1
)

"%CSC%" /noconfig /target:winexe /out:"MedTRx_EPD.exe" /win32icon:"src\logo.ico" /r:"AdvNFC.dll","RFID.dll","statemap.dll","Lz4Net.dll","System.dll","System.Core.dll","System.Drawing.dll","System.Management.dll","System.Windows.Forms.dll" /platform:anycpu src\Program.cs src\MainForm.cs src\MainForm.Designer.cs src\TagRenderer.cs src\AppSettings.cs src\Localization.cs src\AssemblyInfo.cs

if %ERRORLEVEL% EQU 0 (
    echo.
    echo [SUCCESS] MedTRx_EPD.exe built successfully!
    echo.
) else (
    echo.
    echo [ERROR] Build failed.
    echo.
    pause
)