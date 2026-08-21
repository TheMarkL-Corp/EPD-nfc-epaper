using System;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;

[assembly: AssemblyCopyright("jogtek")]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: AssemblyTitle("API")]
[assembly: AssemblyDescription("NFC API")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("jogtek")]
[assembly: AssemblyProduct("NFC API")]
[assembly: TargetFramework(".NETFramework,Version=v4.0", FrameworkDisplayName = ".NET Framework 4")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("ff894813-08ad-4bc3-a1fc-10c3b62b9fe9")]
[assembly: AssemblyFileVersion("2018.04.27.0")]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: CompilationRelaxations(8)]
[assembly: AssemblyVersion("2018.4.27.0")]
namespace J_RFID
{
	public class RFIDAPI
	{
		private static SerialPort sptCom;

		private static SerialPort sptCom2;

		private string Uid14443A = "";

		private string ChipSRIX4K = "";

		private string Firmware_mode = "";

		private int Power_set;

		private int Err;

		private byte ComSend(string strSend, out string strRece, int delTime)
		{
			string empty2 = string.Empty;
			string empty = string.Empty;
			strRece = "";
			try
			{
				sptCom.Write(strSend);
				Thread.Sleep(delTime);
				empty = (strRece = sptCom.ReadExisting());
				return 0;
			}
			catch
			{
				return 1;
			}
		}

		public byte RFID_GetAPIVersionString(out string strVersion)
		{
			strVersion = "V18.04.27";
			return 0;
		}

		public byte RFID_OpenReader(string COMPort)
		{
			byte b = 0;
			string strRece = "";
			try
			{
				if (sptCom == null)
				{
					sptCom = new SerialPort(COMPort, 115200);
				}
				if (!sptCom.IsOpen)
				{
					sptCom.Encoding = Encoding.Default;
					sptCom.Parity = Parity.None;
					sptCom.DataBits = 8;
					sptCom.StopBits = StopBits.One;
					sptCom.ReadTimeout = 50;
					sptCom.WriteTimeout = 50;
					b = 1;
					sptCom.Open();
					return 0;
				}
				sptCom.Close();
				sptCom = null;
				return 2;
			}
			catch
			{
				if (b == 1)
				{
					Thread.Sleep(100);
					ComSend("0108000304FF0000", out strRece, 100);
					if (strRece.Length > 25)
					{
						return 0;
					}
				}
				sptCom.Close();
				sptCom = null;
				sptCom.Dispose();
				return 2;
			}
		}

		public byte RFID_CloseReader(string COMPort)
		{
			try
			{
				if (sptCom.IsOpen)
				{
					sptCom.Close();
					sptCom = null;
					sptCom.Dispose();
					return 0;
				}
			}
			catch
			{
				return 3;
			}
			return 1;
		}

		public byte RFID_HRate()
		{
			string strRece = "";
			Err = ComSend("0108000304FC0000", out strRece, 20);
			Firmware_mode = "FC";
			if (Err != 0)
			{
				return 4;
			}
			Err = ComSend("010C00030410002101020000", out strRece, 20);
			if (Err != 0)
			{
				return 4;
			}
			Err = ComSend("0109000304F0000000", out strRece, 20);
			if (Err != 0)
			{
				return 4;
			}
			Err = ComSend("0109000304F1FF0000", out strRece, 20);
			if (Err != 0)
			{
				return 4;
			}
			return 0;
		}

