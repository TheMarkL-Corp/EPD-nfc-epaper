# Learning Module 03: AdvNFCWrap High-Level SDK Architecture

## 1. Executive Summary
`AdvNFCWrap.dll` provides a developer-friendly C# facade over `AdvNFC.dll` and `RFID.dll`. It adds asynchronous task wrappers (`async/await`), tag event dispatching, automatic COM port discovery, image preprocessing, color conversion, and dithering algorithms.

---

## 2. Architecture & Class Diagram

```
+-------------------------------------------------------------+
|                       NFCWrap Facade                        |
|  - ConnectTagAsync() / DisconnectTag()                      |
|  - GetPort() / GetTagID() / GetVersion()                    |
|  - DrawImageAsync(Bitmap, bool bDithering)                  |
|  - UnlockPinCode(pin) / SetPingCode(pin)                    |
+-------------------------------------------------------------+
         |                       |                     |
         v                       v                     v
+-----------------+    +------------------+    +---------------+
|   NFCManager    |    |  ImageGenerator  |    |  BSCAdjuster  |
|  - Tag Listener |    |  - EPDModel      |    |  - Brightness |
|  - EDP API      |    |  - Dithering     |    |  - Sharpness  |
|  - Buffer Q     |    |  - DirectBitmap  |    |  - Contrast   |
+-----------------+    +------------------+    +---------------+
```

---

## 3. High-Level API Methods (`AdvNFCWrap.NFCWrap`)

### Connection & Discovery
- `string GetPort()`: Scans all system COM ports, tests each with `RFID_OpenReader` and `RFID_FWVersion`, and returns the active reader port (or `"0101"` if none found).
- `string ConnectTag()` / `Task<string> ConnectTagAsync()`: Initializes NFC state machine on the configured port.
- `string DisconnectTag()`: Gracefully halts NFC polling and closes COM port.
- `string GetTagID()`: Returns the 16-hex-digit UID of the currently present tag.
- `string GetVersion()`: Returns firmware version (e.g. `"1.3.2"`).
- `string GetPlatformName()`: Returns device model string (e.g. `"EPD-210"`).
- `string GetSN()`: Returns 12-byte serial number string.

### Image Transmission APIs
- `string DrawImage(Bitmap oImage)`: Synchronously transmits bitmap to display.
- `string DrawImage(Bitmap oImage, bool bDithering)`: Applies Floyd-Steinberg dithering before transmission.
- `Task<string> DrawImageAsync(Bitmap oImage)`: Asynchronous non-blocking image upload.
- `Task<string> DrawImageAsync(Bitmap oImage, bool bDithering)`: Asynchronous upload with optional dithering.

### Security & Storage APIs
- `string UnlockPinCode(string strData)`: Unlocks tag using 4-character PIN.
- `string SetPingCode(string strData)`: Updates access PIN code.
- `string GetPinCodeStatus()`: Returns PIN status (`0x00` = unlocked/no PIN, `0x01` = locked).
- `string WriteTagData(string strData)` / `string GetTagData()`: Writes/reads EEPROM user storage.
- `string WriteTagDataFlash(string strData)` / `string GetTagDataFlash()`: Writes/reads external Flash user storage.

---

## 4. Supported Display Hardware Models (`AdvNFCWrap.model.EPDModel`)

| Model Name | Resolution (W x H) | Color Mode | Raw Buffer Size | Image Reversed |
| :--- | :--- | :--- | :--- | :--- |
| **EPD-210** | 296 x 128 (2.13") | 1-bit Black/White | 4,736 Bytes (4.62 KB) | True (Mirrored X) |
| **EPD-302** | 416 x 240 (3.7") | 1-bit Black/White | 12,480 Bytes (12.18 KB)| True (Mirrored X) |
| **EPD-303** | 416 x 240 (3.7") | 3-Color (Red/Black/White) | 24,960 Bytes (24.37 KB)| True (Mirrored X) |
| **EPD-304** | 416 x 240 (3.7") | 4-Color (BWYR - Black/White/Yellow/Red)| 24,960 Bytes (24.37 KB)| True (Mirrored X) |

---

## 5. SDK Callback Interfaces

### `TagState` Interface
```csharp
public interface TagState
{
    void onTagState(nTagState state);
}
// States: NFC_TAG_STATE_TAG_OFF (0), NFC_TAG_STATE_TAG_ON (1), NFC_TAG_STATE_COMM_ON (2)
```

### `ProcessState` Interface
```csharp
public interface ProcessState
{
    void onProcessState(nImageState state, object data);
}
// States: DIState_Erase (0), DIState_SendData (1, data = 0..100%), DIState_WriteToEPD (2), DIState_Finish (3), DIState_Error (4)
```
