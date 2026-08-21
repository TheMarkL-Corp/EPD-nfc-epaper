using com.advantech.nfc;
using com.advantech.nfc.cmd;
using J_RFID;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO.Ports;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyTitle("AdvNFCWrap")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("AdvNFCWrap")]
[assembly: AssemblyCopyright("Copyright ©  2021")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("edeaa656-624f-4165-9f23-291560aca64e")]
[assembly: AssemblyFileVersion("1.0.1.0")]
[assembly: TargetFramework(".NETFramework,Version=v4.6.1", FrameworkDisplayName = ".NET Framework 4.6.1")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion("1.0.1.0")]
namespace AdvNFCWrap
{
	public static class Constants
	{
		public static string FW_SUPPORT = "";
	}
	public class NFCError
	{
		public static string NFC_MSG_SUCCESS = "0000";

		public static string NFC_MSG_PORT_EMPTY = "0101";

		public static string NFC_MSG_TAG_NOREADY = "0201";

		public static string NFC_MSG_TAG_COMMAND_ERROR = "0202";

		public static string NFC_MSG_TAG_FW_NO_SUPPORT = "0203";

		public static string NFC_MSG_DATA_LENGTH_TOOLARGE = "0301";

		private static Dictionary<string, string> defaultMessages = new Dictionary<string, string>
		{
			{
				NFC_MSG_SUCCESS,
				"Success"
			},
			{
				NFC_MSG_PORT_EMPTY,
				"Port is empty"
			},
			{
				NFC_MSG_TAG_NOREADY,
				"Tag not ready"
			},
			{
				NFC_MSG_TAG_COMMAND_ERROR,
				"Tag command error"
			},
			{
				NFC_MSG_TAG_FW_NO_SUPPORT,
				"Tag firmware version not support"
			},
			{
				NFC_MSG_DATA_LENGTH_TOOLARGE,
				"Data length too large"
			}
		};

		public string Code
		{
			get;
			set;
		}

		public string Content
		{
			get;
			set;
		}

		public NFCError(string code)
		{
			if (code == NFC_MSG_TAG_FW_NO_SUPPORT)
			{
				setFWSupport();
			}
			Code = code;
			if (defaultMessages.ContainsKey(Code))
			{
				Content = defaultMessages[Code];
			}
			else
			{
				Content = code;
				Code = "0000";
			}
		}

		public override string ToString()
		{
			return "Code: " + Code + ", Message: " + Content;
		}

		public void setFWSupport()
		{
			defaultMessages[NFC_MSG_TAG_FW_NO_SUPPORT] = $"Tag firmware version not support, the minimum support version is [ {Constants.FW_SUPPORT} ]";
		}
	}
	public class NFCWrap : NFCTagChangeListener, IDrawImageCallback
	{
		public enum nTagState
		{
			NFC_TAG_STATE_TAG_OFF,
			NFC_TAG_STATE_TAG_ON,
			NFC_TAG_STATE_COMM_ON
		}

		public interface TagState
		{
			void onTagState(nTagState state);
		}

		public enum nImageState
		{
			DIState_Erase,
			DIState_SendData,
			DIState_WriteToEPD,
			DIState_Finish,
			DIState_Error
		}

		public interface ProcessState
		{
			void onProcessState(nImageState state, object data);
		}

		public struct myColor
		{
			public byte A;

			public byte R;

			public byte G;

			public byte B;
		}

		public class VersionComparer
		{
			public static int CompareVersions(string version1, string version2)
			{
				Version version3 = new Version(version1);
				Version value = new Version(version2);
				return version3.CompareTo(value);
			}
		}

		private static NFCManager manager = null;

		private static INFCEDPAPI api = null;

		private static INFCCommand nfc = null;

		private static TagState _tagState = null;

		private static ProcessState _processState = null;

		private string comPort = "";

		private static RFIDAPI NFC_API = new RFIDAPI();

		private bool _TagReady;

		private static bool chkTagConnected = false;

		private bool _bNFC;

		private bool _bNFCData;

