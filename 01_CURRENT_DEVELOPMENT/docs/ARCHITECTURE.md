# Architecture & Technical Reference: AG_EPD Tag

## 1. System Architecture

```
+-----------------------------------------------------------------------+
|                       AG_EPD Tag Application                          |
|  - Real-Time WYSIWYG TagRenderer (GDI+ 296x128)                      |
|  - MainForm UI (Auto-detect COM, Live LED Status, Flasher)           |
+-----------------------------------------------------------------------+
                                   │
                                   │ Direct Native C# Bindings
                                   ▼
+-----------------------------------------------------------------------+
|  AdvNFC.dll (NFCManager, LeoD30EPDAPI, D30Command, EinkImage)         |
|  - ST25DV Fast Transfer Mode (FTM) Mailbox Synchronization            |
|  - SMC Finite State Machine (Init -> FTM Setup -> Handshake -> Ready) |
|  - DKE Panel Bottom-Up Rasterization (img_forDKEEPD_BW)               |
|  - Segmented LZ4-HC Compression (1KB / 4KB Blocks)                    |
+-----------------------------------------------------------------------+
                                   │
                                   │ UART Framing (115,200 baud, 8N1)
                                   ▼
+-----------------------------------------------------------------------+
|  RFID.dll (J_RFID.RFIDAPI)                                            |
|  - Jogtek Envelope: 01 [LEN] 00 03 04 [CMD] [ARGS] 00 00              |
|  - ISO 15693 Inventory (0x01) & ST25DV Fast Commands (0xCA/0xCC)      |
+-----------------------------------------------------------------------+
```

---

## 2. Firmware & Hardware Panel Negotiation

To eliminate inverted or backwards display flashing, `AG_EPD Tag` queries the tag firmware version upon detection:

$$\text{VersionCode} = \text{Major} \times 100 + \text{Minor} \times 10 + \text{Build}$$

- **Firmware $\ge 4.0.0$ (DKE E-Paper Panels)**:
  Uses bottom-up vertical row scanning (`img_forDKEEPD_BW`):
  $$Y_{\text{sample}} = (\text{Height} - 1) - (j + k)$$
  Uses $4096\text{B}$ LZ4-HC segmented compression chunks with a 2-byte little-endian header.
- **Firmware $< 4.0.0$ (Legacy Panels)**:
  Uses top-down vertical row scanning (`img_forEPD_BW`):
  $$Y_{\text{sample}} = j + k$$
  Uses $1024\text{B}$ LZ4-HC chunks or uncompressed streaming.

---

## 3. Communication Protocol Details
1. **PIN Unlock**: Transmits ASCII `"0000"` (`0x30, 0x30, 0x30, 0x30`) via `CMD_PINCODE_UNLOCK` (`0xA1`).
2. **Flash Erase**: Sends `CMD_ERASE_IMAGE_FLASH` (`0x80`) with `lz4flag = 1`.
3. **Data Streaming**: Streams 240-byte chunks via `CMD_WRITE_IMAGE_FLASH_NOACK` (`0x8E`).
4. **Flash Verification**: Issues `CMD_CHECK_IMAGE_FLASH` (`0x82`) to verify MCU CRC.
5. **Physical Refresh**: Triggers e-paper waveform update via `CMD_END_WRITE_FLASH_AND_EPD` (`0x85`) with parameters `[pages=1, width=296, height=128]`.
