# Learnings, Protocol Specifications & Architecture Notes

This directory contains the complete reverse-engineered protocol documentation, hardware driver specifications, and architecture breakdown for 2.13" EPD NFC E-Paper programming.

---

## Document Index

| # | Document | Description |
|---|---|---|
| **01** | [`01_RFID_Hardware_Driver_Layer.md`](./01_RFID_Hardware_Driver_Layer.md) | Jogtek RFID USB/Serial reader communication protocol, baud rates, and native ISO15693 commands. |
| **02** | [`02_AdvNFC_Protocol_and_State_Machine.md`](./02_AdvNFC_Protocol_and_State_Machine.md) | ST25DV Fast Transfer Mode (FTM) protocol, packet fragmentation, and state machine workflow. |
| **03** | [`03_AdvNFCWrap_High_Level_SDK.md`](./03_AdvNFCWrap_High_Level_SDK.md) | High-level managed C# wrapper API analysis, event dispatching, and background workers. |
| **04** | [`04_Application_1_EPD210_NFCApp.md`](./04_Application_1_EPD210_NFCApp.md) | Architectural review of the standard EPD-210 application UI and flashing pipeline. |
| **05** | [`05_Application_2_NFC_Demo_v103.md`](./05_Application_2_NFC_Demo_v103.md) | Analysis of the NFC Demo v1.0.3 sample application codebase. |
| **06** | [`06_Application_3_EPD210_Installer_Public_Suite.md`](./06_Application_3_EPD210_Installer_Public_Suite.md) | Breakdown of dependencies, runtime prerequisites, and setup package structure. |
| **07** | [`07_Image_Processing_and_Dithering_Pipeline.md`](./07_Image_Processing_and_Dithering_Pipeline.md) | 296x128 1-bit bitmap transformation, Floyd-Steinberg dithering, row packing, and LZ4 compression. |
| **08** | [`08_Recreation_Blueprint_and_API_Reference.md`](./08_Recreation_Blueprint_and_API_Reference.md) | Complete step-by-step developer blueprint for building custom NFC EPD flashing applications. |
| **09** | [`09_Application_4_LEO_D30_Factory_Tool.md`](./09_Application_4_LEO_D30_Factory_Tool.md) | In-depth analysis of the Linchun Factory Tool, firmware auto-negotiation, and screen inversion prevention. |
| **Sum** | [`CONSOLIDATED_SUMMARY.md`](./CONSOLIDATED_SUMMARY.md) | High-level synthesis of findings, architecture decisions, and key technical takeaways. |