		private int nTryCount = 100;

		private static nImageState mImageState;

		public TagState TagStateListener
		{
			get
			{
				return _tagState;
			}
			set
			{
				_tagState = value;
			}
		}

		public ProcessState ProcessStateListener
		{
			get
			{
				return _processState;
			}
			set
			{
				_processState = value;
			}
		}

		public NFCWrap()
		{
			comPort = "";
			CloseNFC();
			CloseNFCData();
		}

		public NFCWrap(string strPort)
		{
			comPort = strPort;
			CloseNFC();
			CloseNFCData();
		}

		public string GetPort()
		{
			CloseNFC();
			CloseNFCData();
			string result = NFCError.NFC_MSG_PORT_EMPTY;
			string[] portNames = SerialPort.GetPortNames();
			for (int i = 0; i < portNames.Length; i++)
			{
				string cOMPort = portNames[i];
				NFC_API.RFID_OpenReader(cOMPort);
				string FirmwareVer = "";
				if (NFC_API.RFID_FWVersion(out FirmwareVer) == 0)
				{
					NFC_API.RFID_CloseReader(comPort);
					result = portNames[i];
					break;
				}
				NFC_API.RFID_CloseReader(comPort);
			}
			return result;
		}

		public string ConnectTag()
		{
			string result = NFCError.NFC_MSG_SUCCESS;
			if (comPort != "")
			{
				OpenNFC();
			}
			else
			{
				result = NFCError.NFC_MSG_PORT_EMPTY;
			}
			return result;
		}

		public async Task<string> ConnectTagAsync()
		{
			string text;
			if (comPort != "")
			{
				OpenNFC();
				text = await chkTagState();
			}
			else
			{
				text = NFCError.NFC_MSG_PORT_EMPTY;
			}
			Console.WriteLine("ConnectTagAsync : [" + text + "]");
			return text;
		}

		private static async Task<string> chkTagState()
		{
			string result = NFCError.NFC_MSG_TAG_NOREADY;
			await Task.Run(delegate
			{
				Thread.Sleep(3000);
				result = (chkTagConnected ? NFCError.NFC_MSG_SUCCESS : NFCError.NFC_MSG_TAG_NOREADY);
			});
			return result;
		}

		public string DisconnectTag()
		{
			string text = NFCError.NFC_MSG_SUCCESS;
			if (comPort != "")
			{
				CloseNFC();
				CloseNFCData();
			}
			else
			{
				text = NFCError.NFC_MSG_PORT_EMPTY;
			}
			Console.WriteLine("DisconnectTag : [" + text + "]");
			return text;
		}

		public string GetTagID()
		{
			if (api == null)
			{
				OpenNFC();
			}
			string text = "";
			if (_TagReady)
			{
				byte[] tagID = api.getTagID();
				if (tagID == null)
				{
					text = NFCError.NFC_MSG_TAG_COMMAND_ERROR;
				}
				else
				{
					byte[] array = tagID;
					foreach (byte b in array)
					{
						text += $"{b:X2}";
					}
				}
			}
			else
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			Console.WriteLine("GetTagID : [" + text + "]");
			return text;
		}

		public string GetVersion()
		{
			if (api == null)
			{
				OpenNFC();
			}
			string text = "";
			text = ((!_TagReady) ? NFCError.NFC_MSG_TAG_NOREADY : api.GetVersion());
			Console.WriteLine("GetVersion : [" + text + "]");
			return text;
		}

		public string GetPlatformName()
		{
			if (api == null)
			{
				OpenNFC();
			}
			string text = "";
			text = ((!_TagReady) ? NFCError.NFC_MSG_TAG_NOREADY : api.GetPlatformName());
			Console.WriteLine("GetPlatformName : [" + text + "]");
			return text;
		}

		public string GetSN()
		{
			if (api == null)
			{
				OpenNFC();
			}
			string text = "";
			text = ((!_TagReady) ? NFCError.NFC_MSG_TAG_NOREADY : api.GetSN());
			Console.WriteLine("GetSN : [" + text + "]");
			return text;
		}

