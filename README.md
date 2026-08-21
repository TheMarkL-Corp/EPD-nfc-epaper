# MedTRx EPD / NFC E-Paper Programmer

This repository contains the active development codebase and deployable portable release packages for the **MedTRx EPD (v1.0.1)** 2.13" NFC E-Paper Programmer application.

---

## 🧭 Repository Structure

```
EPD-nfc-epaper/
│
├── 📁 01_CURRENT_DEVELOPMENT/             # 🛠️ ACTIVE DEVELOPMENT & SOURCE CODE
│   ├── src/                               # Complete C# Source Code (Forms, Renderer, UI)
│   ├── build.bat / build.ps1              # 1-Click compiler scripts (builds without VS)
│   ├── MedTRx_EPD.exe                     # Compiled development executable
│   ├── AdvNFC.dll, RFID.dll, ...          # Runtime & reference libraries
│   ├── x64/, x86/                         # Native LZ4 engines
│   └── docs/                              # Architecture and design documentation
│
├── 📁 02_DEPLOYED_PACKAGE/                # 📦 READY-TO-DISTRIBUTE DEPLOYMENT
│   ├── MedTRx_EPD.exe                     # Standalone Portable App (v1.0.1)
│   ├── CDM2123620_Setup_NFC_Driver.zip    # FTDI / NFC USB Reader Driver Installer
│   ├── USER_MANUAL.md                     # End-user visual manual & guide
│   ├── MedTRx_EPD_v1.0.1_Portable.zip     # 1-Click deployable zip package
│   └── *.dll, x64/, x86/                  # All required portable runtime libraries
│
├── 📄 .gitignore                           # Excludes build artifacts & local study folders
└── 📄 README.md                            # Repository overview & quick start
```

---

## 🚀 Quick Start Guide

### 1. Active Development (`01_CURRENT_DEVELOPMENT`)
- Source code is in [`01_CURRENT_DEVELOPMENT/src/`](./01_CURRENT_DEVELOPMENT/src/).
- To build the application, execute `build.bat` (Command Prompt) or `powershell .\build.ps1` (PowerShell).

### 2. Standalone Deployment (`02_DEPLOYED_PACKAGE`)
- Double-click [`02_DEPLOYED_PACKAGE/MedTRx_EPD.exe`](./02_DEPLOYED_PACKAGE/MedTRx_EPD.exe) to run.
- For distribution to end users, share [`MedTRx_EPD_v1.0.1_Portable.zip`](./02_DEPLOYED_PACKAGE/MedTRx_EPD_v1.0.1_Portable.zip).
- Refer to [`02_DEPLOYED_PACKAGE/USER_MANUAL.md`](./02_DEPLOYED_PACKAGE/USER_MANUAL.md) for full operational instructions.