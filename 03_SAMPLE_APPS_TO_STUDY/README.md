# Sample Apps & Reference SDKs

This directory contains vendor-provided sample applications, factory test tools, official SDK packages, and their reverse-engineered C# source code for studying protocol implementations.

---

## Directory Overview

### 1. `01_Linchun_SDK/`
- **`EPD NFC Writer NFC DLL v1.0.11/`**: The latest official driver and protocol dynamic link library package (`AdvNFC.dll`, `RFID.dll`, `Lz4Net.dll`, `statemap.dll`).
- **`EPD30x_Factory tool_9319_25993_316/`**: OEM factory test utility for EPD-30x / EPD-210 tag verification, panel handshake debugging, and raw register communication.

### 2. `02_NFC_Demo_v103/`
- **`NFC_Demo v1.0.3/`**: Reference desktop demonstration application illustrating basic tag connectivity, image bitmap conversion, and tag flashing.

### 3. `03_EPD210_Public_Installer/`
- **`EPD-210 NFC for Public/1.3.2/EPD-210 NFC.msi`**: The original vendor MSI installer package.
- **`Extracted_Files/`**: Extracted binaries, system dependencies, icon assets, and driver files from the MSI.

### 4. `04_Decompiled_Sources/`
- **`EPD-210/`**: Decompiled source code for the public EPD-210 application.
- **`LEO_D30_Factory_tool/`**: Decompiled source code for the factory testing tool (contains low-level firmware handshake logic).
- **`NFC_Demo_v103/`**: Decompiled source code for the NFC Demo v1.0.3 application.
- **`MSI_Installed/`**: Decompiled code from assemblies installed via MSI.
- **`Lz4Net_all.cs`**: Decompiled native compression wrapper logic.