		public byte RFID_WorkingType(byte Type, byte Power)
		{
			if (Power != 0 && Power != 1)
			{
				return 1;
			}
			string strRece = "";
			long num = 1L;
			Power_set = Power;
			try
			{
				switch (Type)
				{
				case 0:
					if (Power == 1)
					{
						num = ComSend("0108000304FC0000", out strRece, 20);
						Firmware_mode = "FC";
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("010C00030410002101000000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("0109000304F0000000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("0109000304F1FF0000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
					}
					if (Power == 0)
					{
						num = ComSend("0108000304FC0000", out strRece, 20);
						Firmware_mode = "FC";
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("010C00030410003101000000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("0109000304F0000000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("0109000304F1FF0000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
					}
					return 0;
				case 1:
					if (Power == 1)
					{
						num = ComSend("0108000304FC0000", out strRece, 20);
						Firmware_mode = "FC";
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("010C00030410002101090000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("0109000304F0000000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("0109000304F1FF0000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
					}
					if (Power == 0)
					{
						num = ComSend("0108000304FC0000", out strRece, 20);
						Firmware_mode = "FC";
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("010C00030410003101090000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("0109000304F0000000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("0109000304F1FF0000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
					}
					return 0;
				case 2:
					if (Power == 1)
					{
						num = ComSend("0108000304FC0000", out strRece, 20);
						Firmware_mode = "FC";
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("010C000304100021010C0000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("0109000304F0000000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("0109000304F1FF0000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
					}
					if (Power == 0)
					{
						num = ComSend("0108000304FC0000", out strRece, 20);
						Firmware_mode = "FC";
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("010C000304100031010C0000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("0109000304F0000000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("0109000304F1FF0000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
					}
					return 0;
				case 3:
					if (Power == 1)
					{
						num = ComSend("0108000304FC0000", out strRece, 20);
						Firmware_mode = "FC";
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("010C000304100021011A0000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("0109000304F0000000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("0109000304F1FF0000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
					}
					if (Power == 0)
					{
						num = ComSend("0108000304FC0000", out strRece, 20);
						Firmware_mode = "FC";
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("010C000304100031011A0000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("0109000304F0000000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
						num = ComSend("0109000304F1FF0000", out strRece, 20);
						if (num != 0)
						{
							return 4;
						}
					}
					return 0;
				}
			}
			catch
			{
				return 1;
			}
			return 1;
		}

		public byte RFID_FWVersion(out string FirmwareVer)
		{
			try
			{
				FirmwareVer = "";
				string strRece = "";
				string text = "";
				ComSend("0108000304FF0000", out strRece, 100);
				if (strRece.IndexOf("TM-") <= -1)
				{
					if (strRece.Length <= 22)
					{
						return 1;
					}
					text = (FirmwareVer = strRece.Substring(17, strRece.Length - 17));
					return 0;
				}
				FirmwareVer = strRece.Substring(strRece.IndexOf("TM-"));
				return 0;
			}
			catch
			{
				FirmwareVer = "";
				return 1;
			}
		}

		public byte RFID_AntennaControl(byte Select)
		{
			string strRece = "";
			try
			{
				switch (Select)
				{
				case 0:
					ComSend("010A0003041000810000", out strRece, 20);
					return 0;
				case 1:
					ComSend("010A0003041000210000", out strRece, 20);
					return 0;
				default:
					return 1;
				}
			}
			catch
			{
				return 1;
			}
		}

		public byte RFID_ISO15693Inventory(string Flag, string Afi, string Timeout, out string Uid)
		{
			int num = 0;
			int num2 = 100;
			string strRece = "";
			Uid = "";
			try
			{
				num2 = Convert.ToInt16(Timeout);
				num = Convert.ToInt16(Flag) / 10;
				if (num % 2 == 0)
				{
					ComSend("010B00030414" + Flag + "01000000", out strRece, num2);
				}
				else
				{
					ComSend("010C00030414" + Flag + "01" + Afi + "000000", out strRece, num2);
				}
				if (strRece.Length >= 1)
				{
					char[] array = strRece.ToCharArray();
					string text = "";
					string text2 = "";
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i] == '[' && array[i + 1] != ',' && array[i + 2] != ',' && array[i + 1] != ']')
						{
							for (int j = 1; j < 17; j++)
							{
								text += array[i + j].ToString();
							}
							text = text.Substring(14, 2) + text.Substring(12, 2) + text.Substring(10, 2) + text.Substring(8, 2) + text.Substring(6, 2) + text.Substring(4, 2) + text.Substring(2, 2) + text.Substring(0, 2);
							text2 += text;
							text = "";
						}
					}
					Uid = text2;
					if (Uid.Length >= 16)
					{
						for (int k = 0; k < Uid.Length; k += 2)
						{
							int.Parse(Uid.Substring(k, 2), NumberStyles.AllowHexSpecifier);
						}
						if (Uid.Length % 16 == 0)
						{
							return 0;
						}
						Uid = "";
						return 1;
					}
					Uid = "";
					return 5;
				}
				Uid = "";
				return 2;
			}
			catch
			{
				Uid = "";
				return 1;
			}
		}

		public byte RFID_ISO15693StayQuiet(string Flag, string Uid)
		{
			int num = 0;
			string strRece = "";
			try
			{
				num = Convert.ToInt16(Flag) / 10;
				num &= 2;
				if (num != 2)
				{
					ComSend("010A0003041800020000", out strRece, 100);
					return 0;
				}
				ComSend("011200030418" + Flag + "02" + Uid + "0000", out strRece, 100);
				return 0;
			}
			catch
			{
				return 1;
			}
		}

		public byte RFID_ISO15693Select(string Flag, string Uid)
		{
			int num = 0;
			string strRece = "";
			try
			{
				num = Convert.ToInt16(Flag) / 10;
				num &= 2;
				if (num != 2)
				{
					ComSend("010A0003041800250000", out strRece, 100);
					return 0;
				}
				ComSend("010A00030418" + Flag + "25" + Uid + "0000", out strRece, 100);
				return 0;
			}
			catch
			{
				return 1;
			}
		}

		public byte RFID_ISO15693Reset2Ready(string Flag, string Uid)
		{
			int num = 0;
			string strRece = "";
			try
			{
				num = Convert.ToInt16(Flag) / 10;
				num &= 2;
				if (num != 2)
				{
					ComSend("010A0003041800260000", out strRece, 100);
					return 0;
				}
				ComSend("011200030418" + Flag + "26" + Uid + "0000", out strRece, 100);
				return 0;
			}
			catch
			{
				return 1;
			}
		}

		public byte RFID_ISO15693Read(string Flag, string Uid, string Block, out string Data)
		{
			string strRece = "";
			string text = "";
			if (Flag.Length < 2)
			{
				Flag = "0" + Flag;
			}
			if (Block.Length < 2)
			{
				Block = "0" + Block;
			}
			Data = "";
			if (Flag.Length == 2)
			{
				try
				{
					text = Uid;
					if (text.Length == 16)
					{
						text = text.Substring(14, 2) + text.Substring(12, 2) + text.Substring(10, 2) + text.Substring(8, 2) + text.Substring(6, 2) + text.Substring(4, 2) + text.Substring(2, 2) + text.Substring(0, 2);
					}
					if (text.Length == 16)
					{
						ComSend("011300030418" + Flag + "20" + text + Block + "0000", out strRece, 100);
					}
					else
					{
						ComSend("010B00030418" + Flag + "20" + Block + "0000", out strRece, 100);
					}
					char[] array = strRece.ToCharArray();
					int num = 0;
					int num2 = 0;
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i] == '[')
						{
							num = i;
						}
						if (array[i] == ']')
						{
							num2 = i;
						}
						if (num2 != 0)
						{
							string text2 = "";
							text2 = strRece.Substring(num, num2 - num + 1);
							if (text2.Length > 10)
							{
								Data = text2.Substring(3, 8);
								return 0;
							}
							num2 = 0;
						}
					}
					return 5;
				}
				catch
				{
					return 1;
				}
			}
			return 6;
		}

		public byte RFID_M24LRXXRead(string Flag, string Uid, string Block, out string Data)
		{
			string strRece = "";
			string text = "";
			if (Flag.Length < 2)
			{
				Flag = "0" + Flag;
			}
			switch (Block.Length)
			{
			case 3:
				Block = "0" + Block;
				break;
			case 2:
				Block = "00" + Block;
				break;
			case 1:
				Block = "000" + Block;
				break;
			}
			Data = "";
			if (Flag.Length == 2)
			{
				try
				{
					text = Uid;
					if (text.Length == 16)
					{
						text = text.Substring(14, 2) + text.Substring(12, 2) + text.Substring(10, 2) + text.Substring(8, 2) + text.Substring(6, 2) + text.Substring(4, 2) + text.Substring(2, 2) + text.Substring(0, 2);
					}
					if (text.Length == 16)
					{
						ComSend("011400030418" + Flag + "20" + text + Block + "0000", out strRece, 100);
					}
					else
					{
						ComSend("010C00030418" + Flag + "20" + Block + "0000", out strRece, 100);
					}
					char[] array = strRece.ToCharArray();
					int num = 0;
					int num2 = 0;
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i] == '[')
						{
							num = i;
						}
						if (array[i] == ']')
						{
							num2 = i;
						}
						if (num2 != 0)
						{
							string text2 = "";
							text2 = strRece.Substring(num, num2 - num + 1);
							if (text2.Length > 10)
							{
								Data = text2.Substring(text2.Length - 9, 8);
								return 0;
							}
							num2 = 0;
						}
					}
					return 5;
				}
				catch
				{
					return 1;
				}
			}
			return 6;
		}

		public byte RFID_MB89R118Read(string Flag, string Uid, string Block, out string Data)
		{
			string strRece = "";
			string text = "";
			if (Flag.Length < 2)
			{
				Flag = "0" + Flag;
			}
			if (Block.Length < 2)
			{
				Block = "0" + Block;
			}
			Data = "";
			if (Flag.Length == 2)
			{
				try
				{
					text = Uid;
					if (text.Length == 16)
					{
						text = text.Substring(14, 2) + text.Substring(12, 2) + text.Substring(10, 2) + text.Substring(8, 2) + text.Substring(6, 2) + text.Substring(4, 2) + text.Substring(2, 2) + text.Substring(0, 2);
					}
					if (text.Length == 16)
					{
						ComSend("011300030418" + Flag + "20" + text + Block + "0000", out strRece, 100);
					}
					else
					{
						ComSend("010B00030418" + Flag + "20" + Block + "0000", out strRece, 100);
					}
					char[] array = strRece.ToCharArray();
					int num = 0;
					int num2 = 0;
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i] == '[')
						{
							num = i;
						}
						if (array[i] == ']')
						{
							num2 = i;
						}
						if (num2 != 0)
						{
							string text2 = "";
							text2 = strRece.Substring(num, num2 - num + 1);
							if (text2.Length > 18)
							{
								Data = text2.Substring(3, 16);
								return 0;
							}
							num2 = 0;
						}
					}
					return 5;
				}
				catch
				{
					return 1;
				}
			}
			return 6;
		}

		public byte RFID_MB89R112Read(string Flag, string Uid, string Block, out string Data)
		{
			string strRece = "";
			string text = "";
			if (Flag.Length < 2)
			{
				Flag = "0" + Flag;
			}
			if (Block.Length < 2)
			{
				Block = "0" + Block;
			}
			Data = "";
			if (Flag.Length != 2)
			{
				return 6;
			}
			if (Uid.Length == 16)
			{
				try
				{
					text = Uid;
					if (text.Length == 16)
					{
						text = text.Substring(14, 2) + text.Substring(12, 2) + text.Substring(10, 2) + text.Substring(8, 2) + text.Substring(6, 2) + text.Substring(4, 2) + text.Substring(2, 2) + text.Substring(0, 2);
					}
					if (text.Length == 16)
					{
						ComSend("011300030418" + Flag + "20" + text + Block + "0000", out strRece, 250);
					}
					char[] array = strRece.ToCharArray();
					int num = 0;
					int num2 = 0;
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i] == '[')
						{
							num = i;
						}
						if (array[i] == ']')
						{
							num2 = i;
						}
						if (num2 != 0)
						{
							string text2 = "";
							text2 = strRece.Substring(num, num2 - num + 1);
							if (text2.Length > 30)
							{
								Data = text2.Substring(3, 64);
								return 0;
							}
							num2 = 0;
						}
					}
					return 5;
				}
				catch
				{
					return 1;
				}
			}
			return 6;
		}

