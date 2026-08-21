# MedTRx EPD Programmer (v1.0.1) — User Manual / 使用手冊

---

## English Version

### 1. Overview
**MedTRx EPD** is a portable standalone Windows utility designed for wirelessly programming **2.13-inch (296x128 B/W) EPD-210 NFC E-Paper tags** using standard Jogtek 13.56 MHz RFID / NFC readers.

---

### 2. Quick Start Guide
1. **Connect Hardware**: Plug your USB NFC Reader into your computer.
2. **Launch Application**: Double-click `MedTRx_EPD.exe`. No installation is required.
3. **Auto COM Port Detection**: The software will automatically detect and connect to the active RFID reader port on startup.
4. **Select Language**: Click the language dropdown in the top-right corner to toggle between **English** and **繁體中文 (Traditional Chinese)**.

---

### 3. Screen Layout & Functions
* **Top Header Bar**:
  * **COM Port**: Displays the connected serial port. The application remembers the last successfully connected port.
  * **Refresh**: Non-blocking asynchronous port scanning to detect newly connected readers.
  * **Tag Status Indicator**:
    * 🔴 **Red (No Tag Detected)**: Reader is active, waiting for a tag.
    * 🟡 **Yellow (Tag Detected - Connecting...)**: Tag detected on RF field, performing UID handshake.
    * 🟢 **Green (Tag Ready to Write)**: Tag successfully authenticated. Displays Tag **UID** and **Firmware Version**.
* **Live WYSIWYG Tag Preview**:
  * Displays a 1-to-1 pixel-accurate visualization of the tag display (296 x 128 px).
* **Tag Text Settings**:
  * **Header Text**: Content displayed at the upper-left banner section.
  * **Main Body Text**: Prominent title displayed in the lower area.
  * *Note: Leaving either input blank leaves that area clean without writing placeholder text.*
* **Display Layout Style**:
  * **Style B (Clean White) [Default]**: White background across the tag with black text.
  * **Style A (Black Header Banner)**: Solid black header bar with crisp white header text and white lower body with black text.
  * **Show Rounded Outer Border**: Toggle the perimeter rounded frame ON/OFF.
* **Tag Writing / Programming**:
  * Place the 2.13" EPD tag flat on the reader.
  * Click **"Write Tag"** (or **"寫入標籤"**).
  * Keep the tag steady while the progress bar advances from 0% to 100%.
  * An acoustic beep will sound upon completion, and the E-Paper display will refresh.

---

## 繁體中文版本 (Traditional Chinese)

### 1. 系統簡介
**MedTRx EPD** 為綠色免安裝軟體，專為 **2.13 吋 (296×128 黑白) EPD-210 NFC 電子紙標籤** 所設計之無線寫入與燒錄工具。

---

### 2. 快速使用步驟
1. **硬體連接**：將 USB NFC 讀卡機插入電腦。
2. **啟動軟體**：雙擊執行 `MedTRx_EPD.exe` 即可直接使用，無需繁複安裝。
3. **自動序列埠偵測**：軟體啟動時會自動搜尋並連接讀卡機通訊埠，並自動記憶上次成功連線的通訊埠。
4. **切換語言**：右上角下拉選單可即時切換 **繁體中文** 與 **English**。

---

### 3. 介面功能說明
* **頂部控制列**：
  * **序列埠 (COM)**：顯示當前連線埠，具備記憶功能。
  * **重新整理**：非同步背景掃描，不會造成畫面凍結。
  * **標籤狀態燈號**：
    * 🔴 **紅燈 (未感應到標籤)**：讀卡機待機中。
    * 🟡 **黃燈 (已感應標籤 - 連線中...)**：感應到標籤，正在讀取 UID 與韌體版本。
    * 🟢 **綠燈 (標籤就緒 - 可寫入)**：標籤就緒，顯示卡號 UID 與韌體版本。
* **即時標籤畫面預覽 (2.13 吋 296×128)**：
  * 所見即所得 (WYSIWYG) 預覽，文字過長會自動等比例縮放以確保清晰顯示。
* **標籤文字設定**：
  * **頂欄文字 (Header)**：顯示於左上方頂欄之文字。
  * **主要內容文字 (Body)**：顯示於下方中央之主要文字。
  * *註：若留白不輸入，標籤該區塊即保持全白，不會寫入任何預設文字。*
* **標籤顯示樣式**：
  * **樣式 B：白底黑字 (預設)**：全白底色，經典簡約排版。
  * **樣式 A：黑底頂欄**：上方黑色橫條搭白色文字，下方白底黑字。
  * **顯示外圍圓角外框**：可自由勾選是否繪製外圍圓角邊框。
* **寫入標籤**：
  * 將 2.13 吋電子紙標籤平放於讀卡機上。
  * 點擊 **"寫入標籤"** 按鈕。
  * 請保持標籤穩定平放，進度條將由 0% 進行至 100%。
  * 寫入成功後將發出提示音，電子紙螢幕即更新完成。

---

## 4. Troubleshooting / 常見問題排除
1. **Status shows "No COM Ports Found" / 顯示未找到通訊埠**:
   - Verify USB cable connection. If needed, install the FTDI USB Virtual COM port driver from `CDM2123620_Setup_NFC_Driver.zip`.
2. **Tag write failed midway / 寫入中途失敗**:
   - Ensure the tag is placed flat on the antenna surface without moving it during the 3-4 second flashing sequence.