		public string SetPingCode(string strData)
		{
			if (api == null)
			{
				OpenNFC();
			}
			string text = "";
			if (_TagReady)
			{
				byte[] pinCode = Hex2Byte(strData);
				text = ((!api.SetPinCode(pinCode)) ? NFCError.NFC_MSG_TAG_COMMAND_ERROR : NFCError.NFC_MSG_SUCCESS);
			}
			else
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			Console.WriteLine("SetPingCode : [" + text + "]");
			return text;
		}

		public string GetPinCodeStatus()
		{
			if (api == null)
			{
				OpenNFC();
			}
			string text = "";
			if (_TagReady)
			{
				byte pinCodeStatus = api.GetPinCodeStatus();
				int num = pinCodeStatus & 0xF;
				string arg;
				switch (pinCodeStatus >> 4)
				{
				case 0:
					arg = "UNLOCKED";
					break;
				case 1:
					arg = "LOCKED";
					break;
				case 2:
					arg = "BLOCKED";
					break;
				default:
					arg = "??";
					break;
				}
				text = $"{arg} ({num})";
			}
			else
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			Console.WriteLine("GetPinCodeStatus : [" + text + "]");
			return text;
		}

		public string UnlockPinCode(string strData)
		{
			if (api == null)
			{
				OpenNFC();
			}
			string text = "";
			if (_TagReady)
			{
				byte[] data = Hex2Byte(strData);
				text = ((!api.UnlockPinCode(data)) ? NFCError.NFC_MSG_TAG_COMMAND_ERROR : NFCError.NFC_MSG_SUCCESS);
			}
			else
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			Console.WriteLine("UnlockPinCode : [" + text + "]");
			return text;
		}

		public string DrawImage(Bitmap oImage)
		{
			if (api == null)
			{
				OpenNFC();
			}
			Bitmap bitmap = resizeImage(oImage, new Size(296, 128));
			string text = "";
			if (_TagReady)
			{
				EinkImage image = newEinkImage(296, 128, 1, bitmap);
				api.DrawImage(image, DrawImageMethod.DIMethod_Normal, this);
			}
			else
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			Console.WriteLine("DrawImage : [" + text + "]");
			return text;
		}

		public string DrawImage(Bitmap oImage, bool bDithering)
		{
			if (api == null)
			{
				OpenNFC();
			}
			Bitmap bitmap = resizeImage(oImage, new Size(296, 128));
			string text = "";
			if (_TagReady)
			{
				if (bDithering)
				{
					Console.WriteLine("Dithering");
					Bitmap bitmap2 = Dithering(bitmap);
					EinkImage image = newEinkImage(296, 128, 1, bitmap2);
					api.DrawImage(image, DrawImageMethod.DIMethod_Normal, this);
				}
				else
				{
					Console.WriteLine("No Dithering");
					EinkImage image2 = newEinkImage(296, 128, 1, bitmap);
					api.DrawImage(image2, DrawImageMethod.DIMethod_Normal, this);
				}
			}
			else
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			Console.WriteLine("DrawImage : [" + text + "]");
			return text;
		}

		public async Task<string> DrawImageAsync(Bitmap oImage)
		{
			if (api == null)
			{
				OpenNFC();
			}
			Bitmap bitmap = resizeImage(oImage, new Size(296, 128));
			string text = "";
			if (_TagReady)
			{
				EinkImage image = newEinkImage(296, 128, 1, bitmap);
				api.DrawImage(image, DrawImageMethod.DIMethod_Normal, this);
			}
			else
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			if (text.Equals(""))
			{
				text = await checkDrawImageState();
			}
			Console.WriteLine("DrawImageAsync strResult : [" + text + "]");
			return text;
		}