		public byte RFID_ISO15693Write(string Flag, string Uid, string Block, string Data)
		{
			string strRece = "";
			string text = "";
			string text2 = "";
			text = Block;
			if (Flag.Length < 2)
			{
				Flag = "0" + Flag;
			}
			if (text.Length < 2)
			{
				text = "0" + text;
			}
			if (Data.Length != 8)
			{
				return 6;
			}
			if (Flag.Length == 2)
			{
				try
				{
					for (int i = 0; i < 4; i++)
					{
						int.Parse(Data.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier);
					}
				}
				catch
				{
					return 6;
				}
				try
				{
					text2 = Uid;
					if (text2.Length == 16)
					{
						text2 = text2.Substring(14, 2) + text2.Substring(12, 2) + text2.Substring(10, 2) + text2.Substring(8, 2) + text2.Substring(6, 2) + text2.Substring(4, 2) + text2.Substring(2, 2) + text2.Substring(0, 2);
					}
					if (text2.Length == 16)
					{
						ComSend("011700030418" + Flag + "21" + text2 + text + Data + "0000", out strRece, 100);
					}
					else
					{
						ComSend("010F00030418" + Flag + "21" + text + Data + "0000", out strRece, 100);
					}
					return 0;
				}
				catch
				{
					return 4;
				}
			}
			return 6;
		}

		public byte RFID_M24LRXXWrite(string Flag, string Uid, string Block, string Data)
		{
			string strRece = "";
			string text = "";
			string text2 = "";
			switch (Block.Length)
			{
			case 3:
				Block = "0" + Block;
				break;
			case 2:
				Block = "00" + Block;
				break;
			case 1:
				Block = "000" + Block;
				break;
			}
			text = Block;
			if (Flag.Length < 2)
			{
				Flag = "0" + Flag;
			}
			if (text.Length < 2)
			{
				text = "0" + text;
			}
			if (Data.Length != 8)
			{
				return 6;
			}
			if (Flag.Length == 2)
			{
				try
				{
					for (int i = 0; i < 4; i++)
					{
						int.Parse(Data.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier);
					}
				}
				catch
				{
					return 6;
				}
				try
				{
					text2 = Uid;
					if (text2.Length == 16)
					{
						text2 = text2.Substring(14, 2) + text2.Substring(12, 2) + text2.Substring(10, 2) + text2.Substring(8, 2) + text2.Substring(6, 2) + text2.Substring(4, 2) + text2.Substring(2, 2) + text2.Substring(0, 2);
					}
					if (text2.Length == 16)
					{
						ComSend("011800030418" + Flag + "21" + text2 + text + Data + "0000", out strRece, 100);
					}
					else
					{
						ComSend("011000030418" + Flag + "21" + text + Data + "0000", out strRece, 100);
					}
					return 0;
				}
				catch
				{
					return 4;
				}
			}
			return 6;
		}

		public byte RFID_MB89RWrite(string Flag, string Uid, string Block, string Data)
		{
			string strRece = "";
			string text = "";
			string text2 = "";
			text = Block;
			if (Flag.Length < 2)
			{
				Flag = "0" + Flag;
			}
			if (text.Length < 2)
			{
				text = "0" + text;
			}
			if (Data.Length != 16)
			{
				return 6;
			}
			if (Flag.Length == 2)
			{
				try
				{
					for (int i = 0; i < 8; i++)
					{
						int.Parse(Data.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier);
					}
				}
				catch
				{
					return 6;
				}
				try
				{
					text2 = Uid;
					if (text2.Length == 16)
					{
						text2 = text2.Substring(14, 2) + text2.Substring(12, 2) + text2.Substring(10, 2) + text2.Substring(8, 2) + text2.Substring(6, 2) + text2.Substring(4, 2) + text2.Substring(2, 2) + text2.Substring(0, 2);
					}
					if (text2.Length == 16)
					{
						ComSend("011B00030418" + Flag + "21" + text2 + text + Data + "0000", out strRece, 100);
					}
					else
					{
						ComSend("011300030418" + Flag + "21" + text + Data + "0000", out strRece, 100);
					}
					return 0;
				}
				catch
				{
					return 4;
				}
			}
			return 6;
		}

		public byte RFID_MB89R112Write(string Flag, string Uid, string Block, string Data)
		{
			string strRece = "";
			string text = "";
			string text2 = "";
			text = Block;
			if (Flag.Length < 2)
			{
				Flag = "0" + Flag;
			}
			if (text.Length < 2)
			{
				text = "0" + text;
			}
			if (Data.Length != 64)
			{
				return 6;
			}
			if (Flag.Length != 2)
			{
				return 6;
			}
			if (Uid.Length == 16)
			{
				try
				{
					for (int i = 0; i < 32; i++)
					{
						int.Parse(Data.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier);
					}
				}
				catch
				{
					return 6;
				}
				try
				{
					text2 = Uid;
					if (text2.Length == 16)
					{
						text2 = text2.Substring(14, 2) + text2.Substring(12, 2) + text2.Substring(10, 2) + text2.Substring(8, 2) + text2.Substring(6, 2) + text2.Substring(4, 2) + text2.Substring(2, 2) + text2.Substring(0, 2);
					}
					if (text2.Length == 16)
					{
						ComSend("013300030418" + Flag + "21" + text2 + text + Data + "0000", out strRece, 100);
					}
					return 0;
				}
				catch
				{
					return 4;
				}
			}
			return 6;
		}

		public byte RFID_ISO15693LockBlock(string Flag, string Uid, byte Block)
		{
			string strRece = "";
			string text = "";
			text = Block.ToString();
			if (text.Length < 2)
			{
				text = "0" + text;
			}
			if (Flag.Length < 2)
			{
				Flag = "0" + Flag;
			}
			if (Uid.Length != 16)
			{
				return 6;
			}
			if (Flag.Length >= 2)
			{
				try
				{
					ComSend("011300030418" + Flag + "22" + Uid + text + "0000", out strRece, 100);
					return 0;
				}
				catch
				{
					return 4;
				}
			}
			return 6;
		}

		public byte RFID_ISO15693WriteAfi(string Flag, string Uid, byte AfiValue)
		{
			string strRece = "";
			string text = "";
			text = AfiValue.ToString();
			if (text.Length < 2)
			{
				text = "0" + text;
			}
			if (Uid.Length == 16)
			{
				try
				{
					ComSend("011300030418" + Flag + "27" + Uid + text + "0000", out strRece, 100);
					return 0;
				}
				catch
				{
					return 4;
				}
			}
			return 6;
		}

		public byte RFID_ISO15693LockAfi(string Flag, string Uid)
		{
			string strRece = "";
			if (Flag.Length < 2)
			{
				Flag = "0" + Flag;
			}
			if (Flag.Length != 2)
			{
				return 6;
			}
			if (Uid.Length == 16)
			{
				try
				{
					ComSend("011200030418" + Flag + "28" + Uid + "0000", out strRece, 100);
					return 0;
				}
				catch
				{
					return 4;
				}
			}
			return 6;
		}

		public byte RFID_ISO15693WriteDsfid(string Flag, string Uid, byte DsfidValue)
		{
			string strRece = "";
			string text = "";
			text = DsfidValue.ToString();
			if (Flag.Length < 2)
			{
				Flag = "0" + text;
			}
			if (text.Length < 2)
			{
				text = "0" + text;
			}
			if (Uid.Length == 16)
			{
				try
				{
					ComSend("011300030418" + Flag + "29" + Uid + text + "0000", out strRece, 100);
					return 0;
				}
				catch
				{
					return 4;
				}
			}
			return 6;
		}

		public byte RFID_ISO15693LockDsfid(string Flag, string Uid)
		{
			string strRece = "";
			if (Flag.Length < 2)
			{
				Flag = "0" + Flag;
			}
			if (Uid.Length == 16)
			{
				try
				{
					ComSend("011200030418" + Flag + "2A" + Uid + "0000", out strRece, 100);
					return 0;
				}
				catch
				{
					return 4;
				}
			}
			return 6;
		}

