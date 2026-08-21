namespace com.advantech.nfc.cmd
{
	public class D30Command : NFCCommand_Jogtek
	{
		public const byte CMD_INVENTORY = 1;

		public const byte CMD_QUIET = 2;

		public const byte CMD_READ_SINGLE_BLOCK = 32;

		public const byte CMD_WRITE_SINGLE_BLOCK = 33;

		public const byte CMD_LOCK_BLOCK = 34;

		public const byte CMD_READ_MULTIPLE_BLOCKS = 35;

		public const byte CMD_WRITE_MULTIPLE_BLOCKS = 36;

		public const byte CMD_SELECT = 37;

		public const byte CMD_RESET_TO_READY = 38;

		public const byte CMD_WRITE_AFI = 39;

		public const byte CMD_LOCK_AFI = 40;

		public const byte CMD_WRITE_DSFID = 41;

		public const byte CMD_EXT_READ_SINGLE_BLOCK = 48;

		public const byte CMD_EXT_WRITE_SINGLE_BLOCK = 49;

		public const byte CMD_EXT_LOCK_BLOCK = 50;

		public const byte CMD_EXT_READ_MULTIPLE_BLOCKS = 51;

		public const byte CMD_EXT_WRITE_MULTIPLE_BLOCKS = 52;

		public const byte CMD_LOCK_DSFID = 42;

		public const byte CMD_GET_SYSTEM_INFO = 43;

		public const byte CMD_MULTIPLE_BLOCK_SECURITY_STATUS = 44;

		public const byte CMD_EXT_GET_SYSTEM_INFO = 59;

		public const byte CMD_EXT_MULTIPLE_BLOCK_SECURITY_STATUS = 60;

		public const byte CMD_READ_CONFIGURATION = 160;

		public const byte CMD_WRITE_CONFIGURATION = 161;

		public const byte CMD_MANGE_GPO = 162;

		public const byte CMD_WRITE_MESSSAGE = 170;

		public const byte CMD_READ_MESSAGE_LENGTH = 171;

		public const byte CMD_READ_MESSAGE = 172;

		public const byte CMD_READ_DYN_CONFIGURATION = 173;

		public const byte CMD_WRITE_DYN_CONFIGURATION = 174;

		public const byte CMD_WRITE_PASSWORD = 177;

		public const byte CMD_PRESENT_PASSWORD = 179;

		public const byte CMD_FAST_READ_SINGLE_BLOCK = 192;

		public const byte CMD_FAST_READ_MULTIPLE_BLOCKS = 195;

		public const byte CMD_FAST_EXT_READ_SINGLE_BLOCK = 196;

		public const byte CMD_FAST_EXT_READ_MULTIPLE_BLOCKS = 197;

		public const byte CMD_FAST_WRITE_MESSAGE = 202;

		public const byte CMD_FAST_READ_MESSAGE_LENGTH = 203;

		public const byte CMD_FAST_READ_MESSAGE = 204;

		public const byte CMD_FAST_READ_DYN_CONFIGURATION = 205;

		public const byte CMD_FAST_WRITE_DYN_CONFIGURATION = 206;

		public const byte REQ_FLAG_SUBCARRIER = 1;

		public const byte REQ_FLAG_HIGH_DATA_RATE = 2;

		public const byte REQ_FLAG_INVENTORY = 4;

		public const byte REQ_FLAG_EXT_PROTOCOL = 8;

		public const byte REQ_FLAG_SELECT = 16;

		public const byte REQ_FLAG_ADDRESS = 32;

		public const byte REQ_FLAG_OPTION = 64;

		public const byte REQ_FLAG_RFU = 128;

		public const byte DYN_ADDR_GPO = 0;

		public const byte DYN_ADDR_EH_CTRL = 2;

		public const byte DYN_ADDR_MB_CTRL = 13;

		public const byte CFG_ADDR_EH_MODE = 2;

		public const byte MB_CTRL_BIT_MB_EN = 1;

		public const byte MB_CTRL_BIT_HOST_PUT_MSG = 2;

		public const byte MB_CTRL_BIT_RF_PUT_MSG = 4;

		public const byte MB_CTRL_BIT_RFU = 8;

		public const byte MB_CTRL_BIT_HOST_MISS_MSG = 16;

		public const byte MB_CTRL_BIT_RF_MISS_MSG = 32;

		public const byte MB_CTRL_BIT_HOST_CURRENT_MSG = 64;

		public const byte MB_CTRL_BIT_RF_CURRENT_MSG = 128;

		private const byte IC_MFG_CODE = 2;

		private const byte REQ_FLAG = 2;

		public D30Command(string com_port)
			: base(com_port)
		{
		}

		public void presentPassword()
		{
			byte b = 2;
			byte[] data = new byte[12]
			{
				b,
				179,
				2,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0
			};
			byte[] array = transferRF(data);
			if (array != null && array.Length != 0 && array[0] == 0)
			{
				log("presentPassword OK");
			}
			else
			{
				log("presentPassword fail");
			}
		}

		public byte ReadConfiguration(byte address)
		{
			byte b = 2;
			byte[] data = new byte[4]
			{
				b,
				160,
				2,
				address
			};
			for (int num = 10; num > 0; num--)
			{
				byte[] array = transferRF(data);
				if (array == null)
				{
					log($"Reg[{address:X2}]=??");
				}
				else if (isResponseOK(array))
				{
					if (array.Length >= 2)
					{
						return array[1];
					}
				}
				else if (array.Length >= 2)
				{
					log($"Reg[{address:X2}]={array[1]:X2} [error]");
				}
				else
				{
					log($"Reg[{address:X2}]=?? [error]");
				}
			}
			throw new NFCException(NFCExceptionType.NFC_EXCEPTION_TYPE_ERROR);
		}

		public void WriteConfiguration(byte address, byte data)
		{
			byte b = 2;
			byte[] data2 = new byte[5]
			{
				b,
				161,
				2,
				address,
				data
			};
			for (int num = 10; num > 0; num--)
			{
				byte[] array = transferRF(data2);
				if (array == null)
				{
					log($"Reg[{address:X}]=??");
				}
				else if (isResponseOK(array))
				{
					if (array.Length == 1)
					{
						break;
					}
				}
				else if (array.Length >= 2)
				{
					log($"Reg[{address:X2}]={array[1]:X} [error]");
				}
			}
		}

		public byte readDynConfig(byte address)
		{
			byte b = 2;
			byte[] data = new byte[4]
			{
				b,
				173,
				2,
				address
			};
			for (int num = 10; num > 0; num--)
			{
				byte[] array = transferRF(data);
				if (array == null)
				{
					log($"Dyn[{address:X2}]=??");
				}
				else if (isResponseOK(array))
				{
					if (array.Length >= 2)
					{
						return array[1];
					}
				}
				else if (array.Length >= 2)
				{
					log($"Dyn[{address:X2}]={array[1]:X2} [error]");
				}
				else
				{
					log($"Dyn[{address:X2}]=?? [error]");
				}
			}
			throw new NFCException(NFCExceptionType.NFC_EXCEPTION_TYPE_ERROR);
		}

		public void writeDynConfig(byte address, int data)
		{
			byte b = 2;
			byte[] data2 = new byte[5]
			{
				b,
				174,
				2,
				address,
				(byte)data
			};
			for (int num = 10; num > 0; num--)
			{
				byte[] array = transferRF(data2);
				if (array == null)
				{
					log($"Dyn[{address:X}]=??");
				}
				else if (isResponseOK(array))
				{
					if (array.Length == 1)
					{
						return;
					}
				}
				else if (array.Length >= 2)
				{
					log($"Dyn[{address:X2}]={array[1]:X} [error]");
				}
			}
			throw new NFCException(NFCExceptionType.NFC_EXCEPTION_TYPE_ERROR);
		}

		public byte readMessageLength()
		{
			byte b = 2;
			byte[] data = new byte[3]
			{
				b,
				171,
				2
			};
			for (int num = 10; num > 0; num--)
			{
				byte[] array = transferRF(data);
				if (array == null)
				{
					log(string.Format("MSG_LEN=??"));
				}
				else if (isResponseOK(array))
				{
					if (array.Length >= 2)
					{
						log($"MSG_LEN={array[1]}");
						return array[1];
					}
				}
				else if (array.Length >= 2)
				{
					log($"MSG_LEN={array[1]} [error]");
				}
				else
				{
					log(string.Format("MSG_LEN==?? [error]"));
				}
			}
			throw new NFCException(NFCExceptionType.NFC_EXCEPTION_TYPE_ERROR);
		}

		public byte[] readMessage(int pointer, int len)
		{
			byte b = 2;
			byte[] data = new byte[5]
			{
				b,
				172,
				2,
				(byte)pointer,
				(byte)len
			};
			for (int num = 10; num > 0; num--)
			{
				byte[] array = transferRF(data);
				if (array == null)
				{
					log(string.Format("READ_MSG=??"));
				}
				else if (isResponseOK(array))
				{
					if (array.Length >= 2)
					{
						return array;
					}
				}
				else if (array.Length >= 2)
				{
					log($"READ_MSG={array[1]:X2} [error]");
				}
				else
				{
					log(string.Format("READ_MSG==?? [error]"));
				}
			}
			throw new NFCException(NFCExceptionType.NFC_EXCEPTION_TYPE_ERROR);
		}

		public void writeMessage(byte[] data)
		{
			byte b = 2;
			if (data == null || data.Length == 0)
			{
				return;
			}
			int num = data.Length;
			byte[] array = new byte[4 + num];
			array[0] = b;
			array[1] = 170;
			array[2] = 2;
			array[3] = (byte)(num - 1);
			for (int i = 0; i < num; i++)
			{
				array[4 + i] = data[i];
			}
			for (int num2 = 10; num2 > 0; num2--)
			{
				byte[] array2 = transferRF(array);
				if (array2 == null)
				{
					log(string.Format("WRITE_MSG=??"));
				}
				else
				{
					if (isResponseOK(array2))
					{
						return;
					}
					if (array2.Length >= 2)
					{
						log($"WRITE_MSG={array2[1]:X2} [error]");
					}
					else
					{
						log(string.Format("WRITE_MSG==?? [error]"));
					}
				}
			}
			throw new NFCException(NFCExceptionType.NFC_EXCEPTION_TYPE_ERROR);
		}

		public void ResetToReady()
		{
			byte b = 2;
			byte[] data = new byte[2]
			{
				b,
				38
			};
			for (int num = 10; num > 0; num--)
			{
				if (transferRF(data) != null)
				{
					return;
				}
			}
			log("Cannot Reset to Ready");
			throw new NFCException(NFCExceptionType.NFC_EXCEPTION_TYPE_ERROR);
		}
	}
}