		public async Task<string> DrawImageAsync(Bitmap oImage, bool bDithering)
		{
			if (api == null)
			{
				OpenNFC();
			}
			Bitmap bitmap = resizeImage(oImage, new Size(296, 128));
			string text = "";
			if (_TagReady)
			{
				if (bDithering)
				{
					Console.WriteLine("Dithering");
					Bitmap bitmap2 = Dithering(bitmap);
					EinkImage image = newEinkImage(296, 128, 1, bitmap2);
					api.DrawImage(image, DrawImageMethod.DIMethod_Normal, this);
				}
				else
				{
					Console.WriteLine("No Dithering");
					EinkImage image2 = newEinkImage(296, 128, 1, bitmap);
					api.DrawImage(image2, DrawImageMethod.DIMethod_Normal, this);
				}
			}
			else
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			if (text.Equals(""))
			{
				text = await checkDrawImageState();
			}
			Console.WriteLine("DrawImageAsync strResult : [" + text + "]");
			return text;
		}

		private static async Task<string> checkDrawImageState()
		{
			await Task.Run(delegate
			{
				Thread.Sleep(1000);
			});
			if (mImageState == nImageState.DIState_Finish)
			{
				return NFCError.NFC_MSG_SUCCESS;
			}
			if (mImageState == nImageState.DIState_Error)
			{
				return NFCError.NFC_MSG_TAG_COMMAND_ERROR;
			}
			return await checkDrawImageState();
		}