		public byte RFID_WriteDefaultKey(byte DefaultKeyIndx, string DefaultKey)
		{
			string strRece = "";
			try
			{
				if (DefaultKey.Length == 12)
				{
					switch (DefaultKeyIndx)
					{
					case 0:
						ComSend("010E000304E2" + DefaultKey + "0000", out strRece, 100);
						return 0;
					case 1:
						ComSend("010E000304E4" + DefaultKey + "0000", out strRece, 100);
						return 0;
					default:
						return 6;
					}
				}
				return 6;
			}
			catch
			{
				return 4;
			}
		}

		public byte RFID_OpenCard(out string Uid, out string Ctype)
		{
			string strRece = "";
			string text = "";
			long num = 0L;
			int num2 = 0;
			int num3 = 0;
			Uid = "";
			Ctype = text;
			try
			{
				num = ComSend("0108000304FD0000", out strRece, 60);
				Firmware_mode = "FD";
				if (num == 0)
				{
					num = ComSend("010A0003041000010000", out strRece, 60);
					if (num == 0)
					{
						num = ComSend("010C00030410002101080000", out strRece, 60);
						if (num == 0)
						{
							num = ComSend("0109000304F0000000", out strRece, 30);
							if (num == 0)
							{
								num = ComSend("0109000304F1FF0000", out strRece, 30);
								if (num == 0)
								{
									num = ComSend("0109000304A0010000", out strRece, 100);
									if (num == 0)
									{
										num2 = strRece.IndexOf('(');
										num3 = strRece.IndexOf(')');
										if (num3 - num2 > 1)
										{
											text = strRece.Substring(num2 + 1, num3 - num2 - 1);
										}
										Ctype = text;
										num2 = 0;
										num3 = 0;
										num2 = strRece.IndexOf('[');
										num3 = strRece.IndexOf(']');
										if (num3 - num2 > 1)
										{
											Uid = strRece.Substring(num2 + 1, num3 - num2 - 1);
										}
										Uid14443A = Uid;
										if (Uid.Length >= 4)
										{
											return 0;
										}
										return 5;
									}
									return 4;
								}
								return 1;
							}
							return 1;
						}
						return 4;
					}
					return 4;
				}
				return 4;
			}
			catch
			{
				Uid14443A = "";
				return 1;
			}
		}

		public byte RFID_CloseCard()
		{
			long num = 0L;
			string strRece = "";
			try
			{
				num = ComSend("0108000304FD0000", out strRece, 80);
				if (num == 0)
				{
					Uid14443A = "";
					return 0;
				}
				return 4;
			}
			catch
			{
				return 1;
			}
		}

		public byte RFID_ReadMifareOneBlock(byte KeyType, byte DefaultKey, byte DefaultKeyIndx, string Block, string Key, out string Data)
		{
			string strRece = "";
			string text = "";
			string text2 = "";
			long num = 0L;
			int num2 = 0;
			int num3 = 0;
			Data = "";
			text = Block;
			if (text.Length < 2)
			{
				text = "0" + text;
			}
			try
			{
				switch (DefaultKey)
				{
				case 0:
					if (Key.Length == 12)
					{
						if (!(Firmware_mode != "FD"))
						{
							num = ComSend("010A0003041850000000", out strRece, 60);
							if (num == 0)
							{
								num = ComSend("010D000304A2" + Uid14443A + "0000", out strRece, 60);
								if (num == 0)
								{
									num = ComSend("010E000304C0" + Key + "0000", out strRece, 40);
									if (num == 0)
									{
										switch (KeyType)
										{
										case 0:
											num = ComSend("010F000304C160" + text + Uid14443A + "0000", out strRece, 40);
											if (num != 0)
											{
												return 4;
											}
											break;
										case 1:
											num = ComSend("010F000304C161" + text + Uid14443A + "0000", out strRece, 40);
											if (num != 0)
											{
												return 4;
											}
											break;
										default:
											return 6;
										}
										num = ComSend("010C000304C23D6E98990000", out strRece, 60);
										if (num == 0)
										{
											if (strRece.IndexOf("Success") > 0)
											{
												num = ComSend("010A000304C830" + text + "0000", out strRece, 100);
												if (num == 0)
												{
													num2 = strRece.IndexOf('[');
													num3 = strRece.IndexOf(']');
													if (num3 - num2 > 1)
													{
														Data = strRece.Substring(num2 + 1, num3 - num2 - 1);
													}
													if (Data.Length >= 32)
													{
														return 0;
													}
													return 5;
												}
												return 4;
											}
											return 7;
										}
										return 4;
									}
									return 4;
								}
								return 4;
							}
							return 4;
						}
						return 4;
					}
					return 6;
				case 1:
					switch (DefaultKeyIndx)
					{
					case 0:
						ComSend("0108000304E30000", out strRece, 60);
						text2 = strRece.Substring(18, 12);
						break;
					case 1:
						ComSend("0108000304E50000", out strRece, 60);
						text2 = strRece.Substring(18, 12);
						break;
					default:
						return 6;
					}
					num = ComSend("010A0003041850000000", out strRece, 40);
					if (num == 0)
					{
						num = ComSend("010D000304A2" + Uid14443A + "0000", out strRece, 40);
						if (num == 0)
						{
							num = ComSend("010E000304C0" + text2 + "0000", out strRece, 40);
							if (num == 0)
							{
								switch (KeyType)
								{
								case 0:
									num = ComSend("010F000304C160" + text + Uid14443A + "0000", out strRece, 40);
									if (num != 0)
									{
										return 4;
									}
									break;
								case 1:
									num = ComSend("010F000304C161" + text + Uid14443A + "0000", out strRece, 40);
									if (num != 0)
									{
										return 4;
									}
									break;
								default:
									return 6;
								}
								num = ComSend("010C000304C23D6E98990000", out strRece, 60);
								if (num == 0)
								{
									if (strRece.IndexOf("Success") > 0)
									{
										num = ComSend("010A000304C830" + text + "0000", out strRece, 100);
										if (num == 0)
										{
											num2 = strRece.IndexOf('[');
											num3 = strRece.IndexOf(']');
											if (num3 - num2 > 1)
											{
												Data = strRece.Substring(num2 + 1, num3 - num2 - 1);
											}
											if (Data.Length >= 32)
											{
												return 0;
											}
											return 5;
										}
										return 4;
									}
									return 7;
								}
								return 4;
							}
							return 4;
						}
						return 4;
					}
					return 4;
				default:
					return 1;
				}
			}
			catch
			{
				return 1;
			}
		}

		public byte RFID_SetMifareMode()
		{
			string strRece = "";
			Err = ComSend("0108000304FD0000", out strRece, 80);
			Firmware_mode = "FD";
			if (Err != 0)
			{
				return 4;
			}
			Err = ComSend("010A0003041000010000", out strRece, 60);
			if (Err != 0)
			{
				return 4;
			}
			Err = ComSend("010C00030410002101080000", out strRece, 60);
			if (Err != 0)
			{
				return 4;
			}
			Err = ComSend("0109000304F0000000", out strRece, 30);
			if (Err != 0)
			{
				return 1;
			}
			Err = ComSend("0109000304F1FF0000", out strRece, 30);
			if (Err != 0)
			{
				return 1;
			}
			return 0;
		}

