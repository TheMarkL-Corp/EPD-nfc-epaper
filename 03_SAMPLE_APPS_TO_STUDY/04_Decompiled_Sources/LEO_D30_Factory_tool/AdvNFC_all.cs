using com.advantech.nfc.api;
using com.advantech.nfc.cmd;
using J_RFID;
using Lz4Net;
using statemap;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyTitle("AdvNFC")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("AdvNFC")]
[assembly: AssemblyCopyright("Copyright ©  2018")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("eb9a3ff4-0683-4e5b-9ebe-a4006a808750")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: TargetFramework(".NETFramework,Version=v4.5.2", FrameworkDisplayName = ".NET Framework 4.5.2")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion("1.0.0.0")]
public class BlockingQueue<T>
{
	private string m_name;

	private readonly int m_maxSize;

	private Queue<T> m_queue;

	private bool m_isRunning;

	private ManualResetEvent m_enqueueWait;

	private ManualResetEvent m_dequeueWait;

	public Action<string> m_actionOutLog;

	public int Count => m_queue.Count;

	public BlockingQueue(int maxSize, string name = "BlockingQueue", bool isRunning = false)
	{
		m_maxSize = maxSize;
		m_name = name;
		m_queue = new Queue<T>(m_maxSize);
		m_isRunning = isRunning;
		m_enqueueWait = new ManualResetEvent(false);
		m_dequeueWait = new ManualResetEvent(false);
	}

	private void OutLog(string message)
	{
	}

	public void Open()
	{
		m_isRunning = true;
	}

	public void Close()
	{
		m_isRunning = false;
		m_dequeueWait.Set();
	}

	public void Enqueue(T item)
	{
		if (m_isRunning)
		{
			while (true)
			{
				lock (m_queue)
				{
					if (m_queue.Count < m_maxSize)
					{
						m_queue.Enqueue(item);
						m_enqueueWait.Reset();
						m_dequeueWait.Set();
						OutLog(m_name + " 入队成功.");
						return;
					}
				}
				m_enqueueWait.WaitOne();
			}
		}
		OutLog(m_name + " 队列终止，不允许入队");
	}

	public bool Dequeue(ref T item)
	{
		while (m_isRunning)
		{
			lock (m_queue)
			{
				if (m_queue.Count > 0)
				{
					item = m_queue.Dequeue();
					m_dequeueWait.Reset();
					m_enqueueWait.Set();
					OutLog(m_name + " 出队成功.");
					return true;
				}
			}
			m_dequeueWait.WaitOne();
		}
		lock (m_queue)
		{
			return false;
		}
	}

	public void Clear()
	{
		m_queue.Clear();
	}
}
namespace com.advantech.nfc
{
	public class FWinfo
	{
		private int checksum;

		private int startAddress;

		private int EndAddress;

		private int datalength;

		private int unMagic;

		private byte[] data;

		private byte[] version;

		private byte flag;

		public int getChecksum()
		{
			return checksum;
		}

		public int getstartAddress()
		{
			return startAddress;
		}

		public int getEndAddress()
		{
			return EndAddress;
		}

		public byte[] getData()
		{
			return data;
		}

		public int getDatalen()
		{
			return datalength;
		}

		public byte[] getFwver()
		{
			return version;
		}

		public int getMagic()
		{
			return unMagic;
		}

		public byte getOTAtype()
		{
			return flag;
		}

		public void Getdata_info(byte[] fwdata, int index)
		{
			int num = 256;
			flag = (byte)(index + 1);
			version = new byte[4];
			switch (index)
			{
			case 0:
				num = fwdata.Length - 256;
				break;
			case 1:
				num = 20224;
				break;
			}
			startAddress = (fwdata[num + 4] | (fwdata[num + 5] << 8) | (fwdata[num + 6] << 16) | (fwdata[num + 7] << 24));
			EndAddress = (fwdata[num + 8] | (fwdata[num + 9] << 8) | (fwdata[num + 10] << 16) | (fwdata[num + 11] << 24));
			datalength = EndAddress + 1 - startAddress;
			data = new byte[datalength];
			Array.Copy(fwdata, startAddress, data, 0, datalength);
			switch (index)
			{
			case 0:
				num = datalength - 256;
				break;
			case 1:
				num = datalength - 1280;
				break;
			}
			checksum = (data[num] | (data[num + 1] << 8) | (data[num + 2] << 16) | (data[num + 3] << 24));
			startAddress = (data[num + 4] | (data[num + 5] << 8) | (data[num + 6] << 16) | (data[num + 7] << 24));
			EndAddress = (data[num + 8] | (data[num + 9] << 8) | (data[num + 10] << 16) | (data[num + 11] << 24));
			unMagic = (data[num + 16] | (data[num + 17] << 8) | (data[num + 18] << 16) | (data[num + 19] << 24));
			version[0] = data[num + 20];
			version[1] = data[num + 21];
			version[2] = data[num + 22];
			version[3] = data[num + 23];
		}
	}
	public class Logfile
	{
		public void WriteLog(string message)
		{
			string text = AppDomain.CurrentDomain.BaseDirectory + "\\Log\\";
			string path = text + DateTime.Now.ToString("yyyyMMdd") + ".txt";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			if (!File.Exists(path))
			{
				File.Create(path).Close();
			}
			using (StreamWriter w = File.AppendText(path))
			{
				Log(message, w);
			}
		}

		public void Devinfo_Log(string message)
		{
			string text = AppDomain.CurrentDomain.BaseDirectory + "\\Log\\";
			string path = text + "Deviceinfo.txt";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			if (!File.Exists(path))
			{
				File.Create(path).Close();
			}
			else
			{
				try
				{
					File.Delete(path);
				}
				catch (IOException ex)
				{
					Console.WriteLine(ex.Message);
				}
				File.Create(path).Close();
			}
			using (StreamWriter w = File.AppendText(path))
			{
				Log_data(message, w);
			}
		}

		private static void Log(string logMessage, TextWriter w)
		{
			w.Write("\r\nLog Entry : ");
			DateTime now = DateTime.Now;
			string arg = now.ToLongTimeString();
			now = DateTime.Now;
			w.WriteLine("{0} {1}", arg, now.ToLongDateString());
			w.WriteLine("  :");
			w.WriteLine("  :{0}", logMessage);
			w.WriteLine("-------------------------------");
		}

		private static void Log_data(string logMessage, TextWriter w)
		{
			w.Write("\r\nLog Entry : ");
			DateTime now = DateTime.Now;
			string arg = now.ToLongTimeString();
			now = DateTime.Now;
			w.WriteLine("{0} {1}", arg, now.ToLongDateString());
			w.WriteLine("{0}", logMessage);
			w.WriteLine("-------------------------------");
		}
	}
	public class CRC
	{
		private uint POLYNOMIAL;

		private uint INITIAL_REMAINDER;

		private uint FINAL_XOR_VALUE;

		private bool reflect_data;

		private bool reflect_remainer;

		private const int WIDTH = 32;

		private const int TOPBIT = int.MinValue;

		private uint[] crcTable;

		public CRC(uint polynomial, uint initial_remainder, uint final_xor_value, bool reflect_data, bool reflect_remainer)
		{
			POLYNOMIAL = polynomial;
			INITIAL_REMAINDER = initial_remainder;
			FINAL_XOR_VALUE = final_xor_value;
			this.reflect_data = reflect_data;
			this.reflect_remainer = reflect_remainer;
			crcTable = new uint[256];
			Init();
		}

		private void Init()
		{
			for (int i = 0; i < 256; i++)
			{
				uint num = (uint)(i << 24);
				for (byte b = 8; b > 0; b = (byte)(b - 1))
				{
					num = (((num & -2147483648) == 0L) ? (num << 1) : ((num << 1) ^ POLYNOMIAL));
				}
				crcTable[i] = num;
			}
		}

		private uint reflect(uint data, byte nBits)
		{
			uint num = 0u;
			for (byte b = 0; b < nBits; b = (byte)(b + 1))
			{
				if ((data & 1) != 0)
				{
					num = (uint)((int)num | (1 << nBits - 1 - b));
				}
				data >>= 1;
			}
			return num;
		}

		private byte REFLECT_DATA(byte x)
		{
			if (reflect_data)
			{
				return (byte)reflect(x, 8);
			}
			return x;
		}

		private uint REFLECT_REMAINDER(uint x)
		{
			if (reflect_remainer)
			{
				return reflect(x, 32);
			}
			return x;
		}

		public uint CRC_Calc(byte[] message)
		{
			int num = message.Length;
			uint num2 = INITIAL_REMAINDER;
			for (int i = 0; i < num; i++)
			{
				byte b = (byte)(REFLECT_DATA(message[i]) ^ (byte)(num2 >> 24));
				num2 = (crcTable[b] ^ (num2 << 8));
			}
			return REFLECT_REMAINDER(num2) ^ FINAL_XOR_VALUE;
		}
	}
	public class Helper
	{
		public static byte[] StringToByteArray(string hex)
		{
			int length = hex.Length;
			if (length != 0 && (length & 1) != 1)
			{
				byte[] array = new byte[length / 2];
				try
				{
					for (int i = 0; i < length; i += 2)
					{
						array[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
					}
					return array;
				}
				catch
				{
					return null;
				}
			}
			return null;
		}
	}
	public enum NFCExceptionType
	{
		NFC_EXCEPTION_TYPE_ERROR,
		NFC_EXCEPTION_TYPE_NO_TAG,
		NFC_EXCEPTION_TYPE_BUSY,
		NFC_EXCEPTION_TYPE_SIZE
	}
	public class NFCException : Exception
	{
		public NFCExceptionType type;

