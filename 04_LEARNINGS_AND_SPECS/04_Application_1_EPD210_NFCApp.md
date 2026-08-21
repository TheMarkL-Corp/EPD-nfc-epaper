# Learning Module 04: Application 1 - EPD-210 NFC Desktop Application

## 1. Overview
`NFCApp.exe` is the specialized desktop application designed for production and evaluation of the **EPD-210 (2.13" 296x128 B/W NFC E-Paper)** display. It demonstrates tag interrogation, manual bitmap generation, barcode and QR code synthesis, PIN security, and automated CSV-driven batch flashing.

---

## 2. Architecture & UI Form Structure

```
+-------------------------------------------------------------------+
|                           NFCApp.exe                              |
+---------------------------------+---------------------------------+
|   Form1: Single Tag Management  |   Form2: Batch Automation       |
|  - Serial Port Selection        |  - CSV Import (`sample2.csv`)   |
|  - Tag Interrogation (UID/Ver)  |  - Automated Tag Sensor Trigger |
|  - PIN Lock / Unlock            |  - Real-time Label Rendering    |
|  - Barcode + QR Canvas Builder  |  - Auto Dequeue on Flash Success|
|  - 1-Click Flash Burning        |  - Sound Effects (Beep / Error) |
+---------------------------------+---------------------------------+
```

---

## 3. Form 1: Interactive Tag Console & Label Builder

### GDI+ Label Composition Pipeline (`btnCreateImage2_Click`)
Form 1 constructs a 296 x 128 32-bit ARGB bitmap composed of multiple dynamic elements:

```csharp
Bitmap bitmap = new Bitmap(296, 128, PixelFormat.Format32bppArgb);
Graphics graphics = Graphics.FromImage(bitmap);
graphics.Clear(Color.White);

// 1. Draw Text Fields
Font font = new Font("Arial", 12f);
SolidBrush brush = new SolidBrush(Color.Black);
graphics.DrawString("CG4C001501", font, brush, 10f, 10f);
graphics.DrawString("2521M99G14", font, brush, 10f, 30f);
graphics.DrawString("SER CDACURUC", font, brush, 10f, 50f);

// 2. Generate 1D Code 128 Barcode (BarcodeStandard.dll)
Barcode barcode = new Barcode { IncludeLabel = true, LabelFont = new Font("Verdana", 4f), Width = 220, Height = 50 };
Image barcodeImg = barcode.Encode(TYPE.CODE128, "710176121145", 220, 50);
graphics.DrawImage(barcodeImg, new Point(0, 70));

// 3. Generate 2D QR Code (Gma.QrCodeNet.Encoding.dll)
QrEncoder qrEncoder = new QrEncoder(ErrorCorrectionLevel.H);
qrEncoder.TryEncode("ITEM0012345678", out QrCode qrCode);
GraphicsRenderer renderer = new GraphicsRenderer(new FixedModuleSize(3, QuietZoneModules.Two), Brushes.Black, Brushes.White);
renderer.Draw(graphics, qrCode.Matrix, new Point(210, 5));

// 4. Burn to EPD
oNFC.DrawImage(bitmap);
```

---

## 4. Form 2: Production Batch Auto-Flashing Engine

Form 2 automates assembly line flashing using CSV product databases:
- **Input Data Format (`sample2.csv`)**: Semicolon-delimited records with JSON payload:
  `PRODUCT; {"PROD NAME": "MIO-5375C7P-Q4A1", "PRICE": "37,055", "BARCODE": "710176121145", "RATE": "TWD"}`
- **Automated Workflow**:
  1. An operator places an EPD-210 tag onto the reader.
  2. State changes to `NFC_TAG_STATE_COMM_ON`.
  3. Form 2 fetches Tag UID and immediately dequeues row 0 from `DataGridView`.
  4. Dynamically renders product name, price, barcode, and QR code.
  5. Streams image over NFC to tag flash and triggers screen refresh.
  6. On `DIState_Finish`, removes completed row from list and plays `SystemSounds.Beep`.
