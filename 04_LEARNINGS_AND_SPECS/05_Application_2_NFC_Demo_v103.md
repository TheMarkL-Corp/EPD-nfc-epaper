# Learning Module 05: Application 2 - NFC Demo v1.0.3 Multi-Panel Suite

## 1. Overview
`NFC_Demo.exe` (v1.0.3) is an advanced multi-panel demonstration and production station. It expands capabilities beyond single 2.13" tags to include **3.7" high-resolution panels**, **multi-color (Red/Black/White and 4-Color BWYR) displays**, **integrated WebSocket remote control**, and **live dithering previews**.

---

## 2. Key Enhancements over Application 1

| Feature | EPD-210 NFCApp (App 1) | NFC Demo v1.0.3 (App 2) |
| :--- | :--- | :--- |
| **Supported Displays** | EPD-210 (296x128 B/W) only | EPD-210, EPD-302 (416x240 B/W), EPD-303 (416x240 BWR), EPD-304 (416x240 BWYR) |
| **Network Control** | None (Local GUI only) | Built-in WebSocket Server (Port 8082, `/notify`) |
| **Image Adjustments**| None | Live Brightness, Contrast, Sharpness (BSC) Adjuster |
| **Dithering Engine** | Simple Thresholding | Multi-color Floyd-Steinberg Error Diffusion |
| **Orientation Handling**| Fixed Landscape | Automatic 270-degree Portrait-to-Landscape normalization |
| **Configuration** | Static XML Config | Dynamic `config.json` + `sample.csv` |

---

## 3. Headless Automation via WebSocket Server

`NFC_Demo` hosts an internal WebSocket service using `websocket-sharp`:

```csharp
private void newWebSocket()
{
    webSocketServer = new WebSocketServer(8082);
    webSocketServer.AddWebSocketService("/notify", () => new NotifyBehavior(HandleMessageReceived));
    webSocketServer.Start();
}

private void HandleMessageReceived(string message)
{
    if ("reset".Equals(message))
    {
        ResetImage(); // Clears display to blank white
    }
}
```

This enables external MES (Manufacturing Execution Systems), ERPs, or automated scripts on the local network to trigger tag resets or display updates without user interaction.

---

## 4. Multi-Color Storage Bin Picking Template
Form 1 renders complex warehouse picking lists with red alert highlights:
- Header Part Number in **Bold Red** (`#FF0000`)
- Description, Storage Bin location, Requested Quantity, Storage Quantity
- 2D QR Code for inventory scanning (`QRCoder.dll`)
- Live preview canvas updating in real-time as rows are clicked.
