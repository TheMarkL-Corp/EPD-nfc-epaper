Write-Host "===================================================" -ForegroundColor Cyan
Write-Host " Building MedTRx EPD v1.0.1 (Windows Forms App)" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan

$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) {
    Write-Error ".NET Framework 4.x CSC compiler not found at $csc"
    exit 1
}

$sources = @(
    "src\Program.cs",
    "src\MainForm.cs",
    "src\MainForm.Designer.cs",
    "src\TagRenderer.cs",
    "src\AppSettings.cs",
    "src\Localization.cs",
    "src\AssemblyInfo.cs"
)

$references = @(
    "AdvNFC.dll",
    "RFID.dll",
    "statemap.dll",
    "Lz4Net.dll",
    "System.dll",
    "System.Core.dll",
    "System.Drawing.dll",
    "System.Windows.Forms.dll"
)

& $csc /noconfig /target:winexe /out:"MedTRx_EPD.exe" /win32icon:"src\logo.ico" ("/r:" + ($references -join ",")) /platform:anycpu $sources

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n[SUCCESS] MedTRx_EPD.exe built successfully!`n" -ForegroundColor Green
} else {
    Write-Host "`n[ERROR] Build failed with exit code $LASTEXITCODE`n" -ForegroundColor Red
}