		public byte RFID_ORC_Mifare(byte KeyType, string Block, string Key, out string UID, out string Data)
		{
			string strRece = "";
			long num = 0L;
			int num2 = 0;
			int num3 = 0;
			UID = "";
			Data = "";
			try
			{
				if (Firmware_mode != "FD")
				{
					num = ComSend("0108000304FD0000", out strRece, 80);
					Firmware_mode = "FD";
					if (num != 0)
					{
						return 4;
					}
					num = ComSend("010A0003041000010000", out strRece, 60);
					if (num != 0)
					{
						return 4;
					}
					num = ComSend("010C00030410002101080000", out strRece, 60);
					if (num != 0)
					{
						return 4;
					}
					num = ComSend("0109000304F0000000", out strRece, 30);
					if (num != 0)
					{
						return 1;
					}
					num = ComSend("0109000304F1FF0000", out strRece, 30);
				}
				else
				{
					num = ComSend("0108000304FD0000", out strRece, 100);
				}
				if (num != 0)
				{
					return 1;
				}
				num = ComSend("0109000304A0010000", out strRece, 80);
				if (num != 0)
				{
					return 4;
				}
				num2 = strRece.IndexOf('[');
				num3 = strRece.IndexOf(']');
				if (num3 - num2 > 2)
				{
					UID = strRece.Substring(num2 + 1, num3 - num2 - 1);
				}
				else
				{
					num = ComSend("0108000304FD0000", out strRece, 80);
				}
				if (UID.Length < 4)
				{
					return 5;
				}
			}
			catch
			{
				return 1;
			}
			string text = "";
			text = Block;
			if (text.Length < 2)
			{
				text = "0" + text;
			}
			try
			{
				if (Key.Length == 12)
				{
					if (!(Firmware_mode != "FD"))
					{
						num = ComSend("010A0003041850000000", out strRece, 60);
						if (num == 0)
						{
							num = ComSend("010D000304A2" + UID + "0000", out strRece, 60);
							if (num == 0)
							{
								num = ComSend("010E000304C0" + Key + "0000", out strRece, 40);
								if (num == 0)
								{
									switch (KeyType)
									{
									case 0:
										num = ComSend("010F000304C160" + text + UID + "0000", out strRece, 40);
										if (num != 0)
										{
											return 4;
										}
										break;
									case 1:
										num = ComSend("010F000304C161" + text + UID + "0000", out strRece, 40);
										if (num != 0)
										{
											return 4;
										}
										break;
									default:
										return 6;
									}
									num = ComSend("010C000304C23D6E98990000", out strRece, 60);
									if (num == 0)
									{
										if (strRece.IndexOf("Success") > 0)
										{
											num = ComSend("010A000304C830" + text + "0000", out strRece, 100);
											if (num == 0)
											{
												num2 = strRece.IndexOf('[');
												num3 = strRece.IndexOf(']');
												if (num3 - num2 > 1)
												{
													Data = strRece.Substring(num2 + 1, num3 - num2 - 1);
												}
												if (Data.Length >= 32)
												{
													num = ComSend("0108000304FD0000", out strRece, 80);
													return 0;
												}
												return 5;
											}
											return 4;
										}
										return 7;
									}
									return 4;
								}
								return 4;
							}
							return 4;
						}
						return 4;
					}
					return 4;
				}
				return 6;
			}
			catch
			{
				num = ComSend("0108000304FD0000", out strRece, 80);
				return 1;
			}
		}

		public byte RFID_WriteMifareOneBlock(byte KeyType, byte DefaultKey, byte DefaultKeyIndx, string Block, string Key, string Data)
		{
			string strRece = "";
			string text = "";
			string text2 = "";
			long num = 0L;
			text = Block;
			if (text.Length < 2)
			{
				text = "0" + text;
			}
			if (Data.Length == 32)
			{
				try
				{
					for (int i = 0; i < 16; i++)
					{
						int.Parse(Data.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier);
					}
				}
				catch
				{
					return 6;
				}
				try
				{
					switch (DefaultKey)
					{
					case 0:
						if (!(Firmware_mode != "FD"))
						{
							num = ComSend("010A0003041850000000", out strRece, 60);
							if (num == 0)
							{
								num = ComSend("010D000304A2" + Uid14443A + "0000", out strRece, 60);
								if (num == 0)
								{
									if (Key.Length == 12)
									{
										num = ComSend("010E000304C0" + Key + "0000", out strRece, 40);
										if (num == 0)
										{
											switch (KeyType)
											{
											case 0:
												num = ComSend("010F000304C160" + text + Uid14443A + "0000", out strRece, 40);
												if (num != 0)
												{
													return 4;
												}
												break;
											case 1:
												num = ComSend("010F000304C161" + text + Uid14443A + "0000", out strRece, 40);
												if (num != 0)
												{
													return 4;
												}
												break;
											default:
												return 6;
											}
											num = ComSend("010C000304C23D6E98990000", out strRece, 60);
											if (num == 0)
											{
												if (strRece.IndexOf("Success") > 0)
												{
													num = ComSend("010A000304C8A0" + text + "0000", out strRece, 200);
													if (num == 0)
													{
														num = ComSend("0118000304C8" + Data + "0000", out strRece, 200);
														if (num == 0)
														{
															if (strRece.IndexOf("0A") <= 0)
															{
																return 8;
															}
															return 0;
														}
														return 4;
													}
													return 4;
												}
												return 7;
											}
											return 4;
										}
										return 4;
									}
									return 6;
								}
								return 4;
							}
							return 4;
						}
						return 4;
					case 1:
						switch (DefaultKeyIndx)
						{
						case 0:
							ComSend("0108000304E30000", out strRece, 80);
							text2 = strRece.Substring(18, 12);
							break;
						case 1:
							ComSend("0108000304E50000", out strRece, 80);
							text2 = strRece.Substring(18, 12);
							break;
						default:
							return 6;
						}
						num = ComSend("010A0003041850000000", out strRece, 70);
						if (num == 0)
						{
							num = ComSend("010D000304A2" + Uid14443A + "0000", out strRece, 70);
							if (num == 0)
							{
								num = ComSend("010E000304C0" + text2 + "0000", out strRece, 70);
								if (num == 0)
								{
									switch (KeyType)
									{
									case 0:
										num = ComSend("010F000304C160" + text + Uid14443A + "0000", out strRece, 70);
										if (num != 0)
										{
											return 4;
										}
										break;
									case 1:
										num = ComSend("010F000304C161" + text + Uid14443A + "0000", out strRece, 70);
										if (num != 0)
										{
											return 4;
										}
										break;
									default:
										return 6;
									}
									num = ComSend("010C000304C23D6E98990000", out strRece, 70);
									if (num == 0)
									{
										if (strRece.IndexOf("Success") > 0)
										{
											num = ComSend("010A000304C8A0" + text + "0000", out strRece, 80);
											if (num == 0)
											{
												num = ComSend("0118000304C8" + Data + "0000", out strRece, 80);
												if (num == 0)
												{
													if (strRece.IndexOf("0A") <= 0)
													{
														return 8;
													}
													return 0;
												}
												return 4;
											}
											return 4;
										}
										return 7;
									}
									return 4;
								}
								return 4;
							}
							return 4;
						}
						return 4;
					default:
						return 1;
					}
				}
				catch
				{
					return 1;
				}
			}
			return 6;
		}

		public byte RFID_Get14443AUID(string Antenna_ON_OFF, out string Uid, out string Ctype)
		{
			string strRece = "";
			string text = "";
			long num = 0L;
			int num2 = 0;
			int num3 = 0;
			Uid = "";
			Ctype = text;
			try
			{
				num = ComSend("0109000304A0010000", out strRece, 80);
				if (num == 0)
				{
					num2 = strRece.IndexOf('(');
					num3 = strRece.IndexOf(')');
					if (num3 - num2 > 1)
					{
						text = strRece.Substring(num2 + 1, num3 - num2 - 1);
					}
					Ctype = text;
					num2 = 0;
					num3 = 0;
					num2 = strRece.IndexOf('[');
					num3 = strRece.IndexOf(']');
					if (num3 - num2 > 1)
					{
						Uid = strRece.Substring(num2 + 1, num3 - num2 - 1);
					}
					Uid14443A = Uid;
					if (Uid14443A.Length >= 1)
					{
						int num4 = 0;
						int num5 = 0;
						string text2 = "";
						num4 = Uid.Length / 2 + 8;
						text2 = num4.ToString("X");
						if (text2.Length < 2)
						{
							text2 = "0" + text2;
						}
						num = ComSend("010A0003041850000000", out strRece, 80);
						if (num == 0)
						{
							num = ComSend("01" + text2 + "000304A2" + Uid + "0000", out strRece, 80);
							if (num == 0)
							{
								num5 = strRece.IndexOf("[");
								if (!(strRece.Substring(num5 + 3, 1) != "]"))
								{
									if (Uid.Length >= 4)
									{
										if (Antenna_ON_OFF == "1")
										{
											ComSend("010A0003041000800000", out strRece, 20);
											if (Power_set == 1)
											{
												ComSend("010A0003041000210000", out strRece, 20);
											}
											else
											{
												ComSend("010A0003041000310000", out strRece, 20);
											}
										}
										return 0;
									}
									return 5;
								}
								Uid = "";
								return 5;
							}
							return 4;
						}
						return 4;
					}
					return 1;
				}
				return 4;
			}
			catch
			{
				if (Antenna_ON_OFF == "1")
				{
					ComSend("010A0003041000800000", out strRece, 20);
					if (Power_set == 1)
					{
						ComSend("010A0003041000210000", out strRece, 20);
					}
					else
					{
						ComSend("010A0003041000310000", out strRece, 20);
					}
				}
				Uid = "";
				return 1;
			}
		}

