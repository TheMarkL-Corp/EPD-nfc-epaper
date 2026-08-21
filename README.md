# EPD NFC E-Paper Workspace

Welcome to the **MedTRx EPD / NFC E-Paper** engineering and development repository. This workspace has been systematically organized into numbered categories to make navigation and development effortless.

---

## 🧭 Directory Map & Quick Navigation

```
d:\Antigravity Projects\EPD-nfc-epaper\
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
├── 📁 03_SAMPLE_APPS_TO_STUDY/            # 🔍 VENDOR REFERENCE APPS & SDKs
│   ├── 01_Linchun_SDK/                    # Official Linchun DLL v1.0.11 & EPD30x Factory Tool
│   ├── 02_NFC_Demo_v103/                  # Reference NFC Demo v1.0.3 sample application
│   ├── 03_EPD210_Public_Installer/        # Original EPD-210 NFC.msi & extracted files
│   └── 04_Decompiled_Sources/             # Decompiled C# source trees of all vendor apps
│
├── 📁 04_LEARNINGS_AND_SPECS/             # 📚 ENGINEERING KNOWLEDGE BASE & PROTOCOLS
│   ├── 00_TABLE_OF_CONTENTS.md            # Clickable documentation index
│   ├── 01_RFID_Hardware_Driver_Layer.md   # RFID reader communication & ISO15693 specs
│   ├── 02_AdvNFC_Protocol_and_State_Machine.md # ST25DV FTM flashing protocol & state machine
│   ├── 03_AdvNFCWrap_High_Level_SDK.md    # High-level C# SDK API reference
│   ├── 07_Image_Processing_and_Dithering_Pipeline.md # 296x128 1-bit dithering & LZ4 compression
│   ├── 08_Recreation_Blueprint_and_API_Reference.md  # Custom EPD app recreation blueprint
│   ├── 09_Application_4_LEO_D30_Factory_Tool.md      # Auto-firmware negotiation & panel fix
│   └── CONSOLIDATED_SUMMARY.md            # Complete reverse engineering summary
│
└── 📁 _ARCHIVE_TOOLS/                      # 🗄️ INTERNAL TOOLS & REVERSE ENGINEERING ARCHIVE
    ├── decompiler_tools/                  # Decompiler libraries (Cecil, ICSharpCode, ILSpy)
    ├── nuget_packages/                    # Downloaded .zip archives & package dependencies
    └── reverse_engineering_scripts/       # Extraction scripts & Python analyzers
```

---

## 🚀 Quick Start Guide

### 1. Where is the active code?
Navigate to [`01_CURRENT_DEVELOPMENT/`](./01_CURRENT_DEVELOPMENT/).
- Source files are in [`01_CURRENT_DEVELOPMENT/src/`](./01_CURRENT_DEVELOPMENT/src/).
- To compile the project at any time, run `build.bat` or `powershell .\build.ps1`.

### 2. Where is the app ready to run or send to clients?
Navigate to [`02_DEPLOYED_PACKAGE/`](./02_DEPLOYED_PACKAGE/).
- Double click [`MedTRx_EPD.exe`](./02_DEPLOYED_PACKAGE/MedTRx_EPD.exe) to run the portable application.
- To distribute, use [`MedTRx_EPD_v1.0.1_Portable.zip`](./02_DEPLOYED_PACKAGE/MedTRx_EPD_v1.0.1_Portable.zip) or copy the folder contents.
- Read [`USER_MANUAL.md`](./02_DEPLOYED_PACKAGE/USER_MANUAL.md) for full operational instructions.

### 3. Where are the sample apps provided to study?
Navigate to [`03_SAMPLE_APPS_TO_STUDY/`](./03_SAMPLE_APPS_TO_STUDY/).
- Contains the Linchun Factory Tool, NFC Demo v1.0.3, EPD-210 installer, and reverse-engineered C# source code in [`04_Decompiled_Sources/`](./03_SAMPLE_APPS_TO_STUDY/04_Decompiled_Sources/).

### 4. Where are our learnings and documentation?
Navigate to [`04_LEARNINGS_AND_SPECS/`](./04_LEARNINGS_AND_SPECS/).
- Start with [`00_TABLE_OF_CONTENTS.md`](./04_LEARNINGS_AND_SPECS/00_TABLE_OF_CONTENTS.md) or [`CONSOLIDATED_SUMMARY.md`](./04_LEARNINGS_AND_SPECS/CONSOLIDATED_SUMMARY.md) to explore the 10 protocol and reverse-engineering research modules.