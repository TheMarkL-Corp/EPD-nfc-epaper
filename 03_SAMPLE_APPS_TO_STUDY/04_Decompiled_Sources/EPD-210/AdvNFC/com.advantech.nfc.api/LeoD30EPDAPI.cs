using com.advantech.nfc.cmd;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace com.advantech.nfc.api
{
	public class LeoD30EPDAPI : INFCEDPAPI, NFCSTATEChangeCallback
	{
		private const int DEFAULT_TIMEOUT = 1000;

		private const string INVALID_VERSION = "??";

		private const string TAG = "LEOAPI";

		public const int ACK_SUCCESS = 1;

		public const int ACK_ERROR = 0;

		public const int CMD_VERSION = 240;

		public const int CMD_PLATFORM_NAME = 241;

		public const int CMD_BOOT_TO_LOADER = 244;

		public const int CMD_GET_SN = 246;

		public const int CMD_ERASE_IMAGE_FLASH = 128;

		public const int CMD_WRITE_IMAGE_FLASH = 129;

		public const int CMD_CHECK_IMAGE_FLASH = 130;

		public const int CMD_WRITE_USER_DATA_FLASH = 131;

		public const int CMD_READ_USER_DATA_FLASH = 132;

		public const int CMD_END_WRITE_FLASH_AND_EPD = 133;

		public const int CMD_GET_EPD_STATUS = 136;

		public const int CMD_WRITE_IMAGE_FLASH_NOACK = 142;

		public const int CMD_WRITE_EPD = 144;

		public const int CMD_PINGCODE_STATUS = 160;

		public const int CMD_PINCODE_UNLOCK = 161;

		public const int CMD_PINCODE_RESET = 162;

		public const int CMD_PINCODE_SET = 163;

		public const int CMD_SYSTEM_SET = 164;

		public const int BL_CMD_GET_CHIP_ID = 0;

		public const int BL_CMD_GET_PRODUCT_ID = 1;

		public const int BL_CMD_CHECK_USER_AREA_EMPTY = 6;

		public const int BL_CMD_GET_USER_AP_VERSION = 11;

		public const int BL_CMD_GET_BOOTLOADER_INFO = 16;

		public const int BL_CMD_ERASE_FLASH = 4;

		public const int BL_CMD_WRITE_BLOCK = 7;

		public const int BL_CMD_WRITE_CHECKSUM = 9;

		public const int BL_CMD_WRITE_AP_SIZE = 25;

		public const int BL_CMD_WRITE_AP_VERSION = 10;

		public const int BL_CMD_WRITE_BOOTLOADER_INFO = 15;

		public const int BL_CMD_WRITE_CHECKSUM_TAG = 12;

		public const int BL_CMD_READ_CHECKSUM_TAG = 13;

		public const int BL_CMD_WRITE_COMPLETE_TAG = 14;

		public const int BL_CMD_RESET_SYSTEM = 18;

		public const int BL_CMD_GET_BOOTLOADER_VERSION = 3;

		public const int BL_CMD_WRITE_BOOTLOADER_VERSION = 5;

		public const int BL_CMD_READ_BOOTLOADER_INFO = 16;

		private const int CMD_SHORT_TIMEOUT = 1000;

		private const int CMD_LONG_TIMEOUT = 20000;

		private D30Command _d30_command;

		private bool _busy;

		private bool _drawing;

		private NFCState _nfcState;

		private bool check_epd;

		private bool _enable_log = true;

		private bool testFlag;

		public LeoD30EPDAPI(INFCCommand nfc_command, NFCState state)
		{
			_d30_command = (D30Command)nfc_command;
			_nfcState = state;
			_nfcState.setStateChangeCallback(this);
			_busy = false;
			_drawing = false;
			testFlag = false;
		}

		public void onNFCStateChange(NFCSTATE new_state)
		{
			check_epd = (new_state == NFCSTATE.NFCSTATE_READY || new_state == NFCSTATE.NFCSTATE_BUSY);
		}

		protected void log(string s)
		{
			bool enable_log = _enable_log;
		}

		private bool waitTxReady(int count)
		{
			while (count > 0)
			{
				if (_nfcState.readyToTx())
				{
					return true;
				}
				Thread.Sleep(1);
				count--;
			}
			return false;
		}

		private bool checkChecksum(byte[] recv)
		{
			int num = 0;
			foreach (byte b in recv)
			{
				num += (b & 0xFF);
			}
			return (num & 0xFF) == 0;
		}

		private void clearRx()
		{
			while (_nfcState.getRx() != null)
			{
			}
		}

		private bool CheckResponse(byte[] recv)
		{
			if (recv != null && recv.Length == 3)
			{
				return recv[1] == 1;
			}
			return false;
		}

		private void TxCommand(int command, byte[] data, int timeout_ms)
		{
			clearRx();
			try
			{
				byte[] data2 = _nfcState.buildNFCPacket((byte)command, data);
				if (!waitTxReady(timeout_ms))
				{
					log("wait tx ready timeout");
				}
				else
				{
					_nfcState.addEvent(FTMEventType.FTMEVENT_TX_MESSAGE, data2);
				}
			}
			catch (Exception ex)
			{
				log("exception " + ex.ToString());
			}
		}

		private byte[] TranceiveCommand(int command, byte[] data, int timeout_ms)
		{
			clearRx();
			try
			{
				byte[] data2 = _nfcState.buildNFCPacket((byte)command, data);
				if (waitTxReady(timeout_ms))
				{
					_nfcState.addEvent(FTMEventType.FTMEVENT_TX_MESSAGE, data2);
					while (timeout_ms > 0)
					{
						byte[] rx = _nfcState.getRx();
						if (rx != null)
						{
							if (checkChecksum(rx))
							{
								return rx;
							}
							log("response checksum error");
							return null;
						}
						Thread.Sleep(1);
						timeout_ms--;
					}
					return null;
				}
				log("wait tx ready timeout");
				return null;
			}
			catch (Exception ex)
			{
				log("exception " + ex.ToString());
				return null;
			}
		}

		public bool CheckEPDStatus()
		{
			return CheckResponse(TranceiveCommand(136, null, 1000));
		}

		public string GetPlatformName()
		{
			_busy = true;
			byte[] array = TranceiveCommand(241, null, 1000);
			if (array != null && array.Length == 14)
			{
				string text = "";
				for (int i = 1; i < 13; i++)
				{
					text += Convert.ToChar(array[i]).ToString();
				}
				_busy = false;
				return text;
			}
			_busy = false;
			return "Unkown";
		}

		public byte[] getTagID()
		{
			return NFCManager.getInstance().getTagID();
		}

		public string GetVersion()
		{
			_busy = true;
			byte[] array = TranceiveCommand(240, null, 1000);
			if (array != null && array.Length == 4)
			{
				_busy = false;
				return $"{(int)array[1]}.{(int)array[2]}";
			}
			if (array != null && array.Length == 5)
			{
				_busy = false;
				return $"{(int)array[1]}.{(int)array[2]}.{(int)array[3]}";
			}
			return "??";
		}

		public bool isBusy()
		{
			if (!_busy)
			{
				return _drawing;
			}
			return true;
		}

		public bool isValid()
		{
			if (_d30_command != null)
			{
				return _d30_command.isValid();
			}
			return false;
		}

		public byte[] RxData()
		{
			return _nfcState.getRx();
		}

		public void TestAPI()
		{
			if (!testFlag)
			{
				TxCommand(240, null, 1000);
			}
			else
			{
				byte[] data = new byte[8]
				{
					16,
					2,
					16,
					16,
					16,
					16,
					16,
					3
				};
				_nfcState.addEvent(FTMEventType.FTMEVENT_TX_MESSAGE, data);
			}
			testFlag = !testFlag;
		}

		public void TxData(byte[] data)
		{
			_nfcState.addEvent(FTMEventType.FTMEVENT_TX_MESSAGE, data);
		}

		public string GetSN()
		{
			_busy = true;
			byte[] array = TranceiveCommand(246, null, 1000);
			if (array != null && array.Length == 14)
			{
				string text = "";
				for (int i = 1; i < 13; i++)
				{
					text += $"{array[i]:X2}";
				}
				_busy = false;
				return text;
			}
			_busy = false;
			return "Unkown";
		}

		public byte GetPinCodeStatus()
		{
			_busy = true;
			byte[] array = TranceiveCommand(160, null, 1000);
			if (array != null && array.Length == 3)
			{
				return array[1];
			}
			_busy = false;
			return 16;
		}

		public bool UnlockPinCode(byte[] data)
		{
			if (data.Length != 4)
			{
				return false;
			}
			_busy = true;
			byte[] recv = TranceiveCommand(161, data, 1000);
			_busy = false;
			return CheckResponse(recv);
		}

		public bool SetPinCode(byte[] data)
		{
			if (data.Length != 4)
			{
				return false;
			}
			_busy = true;
			byte[] recv = TranceiveCommand(163, data, 1000);
			_busy = false;
			return CheckResponse(recv);
		}

		public bool ResetPinCode(byte[] data)
		{
			if (data.Length != 8)
			{
				return false;
			}
			_busy = true;
			byte[] recv = TranceiveCommand(162, data, 1000);
			_busy = false;
			return CheckResponse(recv);
		}

		public byte[] SystemRest()
		{
			byte[] data = new byte[8]
			{
				48,
				48,
				48,
				48,
				48,
				48,
				48,
				48
			};
			_busy = true;
			byte[] array = TranceiveCommand(164, data, 1000);
			if (array != null && array.Length == 7)
			{
				return array;
			}
			array[0] = 0;
			array[1] = 0;
			_busy = false;
			return array;
		}

		public async void DrawImage(EinkImage image, DrawImageMethod method, IDrawImageCallback cb)
		{
			if (_drawing)
			{
				throw new NFCException(NFCExceptionType.NFC_EXCEPTION_TYPE_BUSY);
			}
			NFCManager.getInstance();
			if (cb == null)
			{
				throw new NFCException(NFCExceptionType.NFC_EXCEPTION_TYPE_ERROR);
			}
			if (_nfcState.getNFCState() != NFCSTATE.NFCSTATE_READY)
			{
				throw new NFCException(NFCExceptionType.NFC_EXCEPTION_TYPE_BUSY);
			}
			image.getData();
			image.getPages();
			if (method == DrawImageMethod.DIMethod_Normal)
			{
				await Task.Factory.StartNew(delegate
				{
					DrawImageNormal(image, cb);
				}, TaskCreationOptions.LongRunning);
			}
			else
			{
				DrawImageMethod drawImageMethod = method;
			}
		}

		public string WriteUserData(byte[] data)
		{
			string result = "";
			int num = _d30_command.getMaxNFCLength() - 2 - 3 - 4;
			int num2 = data.Length / num + 1;
			int num3 = 0;
			int num4 = num - (num & 3);
			for (int i = 0; i < num2; i++)
			{
				int num5 = num4;
				int num6 = i * num5;
				int num7 = num6 + num5;
				if (i + 1 >= num2)
				{
					num7 = data.Length;
				}
				int num8 = num7 - num6;
				byte[] array = new byte[num8];
				Array.Copy(data, num6, array, 0, num8);
				int num9;
				for (num9 = 5; num9 > 0; num9--)
				{
					if (NFCManager.getInstance().getTagID() == null)
					{
						log("NFC Card Off");
						_drawing = false;
						return "NFC Card Off";
					}
					if (WriteDataFlash(num3, array))
					{
						result = "Success";
						break;
					}
				}
				if (num9 == 0)
				{
					log("Write Image Fail");
					_drawing = false;
					return "Write Image Fail";
				}
				num3 += num4;
			}
			return result;
		}

		public byte[] ReadUserData(int pos)
		{
			int num;
			for (num = 5; num > 0; num--)
			{
				if (NFCManager.getInstance().getTagID() == null)
				{
					log("NFC Card Off");
					_drawing = false;
					return new byte[1]
					{
						9
					};
				}
				byte[] array = ReadDataFlash(pos);
				if (array != null)
				{
					return array;
				}
			}
			if (num == 0)
			{
				log("Write Image Fail");
				_drawing = false;
				return null;
			}
			return null;
		}

		private void DrawImageNormal(EinkImage image, IDrawImageCallback cb)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			_d30_command.ClearStopwatch();
			int height = image.getHeight();
			int width = image.getWidth();
			int num = height * width / 8;
			int num2 = image.getlz4();
			byte[] array;
			byte lz4flag;
			if (num2 < num && num2 != 0)
			{
				num2 += num2 % 4;
				array = image.getlz4Data();
				lz4flag = 1;
			}
			else
			{
				array = image.getData();
				num2 = array.Length;
				lz4flag = 0;
			}
			cb.onProgress(DrawImageState.DIState_Erase, 0);
			if (!EraseImageFlash(lz4flag))
			{
				_drawing = false;
				cb.onProgress(DrawImageState.DIState_Error, 0);
			}
			else
			{
				int num3 = _d30_command.getMaxNFCLength() - 2 - 3 - 4;
				int pages = image.getPages();
				int i = 0;
				for (int num4 = num3 - (num3 & 3); i < num2; i += num4)
				{
					cb.onProgress(DrawImageState.DIState_SendData, i * 100 / num2);
					int num5 = (i + num4 <= num2) ? (i + num4) : num2;
					byte[] array2 = new byte[num5 - i];
					Array.Copy(array, i, array2, 0, num5 - i);
					log($"{i}/{num2}");
					int num6;
					for (num6 = 5; num6 > 0; num6--)
					{
						if (NFCManager.getInstance().getTagID() == null)
						{
							log("NFC Card Off");
							_drawing = false;
							cb.onProgress(DrawImageState.DIState_Error, 0);
							return;
						}
						if (WriteImageFlashNOACK(i, array2))
						{
							break;
						}
					}
					if (num6 == 0)
					{
						log("Write Image Fail");
						_drawing = false;
						cb.onProgress(DrawImageState.DIState_Error, 0);
						return;
					}
				}
				cb.onProgress(DrawImageState.DIState_SendData, 100);
				if (!CheckImageFlash())
				{
					log("Check Image Flash Fail");
					_drawing = false;
					cb.onProgress(DrawImageState.DIState_Error, 0);
				}
				else
				{
					stopwatch.Stop();
					log("eplased=" + $"{stopwatch.ElapsedMilliseconds} ms jogtek={_d30_command.GetElapsed()}");
					cb.onProgress(DrawImageState.DIState_WriteToEPD, 100);
					if (!WriteFlashToEPD((byte)pages))
					{
						log("Write Flash to EPD fail");
						_drawing = false;
						cb.onProgress(DrawImageState.DIState_Error, 0);
					}
					else
					{
						_drawing = false;
						cb.onProgress(DrawImageState.DIState_Finish, 100);
					}
				}
			}
		}

		private bool WriteFlashToEPD(byte pages)
		{
			byte[] obj = new byte[5]
			{
				1,
				40,
				0,
				128,
				0
			};
			obj[4] = pages;
			byte[] data = obj;
			byte[] recv = TranceiveCommand(144, data, 20000);
			return CheckResponse(recv);
		}

		private bool CheckImageFlash()
		{
			byte[] recv = TranceiveCommand(130, null, 1000);
			return CheckResponse(recv);
		}

		private bool WriteImageFlash(int address, byte[] data)
		{
			byte[] array = new byte[2 + data.Length];
			array[0] = (byte)((address >> 8) & 0xFF);
			array[1] = (byte)(address & 0xFF);
			Array.Copy(data, 0, array, 2, data.Length);
			for (int i = 0; i < 5; i++)
			{
				byte[] array2 = TranceiveCommand(129, array, 20000);
				if (CheckResponse(array2))
				{
					return true;
				}
				if (array2 == null)
				{
					return false;
				}
			}
			return false;
		}

		private bool WriteDataFlash(int address, byte[] data)
		{
			byte[] array = new byte[2 + data.Length];
			array[0] = (byte)((address >> 8) & 0xFF);
			array[1] = (byte)(address & 0xFF);
			Array.Copy(data, 0, array, 2, data.Length);
			byte[] array2 = TranceiveCommand(131, array, 20000);
			if (array2 == null || array2.Length != 3)
			{
				return false;
			}
			if (array2[0] == 0 && array2[1] == 1 && array2[2] == 255)
			{
				return true;
			}
			return false;
		}

		private byte[] ReadDataFlash(int address)
		{
			return TranceiveCommand(132, new byte[2]
			{
				(byte)((address >> 8) & 0xFF),
				(byte)(address & 0xFF)
			}, 1000);
		}

		private bool WriteImageFlashNOACK(int address, byte[] data)
		{
			byte[] array = new byte[2 + data.Length];
			array[0] = (byte)((address >> 8) & 0xFF);
			array[1] = (byte)(address & 0xFF);
			Array.Copy(data, 0, array, 2, data.Length);
			TxCommand(142, array, 1000);
			return true;
		}

		private bool EraseImageFlash(byte lz4flag)
		{
			byte[] data = new byte[1]
			{
				lz4flag
			};
			byte[] recv = TranceiveCommand(128, data, 1000);
			return CheckResponse(recv);
		}
	}
}