		public byte RFID_Get14443BUID(out string Uid)
		{
			string strRece = "";
			long num = 0L;
			int num2 = 0;
			int num3 = 0;
			Uid = "";
			try
			{
				num = ComSend("0109000304B0000000", out strRece, 300);
				if (num == 0)
				{
					for (int i = 0; i < strRece.Length; i++)
					{
						if (strRece.Substring(i, 1) == "[")
						{
							num2 = i;
						}
						if (strRece.Substring(i, 1) == "]")
						{
							num3 = i;
						}
						if (num3 - num2 > 3)
						{
							Uid = strRece.Substring(num2 + 1, num3 - num2 - 1);
							break;
						}
					}
					if (Uid.Length >= 4)
					{
						return 0;
					}
					return 5;
				}
				return 4;
			}
			catch
			{
				return 1;
			}
		}

		public byte RFID_ReadUltraLightBlock(string Block, out string Data)
		{
			string strRece = "";
			int num = 0;
			int num2 = 0;
			Data = "";
			if (Block.Length < 2)
			{
				Block = "0" + Block;
			}
			if (Block.Length >= 2)
			{
				try
				{
					ComSend("0108000304FF0000", out strRece, 100);
					ComSend("0108000304FD0000", out strRece, 100);
					Firmware_mode = "FD";
					ComSend("010A0003041000010000", out strRece, 100);
					ComSend("010C00030410002101080000", out strRece, 250);
					ComSend("0109000304A0010000", out strRece, 250);
					ComSend("010A0003041830" + Block + "0000", out strRece, 80);
					num = strRece.IndexOf("[");
					num2 = strRece.IndexOf("]");
					if (num2 - num > 7)
					{
						Data = strRece.Substring(num + 1, 8);
					}
					if (Data.Length <= 7)
					{
						return 5;
					}
					return 0;
				}
				catch
				{
					return 1;
				}
			}
			return 6;
		}

		public byte RFID_WriteUltraLightBlock(string Block, string Data)
		{
			string strRece = "";
			if (Block.Length < 2)
			{
				Block = "0" + Block;
			}
			if (Block.Length < 2)
			{
				return 6;
			}
			if (Data.Length == 8)
			{
				try
				{
					for (int i = 0; i < 4; i++)
					{
						int.Parse(Data.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier);
					}
				}
				catch
				{
					return 6;
				}
				try
				{
					ComSend("0108000304FD0000", out strRece, 100);
					Firmware_mode = "FD";
					ComSend("010A0003041000010000", out strRece, 100);
					ComSend("010C00030410002101080000", out strRece, 200);
					ComSend("0109000304A0010000", out strRece, 200);
					ComSend("010A0003041830" + Block + "0000", out strRece, 100);
					ComSend("010A00030418A0" + Block + "0000", out strRece, 100);
					ComSend("011800030418" + Data + "05060708090A0B0C0D0E0F110000", out strRece, 100);
					if (strRece.IndexOf("z") <= 0)
					{
						return 1;
					}
					return 0;
				}
				catch
				{
					return 1;
				}
			}
			return 6;
		}

		public byte RFID_14443BSelect(string Ctype, out string IDNum)
		{
			string strRece = "";
			long num = 0L;
			IDNum = "";
			try
			{
				if (Ctype == "01")
				{
					num = ComSend("0109000304B0040000", out strRece, 80);
				}
				if (Ctype == "02")
				{
					num = ComSend("010A0003041806000000", out strRece, 80);
					num = ComSend("010A000304180E200000", out strRece, 80);
				}
				if (Ctype == "03")
				{
					ComSend("010A000304180E" + ChipSRIX4K + "0000", out strRece, 80);
					ComSend("0109000304180B0000", out strRece, 80);
				}
				if (num == 0)
				{
					if (Ctype == "01")
					{
						char[] array = strRece.ToCharArray();
						string text = "";
						try
						{
							for (int i = 0; i < array.Length; i++)
							{
								if (array[i] == '[' && array[i + 1] != ']')
								{
									for (int j = 1; j < 9; j++)
									{
										text += array[i + 2 + j].ToString();
									}
								}
							}
						}
						catch
						{
							return 1;
						}
						IDNum = text;
					}
					if (Ctype == "02" || Ctype == "03")
					{
						int num2 = 0;
						int num3 = 0;
						string text2 = "";
						try
						{
							num2 = strRece.IndexOf('[');
							num3 = strRece.IndexOf(']');
							text2 = strRece.Substring(num2 + 1, num3 - num2 - 1);
							string text3 = "";
							for (int num4 = text2.Length - 2; num4 > -1; num4 -= 2)
							{
								text3 += text2.Substring(num4, 2);
							}
							text2 = text3;
						}
						catch
						{
							return 1;
						}
						IDNum = text2;
					}
					if (IDNum.Length >= 4)
					{
						return 0;
					}
					return 5;
				}
				return 4;
			}
			catch
			{
				return 1;
			}
		}

		public byte RFID_SRIX4KChipID(out string ChipID)
		{
			string strRece = "";
			long num = 0L;
			ChipID = "";
			try
			{
				num = ComSend("010A0003041806000000", out strRece, 100);
				if (num == 0)
				{
					int num2 = 0;
					int num3 = 0;
					string text = "";
					try
					{
						num2 = strRece.IndexOf('[');
						num3 = strRece.IndexOf(']');
						text = strRece.Substring(num2 + 1, num3 - num2 - 1);
					}
					catch
					{
						return 1;
					}
					ChipID = text;
					if (ChipID.Length >= 2)
					{
						ChipSRIX4K = ChipID;
						return 0;
					}
					return 5;
				}
				return 4;
			}
			catch
			{
				return 1;
			}
		}

		public byte RFID_SRIX4KReadBlock(string Block, out string Data)
		{
			string strRece = "";
			int num = 0;
			int num2 = 0;
			Data = "";
			if (Block.Length < 2)
			{
				Block = "0" + Block;
			}
			if (Block.Length >= 2)
			{
				try
				{
					ComSend("010A000304180E" + ChipSRIX4K + "0000", out strRece, 80);
					num = strRece.IndexOf("[");
					num2 = strRece.IndexOf("]");
					if (num2 - num < 3)
					{
						for (int i = 0; i < 10; i++)
						{
							ComSend("010A000304180E" + ChipSRIX4K + "0000", out strRece, 80);
							num = strRece.IndexOf("[");
							num2 = strRece.IndexOf("]");
							ComSend("010A0003041808" + Block + "0000", out strRece, 80);
							if (num2 - num > 2)
							{
								i = 10;
							}
						}
					}
					else
					{
						ComSend("010A0003041808" + Block + "0000", out strRece, 80);
					}
					num = strRece.IndexOf("[");
					Data = strRece.Substring(num + 1, 8);
					if (Data.Length != 8)
					{
						return 5;
					}
					return 0;
				}
				catch
				{
					return 5;
				}
			}
			return 6;
		}

