# Consolidated Master Summary Report: NFC-Type E-Paper Solution (EPD)

## 1. Project Overview & Scope
This report provides a unified technical synthesis of the NFC-type E-paper Solution (EPD). The solution enables battery-less or ultra-low-power electronic paper tags (2.13", 2.9", 3.7", etc.) to be powered and updated wirelessly over NFC using standard 13.56 MHz RFID readers.

---

## 2. Comparative Matrix of All Sample Applications

| Dimension | App 1: EPD-210 NFCApp | App 2: NFC Demo v1.0.3 | App 3: Public Release Suite | App 4: LEO-D30 Factory Tool |
| :--- | :--- | :--- | :--- | :--- |
| **Location** | `EPD-210-files` | `NFC_Demo v1.0.3 1` | `EPD-210 NFC for Public 1` | `Linchun SDK/EPD30x_Factory tool...` |
| **Primary Target** | Single & batch EPD-210 tags (2.13") | Multi-panel evaluation & automation | Production enterprise deployment | Ultra-lightweight production & R&D testing |
| **Supported Models**| EPD-210 (296x128 B/W) | EPD-210, EPD-302, EPD-303, EPD-304 | Full EPD-210 runtime ecosystem | `EPD-210--TC2`, `EPD-302/303/304--TC2`, `D30-ED29/EL29` |
| **Direct Driver Call**| Via `AdvNFCWrap.dll` | Via `AdvNFCWrap.dll` | Via `AdvNFCWrap.dll` | **Direct `AdvNFC.dll` & `RFID.dll` (No Wrap DLL)** |
| **Binary Footprint**| 166 KB | 120 KB | Full MSI Installer | **23 KB (Thinnest)** |
| **Remote Control** | Local GUI only | WebSocket Server (Port 8082, `/notify`) | Standalone installation | Local GUI only |
| **DKE Reverse Mode**| No | No | No | **Yes (for FW $\ge 4.0.0$)** |
| **Segmented LZ4** | Monolithic block | Monolithic block | Monolithic block | **Segmented 1KB/4KB/5KB with 2B header** |
| **Barcode Engines** | `BarcodeStandard` + `QrCodeNet` | `QRCoder` + Template Engine | Bundled enterprise assemblies | Direct image loader |

---

## 3. Technology Stack & Protocol Summary

```
+----------------------------------------------------------------------------+
|  Host Applications: NFCApp / NFC_Demo / LEO-D30 Factory Tool               |
+----------------------------------------------------------------------------+
                                      │
+----------------------------------------------------------------------------+
|  High-Level Facades (AdvNFCWrap.dll) OR Direct Native Calls                |
+----------------------------------------------------------------------------+
                                      │
+----------------------------------------------------------------------------+
|  AdvNFC.dll: SMC Finite State Machine, ST25DV FTM Handshake, Flash Streams |
+----------------------------------------------------------------------------+
                                      │
+----------------------------------------------------------------------------+
|  RFID.dll: Jogtek Serial Protocol (115200 8N1), ISO 15693 / ST25DV Drivers |
+----------------------------------------------------------------------------+
                                      │ (115200 Baud UART)
+----------------------------------------------------------------------------+
|  Jogtek NFC Reader Hardware (13.56 MHz HF RF Carrier Field)                |
+----------------------------------------------------------------------------+
                                      │ (ISO 15693 RF Field)
+----------------------------------------------------------------------------+
|  EPD Tag: ST25DV NFC Dynamic Tag IC + MCU + SPI Flash + E-Paper Display    |
+----------------------------------------------------------------------------+
```

---

## 4. Key Takeaways & Re-Creation Readiness
1. **Hardware Communication**: Fully documented UART ASCII hex envelope over 115,200 baud serial connection.
2. **Mailbox Streaming**: ST25DV Fast Transfer Mode (FTM) bypasses EEPROM to achieve high-speed image streaming.
3. **Display Formats**: Exact 1-bit and 2-bit column-major bit-packing specifications identified for standard and DKE panels.
4. **Segmented High-Speed Compression**: 1024B, 4096B, and 5120B chunked LZ4-HC compression with 2-byte little-endian headers.
5. **Autonomous Operation**: Complete state machine and automatic tag detection documented for building robust production tools.