		public void onTagStateChange(NFCTagState state)
		{
			try
			{
				_TagReady = (state == NFCTagState.NFC_TAG_STATE_COMM_ON);
				chkTagConnected = _TagReady;
				_tagState.onTagState((nTagState)state);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		public void onProgress(DrawImageState state, object data)
		{
			try
			{
				mImageState = (nImageState)state;
				_processState.onProcessState((nImageState)state, data);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private EinkImage newEinkImage(int width, int height, int pages, Bitmap bitmap)
		{
			if (GetPlatformName().StartsWith("EPD-210") && isFWSupport("3.0.0"))
			{
				return new EinkImage(296, 128, 1, bitmap, 1, 1024);
			}
			return new EinkImage(296, 128, 1, bitmap);
		}

		public string GetTagData()
		{
			return GetTagDataDo();
		}

		private string GetTagDataDo()
		{
			if (!_bNFCData)
			{
				OpenNFCData();
			}
			string text = "";
			if (_bNFCData)
			{
				string text2 = "";
				string Data = "";
				int num = 0;
				num = NFC_API.RFID_WorkingType(0, 1);
				for (int i = 0; i < 80; i++)
				{
					string text3 = i.ToString().PadLeft(2, '0') + "00";
					int num2 = 0;
					Console.WriteLine("strBlock : [" + text3 + "]");
					while (num2 < nTryCount)
					{
						num = NFC_API.RFID_ST25DVRead("00", "", text3, out Data);
						Console.WriteLine("Data : [" + Data + "] nErr=" + num);
						num2++;
						if (num == 0)
						{
							break;
						}
						Thread.Sleep(100);
					}
					if (num2 >= nTryCount)
					{
						break;
					}
					if (Data.Contains("00"))
					{
						text2 += Data.Substring(0, Data.IndexOf("00"));
						break;
					}
					text2 += Data;
				}
				text = UnHex(text2, "utf-8");
				text = text.Replace("\0", string.Empty);
				if (text == "")
				{
					text = NFCError.NFC_MSG_TAG_COMMAND_ERROR;
				}
			}
			else
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			Console.WriteLine("GetTagData : [" + text + "]");
			return text;
		}

		public string WriteTagData(string strData)
		{
			return WriteTagDataDo(strData);
		}

		private string WriteTagDataDo(string strData)
		{
			if (!_bNFCData)
			{
				OpenNFCData();
			}
			string text = NFCError.NFC_MSG_SUCCESS;
			int num = 0;
			if (_bNFCData)
			{
				Console.WriteLine("WriteTagDataDo strData : [" + strData + "]");
				string text2 = ToHex(strData, "utf-8", false) + "00";
				int num2 = text2.Length / 8 + 1;
				int num3 = 0;
				num3 = NFC_API.RFID_WorkingType(0, 1);
				for (int i = 0; i < num2; i++)
				{
					string str = i.ToString().PadLeft(2, '0');
					str += "00";
					int num4 = 8;
					if (text2.Length - i * 8 < num4)
					{
						num4 = text2.Length - i * 8;
					}
					string text3 = text2.Substring(i * 8, num4);
					text3 = text3.PadRight(8, '0');
					int num5 = 0;
					while (num5 < nTryCount)
					{
						Console.WriteLine("strBlock : [" + str + "] strTemp : [" + text3 + "] nErr" + num3);
						num3 = NFC_API.RFID_ST25DVWrite("00", "", str, text3);
						num5++;
						if (num3 == 0)
						{
							break;
						}
						num++;
						Thread.Sleep(100);
					}
					if (num5 >= nTryCount)
					{
						text = NFCError.NFC_MSG_TAG_COMMAND_ERROR;
						break;
					}
				}
				Console.WriteLine("Err  retry " + num);
			}
			else
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			Console.WriteLine("WriteTagData : [" + text + "]");
			return text;
		}

		public string GetTagDataFlash()
		{
			if (!isFWSupport("3.0.0"))
			{
				Constants.FW_SUPPORT = "3.0.0";
				return NFCError.NFC_MSG_TAG_FW_NO_SUPPORT;
			}
			return GetTagDataFlashDo();
		}

		private string GetTagDataFlashDo()
		{
			string text = "";
			if (_TagReady)
			{
				bool flag = false;
				int num = 0;
				DateTime now = DateTime.Now;
				int num2 = now.Second * 1000;
				now = DateTime.Now;
				int num3 = num2 + now.Millisecond;
				while (!flag)
				{
					byte[] array = api.ReadUserData(num);
					if (array == null)
					{
						return NFCError.NFC_MSG_TAG_COMMAND_ERROR;
					}
					if (array[0] == 9)
					{
						return NFCError.NFC_MSG_TAG_NOREADY;
					}
					int num4 = array[1];
					int num5 = 2;
					if (array[num5 + num4 - 2] == 13 && array[num5 + num4 - 1] == 10)
					{
						flag = true;
						num4 -= 2;
					}
					else
					{
						num += num4;
					}
					byte[] array2 = new byte[num4];
					Array.Copy(array, 2, array2, 0, num4);
					text += Encoding.UTF8.GetString(array2);
					Thread.Sleep(100);
				}
				now = DateTime.Now;
				int num6 = now.Second * 1000;
				now = DateTime.Now;
				int num7 = num6 + now.Millisecond;
				Console.WriteLine("strResult: " + text + " : (" + (num7 - num3) + ") ");
			}
			Thread.Sleep(200);
			return text;
		}

		public string WriteTagDataFlash(string strData)
		{
			if (!isFWSupport("3.0.0"))
			{
				Constants.FW_SUPPORT = "3.0.0";
				return NFCError.NFC_MSG_TAG_FW_NO_SUPPORT;
			}
			return WriteTagDataFlashDo(strData);
		}

		private string WriteTagDataFlashDo(string strData)
		{
			string text = "";
			if (_TagReady)
			{
				ToHex(strData, "utf-8", false);
				byte[] bytes = Encoding.ASCII.GetBytes(strData);
				if (bytes.Length > 250)
				{
					return NFCError.NFC_MSG_DATA_LENGTH_TOOLARGE;
				}
				byte[] array = new byte[bytes.Length + 2];
				Array.Copy(bytes, 0, array, 0, bytes.Length);
				array[bytes.Length] = 13;
				array[bytes.Length + 1] = 10;
				text = api.WriteUserData(array);
			}
			Thread.Sleep(200);
			if ("Write Image Fail".Equals(text))
			{
				text = NFCError.NFC_MSG_TAG_COMMAND_ERROR;
			}
			else if ("NFC Card Off".Equals(text))
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			else if ("Success".Equals(text))
			{
				text = NFCError.NFC_MSG_SUCCESS;
			}
			return text;
		}

		public string getFWData()
		{
			string FirmwareVer = "";
			if (!_bNFCData)
			{
				OpenNFCData();
			}
			if (_bNFCData)
			{
				NFC_API.RFID_FWVersion(out FirmwareVer);
			}
			Console.WriteLine(FirmwareVer);
			return FirmwareVer;
		}

		private void OpenNFC()
		{
			_bNFC = true;
			if (_bNFCData)
			{
				CloseNFCData();
			}
			if (comPort != "")
			{
				nfc = new D30Command(comPort);
				if (nfc.openNFC())
				{
					manager = NFCManager.getInstance();
					manager.TagChange = this;
					manager.setNFCCommand(nfc);
					api = manager.getNfcAPI();
				}
			}
		}

		private void CloseNFC()
		{
			if (api != null)
			{
				api = null;
				manager.setNFCCommand(null);
				nfc.closeNFC();
				nfc = null;
			}
			_bNFC = false;
			_TagReady = false;
		}

		private void OpenNFCData()
		{
			_bNFCData = true;
			if (_bNFC)
			{
				CloseNFC();
			}
			if (comPort != "")
			{
				NFC_API.RFID_OpenReader(comPort);
			}
		}

		private void CloseNFCData()
		{
			NFC_API.RFID_CloseReader(comPort);
			_bNFCData = false;
		}

		private static byte[] Hex2Byte(string asciiString)
		{
			byte[] array = new byte[asciiString.Length];
			for (int i = 0; i < asciiString.Length; i++)
			{
				array[i] = (byte)asciiString[i];
			}
			return array;
		}

		public static string ToHex(string s, string charset, bool fenge)
		{
			if (s.Length % 2 != 0)
			{
				s += " ";
			}
			byte[] bytes = Encoding.GetEncoding(charset).GetBytes(s);
			string text = "";
			for (int i = 0; i < bytes.Length; i++)
			{
				text += $"{bytes[i]:X}";
				if (fenge && i != bytes.Length - 1)
				{
					text += string.Format("{0}", ",");
				}
			}
			return text.ToLower();
		}

		public static string UnHex(string hex, string charset)
		{
			if (hex == null)
			{
				throw new ArgumentNullException("hex");
			}
			hex = hex.Replace(",", "");
			hex = hex.Replace("\n", "");
			hex = hex.Replace("\\", "");
			hex = hex.Replace(" ", "");
			if (hex.Length % 2 != 0)
			{
				hex += "0";
			}
			byte[] array = new byte[hex.Length / 2];
			for (int i = 0; i < array.Length; i++)
			{
				try
				{
					array[i] = byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber);
				}
				catch
				{
					throw new ArgumentException("hex is not a valid hex number!", "hex");
				}
			}
			return Encoding.GetEncoding(charset).GetString(array);
		}

		private Bitmap Dithering(Bitmap oImage)
		{
			Bitmap bitmap = myCopy(oImage);
			Size size = bitmap.Size;
			myColor[] pixelsFrom32BitArgbImage = GetPixelsFrom32BitArgbImage(bitmap);
			for (int i = 0; i < size.Height; i++)
			{
				for (int j = 0; j < size.Width; j++)
				{
					int num = i * size.Width + j;
					myColor myColor = pixelsFrom32BitArgbImage[num];
					FloydSteinbergDithering(pixelsFrom32BitArgbImage, myColor, pixelsFrom32BitArgbImage[num] = TransformPixel(myColor), j, i, size.Width, size.Height);
				}
			}
			Bitmap result = ToBitmap(pixelsFrom32BitArgbImage, size);
			Console.WriteLine("Size Width: " + size.Width + ";Height:" + size.Height);
			return result;
		}

		public static Bitmap resizeImage(Bitmap imgToResize, Size size)
		{
			return new Bitmap(imgToResize, size);
		}

		public static Bitmap myCopy(Image image)
		{
			Size size = image.Size;
			int width = size.Width;
			size = image.Size;
			Bitmap bitmap = new Bitmap(width, size.Height, PixelFormat.Format32bppArgb);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.Clear(Color.Transparent);
				graphics.PageUnit = GraphicsUnit.Pixel;
				graphics.DrawImage(image, new Rectangle(Point.Empty, image.Size));
				return bitmap;
			}
		}

		private unsafe static myColor[] GetPixelsFrom32BitArgbImage(Bitmap bitmap)
		{
			int width = bitmap.Width;
			int height = bitmap.Height;
			myColor[] array = new myColor[width * height];
			BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			myColor* ptr = (myColor*)(void*)bitmapData.Scan0;
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					array[i * width + j] = *ptr;
					ptr++;
				}
			}
			bitmap.UnlockBits(bitmapData);
			return array;
		}

