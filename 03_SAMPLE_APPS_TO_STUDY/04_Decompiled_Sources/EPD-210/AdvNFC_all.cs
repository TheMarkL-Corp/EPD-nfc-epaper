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
	public enum DrawImageMethod
	{
		DIMethod_Normal,
		DIMethod_Direct_To_EPD
	}
	public interface IDrawImageCallback
	{
		void onProgress(DrawImageState state, object data);
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

		string WriteUserData(byte[] data);

		byte[] ReadUserData(int pos);

		void DrawImage(EinkImage image, DrawImageMethod method, IDrawImageCallback cb);

		string GetSN();

		byte GetPinCodeStatus();

		bool UnlockPinCode(byte[] data);

		bool SetPinCode(byte[] data);

		bool ResetPinCode(byte[] data);

		byte[] SystemRest();
	}
	public enum EinkImageTemplate
	{
		EINK_IMAGE_BLACK,
		EINK_IMAGE_WHITE,
		EINK_IMAGE_VERTICAL_0,
		EINK_IMAGE_VERTICAL_1,
		EINK_IMAGE_HORIZOTAL_0,
		EINK_IMAGE_HORIZOTAL_1,
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

		private static readonly byte[] font8x8 = new byte[1024]
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			24,
			60,
			60,
			24,
			24,
			0,
			24,
			0,
			54,
			54,
			0,
			0,
			0,
			0,
			0,
			0,
			54,
			54,
			127,
			54,
			127,
			54,
			54,
			0,
			12,
			62,
			3,
			30,
			48,
			31,
			12,
			0,
			0,
			99,
			51,
			24,
			12,
			102,
			99,
			0,
			28,
			54,
			28,
			110,
			59,
			51,
			110,
			0,
			6,
			6,
			3,
			0,
			0,
			0,
			0,
			0,
			24,
			12,
			6,
			6,
			6,
			12,
			24,
			0,
			6,
			12,
			24,
			24,
			24,
			12,
			6,
			0,
			0,
			102,
			60,
			byte.MaxValue,
			60,
			102,
			0,
			0,
			0,
			12,
			12,
			63,
			12,
			12,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			12,
			12,
			6,
			0,
			0,
			0,
			63,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			12,
			12,
			0,
			96,
			48,
			24,
			12,
			6,
			3,
			1,
			0,
			62,
			99,
			115,
			123,
			111,
			103,
			62,
			0,
			12,
			14,
			12,
			12,
			12,
			12,
			63,
			0,
			30,
			51,
			48,
			28,
			6,
			51,
			63,
			0,
			30,
			51,
			48,
			28,
			48,
			51,
			30,
			0,
			56,
			60,
			54,
			51,
			127,
			48,
			120,
			0,
			63,
			3,
			31,
			48,
			48,
			51,
			30,
			0,
			28,
			6,
			3,
			31,
			51,
			51,
			30,
			0,
			63,
			51,
			48,
			24,
			12,
			12,
			12,
			0,
			30,
			51,
			51,
			30,
			51,
			51,
			30,
			0,
			30,
			51,
			51,
			62,
			48,
			24,
			14,
			0,
			0,
			12,
			12,
			0,
			0,
			12,
			12,
			0,
			0,
			12,
			12,
			0,
			0,
			12,
			12,
			6,
			24,
			12,
			6,
			3,
			6,
			12,
			24,
			0,
			0,
			0,
			63,
			0,
			0,
			63,
			0,
			0,
			6,
			12,
			24,
			48,
			24,
			12,
			6,
			0,
			30,
			51,
			48,
			24,
			12,
			0,
			12,
			0,
			62,
			99,
			123,
			123,
			123,
			3,
			30,
			0,
			12,
			30,
			51,
			51,
			63,
			51,
			51,
			0,
			63,
			102,
			102,
			62,
			102,
			102,
			63,
			0,
			60,
			102,
			3,
			3,
			3,
			102,
			60,
			0,
			31,
			54,
			102,
			102,
			102,
			54,
			31,
			0,
			127,
			70,
			22,
			30,
			22,
			70,
			127,
			0,
			127,
			70,
			22,
			30,
			22,
			6,
			15,
			0,
			60,
			102,
			3,
			3,
			115,
			102,
			124,
			0,
			51,
			51,
			51,
			63,
			51,
			51,
			51,
			0,
			30,
			12,
			12,
			12,
			12,
			12,
			30,
			0,
			120,
			48,
			48,
			48,
			51,
			51,
			30,
			0,
			103,
			102,
			54,
			30,
			54,
			102,
			103,
			0,
			15,
			6,
			6,
			6,
			70,
			102,
			127,
			0,
			99,
			119,
			127,
			127,
			107,
			99,
			99,
			0,
			99,
			103,
			111,
			123,
			115,
			99,
			99,
			0,
			28,
			54,
			99,
			99,
			99,
			54,
			28,
			0,
			63,
			102,
			102,
			62,
			6,
			6,
			15,
			0,
			30,
			51,
			51,
			51,
			59,
			30,
			56,
			0,
			63,
			102,
			102,
			62,
			54,
			102,
			103,
			0,
			30,
			51,
			7,
			14,
			56,
			51,
			30,
			0,
			63,
			45,
			12,
			12,
			12,
			12,
			30,
			0,
			51,
			51,
			51,
			51,
			51,
			51,
			63,
			0,
			51,
			51,
			51,
			51,
			51,
			30,
			12,
			0,
			99,
			99,
			99,
			107,
			127,
			119,
			99,
			0,
			99,
			99,
			54,
			28,
			28,
			54,
			99,
			0,
			51,
			51,
			51,
			30,
			12,
			12,
			30,
			0,
			127,
			99,
			49,
			24,
			76,
			102,
			127,
			0,
			30,
			6,
			6,
			6,
			6,
			6,
			30,
			0,
			3,
			6,
			12,
			24,
			48,
			96,
			64,
			0,
			30,
			24,
			24,
			24,
			24,
			24,
			30,
			0,
			8,
			28,
			54,
			99,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			byte.MaxValue,
			12,
			12,
			24,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			30,
			48,
			62,
			51,
			110,
			0,
			7,
			6,
			6,
			62,
			102,
			102,
			59,
			0,
			0,
			0,
			30,
			51,
			3,
			51,
			30,
			0,
			56,
			48,
			48,
			62,
			51,
			51,
			110,
			0,
			0,
			0,
			30,
			51,
			63,
			3,
			30,
			0,
			28,
			54,
			6,
			15,
			6,
			6,
			15,
			0,
			0,
			0,
			110,
			51,
			51,
			62,
			48,
			31,
			7,
			6,
			54,
			110,
			102,
			102,
			103,
			0,
			12,
			0,
			14,
			12,
			12,
			12,
			30,
			0,
			48,
			0,
			48,
			48,
			48,
			51,
			51,
			30,
			7,
			6,
			102,
			54,
			30,
			54,
			103,
			0,
			14,
			12,
			12,
			12,
			12,
			12,
			30,
			0,
			0,
			0,
			51,
			127,
			127,
			107,
			99,
			0,
			0,
			0,
			31,
			51,
			51,
			51,
			51,
			0,
			0,
			0,
			30,
			51,
			51,
			51,
			30,
			0,
			0,
			0,
			59,
			102,
			102,
			62,
			6,
			15,
			0,
			0,
			110,
			51,
			51,
			62,
			48,
			120,
			0,
			0,
			59,
			110,
			102,
			6,
			15,
			0,
			0,
			0,
			62,
			3,
			30,
			48,
			31,
			0,
			8,
			12,
			62,
			12,
			12,
			44,
			24,
			0,
			0,
			0,
			51,
			51,
			51,
			51,
			110,
			0,
			0,
			0,
			51,
			51,
			51,
			30,
			12,
			0,
			0,
			0,
			99,
			107,
			127,
			127,
			54,
			0,
			0,
			0,
			99,
			54,
			28,
			54,
			99,
			0,
			0,
			0,
			51,
			51,
			51,
			62,
			48,
			31,
			0,
			0,
			63,
			25,
			12,
			38,
			63,
			0,
			56,
			12,
			12,
			7,
			12,
			12,
			56,
			0,
			24,
			24,
			24,
			0,
			24,
			24,
			24,
			0,
			7,
			12,
			12,
			56,
			12,
			12,
			7,
			0,
			110,
			59,
			0,
			0,
			0,
			0,
			0,
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
				case EinkImageTemplate.EINK_IMAGE_RANDOM:
					data[i] = (byte)(random.Next(256) & 0xFF);
					if (pages != 1)
					{
						data[i + num] = data[i];
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_VERTICAL_0:
					data[i] = 85;
					if (pages != 1)
					{
						data[i + num] = data[i];
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_VERTICAL_1:
					data[i] = 170;
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

		public unsafe EinkImage(int width, int height, int pages, EinkImageTemplate type, int lz4flag, int packsize)
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
				case EinkImageTemplate.EINK_IMAGE_RANDOM:
					data[i] = (byte)(random.Next(256) & 0xFF);
					if (pages != 1)
					{
						data[i + num] = data[i];
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_VERTICAL_0:
					data[i] = 85;
					if (pages != 1)
					{
						data[i + num] = data[i];
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_VERTICAL_1:
					data[i] = 170;
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
			if (lz4flag == 0)
			{
				lz4data = new byte[1];
				lz4data[0] = 0;
				lz4size = 0;
			}
			else
			{
				int num3 = 0;
				int num4 = 0;
				int num5 = width * height / 8;
				int num6 = (num5 / packsize + 1) * 2;
				byte[] array = new byte[Lz4.LZ4_compressBound(packsize)];
				int num7 = num5 / packsize + 1;
				int isize = array.Length * num7;
				lz4data = new byte[Lz4.LZ4_compressBound(isize) + num6];
				bool flag = true;
				int num10;
				int num8;
				int num9;
				int num11 = num10 = (num9 = (num8 = 0));
				Console.WriteLine("buffer length=" + array.Length);
				Console.WriteLine("newimage=" + lz4data.Length);
				while (flag)
				{
					fixed (byte* source = &data[num8])
					{
						fixed (byte* destination = &array[0])
						{
							if (num4 == 0)
							{
								num9 = Lz4.LZ4_compressHC(source, destination, packsize);
								Console.WriteLine("finflag=" + num4);
							}
							else
							{
								num9 = Lz4.LZ4_compressHC(source, destination, num3);
								Console.WriteLine("finflag=" + num4);
							}
							if (num10 == 0)
							{
								lz4data[num11] = (byte)num9;
								lz4data[num11 + 1] = (byte)(num9 >> 8);
							}
							else
							{
								lz4data[num11 + 1] = (byte)num9;
								lz4data[num11 + 2] = (byte)(num9 >> 8);
							}
							Console.WriteLine("sz=" + num9);
							Console.WriteLine("start pos=" + num11);
							if (num10 == 0)
							{
								Array.Copy(array, 0, lz4data, num11 + 2, num9);
							}
							else
							{
								Array.Copy(array, 0, lz4data, num11 + 3, num9);
							}
							num10 += num9 + 2;
							num11 = num10;
							Console.WriteLine("end pos=" + num11);
							if (num5 > packsize)
							{
								num8 += packsize;
								num5 -= packsize;
								Console.WriteLine("1_i=" + num8);
								Console.WriteLine("1_datasz=" + num5);
								if (num5 < packsize)
								{
									num3 = num5;
									Console.WriteLine("final_packet=" + num3);
									num4 = 1;
								}
							}
							else if (num5 < packsize)
							{
								num8 += num5;
								num5 -= num5;
								Console.WriteLine("2_i=" + num8);
								Console.WriteLine("2_datasz=" + num5);
								if (num5 == 0)
								{
									lz4data[num11 + 1] = 13;
									lz4data[num11 + 2] = 10;
									num10 += 2;
									flag = false;
								}
							}
							Console.WriteLine("total size=" + num10);
							Console.WriteLine("while state=" + flag.ToString());
						}
					}
				}
				lz4size = num10;
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
			Bitmap bitmap = ResizeImage(bitmap_old, 296, 128);
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

		public unsafe EinkImage(int width, int height, int pages, Bitmap bitmap_old, int lz4flag, int packsize)
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
			if (lz4flag == 0)
			{
				lz4data = new byte[1];
				lz4data[0] = 0;
				lz4size = 0;
			}
			else
			{
				int size = 0;
				int num4 = 0;
				int num5 = 0;
				int num6 = width * height / 8;
				int num7 = (num6 / packsize + 1) * 2;
				byte[] array = new byte[Lz4.LZ4_compressBound(packsize)];
				int num8 = num6 / packsize + 1;
				num5 = array.Length * num8;
				lz4data = new byte[Lz4.LZ4_compressBound(num5) + num7];
				bool flag = true;
				int num11;
				int num9;
				int num10;
				int num12 = num11 = (num10 = (num9 = 0));
				num6 = width * height / 8;
				while (flag)
				{
					fixed (byte* source = &data[num9])
					{
						fixed (byte* destination = &array[0])
						{
							num10 = ((num4 != 0) ? Lz4.LZ4_compressHC(source, destination, size) : Lz4.LZ4_compressHC(source, destination, packsize));
							if (num11 == 0)
							{
								lz4data[num12] = (byte)num10;
								lz4data[num12 + 1] = (byte)(num10 >> 8);
							}
							else
							{
								lz4data[num12 + 1] = (byte)num10;
								lz4data[num12 + 2] = (byte)(num10 >> 8);
							}
							if (num11 == 0)
							{
								Array.Copy(array, 0, lz4data, num12 + 2, num10);
							}
							else
							{
								Array.Copy(array, 0, lz4data, num12 + 3, num10);
							}
							num11 += num10 + 2;
							num12 = num11;
							if (num6 > packsize)
							{
								num9 += packsize;
								num6 -= packsize;
								if (num6 < packsize)
								{
									size = num6;
									num4 = 1;
								}
							}
							else if (num6 < packsize)
							{
								num9 += num6;
								num6 -= num6;
								if (num6 == 0)
								{
									lz4data[num12 + 1] = 13;
									lz4data[num12 + 2] = 10;
									num11 += 2;
									flag = false;
								}
							}
						}
					}
				}
				lz4size = num11;
			}
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

		public void DrawText(int x, int y, string s)
		{
			for (int i = 0; i < s.Length; i++)
			{
				byte b = (byte)(s[i] & 0x7F);
				for (int j = 0; j < 8; j++)
				{
					byte b2 = font8x8[b * 8 + j];
					for (int k = 0; k < 8; k++)
					{
						int color = ((b2 & 1) == 0) ? 1 : 0;
						DrawPixel(x + k, y + j, color);
						b2 = (byte)(b2 >> 1);
					}
				}
				x += 8;
			}
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
		private static bool _enable_log = false;

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
				string text = BitConverter.ToString(data).Replace("-", string.Empty);
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();
				this.stopwatch.Start();
				int delay = (data.Length < 10) ? 30 : 80;
				if (data.Length > getMaxNFCLength())
				{
					return null;
				}
				_rfid_api.RFID_SendBytes(data, out _last_result, delay);
				this.stopwatch.Stop();
				stopwatch.Stop();
				_result = Regex.Match(_last_result, "\\[([^\\]]*)\\]").Groups[1].Value;
				log("RF(=>" + text + " <=" + _result + ")");
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