		public NFCException(NFCExceptionType type)
		{
			this.type = type;
			Console.WriteLine("!!!!!type={0}", type);
		}
	}
	public enum NFCTagState
	{
		NFC_TAG_STATE_TAG_OFF,
		NFC_TAG_STATE_TAG_ON,
		NFC_TAG_STATE_COMM_ON
	}
	public interface NFCTagChangeListener
	{
		void onTagStateChange(NFCTagState state);
	}
	public interface INFCCommand
	{
		bool openNFC();

		void closeNFC();

		byte[] transferRF(byte[] data);

		bool isResponseOK(byte[] data);

		bool isValid();

		int getMaxNFCLength();
	}
	public class NFCManager : NFCSTATEChangeCallback
	{
		private static NFCManager _instance = null;

		private static NFCTagChangeListener _tagChange = null;

		private static RFIDAPI _rfid_api = new RFIDAPI();

		public INFCEDPAPI _epd_api;

		private INFCCommand _nfc_command;

		private NFCState _nfc_state;

		private byte[] _tag;

		private bool _commEnable;

		private NFCTagState _lastState;

		public NFCTagChangeListener TagChange
		{
			get
			{
				return _tagChange;
			}
			set
			{
				_tagChange = value;
			}
		}

		private NFCManager()
		{
		}

		private async void startNFCState()
		{
			await Task.Factory.StartNew(delegate
			{
				_nfc_state.run();
			}, TaskCreationOptions.LongRunning);
		}

		private void stopNFCState()
		{
			if (_nfc_state != null)
			{
				_nfc_state.stop();
			}
		}

		public static NFCManager getInstance()
		{
			if (_instance == null)
			{
				_instance = new NFCManager();
			}
			return _instance;
		}

		public void setNFCCommand(INFCCommand command)
		{
			if (_nfc_command != null)
			{
				stopNFCState();
				_epd_api = null;
				_nfc_state.setNFCCommand(null);
				_nfc_command = null;
				_nfc_state = null;
			}
			if (command != null)
			{
				_nfc_state = new NFCState();
				_nfc_state.setStateChangeCallback(this);
				_nfc_command = command;
				_nfc_state.setNFCCommand(command);
				_epd_api = new LeoD30EPDAPI(command, _nfc_state);
				startNFCState();
			}
		}

		public INFCEDPAPI getNfcAPI()
		{
			return _epd_api;
		}

		public byte[] getTagID()
		{
			return _tag;
		}

		internal void setTagID(byte[] tag)
		{
			_tag = tag?.ToArray();
			if (tag == null)
			{
				doChangeTagListener(NFCTagState.NFC_TAG_STATE_TAG_OFF);
			}
			else
			{
				doChangeTagListener(NFCTagState.NFC_TAG_STATE_TAG_ON);
			}
		}

		public void resetNFCState()
		{
		}

		protected void doChangeTagListener(NFCTagState state)
		{
			if (_tagChange != null && _lastState != state)
			{
				_tagChange.onTagStateChange(state);
				_lastState = state;
			}
		}

		internal void setCommEnable(bool v)
		{
			_commEnable = v;
			if (v)
			{
				doChangeTagListener(NFCTagState.NFC_TAG_STATE_COMM_ON);
			}
			else if (_tag != null)
			{
				doChangeTagListener(NFCTagState.NFC_TAG_STATE_TAG_ON);
			}
			else
			{
				doChangeTagListener(NFCTagState.NFC_TAG_STATE_TAG_OFF);
			}
		}

		public bool getCommEanble()
		{
			return _commEnable;
		}

		public void onNFCStateChange(NFCSTATE new_state)
		{
		}
	}
	public enum DrawImageState
	{
		DIState_Erase,
		DIState_SendData,
		DIState_WriteToEPD,
		DIState_Finish,
		DIState_Error
	}
	public enum SendDataState
	{
		SDState_Getinfo,
		SDState_Reboot_FW_App,
		SDState_Erase,
		SDState_Unlock,
		SDState_SendData,
		SDState_Checksum_APP,
		SDState_Finish,
		SDState_Error,
		SDState_Erase_Error,
		SDState_NOAPP_Error,
		SDState_UPGRADEAPP_Error,
		SDState_BLTAPP_Error,
		SDState_DEV_VOLT_Error,
		SDState_Checksum_Error,
		SDState_Compare_Error
	}
	public enum DrawImageMethod
	{
		DIMethod_Normal,
		DIMethod_Direct_To_EPD
	}
	public interface IDrawImageCallback
	{
		void onProgress(DrawImageState state, object data);
	}
	public interface SendDataCallback
	{
		void onProgress(SendDataState state, object data);
	}
	public interface INFCEDPAPI
	{
		string GetVersion();

		string GetPlatformName();

		bool isValid();

		bool isBusy();

		byte[] getTagID();

		void TestAPI();

		bool CheckEPDStatus();

		void TxData(byte[] data);

		byte[] RxData();

		void DrawImage(EinkImage image, DrawImageMethod method, IDrawImageCallback cb);

		void bigdata(FWinfo fwdata, DrawImageMethod method, SendDataCallback cb);

		string GetSN();

		byte GetPinCodeStatus();

		bool UnlockPinCode(byte[] data);

		bool SetPinCode(byte[] data);

		bool ResetPinCode(byte[] data);

		byte[] SystemRest();
	}
	public class Epd37
	{
		public static int iwidthedge = 416;

		public static int iheightedge = 240;

		public static int size = 24960;
	}
	public class EPD_BW_37
	{
		public static int iwidthedge = 416;

		public static int iheightedge = 240;

		public static int size = 12480;
	}
	public class EPD_BWYR_37
	{
		public static int iwidthedge = 416;

		public static int iheightedge = 240;

		public static int size = 24960;
	}
	public class Epd29
	{
		public static int iwidthedge = 296;

		public static int iheightedge = 128;

		public static int size = 4736;
	}
	public enum EinkImageTemplate
	{
		EINK_IMAGE_BLACK,
		EINK_IMAGE_WHITE,
		EINK_IMAGE_RED,
		EINK_IMAGE_YELLOW,
		EINK_IMAGE_VERTICAL_0,
		EINK_IMAGE_VERTICAL_1,
		EINK_IMAGE_HORIZOTAL_0,
		EINK_IMAGE_HORIZOTAL_1,
		EINK_IMAGE_DITHERING,
		EINK_IMAGE_RANDOM
	}
	public class EinkImage
	{
		private int width;

		private int height;

		private int pages;

		private byte[] data;

		private int lz4size;

		private byte[] lz4data;

		private static readonly byte[] font5x5 = new byte[80]
		{
			124,
			76,
			84,
			100,
			124,
			16,
			48,
			16,
			16,
			56,
			120,
			4,
			56,
			64,
			124,
			124,
			4,
			56,
			4,
			124,
			64,
			64,
			80,
			124,
			16,
			124,
			64,
			120,
			4,
			120,
			124,
			64,
			124,
			68,
			124,
			124,
			4,
			8,
			16,
			16,
			124,
			68,
			124,
			68,
			124,
			124,
			68,
			124,
			4,
			124,
			124,
			68,
			68,
			124,
			68,
			124,
			68,
			120,
			68,
			124,
			124,
			64,
			64,
			64,
			124,
			120,
			68,
			68,
			68,
			120,
			124,
			64,
			120,
			64,
			124,
			124,
			64,
			112,
			64,
			64
		};

		public int getWidth()
		{
			return width;
		}

		public int getHeight()
		{
			return height;
		}

		public int getPages()
		{
			return pages;
		}

		public byte[] getData()
		{
			return data;
		}

		public byte[] getlz4Data()
		{
			return lz4data;
		}

		public int getlz4()
		{
			return lz4size;
		}

		public int findNearestColor(Color color)
		{
			int[,] array = new int[4, 3]
			{
				{
					255,
					255,
					255
				},
				{
					0,
					0,
					0
				},
				{
					255,
					0,
					0
				},
				{
					255,
					255,
					0
				}
			};
			int result = 0;
			int r = color.R;
			int b = color.B;
			int g = color.G;
			int num = 195076;
			int num2;
			int num3 = num2 = 255;
			int num4 = 0;
			for (int i = 0; i < 4; i++)
			{
				int num5 = r - array[i, 0];
				num3 = g - array[i, 1];
				num2 = b - array[i, 2];
				num4 = num5 * num5 + num3 * num3 + num2 * num2;
				if (num4 < num)
				{
					num = num4;
					result = i;
				}
			}
			return result;
		}

