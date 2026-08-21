#define DEBUG
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

		public const int OTA_CMD_GET_BL_VERSION = 176;

		public const int OTA_CMD_ERASE_APP_FLASH = 177;

		public const int OTA_CMD_WRITE_FW_FLASH = 178;

		public const int OTA_CMD_REBOOT_2_APP = 179;

		public const int OTA_CMD_CHKECK_FW_FLASH = 180;

		public const int OTA_CMD_GET_EPD_STAUTS = 181;

		public const int OTA_CMD_GET_FW_MAGIC = 182;

		public const int FLAG_NONE = 0;

		public const int FLAG_USERAPP_UPGRADE = 1;

		public const int FLAG_FWAPP_UPGRADE = 2;

		public const int FLAG_COMBINE_FW_UPGRADE = 3;

		public const int DEV_IN_NORMAL_APP = 0;

		public const int DEV_IN_OTA_APP = 1;

		public const int DEV_IN_BOOTLOADER = 2;

		public const int SWITCH_BTL = 0;

		public const int SWITCH_USR_APP = 1;

		public const int SWITCH_OTA_APP = 2;

		private const int CMD_SHORT_TIMEOUT = 1000;

		private const int CMD_LONG_TIMEOUT = 100000;

		private const int CMD_REFRESH_TIMEOUT = 90000;

		private D30Command _d30_command;

		private bool _busy;

		private bool _drawing;

		private NFCState _nfcState;

		private bool check_epd = false;

		private bool _enable_log = true;

		private bool testFlag = false;

		private DateTime a;

		private DateTime b;

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
			if (_enable_log)
			{
				Debug.Print(s);
			}
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
			byte[] rx;
			do
			{
				rx = _nfcState.getRx();
			}
			while (rx != null);
		}

		private bool CheckResponse(byte[] recv)
		{
			if (recv == null || recv.Length != 3)
			{
				return false;
			}
			return recv[1] == 1;
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

		public bool OTA_CheckEPDVolt()
		{
			return CheckResponse(TranceiveCommand(181, null, 1000));
		}

		public string GetPlatformName()
		{
			_busy = true;
			byte[] array = TranceiveCommand(241, null, 1000);
			if (array == null || array.Length != 14)
			{
				_busy = false;
				return "Unkown";
			}
			string text = "";
			for (int i = 1; i < 13; i++)
			{
				text += Convert.ToChar(array[i]).ToString();
			}
			_busy = false;
			return text;
		}

		public byte[] getTagID()
		{
			return NFCManager.getInstance().getTagID();
		}

		public string GetVersion()
		{
			_busy = true;
			byte[] array = TranceiveCommand(240, null, 1000);
			if (array == null || array.Length != 4)
			{
				if (array == null || array.Length != 5)
				{
					return "??";
				}
				_busy = false;
				return $"{(int)array[1]}.{(int)array[2]}.{(int)array[3]}";
			}
			_busy = false;
			return $"{(int)array[1]}.{(int)array[2]}";
		}

		public string OTA_GetBLVersion()
		{
			_busy = true;
			byte[] array = TranceiveCommand(176, null, 1000);
			if (array == null || array.Length != 4)
			{
				if (array == null || array.Length != 5)
				{
					return "??";
				}
				_busy = false;
				return $"{(int)array[1]}.{(int)array[2]}.{(int)array[3]}";
			}
			_busy = false;
			return $"{(int)array[1]}.{(int)array[2]}";
		}

		public bool isBusy()
		{
			return _busy || _drawing;
		}

		public bool isValid()
		{
			if (_d30_command == null)
			{
				return false;
			}
			return _d30_command.isValid();
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
			if (array == null || array.Length != 14)
			{
				_busy = false;
				return "Unkown";
			}
			string text = "";
			for (int i = 1; i < 13; i++)
			{
				text += $"{array[i]:X2}";
			}
			_busy = false;
			return text;
		}

		public byte GetPinCodeStatus()
		{
			_busy = true;
			byte[] array = TranceiveCommand(160, null, 1000);
			if (array == null || array.Length != 3)
			{
				_busy = false;
				return 16;
			}
			return array[1];
		}

		public bool UnlockPinCode(byte[] data)
		{
			if (data.Length == 4)
			{
				_busy = true;
				byte[] recv = TranceiveCommand(161, data, 1000);
				_busy = false;
				return CheckResponse(recv);
			}
			return false;
		}

		public bool SetPinCode(byte[] data)
		{
			if (data.Length == 4)
			{
				_busy = true;
				byte[] recv = TranceiveCommand(163, data, 1000);
				_busy = false;
				return CheckResponse(recv);
			}
			return false;
		}

		public bool ResetPinCode(byte[] data)
		{
			if (data.Length == 8)
			{
				_busy = true;
				byte[] recv = TranceiveCommand(162, data, 1000);
				_busy = false;
				return CheckResponse(recv);
			}
			return false;
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
			if (array == null || array.Length != 7)
			{
				array[0] = 0;
				array[1] = 0;
				_busy = false;
				return array;
			}
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
			switch (method)
			{
			case DrawImageMethod.DIMethod_Normal:
				await Task.Factory.StartNew(delegate
				{
					DrawImageNormal(image, cb);
				}, TaskCreationOptions.LongRunning);
				break;
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
			if (num != 0)
			{
				return null;
			}
			log("Write Image Fail");
			_drawing = false;
			return null;
		}

		private void DrawImageNormal(EinkImage image, IDrawImageCallback cb)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			_d30_command.ClearStopwatch();
			int height = image.getHeight();
			int width = image.getWidth();
			int pages = image.getPages();
			int num = height * width * pages / 8;
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
					a = DateTime.Now;
					if (!WriteFlashToEPD((byte)pages, width, height))
					{
						log("Write Flash to EPD fail");
						_drawing = false;
						cb.onProgress(DrawImageState.DIState_Error, 0);
					}
					else
					{
						b = DateTime.Now;
						string str = (b - a).ToString();
						log("a time : " + a + " \n b time :" + b);
						log("time : " + str);
						_drawing = false;
						cb.onProgress(DrawImageState.DIState_Finish, 100);
					}
				}
			}
		}

		public async void bigdata(FWinfo fwdata, DrawImageMethod method, SendDataCallback cb)
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
			switch (method)
			{
			case DrawImageMethod.DIMethod_Normal:
				await Task.Factory.StartNew(delegate
				{
					BigdataSend(fwdata, cb);
				}, TaskCreationOptions.LongRunning);
				break;
			}
		}

		private string OTA_GetFW_MAGIC()
		{
			_busy = true;
			byte[] array = TranceiveCommand(182, null, 1000);
			if (array == null || array.Length != 5)
			{
				return "??";
			}
			_busy = false;
			string text = "";
			for (int i = 1; i < array.Length - 1; i++)
			{
				text += Convert.ToChar(array[i]).ToString();
			}
			return text;
		}

		private void BigdataSend(FWinfo fwdata, SendDataCallback cb)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			_d30_command.ClearStopwatch();
			byte b = 0;
			int datalen = fwdata.getDatalen();
			log($"data size={datalen}");
			byte[] array = new byte[datalen];
			array = fwdata.getData();
			int startaddr = fwdata.getstartAddress();
			int endAddress = fwdata.getEndAddress();
			b = fwdata.getOTAtype();
			string text = OTA_GetFW_MAGIC();
			byte devflag = 1;
			cb.onProgress(SendDataState.SDState_Getinfo, 0);
			if (text.Equals("USR"))
			{
				switch (b)
				{
				case 1:
					cb.onProgress(SendDataState.SDState_Reboot_FW_App, 0);
					Switch_2_APP(devflag);
					if (!CheckDevready(1))
					{
						cb.onProgress(SendDataState.SDState_DEV_VOLT_Error, 0);
						return;
					}
					break;
				case 3:
					cb.onProgress(SendDataState.SDState_BLTAPP_Error, 0);
					return;
				case 2:
				{
					byte[] data = new byte[4]
					{
						48,
						48,
						48,
						48
					};
					if (!UnlockPinCode(data))
					{
						cb.onProgress(SendDataState.SDState_Unlock, 0);
						return;
					}
					if (!CheckDevready(0))
					{
						cb.onProgress(SendDataState.SDState_DEV_VOLT_Error, 0);
						return;
					}
					break;
				}
				default:
					cb.onProgress(SendDataState.SDState_Error, 0);
					return;
				}
			}
			else
			{
				if (!text.Equals("BTL"))
				{
					cb.onProgress(SendDataState.SDState_UPGRADEAPP_Error, 0);
					return;
				}
				switch (b)
				{
				case 3:
					if (!CheckDevready(2))
					{
						cb.onProgress(SendDataState.SDState_DEV_VOLT_Error, 0);
						return;
					}
					break;
				case 1:
					if (!CheckDevready(2))
					{
						cb.onProgress(SendDataState.SDState_DEV_VOLT_Error, 0);
						return;
					}
					break;
				case 2:
					cb.onProgress(SendDataState.SDState_BLTAPP_Error, 0);
					return;
				}
			}
			cb.onProgress(SendDataState.SDState_Erase, 0);
			if (!OTA_EraseAppFlash(b, startaddr, endAddress))
			{
				_drawing = false;
				cb.onProgress(SendDataState.SDState_Erase_Error, 0);
			}
			else
			{
				int num = _d30_command.getMaxNFCLength() - 2 - 3 - 4;
				int i = 0;
				for (int num2 = num - (num & 3); i < datalen; i += num2)
				{
					cb.onProgress(SendDataState.SDState_SendData, i * 100 / datalen);
					int num3 = (i + num2 <= datalen) ? (i + num2) : datalen;
					byte[] array2 = new byte[num3 - i];
					Array.Copy(array, i, array2, 0, num3 - i);
					log($"{i}/{datalen}");
					int num4;
					for (num4 = 5; num4 > 0; num4--)
					{
						if (NFCManager.getInstance().getTagID() == null)
						{
							log("NFC Card Off");
							_drawing = false;
							cb.onProgress(SendDataState.SDState_Error, 0);
							return;
						}
						if (OTA_WriteDataFlashNOACK(i, array2))
						{
							break;
						}
					}
					if (num4 == 0)
					{
						log("Write FW data Fail");
						_drawing = false;
						cb.onProgress(SendDataState.SDState_Error, 0);
						return;
					}
				}
				cb.onProgress(SendDataState.SDState_SendData, 100);
				stopwatch.Stop();
				log("eplased=" + $"{stopwatch.ElapsedMilliseconds} ms jogtek={_d30_command.GetElapsed()}");
				cb.onProgress(SendDataState.SDState_Checksum_APP, 100);
				if (!OTA_CheckDataFlash(fwdata))
				{
					log("Check Image Flash Fail");
					_drawing = false;
					cb.onProgress(SendDataState.SDState_Checksum_Error, 0);
				}
				else
				{
					switch (b)
					{
					case 1:
						OTA_Switch_APP(1);
						break;
					case 2:
						Switch_2_APP(0);
						break;
					}
					int fwtype = 0;
					switch (b)
					{
					case 1:
						fwtype = 1;
						break;
					case 2:
						fwtype = 2;
						break;
					}
					if (FW_version_check(fwdata, fwtype))
					{
						cb.onProgress(SendDataState.SDState_Finish, 100);
					}
					else
					{
						cb.onProgress(SendDataState.SDState_Compare_Error, 100);
					}
					_drawing = false;
				}
			}
		}

		private bool WriteFlashToEPD(byte pages, int width, int height)
		{
			byte[] data = new byte[5]
			{
				(byte)(width >> 8),
				(byte)width,
				(byte)(height >> 8),
				(byte)height,
				pages
			};
			byte[] recv = TranceiveCommand(144, data, 90000);
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
				byte[] array2 = TranceiveCommand(129, array, 100000);
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
			byte[] array2 = TranceiveCommand(131, array, 100000);
			if (array2 != null && array2.Length == 3)
			{
				if (array2[0] != 0 || array2[1] != 1 || array2[2] != 255)
				{
					return false;
				}
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

		private bool OTA_CheckDataFlash(FWinfo fwdata)
		{
			byte[] array = new byte[24];
			int checksum = fwdata.getChecksum();
			int num = fwdata.getstartAddress();
			int endAddress = fwdata.getEndAddress();
			int datalen = fwdata.getDatalen();
			int magic = fwdata.getMagic();
			byte[] fwver = fwdata.getFwver();
			byte oTAtype = fwdata.getOTAtype();
			array[0] = (byte)((checksum >> 24) & 0xFF);
			array[1] = (byte)((checksum >> 16) & 0xFF);
			array[2] = (byte)((checksum >> 8) & 0xFF);
			array[3] = (byte)(checksum & 0xFF);
			array[4] = (byte)((num >> 24) & 0xFF);
			array[5] = (byte)((num >> 16) & 0xFF);
			array[6] = (byte)((num >> 8) & 0xFF);
			array[7] = (byte)(num & 0xFF);
			array[8] = (byte)((endAddress >> 24) & 0xFF);
			array[9] = (byte)((endAddress >> 16) & 0xFF);
			array[10] = (byte)((endAddress >> 8) & 0xFF);
			array[11] = (byte)(endAddress & 0xFF);
			array[12] = (byte)((magic >> 24) & 0xFF);
			array[13] = (byte)((magic >> 16) & 0xFF);
			array[14] = (byte)((magic >> 8) & 0xFF);
			array[15] = (byte)(magic & 0xFF);
			array[16] = fwver[3];
			array[17] = fwver[2];
			array[18] = fwver[1];
			array[19] = fwver[0];
			array[20] = 0;
			array[21] = 0;
			array[22] = 0;
			array[23] = oTAtype;
			byte[] recv = TranceiveCommand(180, array, 1000);
			return CheckResponse(recv);
		}

		private bool WriteImageFlashNOACK(int address, byte[] data)
		{
			byte[] array = new byte[2 + data.Length];
			array[0] = (byte)((address >> 8) & 0xFF);
			array[1] = (byte)(address & 0xFF);
			Array.Copy(data, 0, array, 2, data.Length);
			TxCommand(142, array, 100000);
			return true;
		}

		private bool OTA_WriteDataFlashNOACK(int address, byte[] data)
		{
			byte[] array = new byte[2 + data.Length];
			array[0] = (byte)((address >> 8) & 0xFF);
			array[1] = (byte)(address & 0xFF);
			Array.Copy(data, 0, array, 2, data.Length);
			TxCommand(178, array, 100000);
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

		private bool OTA_EraseAppFlash(byte eraseflag, int startaddr, int endaddr)
		{
			byte[] recv = TranceiveCommand(177, new byte[9]
			{
				eraseflag,
				(byte)((startaddr >> 24) & 0xFF),
				(byte)((startaddr >> 16) & 0xFF),
				(byte)((startaddr >> 8) & 0xFF),
				(byte)(startaddr & 0xFF),
				(byte)((endaddr >> 24) & 0xFF),
				(byte)((endaddr >> 16) & 0xFF),
				(byte)((endaddr >> 8) & 0xFF),
				(byte)(endaddr & 0xFF)
			}, 1000);
			return CheckResponse(recv);
		}

		private bool Switch_2_APP(byte devflag)
		{
			byte[] data = new byte[1]
			{
				devflag
			};
			byte[] recv = TranceiveCommand(244, data, 1000);
			return CheckResponse(recv);
		}

		private void OTA_Switch_APP(byte devflag)
		{
			byte[] data = new byte[1]
			{
				devflag
			};
			TxCommand(179, data, 1000);
		}

		private bool CheckDevready(byte devtype)
		{
			int i = 0;
			int num;
			for (num = 10; i < num; i++)
			{
				if (devtype == 0)
				{
					if (CheckEPDStatus())
					{
						break;
					}
				}
				else if (OTA_CheckEPDVolt())
				{
					break;
				}
			}
			if (i < num)
			{
				return true;
			}
			return false;
		}

		private bool FW_version_check(FWinfo fwdata, int fwtype)
		{
			string text = "";
			string text2 = "";
			byte[] array = new byte[4];
			int num = 5;
			array = fwdata.getFwver();
			text = $"{array[2]}.{array[1]}.{array[0]}";
			if (fwtype == 1)
			{
				for (int i = 0; i < num; i++)
				{
					if (GetVersion() != "??")
					{
						break;
					}
				}
				text2 = GetVersion();
			}
			else
			{
				text2 = OTA_GetBLVersion();
			}
			if (!text2.Equals(text))
			{
				return false;
			}
			return true;
		}
	}
}
