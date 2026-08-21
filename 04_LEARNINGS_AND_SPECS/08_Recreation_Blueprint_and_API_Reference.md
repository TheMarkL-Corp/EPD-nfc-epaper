# Learning Module 08: End-to-End Recreation Blueprint & Specification

## 1. Overview
This blueprint contains the complete technical specification and structural design required to recreate the entire NFC E-paper solution from scratch in any modern programming language (C#, Python, C++, TypeScript/Node.js, or Rust).

---

## 2. Complete Architectural Blueprint

```
+-----------------------------------------------------------------------+
|                         APPLICATION LAYER                             |
|  - Web UI / Desktop WinForms / CLI / REST API / WebSockets (Port 8082)|
|  - CSV Batch Engine / Label Designer / Template Synthesizer          |
+-----------------------------------------------------------------------+
                                   |
+-----------------------------------------------------------------------+
|                    HIGH-LEVEL SDK (AdvNFCWrap)                        |
|  - DirectBitmap / Fast Pixel Memory Buffer                            |
|  - Floyd-Steinberg / Burkes Multi-color Dithering Engine              |
|  - Model Catalog (EPD-210, EPD-302, EPD-303, EPD-304)                |
|  - Async Task API / Security PIN / User Flash Storage API            |
+-----------------------------------------------------------------------+
                                   |
+-----------------------------------------------------------------------+
|                 PROTOCOL & STATE ENGINE (AdvNFC)                      |
|  - 4-State Connection FSM (Init -> Setup -> TestComm -> Ready)        |
|  - ST25DV Fast Transfer Mode (FTM) Mailbox Synchronization            |
|  - Packet Framing: [CMD(1B)][LEN(1B)][DATA(NB)][CHECKSUM(1B)]         |
|  - Commands: Erase (0x80), Stream (0x8E), Verify (0x82), Refresh(0x85)|
+-----------------------------------------------------------------------+
                                   |
+-----------------------------------------------------------------------+
|                    HARDWARE DRIVER (RFID API)                         |
|  - SerialPort UART @ 115,200 8N1                                      |
|  - Jogtek ASCII Hex Envelope: 01 [LEN] 00 03 04 [CMD] [ARGS] 00 00    |
|  - ISO 15693 Inventory (0x01) / Extended Read/Write (0x30/0x31)       |
|  - ST25DV Mailbox Fast Write (0xCA) / Fast Read (0xCC)                |
+-----------------------------------------------------------------------+
```

---

## 3. Step-by-Step Implementation Guide

### Step 1: Implement Serial Reader Driver (`RfidDriver`)
1. Open serial port at 115,200 baud, 8N1.
2. Implement framing method `SendFrame(cmd, payload)` formatting string `"01" + HexLen + "000304" + cmd + payload + "0000"`.
3. Implement `Inventory()` returning detected 16-hex-digit Tag UID.
4. Implement `FastWriteMailbox(data)` (`0xCA`) and `FastReadMailbox()` (`0xCC`).

### Step 2: Implement Protocol Engine (`EpdProtocol`)
1. Build packet builder calculating 2's complement checksum:
   `checksum = (256 - (cmd + len + sum(data))) & 0xFF`.
2. Implement FTM handshake: enable mailbox register `0x0D` bit 0, enable energy harvesting register `0x02`.
3. Implement `DrawImage(rawBytes, lz4Flag)` sequence:
   - Send `CMD_ERASE_IMAGE_FLASH (0x80)` with `lz4Flag`.
   - Chunk bytes into $(MaxNFC - 9)$ aligned chunks and stream with `CMD_WRITE_IMAGE_FLASH_NOACK (0x8E)`.
   - Send `CMD_CHECK_IMAGE_FLASH (0x82)` to verify tag CRC.
   - Send `CMD_END_WRITE_FLASH_AND_EPD (0x85)` with `[pages, width, height]` to trigger e-paper refresh.
   - Poll `CMD_GET_EPD_STATUS (0x88)` until idle.

### Step 3: Implement Image Engine (`ImageProcessor`)
1. Resize input bitmap to panel dimensions ($296 	imes 128$ or $416 	imes 240$).
2. Apply Floyd-Steinberg error diffusion quantization against target palette.
3. Pack pixels into column-major vertical byte arrays with horizontal mirroring.
4. Attempt LZ4 compression; select smaller payload.

---

## 4. Re-creation Verification Checklist
- [x] Serial communication verified with Jogtek reader at 115,200 bps.
- [x] Tag UID discovered via ISO 15693 Inventory.
- [x] ST25DV mailbox initialized and verified.
- [x] Firmware version, SN, and platform name read successfully.
- [x] Dithered bitmap bit-packed and streamed over FTM mailbox.
- [x] Flash checksum verified and E-paper refresh executed cleanly.