		public char[,] img2rgb_forEPD37(Bitmap image)
		{
			int num = 255;
			int num2 = 0;
			char[,] array = new char[image.Width, image.Height];
			for (int i = 0; i < image.Width; i++)
			{
				for (int j = 0; j < image.Height; j++)
				{
					Color pixel = image.GetPixel(i, image.Height - j - 1);
					int b = pixel.B;
					int g = pixel.G;
					int r = pixel.R;
					if (r == num && g == num && b == num)
					{
						array[i, j] = 'w';
					}
					else if (r == num2 && g == num2 && b == num2)
					{
						array[i, j] = 'b';
					}
					else if (r == num && g == num2 && b == num2)
					{
						array[i, j] = 'r';
					}
					else if (r > 200 && 50 < r - g && 50 < r - b)
					{
						array[i, j] = 'r';
					}
					else if (r > 150 && 10 > g && 10 > b)
					{
						array[i, j] = 'r';
					}
					else if ((double)r * 0.299 + (double)g * 0.587 + (double)b * 0.144 > 192.0)
					{
						array[i, j] = 'w';
					}
					else
					{
						array[i, j] = 'b';
					}
				}
			}
			image.Dispose();
			return array;
		}

		public byte[] rgb2_forEPD37(char[,] rgbTable, int page)
		{
			byte[] array = (page <= 1) ? new byte[rgbTable.GetLength(1) * rgbTable.GetLength(0) / 8] : new byte[rgbTable.GetLength(1) * rgbTable.GetLength(0) / 4];
			int num = 0;
			for (int i = 0; i < rgbTable.GetLength(0); i++)
			{
				for (int j = 0; j < rgbTable.GetLength(1); j += 8)
				{
					byte b = 0;
					int num2 = 0;
					for (int k = 0; k < 8; k++)
					{
						switch (rgbTable[rgbTable.GetLength(0) - i - 1, j + k])
						{
						case 'w':
							num2 = 1 << 7 - k;
							break;
						case 'r':
							num2 = 1 << 7 - k;
							break;
						case 'b':
							num2 = 0;
							break;
						}
						b = (byte)(b + (byte)num2);
					}
					array[num] = b;
					num++;
				}
			}
			if (page > 1)
			{
				for (int l = 0; l < rgbTable.GetLength(0); l++)
				{
					for (int m = 0; m < rgbTable.GetLength(1); m += 8)
					{
						byte b2 = 0;
						for (int n = 0; n < 8; n++)
						{
							if (rgbTable[rgbTable.GetLength(0) - l - 1, m + n] == 'r')
							{
								int num3 = 1 << 7 - n;
								b2 = (byte)(b2 + (byte)num3);
							}
							else
							{
								int num4 = 0;
								b2 = (byte)(b2 + (byte)num4);
							}
						}
						array[num] = b2;
						num++;
					}
				}
			}
			return array;
		}

		public byte[] img_forEPD_BW(Bitmap image)
		{
			int num = image.Width;
			int num2 = image.Height;
			int num3 = 0;
			byte[] array = new byte[num * num2 / 8];
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j += 8)
				{
					int num4 = 0;
					for (int k = 0; k < 8; k++)
					{
						Color pixel = image.GetPixel(num - i - 1, j + k);
						num4 *= 2;
						if (ConvertPixel(pixel))
						{
							num4 |= 1;
						}
					}
					array[num3] = (byte)(num4 & 0xFF);
					num3++;
				}
			}
			return array;
		}

		public byte[] img_forDKEEPD_BW(Bitmap image)
		{
			int num = image.Width;
			int num2 = image.Height;
			int num3 = 0;
			byte[] array = new byte[num * num2 / 8];
			for (int i = 0; i < num; i++)
			{
				for (int num4 = num2 - 1; num4 > 0; num4 -= 8)
				{
					int num5 = 0;
					for (int j = 0; j < 8; j++)
					{
						Color pixel = image.GetPixel(num - i - 1, num4 - j);
						num5 *= 2;
						if (ConvertPixel(pixel))
						{
							num5 |= 1;
						}
					}
					array[num3] = (byte)(num5 & 0xFF);
					num3++;
				}
			}
			return array;
		}

