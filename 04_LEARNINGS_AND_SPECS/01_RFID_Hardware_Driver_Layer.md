# Learning Module 01: RFID Hardware Driver Layer & Reader Communications

## 1. Executive Summary
The RFID Hardware Driver Layer is encapsulated in `RFID.dll` (Namespace `J_RFID`, Class `RFIDAPI`). It serves as the physical and transport interface between the host computer and the Jogtek NFC/RFID Reader. It abstracts serial communication, handles ISO/IEC 15693 / ISO 14443 standard RFID protocols, and provides specialized hooks for STMicroelectronics ST25DV dynamic tags and Fujitsu FRAM tags.

---

## 2. Hardware Interface & Serial Transport Architecture
The Jogtek reader operates as a Virtual COM Port (USB-to-UART bridge) with the following UART serial parameters:
- **Baud Rate**: 115,200 bps
- **Data Bits**: 8 bits
- **Parity**: None
- **Stop Bits**: 1 (8N1)
- **Encoding**: Default / ASCII
- **Read/Write Timeout**: 50 ms

### Reader Message Envelope Structure
Commands sent from the host to the reader over UART follow a fixed-frame format:

```
+----+--------+------------+----+----+---------+-------------------+----+----+
| SOF| Length | PacketType | H1 | H2 | Command | Payload / Data    | T1 | T2 |
| 01 | [LEN]  | 00         | 03 | 04 | [CMD]   | [VAR ARGS...]     | 00 | 00 |
+----+--------+------------+----+----+---------+-------------------+----+----+
```

- **SOF (`01`)**: Start of Frame byte (ASCII hex `01`).
- **Length (`[LEN]`)**: 1-byte hex string representing the length of the command frame from Length to Tail.
- **Header Constants (`00 03 04`)**: Reader transport envelope header.
- **Command (`[CMD]`)**: The specific internal reader command or subcarrier mode.
- **Payload / Arguments**: Target Flags, Tag UID (8 bytes reversed / little-endian hex), Block addresses, and Data.
- **Tail (`00 00`)**: End of frame terminator bytes.

---

## 3. Core Reader Hardware APIs (`RFIDAPI`)

### Reader Lifecycle Management
- `RFID_OpenReader(string COMPort)`: Opens the specified serial port (e.g., `COM3`), verifies communication by sending ping frame `0108000304FF0000`, and sets up timeouts.
- `RFID_CloseReader(string COMPort)`: Closes and disposes of the `SerialPort` instance.
- `RFID_FWVersion(out string FirmwareVer)`: Queries the reader microcontroller firmware version.
- `RFID_R_ReaderSN(out string SN)`: Reads the reader's 8-character unique hardware serial number.
- `RFID_W_ReaderSN(string SN)`: Burns an 8-character serial number into reader non-volatile memory.
- `RFID_AntennaControl(byte Select)`: Toggles the 13.56 MHz RF field (1 = Enable RF Field, 0 = Disable RF Field).
- `HF_CA()`: Sends Carrier Activation (`0108000304CA0000`).

---

## 4. ISO 15693 & ST25DV RF Commands

### UID Inversion Mechanism
ISO 15693 tags transmit their 64-bit (8-byte) UID with LSB first over the air. `RFIDAPI` reverses the 16-hex-character string between RF and host presentation:
$$\text{UID}_{\text{Host}} = \text{UID}_{\text{RF}}[14..16] + \text{UID}_{\text{RF}}[12..14] + \dots + \text{UID}_{\text{RF}}[0..2]$$

### ISO 15693 & ST25DV Command Matrix
| API Method | Reader Hex Command Code | Protocol Function |
| :--- | :--- | :--- |
| `RFID_ISO15693Inventory` | `0x01` | Discovers tags in the RF field and retrieves UID + RSSI |
| `RFID_ISO15693StayQuiet` | `0x02` | Silences an addressed tag |
| `RFID_ISO15693Select` | `0x25` | Selects a specific tag |
| `RFID_ISO15693Reset2Ready` | `0x26` | Resets tag state to Ready |
| `RFID_ISO15693Read` | `0x20` | Reads a single 4-byte block (standard 8-bit block address) |
| `RFID_ISO15693Write` | `0x21` | Writes 4 bytes to a single EEPROM block |
| `RFID_ISO15693LockBlock` | `0x22` | Permanently locks an EEPROM block from further writes |
| `RFID_ST25DVRead` | `0x30` | Extended Read Single Block (16-bit block address for 64K tags) |
| `RFID_ST25DVWrite` | `0x31` | Extended Write Single Block (16-bit block address) |

---

## 5. Fast Transfer Mode (FTM) / Mailbox Fast Commands
To transmit high-resolution e-paper bitmap images (e.g. 25 KB) in milliseconds, writing to EEPROM is too slow (5 ms write cycle per 4 bytes = 31 seconds). The solution uses the **ST25DV Fast Transfer Mode (FTM)** dual-port RAM mailbox:
- **`CMD_FAST_WRITE_MESSAGE` (`0xCA`)**: Directly streams up to 256 bytes per packet into tag RAM buffer.
- **`CMD_FAST_READ_MESSAGE` (`0xCC`)**: Reads MCU response buffer from mailbox RAM.
- **`CMD_FAST_READ_MESSAGE_LENGTH` (`0xCB`)**: Gets number of bytes waiting in mailbox.
- **`CMD_FAST_READ/WRITE_DYN_CONFIGURATION` (`0xCD` / `0xCE`)**: Reads and writes dynamic registers (Mailbox Control at address `0x0D`, Energy Harvesting at address `0x02`).

---

## 6. Status & Error Codes
- `0`: Success (`NFC_SUCCESS`)
- `1`: Serial port communication error / timeout
- `2`: Unable to open COM port (port in use or disconnected)
- `3`: Unable to close COM port
- `4`: RF tag command failure / no ACK
- `6`: Invalid parameter length or format
