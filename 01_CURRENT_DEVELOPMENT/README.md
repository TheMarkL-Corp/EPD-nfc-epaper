# MedTRx EPD v1.0.2: Current Development Workspace

## 1. Overview
**MedTRx EPD** (v1.0.2) is a lightweight, portable Windows Forms application for designing and wirelessly programming **2.13-inch (296x128 B/W) EPD-210 NFC E-Paper tags** using standard 13.56 MHz RFID readers.

It incorporates the latest protocol and driver engine from the Linchun SDK, featuring automatic firmware negotiation to prevent inverted or mirrored screen flashing on newer DKE hardware panels.

---

## 2. Key Features in v1.0.2
- **Direct FTDI Hardware PNP Auto-Discovery (`VID_0403` & `PID_6015`)**:
  - Uses `System.Management` WMI (`Win32_PnPEntity`) to instantly discover and bind to the Jogtek FTDI USB UART bridge without probing Bluetooth ports.
  - Automatic fallback to sequential probe if WMI is restricted.
- **Single-Instance Protection & Clean Lifecycle**:
  - Global application mutex ensures only one instance runs at a time, preventing COM port sharing conflicts.
  - Automatic resource disposal and reader release on application exit.
- **Dual Language Support (English & 繁體中文 Traditional Chinese)**:
  - Instant on-the-fly language switching via the top-right dropdown.
  - Remembers the user's preferred language automatically.
- **Smart Non-Blocking COM Port Management**:
  - **Zero UI Freezing**: Port probing and refresh execute asynchronously in background tasks.
  - **Memory of Last Port**: Automatically prioritizes and connects to the last successfully connected COM port.
- **Dual Style Selector (with Instant WYSIWYG Preview)**:
  - **Style B (Clean White) [Default]**: Full white canvas with upper-left subtitle and prominent lower title.
  - **Style A (Black Header Banner)**: Black header bar with white subtitle and lower white main title.
- **Enhanced Typography & Alignment**:
  - **Line 2 (Upper Subtitle)**: Positioned at the upper-left corner.
  - **Line 1 (Lower Main Title)**: Positioned on the lower half with a prominent large font and dynamic auto-scaling.
- **Blank Line Support**: Leaving any line blank keeps that section clean without drawing placeholder text.
- **Live Visual Status Indicators**:
  - 🔴 **Red**: No tag detected on RF field.
  - 🟡 **Yellow**: Tag detected on reader (reading UID & performing handshake).
  - 🟢 **Green**: Tag ready to program (shows UID & Firmware Version).
- **One-Click Tag Flashing**: High-speed ST25DV Fast Transfer Mode (FTM) flashing with progress bar, duration counter, and beep sound upon completion.
- **Fully Portable**: Self-contained with all native and managed dependencies included.

---

## 3. Directory Layout
```
01_CURRENT_DEVELOPMENT/
├── build.bat                    # 1-Click build script for Windows Command Prompt
├── build.ps1                    # 1-Click build script for PowerShell
├── MedTRx_EPD.exe               # Main Executable v1.0.2 (with embedded logo.ico)
├── AG_EPD_Tag.exe.config        # .NET 4.6.1+ Configuration
├── logo.ico                     # Application Icon
├── app_settings.ini             # Persistent User Settings (Language, Port, Style)
├── AdvNFC.dll                   # Core Protocol & State Engine
├── RFID.dll                     # Serial & ISO 15693 Driver
├── statemap.dll                 # State Machine Compiler Engine
├── Lz4Net.dll                   # High-Speed Compression Wrapper
├── x86/lz4X86.dll               # 32-bit Native LZ4 Core
├── x64/lz4X64.dll               # 64-bit Native LZ4 Core
├── docs/                        # Architecture & User Documentation
└── src/                         # Full C# Source Code & Project Files
    ├── Program.cs               # Main entrypoint
    ├── MainForm.cs              # UI Form logic & event handlers
    ├── MainForm.Designer.cs     # Windows Forms layout definition
    ├── TagRenderer.cs           # 296x128 bitmap & canvas rendering engine
    ├── AppSettings.cs           # INI configuration manager
    ├── Localization.cs          # Dynamic English / Traditional Chinese strings
    ├── AssemblyInfo.cs          # Version metadata & attributes
    ├── AG_EPD_Tag.csproj        # Visual Studio C# Project File
    └── logo.ico                 # Embedded resource icon
```

---

## 4. How to Build
To build the application without needing Visual Studio:
- **Command Prompt**: Run `build.bat`
- **PowerShell**: Run `.\build.ps1`
- **Visual Studio**: Open `src/AG_EPD_Tag.csproj` and build in Release or Debug mode.

---

## 5. Requirements
- Windows 7 / 8 / 10 / 11 (32-bit or 64-bit)
- .NET Framework 4.6.1 or higher
- Jogtek HF RFID Reader connected via USB (Virtual COM Port at 115200 8N1)