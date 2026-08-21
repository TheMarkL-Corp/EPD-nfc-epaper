# Learning Module 09: Application 4 - LEO-D30 Factory Tool (Linchun SDK)

## 1. Executive Summary
The **LEO-D30 Factory Tool** (located in `Linchun SDK\EPD30x_Factory tool_9319_25993_316`) is the newest, lightest, and lowest-overhead testing tool developed for production lines and R&D validation. 

Unlike Applications 1 and 2, which wrap functionality in heavy layers with GDI+ barcode builders, web servers, or `AdvNFCWrap.dll`, the Factory Tool connects **directly to `AdvNFC.dll` and `RFID.dll`** with zero wrapper overhead. The executable is a compact **23 KB**.

---

## 2. Architecture: Direct Driver Binding

```
+-------------------------------------------------------------------+
|               LEO-D30 Factory Tool.exe (23 KB)                    |
+-------------------------------------------------------------------+
                                  |
                                  | Direct Native C# Calls (No AdvNFCWrap.dll)
                                  v
+-------------------------------------------------------------------+
|  AdvNFC.dll (NFCManager, LeoD30EPDAPI, D30Command, EinkImage)     |
+-------------------------------------------------------------------+
                                  |
                                  | UART COM Port Framing (115200 8N1)
                                  v
+-------------------------------------------------------------------+
|  RFID.dll (RFIDAPI - Jogtek ISO15693 / ST25DV FTM Driver)         |
+-------------------------------------------------------------------+
```

### Direct Connection Sequence
```csharp
private void buildConnection()
{
    nfc = new D30Command(comboBoxPorts.Text);
    if (nfc.openNFC())
    {
        manager.setNFCCommand(nfc);
        api = manager.getNfcAPI();
    }
    else
    {
        nfc = null;
        MessageBox.Show("Cannot open NFC Reader");
    }
}
```

---

## 3. Advanced Model, Firmware & Compression Matrix

The Factory Tool introduces fine-grained hardware model detection based on Platform Name strings and numeric Firmware Version comparisons (`Major * 100 + Minor * 10 + Build`):

| Platform Name (`GetPlatformName()`) | FW Version (`GetVersion()`) | Color / Pages | Dimensions | Segment Packet Size | DKE Reverse |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **`EPD-210--TC2`** | $< 1.0.0$ | 1-bit B/W (1 page) | $296 	imes 128$ | LZ4 Disabled (`lz4=0`) | Standard (`img_forEPD_BW`) |
| **`EPD-210--TC2`** | $1.0.0 \dots 3.9.9$ | 1-bit B/W (1 page) | $296 	imes 128$ | LZ4 Enabled (`lz4packsize=1024B`) | Standard (`img_forEPD_BW`) |
| **`EPD-210--TC2`** | $\ge 4.0.0$ | 1-bit B/W (1 page) | $296 	imes 128$ | LZ4 Enabled (`lz4packsize=4096B`) | **DKE Mode** (`img_forDKEEPD_BW`) |
| **`D30-ED29-TC2`** | Any | 1-bit B/W (1 page) | $296 	imes 128$ | LZ4 Disabled (`lz4=0`) | Standard |
| **`D30-EL29-TC2`** | Any | 1-bit B/W (1 page) | $296 	imes 128$ | LZ4 Disabled (`lz4=0`) | Standard |
| **`EPD-302--TC2`** | Any | 1-bit B/W (1 page) | $416 	imes 240$ | LZ4 Enabled (`lz4packsize=5120B`) | Standard |
| **`EPD-303--TC2`** | Any | 3-Color BWR (2 pages) | $416 	imes 240$ | LZ4 Enabled (`lz4packsize=5120B`) | Standard |
| **`EPD-304--TC2`** | Any | 4-Color BWYR (2 pages) | $416 	imes 240$ | LZ4 Enabled (`lz4packsize=5120B`) | Standard |

---

## 4. DKE E-Paper Memory Layout vs Standard E-Paper

A critical discovery in this version is the physical raster difference for DKE displays (`img_forDKEEPD_BW`):

### Standard EPD (`img_forEPD_BW`):
Scans rows from top to bottom ($j = 0 \dots Height-1$):
```csharp
for (int i = 0; i < width; i++) {
    for (int j = 0; j < height; j += 8) {
        // Samples pixel at (width - i - 1, j + k)
    }
}
```

### DKE EPD (`img_forDKEEPD_BW` - FW $\ge 4.0.0$):
Scans rows from bottom to top ($num4 = Height - 1 \dots 0$):
```csharp
for (int i = 0; i < width; i++) {
    for (int num4 = height - 1; num4 > 0; num4 -= 8) {
        // Samples pixel at (width - i - 1, num4 - j)
    }
}
```

---

## 5. Segmented High-Compression LZ4 Engine (`Lz4comp_segment`)

Rather than compressing the entire image buffer as a single monolithic block, the Factory Tool segments the buffer into fixed chunks (`packsize` = 1024, 4096, or 5120 bytes) using `LZ4_compressHC`:

1. **Chunk Header**: Each compressed segment is prefixed with a 2-byte little-endian length:
   - `Header[0] = (byte)(chunk_compressed_size & 0xFF)`
   - `Header[1] = (byte)((chunk_compressed_size >> 8) & 0xFF)`
2. **Payload**: Followed immediately by `chunk_compressed_size` bytes of LZ4-HC compressed payload.
3. **Tail Delimiter**: The multi-segment stream is terminated with carriage return / line feed `\r\n` (`0x0D, 0x0A`).
4. **Tag Decompression**: The tag microcontroller receives the segments over the ST25DV mailbox and decompresses each block directly into display RAM.

---

## 6. Factory UI & Real-Time Production Features
- **LED State Indicators**:
  - `red_led.png` = `NFC_TAG_STATE_TAG_OFF` (No tag on RF carrier field)
  - `yellow_led.png` = `NFC_TAG_STATE_TAG_ON` (Tag UID read, performing FTM handshake)
  - `green_led.png` = `NFC_TAG_STATE_COMM_ON` (Tag ready; `btnUpdate` automatically enabled)
- **Spend Time Counter**: 100ms timer (`spendtimer`) measuring exact upload duration in tenths of a second.
- **Factory Log Generator (`Logfile.Devinfo_Log`)**: Writes device logs recording Tag UID (`DeviceID`) and `FWVersion`.
- **Default PIN Unlock**: Automatically sends ASCII zero password `[48, 48, 48, 48]` (`"0000"`).
