using J_RFID;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace com.advantech.nfc.cmd
{
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
