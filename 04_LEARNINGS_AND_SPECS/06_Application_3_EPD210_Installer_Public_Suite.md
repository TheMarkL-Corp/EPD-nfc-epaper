# Learning Module 06: Application 3 - Public Installer & Deployment Suite

## 1. Overview
The third application artifact is the official public release distribution package: `EPD-210 NFC.msi` (Version 1.3.2). It encapsulates the complete enterprise deployment runtime, installer manifest, native architecture dependencies, and system integration.

---

## 2. Installer Manifest & Dependency Graph

```
+-------------------------------------------------------------------+
|                     EPD-210 NFC Installer (MSI)                   |
+-------------------------------------------------------------------+
  |-- NFCApp.exe                  (Target Application Executable)
  |-- NFCApp.exe.config           (Runtime .NET Binding Redirects)
  |-- AdvNFCWrap.dll              (High-Level C# Wrapper)
  |-- AdvNFC.dll                  (Core Protocol & State Engine)
  |-- RFID.dll                    (Serial / Jogtek Driver)
  |-- statemap.dll                (SMC FSM Runtime Engine)
  |-- Lz4Net.dll                  (Managed LZ4 Compression Interface)
  |-- x86/lz4X86.dll              (32-bit Native High-Speed LZ4 Core)
  |-- x64/lz4X64.dll              (64-bit Native High-Speed LZ4 Core)
  |-- BarcodeStandard.dll         (1D Barcode Rendering Engine)
  |-- Gma.QrCodeNet.Encoding.dll  (2D QR Code Generator)
  |-- sample2.csv                 (Default Production Batch Template)
```

---

## 3. Native Architecture Bridge (x86 vs x64)
Because image compression must run in microseconds to maintain real-time NFC throughput, `Lz4Net.dll` dynamically loads the appropriate native binary based on OS architecture:
- On 32-bit Windows / x86 processes: loads `x86/lz4X86.dll`
- On 64-bit Windows / x64 processes: loads `x64/lz4X64.dll`

---

## 4. .NET Framework Runtime Environment
The applications target **.NET Framework 4.5 / 4.8** with assembly binding redirections for `System.Runtime`, `System.Threading.Tasks`, and `System.Buffers` configured inside `NFCApp.exe.config`.