		private static myColor TransformPixel(myColor pixel)
		{
			byte num = (byte)(0.299 * (double)(int)pixel.R + 0.587 * (double)(int)pixel.G + 0.114 * (double)(int)pixel.B);
			myColor result = default(myColor);
			if (num < 127)
			{
				result.A = byte.MaxValue;
				result.R = 0;
				result.G = 0;
				result.B = 0;
			}
			else
			{
				result.A = byte.MaxValue;
				result.R = byte.MaxValue;
				result.G = byte.MaxValue;
				result.B = byte.MaxValue;
			}
			return result;
		}

		private unsafe static Bitmap ToBitmap(myColor[] data, Size size)
		{
			Bitmap bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
			int width = bitmap.Width;
			int height = bitmap.Height;
			BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			myColor* ptr = (myColor*)(void*)bitmapData.Scan0;
			for (int i = 0; i < size.Height; i++)
			{
				for (int j = 0; j < size.Width; j++)
				{
					int num = i * size.Width + j;
					myColor myColor = *ptr = data[num];
					ptr++;
				}
			}
			bitmap.UnlockBits(bitmapData);
			return bitmap;
		}

		private static byte myToByte(int value)
		{
			if (value < 0)
			{
				value = 0;
			}
			else if (value > 255)
			{
				value = 255;
			}
			return (byte)value;
		}