		public byte RFID_SRIX4KWriteBlock(string Block, string Data)
		{
			string strRece = "";
			if (Block.Length < 2)
			{
				Block = "0" + Block;
			}
			if (Block.Length < 2)
			{
				return 6;
			}
			if (Data.Length == 8)
			{
				try
				{
					for (int i = 0; i < 4; i++)
					{
						int.Parse(Data.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier);
					}
				}
				catch
				{
					return 6;
				}
				try
				{
					ComSend("010A000304180E" + ChipSRIX4K + "0000", out strRece, 80);
					ComSend("010E0003041809" + Block + Data + "0000", out strRece, 80);
					ComSend("010A000304180E" + ChipSRIX4K + "0000", out strRece, 80);
					ComSend("010A0003041808" + Block + "0000", out strRece, 80);
					if (strRece.IndexOf(Data) < 1)
					{
						for (int j = 0; j < 10; j++)
						{
							ComSend("010A000304180E" + ChipSRIX4K + "0000", out strRece, 80);
							ComSend("010A0003041808" + Block + "0000", out strRece, 80);
							if (strRece.IndexOf(Data) > 0)
							{
								j = 10;
							}
						}
					}
					if (strRece.IndexOf(Data) <= 0)
					{
						return 1;
					}
					return 0;
				}
				catch
				{
					return 4;
				}
			}
			return 6;
		}

		public byte Felica_GetPicc(out string PICC)
		{
			string strRece = "";
			PICC = "";
			try
			{
				ComSend("010900030444000000", out strRece, 100);
				int num = 0;
				int num2 = 0;
				num = strRece.IndexOf("[");
				num2 = strRece.IndexOf("]");
				if (num2 - num >= 36)
				{
					PICC = strRece.Substring(num2 - 32, 16);
					return 0;
				}
				return 1;
			}
			catch
			{
				return 1;
			}
		}

		public byte Felica_Read(string Block, string PICC, out string Data)
		{
			string strRece = "";
			string text = "";
			Data = "";
			text = Block;
			if (text.Length < 2)
			{
				text = "0" + text;
			}
			try
			{
				ComSend("0118000304181006" + PICC + "0109000180" + text + "0000", out strRece, 100);
				int num = 0;
				int num2 = 0;
				num = strRece.IndexOf("[");
				num2 = strRece.IndexOf("]");
				if (num2 - num >= 50)
				{
					Data = strRece.Substring(num2 - 32, 32);
					return 0;
				}
				return 1;
			}
			catch
			{
				return 1;
			}
		}

		public byte Felica_Write(string Block, string PICC, string Data)
		{
			string strRece = "";
			string text = "";
			Data += "00000000000000000000000000000000";
			Data = Data.Substring(0, 32);
			text = Block;
			if (text.Length < 2)
			{
				text = "0" + text;
			}
			try
			{
				ComSend("0128000304182008" + PICC + "0109000180" + text + Data + "0000", out strRece, 100);
				return 0;
			}
			catch
			{
				return 1;
			}
		}

		public byte NTAG_Write(string Block, string Data)
		{
			try
			{
				string strRece = "";
				if (Block.Length != 0)
				{
					if (Block.Length == 1)
					{
						Block = "0" + Block;
					}
					Data += "00000000";
					Data = Data.Substring(0, 8);
					ComSend("010E00030418A2" + Block + Data + "0000", out strRece, 100);
					strRece.IndexOf("[");
					if (strRece.IndexOf("]") - strRece.IndexOf("[") <= 1)
					{
						return 1;
					}
					return 0;
				}
				return 1;
			}
			catch
			{
				return 1;
			}
		}

		public byte NTAG_Read(string Block, out string Data)
		{
			try
			{
				Data = "";
				string strRece = "";
				int num = 0;
				int num2 = 0;
				if (Block.Length != 0)
				{
					if (Block.Length == 1)
					{
						Block = "0" + Block;
					}
					ComSend("010A0003041830" + Block + "0000", out strRece, 100);
					num = strRece.IndexOf("[");
					num2 = strRece.IndexOf("]");
					if (num2 - num <= 32)
					{
						return 1;
					}
					Data = strRece.Substring(num + 1, 32);
					return 0;
				}
				return 1;
			}
			catch
			{
				Data = "";
				return 1;
			}
		}

		public byte RFID_SendCommand(string Command, out string data, int delay)
		{
			data = "";
			try
			{
				if (Command.Length % 2 <= 0)
				{
					string text = (Command.Length / 2 + 8).ToString("X");
					if (text.Length < 2)
					{
						text = "0" + text;
					}
					sptCom.Write("01" + text + "00030418" + Command + "0000");
					Thread.Sleep(delay);
					data = sptCom.ReadExisting();
					return 0;
				}
				return 1;
			}
			catch
			{
				return 1;
			}
		}

		public byte RFID_SendBytes(byte[] Command, out string data, int delay)
		{
			data = "";
			byte[] array = new byte[2];
			byte[] buffer = array;
			try
			{
				string text = (Command.Length + 8).ToString("X");
				if (text.Length < 2)
				{
					text = "0" + text;
				}
				sptCom.Write("01" + text + "00030417");
				sptCom.Write(Command, 0, Command.Length);
				sptCom.Write(buffer, 0, 2);
				Thread.Sleep(delay);
				data = sptCom.ReadExisting();
				return 0;
			}
			catch
			{
				return 1;
			}
		}

		public byte UHC_OpenReader(string COMPort)
		{
			try
			{
				if (sptCom2 == null)
				{
					sptCom2 = new SerialPort(COMPort, 38400);
				}
				if (!sptCom2.IsOpen)
				{
					sptCom2.Encoding = Encoding.Default;
					sptCom2.Parity = Parity.None;
					sptCom2.DataBits = 8;
					sptCom2.StopBits = StopBits.One;
					sptCom2.ReadTimeout = 500;
					sptCom2.WriteTimeout = 500;
					sptCom2.Open();
					Thread.Sleep(1000);
					return 0;
				}
				sptCom2.Close();
				sptCom2 = null;
				return 2;
			}
			catch
			{
				sptCom2.Close();
				sptCom2 = null;
				sptCom2.Dispose();
				return 2;
			}
		}

		public byte UHF_CloseReader(string COMPort)
		{
			try
			{
				if (sptCom2.IsOpen)
				{
					sptCom2.Close();
					sptCom2 = null;
					sptCom2.Dispose();
					return 0;
				}
			}
			catch
			{
				return 3;
			}
			return 1;
		}

		public byte UHF_FwVersion(out string outData)
		{
			outData = "";
			try
			{
				byte[] buffer = new byte[2]
				{
					86,
					13
				};
				string text = "";
				sptCom2.Write(buffer, 0, 2);
				Thread.Sleep(100);
				text = (outData = sptCom2.ReadExisting());
				return 0;
			}
			catch
			{
				return 1;
			}
		}

		public byte UHF_GetEPC(string Time, out string outData)
		{
			outData = "";
			int millisecondsTimeout = Convert.ToInt32(Time);
			try
			{
				byte[] buffer = new byte[2]
				{
					81,
					13
				};
				string text = "";
				sptCom2.Write(buffer, 0, 2);
				Thread.Sleep(millisecondsTimeout);
				text = sptCom2.ReadExisting();
				Err = crc16(text.Substring(2, text.Length - 4));
				if (Err == 0)
				{
					if (text.Length <= 30)
					{
						return 1;
					}
					outData += text.Substring(6, 24);
					return 0;
				}
				return 1;
			}
			catch
			{
				return 1;
			}
		}

		public byte UHF_GetMultiEPC(string Time, out string outData)
		{
			outData = "";
			int num = 0;
			int millisecondsTimeout = Convert.ToInt32(Time);
			try
			{
				byte[] buffer = new byte[2]
				{
					85,
					13
				};
				string text = "";
				sptCom2.Write(buffer, 0, 2);
				Thread.Sleep(millisecondsTimeout);
				text = sptCom2.ReadExisting();
				Err = crc16(text.Substring(2, text.Length - 4));
				if (Err == 0)
				{
					for (int i = 0; i < text.Length; i++)
					{
						num = text.IndexOf("U", num);
						if (num > 0 && num + 29 < text.Length)
						{
							outData += text.Substring(num + 5, 24);
							num += 29;
						}
						if (num + 29 > text.Length)
						{
							break;
						}
					}
					return 0;
				}
				return 1;
			}
			catch
			{
				return 1;
			}
		}