		public byte[] img_forEPD37_BW(Bitmap image)
		{
			int num = image.Width;
			int num2 = image.Height;
			int num3 = 0;
			byte[] array = new byte[num * num2 / 8];
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j += 8)
				{
					int num4 = 0;
					for (int k = 0; k < 8; k++)
					{
						Color pixel = image.GetPixel(i, j + k);
						num4 *= 2;
						if (ConvertPixel(pixel))
						{
							num4 |= 1;
						}
					}
					array[num3] = (byte)(num4 & 0xFF);
					num3++;
				}
			}
			return array;
		}

		public char[,] img2rgbY_EPD37(Bitmap image)
		{
			int num = 255;
			int num2 = 0;
			char[,] array = new char[image.Width, image.Height];
			for (int i = 0; i < image.Width; i++)
			{
				for (int j = 0; j < image.Height; j++)
				{
					Color pixel = image.GetPixel(i, image.Height - j - 1);
					int b = pixel.B;
					int g = pixel.G;
					int r = pixel.R;
					if (r == num && g == num && b == num)
					{
						array[i, j] = 'w';
					}
					else if (r == num2 && g == num2 && b == num2)
					{
						array[i, j] = 'b';
					}
					else if (r == num && g == num2 && b == num2)
					{
						array[i, j] = 'r';
					}
					else if (r == num && g == num && b == num2)
					{
						array[i, j] = 'y';
					}
					else if (r > 127 && 100 < r - g && 100 < r - b)
					{
						array[i, j] = 'r';
					}
					else if (r > 180 && g >= 158 && b < 169)
					{
						array[i, j] = 'y';
					}
					else if ((double)r * 0.299 + (double)g * 0.587 + (double)b * 0.144 > 184.0)
					{
						array[i, j] = 'w';
					}
					else
					{
						array[i, j] = 'b';
					}
				}
			}
			image.Dispose();
			return array;
		}

		public byte[] rgbY2bin_EPD37(char[,] rgbTable)
		{
			byte[] array = new byte[rgbTable.GetLength(1) * rgbTable.GetLength(0) / 4];
			int num = 0;
			for (int i = 0; i < rgbTable.GetLength(0); i++)
			{
				for (int j = 0; j < rgbTable.GetLength(1); j += 4)
				{
					byte b = 0;
					for (int k = 0; k < 4; k++)
					{
						int num2 = 0;
						switch (rgbTable[rgbTable.GetLength(0) - i - 1, j + k])
						{
						case 'w':
							num2 = 1;
							break;
						case 'r':
							num2 = 3;
							break;
						case 'y':
							num2 = 2;
							break;
						case 'b':
							num2 = 0;
							break;
						}
						b = (byte)(b + (byte)num2);
						if (k < 3)
						{
							b = (byte)(b << 2);
						}
					}
					array[num] = b;
					num++;
				}
			}
			return array;
		}

		public EinkImage(int width, int height, int pages)
		{
			this.width = width;
			this.height = height;
			this.pages = pages;
			int num = width * height / 8;
			data = new byte[num * pages];
			for (int i = 0; i < num; i++)
			{
				data[i] = 0;
			}
		}

		public EinkImage(int width, int height, int pages, EinkImageTemplate type)
		{
			this.width = width;
			this.height = height;
			this.pages = pages;
			Random random = new Random();
			int num = width * height / 8;
			data = new byte[num * pages];
			int num2 = height / 8;
			for (int i = 0; i < num; i++)
			{
				switch (type)
				{
				case EinkImageTemplate.EINK_IMAGE_BLACK:
					data[i] = 0;
					if (pages != 1)
					{
						data[i + num] = 0;
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_WHITE:
					data[i] = byte.MaxValue;
					if (pages != 1)
					{
						data[i + num] = byte.MaxValue;
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_YELLOW:
					data[i] = 170;
					if (pages != 1)
					{
						data[i + num] = 170;
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_RANDOM:
					data[i] = (byte)(random.Next(256) & 0xFF);
					if (pages != 1)
					{
						data[i + num] = data[i];
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_VERTICAL_0:
					data[i] = 60;
					if (pages != 1)
					{
						data[i + num] = data[i];
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_VERTICAL_1:
					data[i] = 204;
					if (pages != 1)
					{
						data[i + num] = data[i];
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_HORIZOTAL_0:
					if (i / num2 % 2 == 0)
					{
						data[i] = byte.MaxValue;
					}
					else
					{
						data[i] = 0;
					}
					if (pages != 1)
					{
						data[i + num] = data[i];
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_HORIZOTAL_1:
					if (i / num2 % 2 == 1)
					{
						data[i] = byte.MaxValue;
					}
					else
					{
						data[i] = 0;
					}
					if (pages != 1)
					{
						data[i + num] = data[i];
					}
					break;
				}
			}
		}

		public EinkImage(int width, int height, int pages, EinkImageTemplate type, int lz4flag, int packsize, string epdname)
		{
			this.width = width;
			this.height = height;
			this.pages = pages;
			Random random = new Random();
			int num = width * height / 8;
			data = new byte[num * pages];
			lz4data = new byte[num * pages];
			int num2 = height / 8;
			for (int i = 0; i < num; i++)
			{
				switch (type)
				{
				case EinkImageTemplate.EINK_IMAGE_BLACK:
					data[i] = 0;
					if (pages != 1)
					{
						data[i + num] = 0;
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_WHITE:
					if (epdname.Equals("EPD-304--TC2"))
					{
						data[i] = 85;
						if (pages != 1)
						{
							data[i + num] = 85;
						}
					}
					else
					{
						data[i] = byte.MaxValue;
						if (pages != 1)
						{
							data[i + num] = 0;
						}
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_RED:
					data[i] = byte.MaxValue;
					if (pages != 1)
					{
						data[i + num] = byte.MaxValue;
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_YELLOW:
					data[i] = 170;
					if (pages != 1)
					{
						data[i + num] = 170;
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_RANDOM:
					data[i] = (byte)(random.Next(256) & 0xFF);
					if (pages != 1)
					{
						data[i + num] = data[i];
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_VERTICAL_0:
					if (epdname.Equals("EPD-304--TC2"))
					{
						data[i] = 75;
						if (pages != 1)
						{
							data[i + num] = 75;
						}
					}
					else
					{
						data[i] = 60;
						if (pages != 1)
						{
							data[i + num] = 12;
						}
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_VERTICAL_1:
					if (epdname.Equals("EPD-304--TC2"))
					{
						data[i] = 180;
						if (pages != 1)
						{
							data[i + num] = 180;
						}
					}
					else
					{
						data[i] = 204;
						if (pages != 1)
						{
							data[i + num] = 192;
						}
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_HORIZOTAL_0:
					if (epdname.Equals("EPD-304--TC2"))
					{
						int num5 = 104;
						int num6 = 0;
						int num7 = 0;
						num2 = height / 4;
						num6 = i / num2 % num5;
						if (num6 >= 0 && num6 <= 52)
						{
							data[i] = 85;
						}
						else
						{
							data[i] = 0;
							num7 = 1;
						}
						if (pages != 1)
						{
							if (num7 == 0)
							{
								data[i + num] = 170;
							}
							else
							{
								data[i + num] = byte.MaxValue;
							}
						}
					}
					else if (epdname.Equals("EPD-303--TC2"))
					{
						int num8 = i / num2 % 52;
						if (num8 >= 0 && num8 < 17)
						{
							data[i] = byte.MaxValue;
							if (pages != 1)
							{
								data[i + num] = 0;
							}
						}
						else if (num8 >= 17 && num8 <= 34)
						{
							data[i] = 0;
							if (pages != 1)
							{
								data[i + num] = data[i];
							}
						}
						else
						{
							data[i] = byte.MaxValue;
							if (pages != 1)
							{
								data[i + num] = data[i];
							}
						}
					}
					else
					{
						int num9 = i / num2 % 10;
						if (num9 >= 0 && num9 <= 4)
						{
							data[i] = byte.MaxValue;
						}
						else
						{
							data[i] = 0;
						}
						if (pages != 1)
						{
							data[i + num] = data[i];
						}
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_HORIZOTAL_1:
					if (epdname.Equals("EPD-304--TC2"))
					{
						int num3 = 12;
						int num4 = 0;
						num2 = height / 4;
						num4 = i / num2 % num3;
						if (num4 >= 0 && num4 < 3)
						{
							data[i] = byte.MaxValue;
						}
						else if (num4 >= 3 && num4 < 6)
						{
							data[i] = 170;
						}
						else if (num4 >= 6 && num4 < 9)
						{
							data[i] = 0;
						}
						else
						{
							data[i] = 85;
						}
						if (pages != 1)
						{
							data[i + num] = data[i];
						}
					}
					else
					{
						if (i / num2 % 2 == 1)
						{
							data[i] = byte.MaxValue;
						}
						else
						{
							data[i] = 0;
						}
						if (pages != 1)
						{
							data[i + num] = data[i];
						}
					}
					break;
				}
			}
			if (lz4flag == 0)
			{
				lz4data = new byte[1];
				lz4data[0] = 0;
				lz4size = 0;
			}
			else
			{
				int page = 1;
				Lz4comp_segment(width, height, page, packsize);
				if (pages > 1)
				{
					Lz4comp_segment(width, height, pages, packsize);
				}
			}
		}

		public static Bitmap ResizeImage(Image image, int width, int height)
		{
			Rectangle destRect = new Rectangle(0, 0, width, height);
			Bitmap bitmap = new Bitmap(width, height);
			bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.CompositingMode = CompositingMode.SourceCopy;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
				using (ImageAttributes imageAttributes = new ImageAttributes())
				{
					imageAttributes.SetWrapMode(WrapMode.TileFlipXY);
					graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
					return bitmap;
				}
			}
		}

		public EinkImage(int width, int height, int pages, Bitmap bitmap_old)
		{
			this.width = width;
			this.height = height;
			this.pages = pages;
			Bitmap bitmap = ResizeImage(bitmap_old, width, height);
			int num = width * height / 8;
			data = new byte[num * pages];
			int num2 = 0;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j += 8)
				{
					int num3 = 0;
					for (int k = 0; k < 8; k++)
					{
						Color pixel = bitmap.GetPixel(width - i - 1, j + k);
						num3 *= 2;
						if (ConvertPixel(pixel))
						{
							num3 |= 1;
						}
					}
					data[num2] = (byte)(num3 & 0xFF);
					if (pages != 1)
					{
						data[num2 + num] = data[num2];
					}
					num2++;
				}
			}
		}

		public EinkImage(int width, int height, int pages, Bitmap bitmap_old, int lz4flag, int lz4packsize, string epdname, string vername)
		{
			this.width = width;
			this.height = height;
			this.pages = pages;
			int num;
			int num2;
			int num3 = num2 = (num = 0);
			string[] array = vername.Split('.');
			if (array.Length == 2)
			{
				num3 = int.Parse(array[0]);
				num2 = int.Parse(array[1]);
			}
			else
			{
				num3 = int.Parse(array[0]);
				num2 = int.Parse(array[1]);
				num = int.Parse(array[2]);
			}
			int num4 = num3 * 100 + num2 * 10 + num;
			if (bitmap_old.Width < bitmap_old.Height)
			{
				bitmap_old.RotateFlip(RotateFlipType.Rotate270FlipNone);
			}
			Bitmap image = ResizeImage(bitmap_old, width, height);
			int num5 = width * height / 8;
			data = new byte[num5 * pages];
			lz4data = new byte[num5 * pages];
			if (num5 == Epd29.size)
			{
				if (num4 < 400)
				{
					data = img_forEPD_BW(image);
				}
				else
				{
					data = img_forDKEEPD_BW(image);
				}
			}
			else if (epdname.Equals("EPD-302--TC2"))
			{
				char[,] rgbTable = img2rgb_forEPD37(image);
				data = rgb2_forEPD37(rgbTable, pages);
			}
			else if (epdname.Equals("EPD-303--TC2"))
			{
				char[,] rgbTable2 = img2rgb_forEPD37(image);
				data = rgb2_forEPD37(rgbTable2, pages);
			}
			else if (epdname.Equals("EPD-304--TC2"))
			{
				char[,] rgbTable3 = img2rgbY_EPD37(image);
				data = rgbY2bin_EPD37(rgbTable3);
			}
			if (lz4flag == 0)
			{
				lz4data = new byte[1];
				lz4data[0] = 0;
				lz4size = 0;
			}
			else
			{
				int page = 1;
				Lz4comp_segment(width, height, page, lz4packsize);
				if (pages > 1)
				{
					Lz4comp_segment(width, height, pages, lz4packsize);
				}
			}
		}

		private unsafe void Lz4comp_segment(int img_width, int img_height, int page, int packsize)
		{
			int size = 0;
			int num = 0;
			int num2 = img_width * img_height / 8;
			int num3 = (num2 / packsize + 1) * 2;
			byte[] array = new byte[Lz4.LZ4_compressBound(packsize)];
			int num4 = num2 / packsize + 1;
			byte[] array2 = new byte[Lz4.LZ4_compressBound(array.Length * num4) + num3];
			bool flag = true;
			int num6;
			int num5;
			int num7 = num6 = (num5 = 0);
			int num8 = (page != 1) ? (img_width * img_height / 8) : 0;
			while (flag)
			{
				fixed (byte* source = &data[num8])
				{
					fixed (byte* destination = &array[0])
					{
						num5 = ((num != 0) ? Lz4.LZ4_compressHC(source, destination, size) : Lz4.LZ4_compressHC(source, destination, packsize));
						if (num6 == 0)
						{
							array2[num7] = (byte)num5;
							array2[num7 + 1] = (byte)(num5 >> 8);
						}
						else
						{
							array2[num7 + 1] = (byte)num5;
							array2[num7 + 2] = (byte)(num5 >> 8);
						}
						if (num6 == 0)
						{
							Array.Copy(array, 0, array2, num7 + 2, num5);
						}
						else
						{
							Array.Copy(array, 0, array2, num7 + 3, num5);
						}
						num6 += num5 + 2;
						num7 = num6;
						if (num2 > packsize)
						{
							num8 += packsize;
							num2 -= packsize;
							if (num2 <= packsize)
							{
								size = num2;
								num = 1;
							}
						}
						else if (num2 <= packsize)
						{
							num8 += num2;
							num2 -= num2;
							if (num2 == 0)
							{
								array2[num7 + 1] = 13;
								array2[num7 + 2] = 10;
								num6 += 2;
								flag = false;
							}
						}
						Console.WriteLine("total size=" + num6);
						Console.WriteLine("while state=" + flag.ToString());
					}
				}
			}
			if (page == 1)
			{
				int num9 = img_width * img_height / 8;
				if (num6 < num9)
				{
					Array.Copy(array2, 0, lz4data, 0, num6 + 1);
				}
			}
			else if (lz4size + num6 < data.Length)
			{
				Array.Copy(array2, 0, lz4data, lz4size, num6 + 1);
			}
			lz4size += num6 + 1;
		}

		private bool ConvertPixel(Color color)
		{
			int r = color.R;
			int g = color.G;
			int b = color.B;
			int num = (r - 255) * (r - 255) + (g - 255) * (g - 255) + (b - 255) * (b - 255);
			if (r * r + g * g + b * b < num)
			{
				return false;
			}
			return true;
		}

		public void DrawText5x5Digit(int x, int y, string s)
		{
			for (int i = 0; i < s.Length; i++)
			{
				byte b = (byte)(s[i] & 0x7F);
				b = ((b < 65) ? ((byte)(b - 48)) : ((byte)(b - 65 + 10)));
				for (int j = 0; j < 5; j++)
				{
					byte b2 = font5x5[b * 5 + j];
					for (int k = 0; k < 6; k++)
					{
						int color = ((b2 & 0x80) == 0) ? 1 : 0;
						DrawPixel(x + k, y + j, color);
						b2 = (byte)(b2 << 1);
					}
				}
				x += 6;
			}
		}

		public void DrawPixel(int x, int y, int color)
		{
			int num = width * height / 8;
			int num2 = y / 8;
			int num3 = y % 8;
			int num4 = height / 8 * (width - x - 1) + num2;
			byte b = data[num4];
			if (color == 0)
			{
				b = (byte)(b & ~(1 << 7 - num3));
				data[num4] = b;
				if (pages == 2)
				{
					data[num4 + num] = b;
				}
			}
			else
			{
				b = (byte)(b | (1 << 7 - num3));
				data[num4] = b;
				if (pages == 2)
				{
					data[num4 + num] = b;
				}
			}
		}
	}
}
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
	public class NFCCommand_Jogtek : INFCCommand
	{
		private static bool _enable_log = true;

		private static RFIDAPI _rfid_api = new RFIDAPI();

		private string _com_port;

		private bool _is_opened;

		private string _UID;

		protected string _last_result;

		protected string _result;

		private Stopwatch stopwatch = new Stopwatch();

		public NFCCommand_Jogtek(string com_port)
		{
			_com_port = com_port;
		}

		public void ClearStopwatch()
		{
			stopwatch.Reset();
		}

		public long GetElapsed()
		{
			return stopwatch.ElapsedMilliseconds;
		}

		protected void log(string s)
		{
			bool enable_log = _enable_log;
		}

		private void logErr(int err)
		{
			logErr("", err);
		}

		private void logErr(string s, int err)
		{
			s = "Jogtek: " + s + ", Err=";
			switch (err)
			{
			case 0:
				log(s + "OK");
				break;
			case 1:
				log(s + "ERR");
				break;
			case 2:
				log(s + "Open Com Port Fail");
				break;
			case 3:
				log(s + "Com Port Not Open");
				break;
			case 4:
				log(s + "Send Data Fail");
				break;
			case 5:
				log(s + "Recv Data fail");
				break;
			case 6:
				log(s + "Parameter Error");
				break;
			case 7:
				log(s + "Key Error");
				break;
			case 8:
				log(s + "WriteMifareOneBlock not reply");
				break;
			case 9:
				log(s + "Firmware version Return Error");
				break;
			default:
				log("unknown error" + err);
				break;
			}
		}

		public bool openNFC()
		{
			int num;
			try
			{
				num = _rfid_api.RFID_OpenReader(_com_port);
				logErr("RFID_OpenReader", num);
			}
			catch
			{
				num = _rfid_api.RFID_CloseReader(_com_port);
				logErr("RFID_CloseReader", num);
			}
			_is_opened = (num == 0);
			if (_is_opened)
			{
				logErr("SetWorkingType", _rfid_api.RFID_WorkingType(0, 1));
				_rfid_api.RFID_HRate();
			}
			return _is_opened;
		}

		public void closeNFC()
		{
			if (_is_opened)
			{
				_is_opened = false;
				try
				{
					_rfid_api.RFID_AntennaControl(0);
					_rfid_api.RFID_CloseReader(_com_port);
				}
				catch (Exception)
				{
				}
				_com_port = null;
			}
		}

		public int getMaxNFCLength()
		{
			return 119;
		}

		public bool isResponseOK(byte[] response)
		{
			if (response == null)
			{
				return false;
			}
			if ((response[0] & 1) != 0)
			{
				return false;
			}
			return true;
		}

		public bool isValid()
		{
			if (!_is_opened)
			{
				NFCManager.getInstance().setTagID(null);
				return false;
			}
			byte[] data = new byte[3]
			{
				38,
				1,
				0
			};
			byte[] array = transferRF(data);
			byte[] array2;
			if (array != null && array.Length >= 10 && array[0] == 0)
			{
				array2 = new byte[8];
				Array.Copy(array, 2, array2, 0, 8);
			}
			else
			{
				array2 = null;
			}
			NFCManager.getInstance().setTagID(array2);
			return array2 != null;
		}

		public byte[] transferRF(byte[] data)
		{
			if (_is_opened)
			{
				BitConverter.ToString(data).Replace("-", string.Empty);
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();
				this.stopwatch.Start();
				int delay = (data.Length < 10) ? 30 : 70;
				if (data.Length > getMaxNFCLength())
				{
					return null;
				}
				_rfid_api.RFID_SendBytes(data, out _last_result, delay);
				this.stopwatch.Stop();
				stopwatch.Stop();
				_result = Regex.Match(_last_result, "\\[([^\\]]*)\\]").Groups[1].Value;
				return Helper.StringToByteArray(_result);
			}
			return null;
		}
	}
}
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

		public const int CMD_WRITE_USER_DATA = 131;

		public const int CMD_READ_USER_DATA = 132;

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

		private const int CMD_SHORTEST_TIMEOUT = 500;

		private const int CMD_SHORT_TIMEOUT = 2000;

		private const int CMD_LONG_TIMEOUT = 7000;

		private const int CMD_REFRESH_TIMEOUT = 90000;

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
				byte[] array = _nfcState.buildNFCPacket((byte)command, data);
				if (array.Length != 0)
				{
					log($"tx len{array.Length}");
					for (int i = 0; i < array.Length; i++)
					{
						log($"tag tx cmd 0x{array[i]:x2}");
					}
				}
				if (waitTxReady(timeout_ms))
				{
					_nfcState.addEvent(FTMEventType.FTMEVENT_TX_MESSAGE, array);
					while (timeout_ms > 0)
					{
						byte[] rx = _nfcState.getRx();
						if (rx != null)
						{
							if (checkChecksum(rx))
							{
								log($"recev len{rx.Length}");
								if (rx.Length != 0)
								{
									for (int j = 0; j < rx.Length; j++)
									{
										log($"response 0x{rx[j]:x2}");
									}
								}
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
			return CheckResponse(TranceiveCommand(136, null, 2000));
		}

		public bool OTA_CheckEPDVolt()
		{
			return CheckResponse(TranceiveCommand(181, null, 2000));
		}

		public string GetPlatformName()
		{
			_busy = true;
			byte[] array = TranceiveCommand(241, null, 2000);
			if (array != null)
			{
				string text = "";
				for (int i = 1; i < array.Length - 1; i++)
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
			byte[] array = TranceiveCommand(240, null, 2000);
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

		public string OTA_GetBLVersion()
		{
			_busy = true;
			byte[] array = TranceiveCommand(176, null, 2000);
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
			byte[] array = TranceiveCommand(246, null, 2000);
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
			byte[] array = TranceiveCommand(160, null, 2000);
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
			byte[] recv = TranceiveCommand(161, data, 2000);
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
			byte[] recv = TranceiveCommand(163, data, 2000);
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
			byte[] recv = TranceiveCommand(162, data, 2000);
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
			byte[] array = TranceiveCommand(164, data, 2000);
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
			log($"lz4 size={num2}; img_size{num}");
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
				stopwatch.Stop();
				log("eplased=" + $"{stopwatch.ElapsedMilliseconds} ms jogtek={_d30_command.GetElapsed()}");
				cb.onProgress(DrawImageState.DIState_WriteToEPD, 100);
				if (!WriteFlashToEPD((byte)pages, width, height))
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
			if (method == DrawImageMethod.DIMethod_Normal)
			{
				await Task.Factory.StartNew(delegate
				{
					BigdataSend(fwdata, cb);
				}, TaskCreationOptions.LongRunning);
			}
			else
			{
				DrawImageMethod drawImageMethod = method;
			}
		}

		private string OTA_GetFW_MAGIC()
		{
			_busy = true;
			byte[] array = TranceiveCommand(182, null, 2000);
			if (array != null && array.Length == 5)
			{
				_busy = false;
				string text = "";
				for (int i = 1; i < array.Length - 1; i++)
				{
					text += Convert.ToChar(array[i]).ToString();
				}
				return text;
			}
			return "??";
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
					int num5 = 0;
					bool flag = false;
					do
					{
						flag = FW_version_check(fwdata, fwtype);
						num5++;
					}
					while (num5 < 2 && !flag);
					if (flag)
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
			byte[] recv = TranceiveCommand(130, null, 2000);
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
				byte[] array2 = TranceiveCommand(129, array, 7000);
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

		private bool OTA_CheckDataFlash(FWinfo fwdata)
		{
			byte[] array = new byte[24];
			int checksum = fwdata.getChecksum();
			int num = fwdata.getstartAddress();
			int endAddress = fwdata.getEndAddress();
			fwdata.getDatalen();
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
			byte[] recv = TranceiveCommand(180, array, 2000);
			return CheckResponse(recv);
		}

		private bool WriteImageFlashNOACK(int address, byte[] data)
		{
			byte[] array = new byte[2 + data.Length];
			array[0] = (byte)((address >> 8) & 0xFF);
			array[1] = (byte)(address & 0xFF);
			Array.Copy(data, 0, array, 2, data.Length);
			TxCommand(142, array, 7000);
			return true;
		}

		private bool OTA_WriteDataFlashNOACK(int address, byte[] data)
		{
			byte[] array = new byte[2 + data.Length];
			array[0] = (byte)((address >> 8) & 0xFF);
			array[1] = (byte)(address & 0xFF);
			Array.Copy(data, 0, array, 2, data.Length);
			TxCommand(178, array, 7000);
			return true;
		}

		private bool EraseImageFlash(byte lz4flag)
		{
			byte[] data = new byte[1]
			{
				lz4flag
			};
			byte[] recv = TranceiveCommand(128, data, 2000);
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
			}, 2000);
			return CheckResponse(recv);
		}

		private bool Switch_2_APP(byte devflag)
		{
			byte[] data = new byte[1]
			{
				devflag
			};
			byte[] recv = TranceiveCommand(244, data, 500);
			return CheckResponse(recv);
		}

		private void OTA_Switch_APP(byte devflag)
		{
			byte[] data = new byte[1]
			{
				devflag
			};
			byte[] recv = TranceiveCommand(179, data, 500);
			CheckResponse(recv);
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
			if (i >= num)
			{
				return false;
			}
			return true;
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
			if (text2.Equals(text))
			{
				return true;
			}
			return false;
		}
	}
	public class NFCBuffer
	{
		private const int DEFAULT_READ_BUFFER_SIZE = 512;

		private const int DEFAULT_WRITE_BUFFER_SIZE = 512;

		private byte[] readBuffer;

		private byte[] writeBuffer;

		public NFCBuffer()
		{
			readBuffer = null;
			writeBuffer = null;
		}

		public bool putReadBuffer(byte[] data)
		{
			if (readBuffer != null)
			{
				return false;
			}
			readBuffer = data.ToArray();
			return true;
		}

		public int getReadBufferLength()
		{
			if (readBuffer == null)
			{
				return 0;
			}
			return readBuffer.Length;
		}

		public byte[] getDataReceived()
		{
			if (readBuffer == null)
			{
				return null;
			}
			byte[] result = readBuffer.ToArray();
			readBuffer = null;
			return result;
		}

		private void clearReadBuffer()
		{
			readBuffer = null;
		}

		public bool putWriteBuffer(byte[] data)
		{
			if (writeBuffer != null)
			{
				return false;
			}
			writeBuffer = data.ToArray();
			return true;
		}

		public int getWriteBufferLength()
		{
			if (writeBuffer == null)
			{
				return 0;
			}
			return writeBuffer.Length;
		}

		public byte[] getDataTransmitted()
		{
			if (writeBuffer == null)
			{
				return null;
			}
			byte[] result = writeBuffer.ToArray();
			writeBuffer = null;
			return result;
		}

		public void clearWriteBuffer()
		{
			writeBuffer = null;
		}
	}
	public enum NFCSTATE
	{
		NFCSTATE_NONE,
		NFCSTATE_INIT,
		NFCSTATE_TEST,
		NFCSTATE_READY,
		NFCSTATE_BUSY
	}
	public enum FTMEventType
	{
		FTMEVENT_WAIT,
		FTMEVENT_TAG_FOUND,
		FTMEVENT_RX_MESSAGE,
		FTMEVENT_TX_MESSAGE,
		FTMEVENT_EXCEPTION,
		FTMEVENT_RESET
	}
	public class FTMEventRec
	{
		public FTMEventType type;

		public object data;

		public FTMEventRec(FTMEventType type, object data)
		{
			this.type = type;
			this.data = data;
		}
	}
	public interface NFCSTATEChangeCallback
	{
		void onNFCStateChange(NFCSTATE new_state);
	}
	public class NFCState
	{
		private bool _enableLog;

		private int _last_state;

		private bool _interrupted;

		internal NFCBuffer nfcBuffer;

		private NFCStateContext _fsm;

		private D30Command _command;

		private NFCSTATEChangeCallback stateChangeCB;

		private NFCSTATE current_state;

		private BlockingQueue<FTMEventRec> eventQ = new BlockingQueue<FTMEventRec>(20, "Q", true);

		private bool txInQ;

		private FTMEventRec currentEvent;

		private Timer timer;

		private const int maxTickTimer = 3;

		private int[] tickTimers = new int[3];

		private bool[] waitInQ = new bool[3];

		private int waitQIndex = -1;

		private bool _stopped;

		public NFCState()
		{
			StartTickTimer();
			nfcBuffer = new NFCBuffer();
			_command = null;
			ResetContext();
		}

		private void ResetContext()
		{
			_fsm = new NFCStateContext(this);
		}

		private void log(string v)
		{
			bool enableLog = _enableLog;
		}

		internal void setNFCCommand(INFCCommand command)
		{
			_command = (D30Command)command;
			if (_command == null)
			{
				ResetContext();
			}
		}

		internal bool readyToTx()
		{
			if (_command == null)
			{
				return false;
			}
			if (txInQ)
			{
				return false;
			}
			if (nfcBuffer.getWriteBufferLength() > 0)
			{
				return false;
			}
			NFCSTATE nFCSTATE = current_state;
			if ((uint)nFCSTATE <= 1u || nFCSTATE == NFCSTATE.NFCSTATE_BUSY)
			{
				return false;
			}
			return true;
		}

		internal byte[] getRx()
		{
			if (_command == null)
			{
				return null;
			}
			if (nfcBuffer.getReadBufferLength() == 0)
			{
				return null;
			}
			return nfcBuffer.getDataReceived();
		}

		internal byte[] buildNFCPacket(byte command, byte[] data)
		{
			NFCManager.getInstance();
			if (_command == null)
			{
				return null;
			}
			int num = (data != null) ? data.Length : 0;
			byte[] array = new byte[num + 3];
			array[0] = command;
			array[1] = (byte)num;
			int num2 = (array[0] & 0xFF) + (array[1] & 0xFF);
			for (int i = 0; i < num; i++)
			{
				array[i + 2] = data[i];
				num2 += (data[i] & 0xFF);
			}
			array[num + 2] = (byte)((256 - num2) & 0xFF);
			return array;
		}

		public void setStateChangeCallback(NFCSTATEChangeCallback cb)
		{
			stateChangeCB = cb;
		}

		public void removeStateChangeCallback()
		{
			stateChangeCB = null;
		}

		public NFCSTATE getNFCState()
		{
			return current_state;
		}

		public void SetNFCState(NFCSTATE s)
		{
			current_state = s;
			if (stateChangeCB != null)
			{
				stateChangeCB.onNFCStateChange(s);
			}
		}

		public void addEvent(FTMEventRec rec)
		{
			if (rec.type == FTMEventType.FTMEVENT_WAIT)
			{
				if (waitInQ[(int)rec.data])
				{
					return;
				}
				waitInQ[(int)rec.data] = true;
			}
			else if (rec.type == FTMEventType.FTMEVENT_TX_MESSAGE)
			{
				txInQ = true;
			}
			eventQ.Enqueue(rec);
		}

		public void addEvent(FTMEventType type, object data)
		{
			addEvent(new FTMEventRec(type, data));
		}

		public FTMEventRec popEvent()
		{
			FTMEventRec item = null;
			eventQ.Dequeue(ref item);
			if (item.type == FTMEventType.FTMEVENT_WAIT)
			{
				waitInQ[(int)item.data] = false;
			}
			else if (item.type == FTMEventType.FTMEVENT_TX_MESSAGE)
			{
				txInQ = false;
			}
			return item;
		}

		internal void StartTickTimer()
		{
			timer = new Timer(timerCallback, null, 0, 10);
			for (int i = 0; i < 3; i++)
			{
				tickTimers[i] = -1;
				waitInQ[i] = false;
			}
		}

		internal void StopTickTimer()
		{
			timer.Dispose();
			timer = null;
		}

		internal void SetWaitTimer(int index, int ms10)
		{
			if (index < 3)
			{
				tickTimers[index] = ms10;
			}
		}

		internal void ClearWaitTimer(int index)
		{
			if (index < 3)
			{
				tickTimers[index] = -1;
			}
		}

		internal void ClearAllWaitTimers()
		{
			for (int i = 0; i < 3; i++)
			{
				tickTimers[i] = -1;
			}
		}

		private void timerCallback(object state)
		{
			for (int i = 0; i < 3; i++)
			{
				if (tickTimers[i] >= 0)
				{
					tickTimers[i]--;
				}
				if (tickTimers[i] == 0)
				{
					addEvent(FTMEventType.FTMEVENT_WAIT, i);
				}
			}
		}

		internal void S(int state, string s)
		{
			log($"{state:0000}: " + s);
			_last_state = state;
		}

		internal void E(int state, string s)
		{
			log($"{_last_state:0000}->{state:0000}: " + s);
		}

		internal void T(int state, string s)
		{
			log($"{_last_state:0000}->{state:0000}: " + s);
		}

		internal void C(string s)
		{
			log($"{_last_state:0000}: " + s);
		}

		internal void run()
		{
			_interrupted = false;
			_fsm.EnterStartState();
			_stopped = false;
			while (true)
			{
				currentEvent = popEvent();
				if (_interrupted)
				{
					break;
				}
				switch (currentEvent.type)
				{
				case FTMEventType.FTMEVENT_WAIT:
					waitQIndex = (int)currentEvent.data;
					_fsm.Wait();
					break;
				case FTMEventType.FTMEVENT_TX_MESSAGE:
					if (currentEvent.data != null)
					{
						nfcBuffer.putWriteBuffer((byte[])currentEvent.data);
						_fsm.TxMessage();
					}
					break;
				case FTMEventType.FTMEVENT_RX_MESSAGE:
					_fsm.RxMessage();
					break;
				case FTMEventType.FTMEVENT_TAG_FOUND:
					_fsm.TagFound();
					break;
				case FTMEventType.FTMEVENT_EXCEPTION:
					_fsm.Exception();
					break;
				case FTMEventType.FTMEVENT_RESET:
					_fsm.Reset();
					break;
				}
			}
			_stopped = true;
		}

		public void stop()
		{
			_interrupted = true;
			addEvent(FTMEventType.FTMEVENT_RESET, 0);
			while (!_stopped)
			{
				Thread.Sleep(1);
			}
		}

		internal void InitQ()
		{
		}

		internal void ClearTag()
		{
			NFCManager.getInstance().setTagID(null);
		}

		internal void CheckTag()
		{
			if (_command != null && _command.isValid())
			{
				addEvent(FTMEventType.FTMEVENT_TAG_FOUND, null);
			}
		}

		internal void setCommEnable(bool v)
		{
			NFCManager.getInstance().setCommEnable(v);
		}

		internal void CreateCommander()
		{
		}

		internal int getWaitIndex()
		{
			return waitQIndex;
		}

		private void dumpMBCtrl(byte mbCtrlDyn)
		{
			string text = "MBCtrlDyn=";
			if ((mbCtrlDyn & 1) != 0)
			{
				text += " MB_EN";
			}
			if ((mbCtrlDyn & 2) != 0)
			{
				text += " HOST_PUT_MSG";
			}
			if ((mbCtrlDyn & 4) != 0)
			{
				text += " RF_PUT_MSG";
			}
			if ((mbCtrlDyn & 8) != 0)
			{
				text += " RFU";
			}
			if ((mbCtrlDyn & 0x10) != 0)
			{
				text += " HOST_MISS_MSG";
			}
			if ((mbCtrlDyn & 0x20) != 0)
			{
				text += " RF_MISS_MSG";
			}
			if ((mbCtrlDyn & 0x40) != 0)
			{
				text += " HOST_CURRENT_MSG";
			}
			if ((mbCtrlDyn & 0x80) != 0)
			{
				text += " RF_CURRENT_MSG";
			}
			log(text);
		}

		private void dumpEHCtrl(byte ehCtrlDyn)
		{
			string text = "EHCtrlDyn=";
			if ((ehCtrlDyn & 1) != 0)
			{
				text += " EH_EN";
			}
			if ((ehCtrlDyn & 2) != 0)
			{
				text += " EH_ON";
			}
			if ((ehCtrlDyn & 4) != 0)
			{
				text += " FIELD_ON";
			}
			if ((ehCtrlDyn & 8) != 0)
			{
				text += " VCC_ON";
			}
			log(text);
		}

		private void dumpData(string m, byte[] recv)
		{
			string text = "";
			for (int i = 0; i < recv.Length; i++)
			{
				text += $"{recv[i]:X2} ";
			}
			log($"{m}={text}");
		}

		internal void SetupFTM()
		{
			if (_command != null)
			{
				try
				{
					_command.presentPassword();
					byte b = _command.readDynConfig(2);
					dumpEHCtrl(b);
					Thread.Sleep(5);
					if (true)
					{
						if (_command.ReadConfiguration(2) != 1)
						{
							_command.WriteConfiguration(2, 1);
						}
						if ((b & 1) == 0)
						{
							_command.writeDynConfig(2, 1);
						}
					}
					else if ((b & 1) == 0 && _command.ReadConfiguration(2) != 0)
					{
						_command.WriteConfiguration(2, 0);
					}
					byte b2 = _command.readDynConfig(13);
					dumpMBCtrl(b2);
					try
					{
						if ((b2 & 1) == 0)
						{
							for (int num = 10; num > 0; num--)
							{
								try
								{
									_command.writeDynConfig(13, 1);
									Thread.Sleep(5);
									b2 = _command.readDynConfig(13);
									dumpMBCtrl(b2);
								}
								catch (NFCException)
								{
								}
							}
						}
						if ((b2 & 1) == 1)
						{
							log("check FTM enable ok");
						}
						else
						{
							log("check FTM enable not ok");
						}
						byte b3 = _command.readMessageLength();
						if (b3 > 0)
						{
							byte[] recv = _command.readMessage(0, 0);
							log("msg len=" + b3.ToString());
							dumpData("RX ", recv);
						}
					}
					catch (NFCException)
					{
						log("Setup FTM fails");
					}
				}
				catch (NFCException)
				{
					ResetContext();
				}
			}
		}

		internal void CheckMBCtrl()
		{
			try
			{
				if (_command != null)
				{
					byte b = _command.readDynConfig(13);
					if ((b & 1) != 0)
					{
						if ((b & 2) != 0)
						{
							addEvent(FTMEventType.FTMEVENT_RX_MESSAGE, null);
						}
						if ((b & 6) == 0 && nfcBuffer.getWriteBufferLength() > 0)
						{
							byte[] dataTransmitted = nfcBuffer.getDataTransmitted();
							_command.writeMessage(dataTransmitted);
							dumpData("TX", dataTransmitted);
						}
					}
				}
			}
			catch (NFCException)
			{
				addEvent(FTMEventType.FTMEVENT_EXCEPTION, null);
			}
		}

		internal void HandleRxMessage()
		{
			try
			{
				int num = _command.readMessageLength();
				byte[] sourceArray = _command.readMessage(0, num);
				byte[] array = new byte[num + 2];
				Array.Copy(sourceArray, 0, array, 0, num + 2);
				dumpData($"HRX({num})", array);
				nfcBuffer.putReadBuffer(array);
			}
			catch (NFCException)
			{
			}
		}

		internal void HandleTxMessage()
		{
			try
			{
				if (nfcBuffer.getWriteBufferLength() > 0)
				{
					byte[] dataTransmitted = nfcBuffer.getDataTransmitted();
					_command.writeMessage(dataTransmitted);
					dumpData("HTX", dataTransmitted);
				}
			}
			catch (NFCException)
			{
			}
		}

		internal void TestAPI()
		{
			log("TestAPI");
			NFCManager.getInstance().getNfcAPI().TestAPI();
		}
	}
	[GeneratedCode("smc", " v. 6.6.0")]
	public sealed class NFCStateContext : FSMContext
	{
		[GeneratedCode("smc", " v. 6.6.0")]
		public abstract class NFCStateState : State
		{
			internal NFCStateState(string name, int id)
				: base(name, id)
			{
			}

			protected internal virtual void Entry(NFCStateContext context)
			{
			}

			protected internal virtual void Exit(NFCStateContext context)
			{
			}

			protected internal virtual void Exception(NFCStateContext context)
			{
				Default(context);
			}

			protected internal virtual void Reset(NFCStateContext context)
			{
				Default(context);
			}

			protected internal virtual void RxMessage(NFCStateContext context)
			{
				Default(context);
			}

			protected internal virtual void TagFound(NFCStateContext context)
			{
				Default(context);
			}

			protected internal virtual void TxMessage(NFCStateContext context)
			{
				Default(context);
			}

			protected internal virtual void Wait(NFCStateContext context)
			{
				Default(context);
			}

			protected internal virtual void Default(NFCStateContext context)
			{
				throw new TransitionUndefinedException("State: " + context.State.Name + ", Transition: " + context.GetTransition());
			}
		}

		[GeneratedCode("smc", " v. 6.6.0")]
		internal abstract class Map1
		{
			[NonSerialized]
			internal static readonly Map1_Default.Map1__0001 _0001 = new Map1_Default.Map1__0001("Map1._0001", 0);

			[NonSerialized]
			internal static readonly Map1_Default.Map1__0100 _0100 = new Map1_Default.Map1__0100("Map1._0100", 1);

			[NonSerialized]
			internal static readonly Map1_Default.Map1__0500 _0500 = new Map1_Default.Map1__0500("Map1._0500", 2);

			[NonSerialized]
			internal static readonly Map1_Default.Map1__1000 _1000 = new Map1_Default.Map1__1000("Map1._1000", 3);

			[NonSerialized]
			private static readonly Map1_Default Default = new Map1_Default("Map1.Default", -1);
		}

		[GeneratedCode("smc", " v. 6.6.0")]
		internal class Map1_Default : NFCStateState
		{
			[GeneratedCode("smc", " v. 6.6.0")]
			internal class Map1__0001 : Map1_Default
			{
				internal Map1__0001(string name, int id)
					: base(name, id)
				{
				}

				protected internal override void Entry(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					owner.S(1, "Init");
					owner.SetNFCState(NFCSTATE.NFCSTATE_NONE);
					owner.ClearTag();
					owner.InitQ();
					owner.setCommEnable(false);
					owner.SetWaitTimer(0, 50);
				}

				protected internal override void Exit(NFCStateContext context)
				{
					context.Owner.ClearAllWaitTimers();
				}

				protected internal override void Default(NFCStateContext context)
				{
				}

				protected internal override void Exception(NFCStateContext context)
				{
				}

				protected internal override void Reset(NFCStateContext context)
				{
				}

				protected internal override void RxMessage(NFCStateContext context)
				{
				}

				protected internal override void TagFound(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					context.State.Exit(context);
					context.ClearState();
					try
					{
						owner.T(100, "Found A new Tag");
						owner.CreateCommander();
					}
					finally
					{
						context.State = Map1._0100;
						context.State.Entry(context);
					}
				}

				protected internal override void TxMessage(NFCStateContext context)
				{
				}

				protected internal override void Wait(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					NFCStateState state = context.State;
					context.ClearState();
					try
					{
						owner.CheckTag();
						owner.SetWaitTimer(0, 50);
					}
					finally
					{
						context.State = state;
					}
				}
			}

			[GeneratedCode("smc", " v. 6.6.0")]
			internal class Map1__0100 : Map1_Default
			{
				internal Map1__0100(string name, int id)
					: base(name, id)
				{
				}

				protected internal override void Entry(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					owner.S(100, "Setup");
					owner.SetWaitTimer(0, 10);
					owner.SetNFCState(NFCSTATE.NFCSTATE_INIT);
				}

				protected internal override void Default(NFCStateContext context)
				{
					context.State.Exit(context);
					context.State = Map1._0001;
					context.State.Entry(context);
				}

				protected internal override void Reset(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					context.State.Exit(context);
					context.ClearState();
					try
					{
						owner.T(1, "Reset");
					}
					finally
					{
						context.State = Map1._0001;
						context.State.Entry(context);
					}
				}

				protected internal override void Wait(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					if (owner.getWaitIndex() == 0)
					{
						context.State.Exit(context);
						context.ClearState();
						try
						{
							owner.SetupFTM();
						}
						finally
						{
							context.State = Map1._0500;
							context.State.Entry(context);
						}
					}
					else
					{
						base.Wait(context);
					}
				}
			}

			[GeneratedCode("smc", " v. 6.6.0")]
			internal class Map1__0500 : Map1_Default
			{
				internal Map1__0500(string name, int id)
					: base(name, id)
				{
				}

				protected internal override void Entry(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					owner.S(500, "Test Communication");
					owner.SetNFCState(NFCSTATE.NFCSTATE_TEST);
					owner.CheckMBCtrl();
					owner.SetWaitTimer(0, 1);
					owner.SetWaitTimer(1, 5);
					owner.SetWaitTimer(2, 500);
				}

				protected internal override void Exit(NFCStateContext context)
				{
					context.Owner.ClearAllWaitTimers();
				}

				protected internal override void Default(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					context.State.Exit(context);
					context.ClearState();
					try
					{
						owner.C("0500 Wrong Event");
					}
					finally
					{
						context.State = Map1._0001;
						context.State.Entry(context);
					}
				}

				protected internal override void Exception(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					context.State.Exit(context);
					context.ClearState();
					try
					{
						owner.E(1, "NFC Exception, reset");
					}
					finally
					{
						context.State = Map1._0001;
						context.State.Entry(context);
					}
				}

				protected internal override void Reset(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					context.State.Exit(context);
					context.ClearState();
					try
					{
						owner.T(1, "Reset");
					}
					finally
					{
						context.State = Map1._0001;
						context.State.Entry(context);
					}
				}

				protected internal override void RxMessage(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					context.State.Exit(context);
					context.ClearState();
					try
					{
						owner.SetNFCState(NFCSTATE.NFCSTATE_BUSY);
						owner.HandleRxMessage();
						owner.SetNFCState(NFCSTATE.NFCSTATE_READY);
					}
					finally
					{
						context.State = Map1._1000;
						context.State.Entry(context);
					}
				}

				protected internal override void TxMessage(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					NFCStateState state = context.State;
					context.ClearState();
					try
					{
						owner.SetNFCState(NFCSTATE.NFCSTATE_BUSY);
						owner.HandleTxMessage();
						owner.SetNFCState(NFCSTATE.NFCSTATE_READY);
					}
					finally
					{
						context.State = state;
					}
				}

				protected internal override void Wait(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					if (owner.getWaitIndex() == 0)
					{
						NFCStateState state = context.State;
						context.ClearState();
						try
						{
							owner.CheckMBCtrl();
							owner.SetWaitTimer(0, 5);
						}
						finally
						{
							context.State = state;
						}
					}
					else if (owner.getWaitIndex() == 1)
					{
						NFCStateState state2 = context.State;
						context.ClearState();
						try
						{
							owner.TestAPI();
							owner.SetWaitTimer(1, 50);
						}
						finally
						{
							context.State = state2;
						}
					}
					else if (owner.getWaitIndex() == 2)
					{
						context.State.Exit(context);
						context.ClearState();
						try
						{
							owner.E(1, "NFCAPI didnt send test command");
						}
						finally
						{
							context.State = Map1._0001;
							context.State.Entry(context);
						}
					}
					else
					{
						base.Wait(context);
					}
				}
			}

			[GeneratedCode("smc", " v. 6.6.0")]
			internal class Map1__1000 : Map1_Default
			{
				internal Map1__1000(string name, int id)
					: base(name, id)
				{
				}

				protected internal override void Entry(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					owner.S(1000, "Ready");
					owner.SetNFCState(NFCSTATE.NFCSTATE_READY);
					owner.CheckMBCtrl();
					owner.SetWaitTimer(0, 1);
					owner.setCommEnable(true);
				}

				protected internal override void Exit(NFCStateContext context)
				{
					context.Owner.ClearAllWaitTimers();
				}

				protected internal override void Default(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					NFCStateState state = context.State;
					context.ClearState();
					try
					{
						owner.C("1000 Wrong Event");
					}
					finally
					{
						context.State = state;
					}
				}

				protected internal override void Exception(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					context.State.Exit(context);
					context.ClearState();
					try
					{
						owner.E(1, "NFC Exception, reset");
					}
					finally
					{
						context.State = Map1._0001;
						context.State.Entry(context);
					}
				}

				protected internal override void Reset(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					context.State.Exit(context);
					context.ClearState();
					try
					{
						owner.T(1, "Reset");
					}
					finally
					{
						context.State = Map1._0001;
						context.State.Entry(context);
					}
				}

				protected internal override void RxMessage(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					NFCStateState state = context.State;
					context.ClearState();
					try
					{
						owner.SetNFCState(NFCSTATE.NFCSTATE_BUSY);
						owner.HandleRxMessage();
						owner.SetNFCState(NFCSTATE.NFCSTATE_READY);
					}
					finally
					{
						context.State = state;
					}
				}

				protected internal override void TagFound(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					context.State.Exit(context);
					context.ClearState();
					try
					{
						owner.T(100, "Re-Found Tag Again");
						owner.CreateCommander();
					}
					finally
					{
						context.State = Map1._0100;
						context.State.Entry(context);
					}
				}

				protected internal override void TxMessage(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					NFCStateState state = context.State;
					context.ClearState();
					try
					{
						owner.SetNFCState(NFCSTATE.NFCSTATE_BUSY);
						owner.HandleTxMessage();
						owner.SetNFCState(NFCSTATE.NFCSTATE_READY);
					}
					finally
					{
						context.State = state;
					}
				}

				protected internal override void Wait(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					if (owner.getWaitIndex() == 0)
					{
						NFCStateState state = context.State;
						context.ClearState();
						try
						{
							owner.CheckMBCtrl();
							owner.SetWaitTimer(0, 1);
						}
						finally
						{
							context.State = state;
						}
					}
					else
					{
						base.Wait(context);
					}
				}
			}

			internal Map1_Default(string name, int id)
				: base(name, id)
			{
			}
		}

		[NonSerialized]
		private NFCState _owner;

		public NFCStateState State
		{
			get
			{
				if (state_ == null)
				{
					throw new StateUndefinedException();
				}
				return (NFCStateState)state_;
			}
			set
			{
				SetState(value);
			}
		}

		public NFCState Owner
		{
			get
			{
				return _owner;
			}
			set
			{
				_owner = value;
			}
		}

		public NFCStateContext(NFCState owner)
			: base(Map1._0001)
		{
			_owner = owner;
		}

		public override void EnterStartState()
		{
			State.Entry(this);
		}

		public void Exception()
		{
			transition_ = "Exception";
			State.Exception(this);
			transition_ = "";
		}

		public void Reset()
		{
			transition_ = "Reset";
			State.Reset(this);
			transition_ = "";
		}

		public void RxMessage()
		{
			transition_ = "RxMessage";
			State.RxMessage(this);
			transition_ = "";
		}

		public void TagFound()
		{
			transition_ = "TagFound";
			State.TagFound(this);
			transition_ = "";
		}

		public void TxMessage()
		{
			transition_ = "TxMessage";
			State.TxMessage(this);
			transition_ = "";
		}

		public void Wait()
		{
			transition_ = "Wait";
			State.Wait(this);
			transition_ = "";
		}
	}
}