		private static void FloydSteinbergDithering(myColor[] original, myColor originalPixel, myColor transformedPixel, int x, int y, int width, int height)
		{
			int num = y * width + x;
			int num2 = originalPixel.R - transformedPixel.R;
			int num3 = originalPixel.G - transformedPixel.G;
			int num4 = originalPixel.B - transformedPixel.B;
			myColor myColor;
			if (x + 1 < width)
			{
				int num5 = num + 1;
				myColor = original[num5];
				myColor.R = myToByte(myColor.R + (num2 * 7 >> 4));
				myColor.G = myToByte(myColor.G + (num4 * 7 >> 4));
				myColor.B = myToByte(myColor.B + (num3 * 7 >> 4));
				original[num5] = myColor;
			}
			if (y + 1 < height)
			{
				int num5;
				if (x - 1 > 0)
				{
					num5 = num + width - 1;
					myColor = original[num5];
					myColor.R = myToByte(myColor.R + (num2 * 3 >> 4));
					myColor.G = myToByte(myColor.G + (num4 * 3 >> 4));
					myColor.B = myToByte(myColor.B + (num3 * 3 >> 4));
					original[num5] = myColor;
				}
				num5 = num + width;
				myColor = original[num5];
				myColor.R = myToByte(myColor.R + (num2 * 5 >> 4));
				myColor.G = myToByte(myColor.G + (num4 * 5 >> 4));
				myColor.B = myToByte(myColor.B + (num3 * 5 >> 4));
				original[num5] = myColor;
				if (x + 1 < width)
				{
					num5 = num + width + 1;
					myColor = original[num5];
					myColor.R = myToByte(myColor.R + (num2 >> 4));
					myColor.G = myToByte(myColor.G + (num4 >> 4));
					myColor.B = myToByte(myColor.B + (num3 >> 4));
					original[num5] = myColor;
				}
			}
		}

		private bool isFWSupport(string v)
		{
			if (VersionComparer.CompareVersions(GetVersion(), v) >= 0)
			{
				return true;
			}
			return false;
		}
	}
}