		public byte UHF_GetRegulation(out string outData)
		{
			outData = "";
			try
			{
				byte[] buffer = new byte[6]
				{
					78,
					52,
					44,
					48,
					48,
					13
				};
				string text = "";
				sptCom2.Write(buffer, 0, 6);
				Thread.Sleep(100);
				text = (outData = sptCom2.ReadExisting());
				if (outData.Length > 5)
				{
					outData = outData.Substring(1, 3);
					switch (outData)
					{
					case "N01":
						outData = "US 902~928";
						break;
					case "N02":
						outData = "TW 922~928";
						break;
					case "N03":
						outData = "CN 920~925";
						break;
					case "N04":
						outData = "CN2 840~845";
						break;
					case "N05":
						outData = "EU 865~868";
						break;
					default:
						return 1;
					}
				}
				return 0;
			}
			catch
			{
				return 1;
			}
		}

		public byte UHF_GetPowerlevel(out string outData)
		{
			outData = "";
			try
			{
				byte[] buffer = new byte[6]
				{
					78,
					48,
					44,
					48,
					48,
					13
				};
				string text = "";
				sptCom2.Write(buffer, 0, 6);
				Thread.Sleep(100);
				text = (outData = sptCom2.ReadExisting());
				return 0;
			}
			catch
			{
				return 1;
			}
		}

		public byte UHF_SetRegulation(string Select)
		{
			try
			{
				byte[] array = new byte[6]
				{
					78,
					53,
					44,
					48,
					48,
					13
				};
				switch (Select)
				{
				case "01":
					array[4] = 49;
					break;
				case "02":
					array[4] = 50;
					break;
				case "03":
					array[4] = 51;
					break;
				case "04":
					array[4] = 52;
					break;
				case "05":
					array[4] = 53;
					break;
				default:
					return 1;
				}
				sptCom2.Write(array, 0, 6);
				Thread.Sleep(100);
				sptCom2.ReadExisting();
				return 0;
			}
			catch
			{
				return 1;
			}
		}

		public byte UHF_SetPowerlevel(string Power)
		{
			try
			{
				if (Power.Length < 2)
				{
					Power = "0" + Power;
				}
				byte[] array = new byte[6]
				{
					78,
					49,
					44,
					70,
					70,
					13
				};
				array[3] = (byte)Convert.ToInt16(Power[0]);
				array[4] = (byte)Convert.ToInt16(Power[1]);
				if ((array[3] <= 47 || array[3] >= 58) && (array[3] <= 64 || array[3] >= 71))
				{
					return 1;
				}
				if ((array[4] <= 47 || array[4] >= 58) && (array[4] <= 64 || array[4] >= 71))
				{
					return 1;
				}
				sptCom2.Write(array, 0, 6);
				Thread.Sleep(100);
				sptCom2.ReadExisting();
				return 0;
			}
			catch
			{
				return 1;
			}
		}

		public byte UHF_ReaderID(out string outData)
		{
			outData = "";
			try
			{
				byte[] buffer = new byte[2]
				{
					83,
					13
				};
				string text = "";
				sptCom2.Write(buffer, 0, 2);
				Thread.Sleep(100);
				text = (outData = sptCom2.ReadExisting());
				return 0;
			}
			catch
			{
				return 1;
			}
		}

		public byte UHF_RBlock(string Block, out string outData)
		{
			outData = "";
			int length = Block.Length;
			try
			{
				byte[] buffer = new byte[3]
				{
					82,
					51,
					44
				};
				byte[] array = new byte[4];
				byte[] array2 = array;
				byte[] buffer2 = new byte[3]
				{
					44,
					49,
					13
				};
				char[] array3 = Block.ToCharArray();
				switch (length)
				{
				case 0:
					return 1;
				case 1:
					array2[0] = (byte)array3[0];
					break;
				case 2:
					array2[0] = (byte)array3[0];
					array2[1] = (byte)array3[1];
					break;
				case 3:
					array2[0] = (byte)array3[0];
					array2[1] = (byte)array3[1];
					array2[2] = (byte)array3[2];
					break;
				case 4:
					array2[0] = (byte)array3[0];
					array2[1] = (byte)array3[1];
					array2[2] = (byte)array3[2];
					array2[3] = (byte)array3[3];
					break;
				default:
					return 1;
				}
				string text = "";
				sptCom2.Write(buffer, 0, 3);
				sptCom2.Write(array2, 0, length);
				sptCom2.Write(buffer2, 0, 3);
				Thread.Sleep(100);
				text = sptCom2.ReadExisting();
				if (text.Length <= 4)
				{
					return 1;
				}
				outData = text.Substring(2, 4);
				return 0;
			}
			catch
			{
				return 1;
			}
		}

		public byte UHF_WBlock(string Block, string Wdata, out string outData)
		{
			outData = "";
			int length = Block.Length;
			int length2 = Wdata.Length;
			if (length2 == 4)
			{
				try
				{
					byte[] buffer = new byte[3]
					{
						87,
						51,
						44
					};
					byte[] array = new byte[4];
					byte[] array2 = array;
					byte[] buffer2 = new byte[3]
					{
						44,
						49,
						44
					};
					byte[] array3 = new byte[5]
					{
						0,
						0,
						0,
						0,
						13
					};
					char[] array4 = Block.ToCharArray();
					char[] array5 = Wdata.ToCharArray();
					array3[0] = (byte)array5[0];
					array3[1] = (byte)array5[1];
					array3[2] = (byte)array5[2];
					array3[3] = (byte)array5[3];
					switch (length)
					{
					case 0:
						return 2;
					case 1:
						array2[0] = (byte)array4[0];
						break;
					case 2:
						array2[0] = (byte)array4[0];
						array2[1] = (byte)array4[1];
						break;
					case 3:
						array2[0] = (byte)array4[0];
						array2[1] = (byte)array4[1];
						array2[2] = (byte)array4[2];
						break;
					case 4:
						array2[0] = (byte)array4[0];
						array2[1] = (byte)array4[1];
						array2[2] = (byte)array4[2];
						array2[3] = (byte)array4[3];
						break;
					default:
						return 3;
					}
					string text = "";
					sptCom2.Write(buffer, 0, 3);
					sptCom2.Write(array2, 0, length);
					sptCom2.Write(buffer2, 0, 3);
					sptCom2.Write(array3, 0, 5);
					Thread.Sleep(100);
					text = (outData = sptCom2.ReadExisting());
					return 0;
				}
				catch
				{
					return 5;
				}
			}
			return 1;
		}

		public byte HexToDecimal(string Hex, out string Decimal)
		{
			ulong num = 0uL;
			Decimal = "";
			try
			{
				num = (ulong)Convert.ToInt64(Hex, 16);
				Decimal = num.ToString();
				return 0;
			}
			catch
			{
				return 1;
			}
		}

		private int crc16(string CRC)
		{
			try
			{
				if (CRC.Length >= 2)
				{
					byte[] array = new byte[CRC.Length / 2];
					int num = 0;
					for (int i = 0; i < CRC.Length; i += 2)
					{
						array[num] = (byte)Convert.ToInt32(CRC.Substring(i, 2), 16);
						num++;
					}
					int num2 = 65535;
					int num3 = 4129;
					int num4 = 0;
					int num5 = 0;
					if (array[0] == 0)
					{
						num4 = 8;
						num5 = 1;
					}
					int j = num4;
					int num6 = num5;
					for (; j < array.Length * 8; j++)
					{
						if (j % 8 == 0)
						{
							num2 ^= ((array[num6++] << 8) & 0xFF00);
						}
						if ((num2 & 0x8000) != 0)
						{
							num2 = (((num2 << 1) & 0xFFFE) ^ num3);
						}
						else
						{
							num2 <<= 1;
							num2 &= 0xFFFE;
						}
					}
					num2 &= 0xFFFF;
					if (num2 != 7439)
					{
						return 1;
					}
					return 0;
				}
				return 1;
			}
			catch
			{
				return 1;
			}
		}
	}
}
