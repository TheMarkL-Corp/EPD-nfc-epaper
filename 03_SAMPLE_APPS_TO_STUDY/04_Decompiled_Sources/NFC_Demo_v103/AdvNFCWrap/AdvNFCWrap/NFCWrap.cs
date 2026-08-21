using com.advantech.nfc;
using com.advantech.nfc.cmd;
using J_RFID;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AdvNFCWrap
{
	/// <summary>
	/// Encapsulated NFC dll library, read / write tag data and refresh images
	/// </summary>
	public class NFCWrap : NFCTagChangeListener, IDrawImageCallback, SendDataCallback
	{
		public enum nTagState
		{
			/// <summary>
			/// Tag is offline
			/// </summary>
			NFC_TAG_STATE_TAG_OFF,
			/// <summary>
			/// Tag is connect on reader 
			/// </summary>
			NFC_TAG_STATE_TAG_ON,
			/// <summary>
			/// Tag is ready
			/// </summary>
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

		public enum nOTAState
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

		public interface ProcessState
		{
			void onProcessState(nImageState state, object data);
		}

		public interface OTAProcessState
		{
			void onOTAProcessState(nOTAState state, object data);
		}

		public struct myColor
		{
			public byte A;

			public byte R;

			public byte G;

			public byte B;

			public myColor(byte a, byte r, byte g, byte b)
			{
				A = a;
				R = r;
				G = g;
				B = b;
			}
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

		private static OTAProcessState _OTAProcessState = null;

		private string comPort = "";

		private static RFIDAPI NFC_API = new RFIDAPI();

		private bool _TagReady = false;

		private static bool chkTagConnected = false;

		private bool _bNFC = false;

		private bool _bNFCData = false;

		private int nTryCount = 100;

		private static nImageState mImageState;

		private static nOTAState mSendDataState;

		private string model;

		private Logfile log;

		/// <summary>
		/// Callback function when the tag status changes
		/// </summary>
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

		/// <summary>
		/// Callback function while refresh image
		/// </summary>
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

		public OTAProcessState OTAProcessStateListener
		{
			get
			{
				return _OTAProcessState;
			}
			set
			{
				_OTAProcessState = value;
			}
		}

		/// <summary>
		/// Empty constructor, from which to call functions
		/// </summary>
		public NFCWrap()
		{
			log = new Logfile();
			comPort = "";
			CloseNFC();
			CloseNFCData();
		}

		/// <summary>
		/// Connect the NFC reader from the given port
		/// </summary>
		/// <param name="strPort">the port name of reader connected</param>
		public NFCWrap(string strPort)
		{
			log = new Logfile();
			comPort = strPort;
			CloseNFC();
			CloseNFCData();
		}

		/// <summary>
		/// Detect the port connected to the NFC reader
		/// </summary>
		/// <returns>Port string/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetPort();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string GetPort()
		{
			CloseNFC();
			CloseNFCData();
			string text = NFCError.NFC_MSG_PORT_EMPTY;
			string[] portNames = SerialPort.GetPortNames();
			int num = 0;
			for (int i = 0; i < portNames.Length; i++)
			{
				string text2 = portNames[i];
				num = NFC_API.RFID_OpenReader(text2);
				Console.WriteLine("RFID_OpenReader : " + num + " Port [" + text2 + "]");
				string FirmwareVer = "";
				num = NFC_API.RFID_FWVersion(out FirmwareVer);
				Console.WriteLine("RFID_FWVersion : " + num);
				if (num == 0)
				{
					NFC_API.RFID_CloseReader(comPort);
					text = portNames[i];
					break;
				}
				NFC_API.RFID_CloseReader(comPort);
			}
			Console.WriteLine("GetPort strResult : " + text);
			return text;
		}

		/// <summary>
		/// Connect tag
		/// </summary>
		/// <returns>
		/// <see cref="T:AdvNFCWrap.NFCError" />
		/// </returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.ConnectTag();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
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

		/// <summary>
		/// Connect tag and return connect status
		/// </summary>
		/// <returns>
		/// <see cref="T:AdvNFCWrap.NFCError" />
		/// </returns>
		/// <example>
		/// <code>
		/// String strReturn = await oNFC.ConnectTagAsync();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public async Task<string> ConnectTagAsync()
		{
			string strResult;
			if (comPort != "")
			{
				OpenNFC();
				strResult = await chkTagState();
			}
			else
			{
				strResult = NFCError.NFC_MSG_PORT_EMPTY;
			}
			Console.WriteLine("ConnectTagAsync : [" + strResult + "]");
			return strResult;
		}

		private static async Task<string> chkTagState()
		{
			string result = NFCError.NFC_MSG_TAG_NOREADY;
			Task subThreadTask = Task.Run(delegate
			{
				Thread.Sleep(3000);
				result = (chkTagConnected ? NFCError.NFC_MSG_SUCCESS : NFCError.NFC_MSG_TAG_NOREADY);
			});
			await subThreadTask;
			return result;
		}

		/// <summary>
		/// Disconnect tag
		/// </summary>
		/// <returns>
		/// <see cref="T:AdvNFCWrap.NFCError" />
		/// </returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.DisconnectTag();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
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

		/// <summary>
		/// Get tag ID
		/// </summary>
		/// <returns>ID/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetTagID();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
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

		/// <summary>
		/// Get tag version
		/// </summary>
		/// <returns>Version/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetVersion();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
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

		/// <summary>
		/// Get tag platform name
		/// </summary>
		/// <returns>platform name/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetPlatformName();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
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

		/// <summary>
		/// Get tag serial number
		/// </summary>
		/// <returns>Serial number/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetSN();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
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

		/// <summary>
		/// Set a new ping code of tag, rember to ulock first
		/// </summary>
		/// <param name="strData"> new ping code</param>
		/// <returns><see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.SetPingCode("0000");
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
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

		/// <summary>
		/// Get ping code status
		/// </summary>
		/// <returns>Port string/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetPinCodeStatus();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string GetPinCodeStatus()
		{
			if (api == null)
			{
				OpenNFC();
			}
			string text = "";
			if (_TagReady)
			{
				int pinCodeStatus = api.GetPinCodeStatus();
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

		/// <summary>
		/// Unlock tag with ping code
		/// </summary>
		/// <param name="strData">ping code</param>
		/// <returns><see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.UnlockPinCode("0000");
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
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

		/// <summary>
		/// Send a image to tag and refresh
		/// </summary>
		/// <param name="oImage">image object</param>
		/// <returns><see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.DrawImage(oImage);
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string DrawImage(Bitmap oImage)
		{
			if (api == null)
			{
				OpenNFC();
			}
			checkSize(out int width, out int height, out int page);
			Bitmap bitmap = resizeImage(oImage, new Size(width, height));
			string text = "";
			if (_TagReady)
			{
				EinkImage image = newEinkImage(width, height, page, bitmap);
				api.DrawImage(image, DrawImageMethod.DIMethod_Normal, this);
			}
			else
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			Console.WriteLine("DrawImage : [" + text + "]");
			return text;
		}

		/// <summary>
		/// Send a image to tag and refresh
		/// </summary>
		/// <param name="oImage">image object</param>
		/// <param name="bDithering">dithering</param>
		/// <returns><see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.DrawImage(oImage, bDithering);
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string DrawImage(Bitmap oImage, bool bDithering)
		{
			if (api == null)
			{
				OpenNFC();
			}
			checkSize(out int width, out int height, out int page);
			Bitmap bitmap = resizeImage(oImage, new Size(width, height));
			string text = "";
			if (_TagReady)
			{
				if (bDithering)
				{
					Console.WriteLine("Dithering");
					Bitmap bitmap2 = Dithering(bitmap);
					EinkImage image = newEinkImage(width, height, page, bitmap2);
					api.DrawImage(image, DrawImageMethod.DIMethod_Normal, this);
				}
				else
				{
					Console.WriteLine("No Dithering");
					EinkImage image2 = newEinkImage(width, height, page, bitmap);
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

		/// <summary>
		/// Send a image to tag and refresh and return display image status
		/// </summary>
		/// <param name="oImage">image object</param>
		/// <returns>OK/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = await oNFC.DrawImageAsync(oImage);
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public async Task<string> DrawImageAsync(Bitmap oImage)
		{
			if (api == null)
			{
				OpenNFC();
			}
			checkSize(out int iwidth, out int iheight, out int ipage);
			Bitmap dImage = resizeImage(oImage, new Size(iwidth, iheight));
			string strResult = "";
			if (_TagReady)
			{
				EinkImage tImage = newEinkImage(iwidth, iheight, ipage, dImage);
				api.DrawImage(tImage, DrawImageMethod.DIMethod_Normal, this);
			}
			else
			{
				strResult = NFCError.NFC_MSG_TAG_NOREADY;
			}
			if (strResult.Equals(""))
			{
				strResult = await checkDrawImageState();
			}
			Console.WriteLine("DrawImageAsync strResult : [" + strResult + "]");
			return strResult;
		}

		/// <summary>
		/// Send a image to tag and refresh and return display image status
		/// </summary>
		/// <param name="oImage">image object</param>
		/// <param name="bDithering">dithering</param>
		/// <returns>OK/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.SystemRest();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public async Task<string> DrawImageAsync(Bitmap oImage, bool bDithering)
		{
			if (api == null)
			{
				OpenNFC();
			}
			checkSize(out int iwidth, out int iheight, out int ipage);
			Bitmap rImage = resizeImage(oImage, new Size(iwidth, iheight));
			string strResult = "";
			if (_TagReady)
			{
				if (bDithering)
				{
					Console.WriteLine("Dithering");
					EinkImage tImage2 = newEinkImage(bitmap: Dithering(rImage), width: iwidth, height: iheight, pages: ipage);
					api.DrawImage(tImage2, DrawImageMethod.DIMethod_Normal, this);
				}
				else
				{
					Console.WriteLine("No Dithering");
					EinkImage tImage = newEinkImage(iwidth, iheight, ipage, rImage);
					api.DrawImage(tImage, DrawImageMethod.DIMethod_Normal, this);
				}
			}
			else
			{
				strResult = NFCError.NFC_MSG_TAG_NOREADY;
			}
			if (strResult.Equals(""))
			{
				strResult = await checkDrawImageState();
			}
			Console.WriteLine("DrawImageAsync strResult : [" + strResult + "]");
			return strResult;
		}

		/// <summary>
		/// reset Pin code to default (0000)
		/// </summary>
		/// <returns>OK/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = await oNFC.DrawImageAsync(oImage, bDithering);
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string SystemRest()
		{
			byte[] array = api.SystemRest();
			string str = "";
			if (array != null)
			{
				if (array.Length != 7)
				{
					str = "Not support error";
					return NFCError.NFC_MSG_TAG_FW_NO_SUPPORT;
				}
				for (int i = 2; i < 6; i++)
				{
					str += $"{array[i]:X2}";
				}
				return NFCError.NFC_MSG_SUCCESS;
			}
			str = "??";
			Console.WriteLine("Crc : [" + str + "]");
			return NFCError.NFC_MSG_TAG_COMMAND_ERROR;
		}

		/// <summary>
		/// FW Upgrade (0000)
		/// </summary>
		/// <returns>OK/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = await oNFC.DrawImageAsync(oImage, bDithering);
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string FWUpgrade(string FileName, int type)
		{
			FileStream fileStream = null;
			BinaryReader binaryReader = null;
			try
			{
				fileStream = new FileStream(FileName, FileMode.Open);
				Console.WriteLine(fileStream);
				Console.WriteLine(fileStream.Length);
				byte[] array = new byte[fileStream.Length];
				binaryReader = new BinaryReader(fileStream);
				FWinfo fWinfo = new FWinfo();
				for (int i = 0; i < fileStream.Length; i++)
				{
					array[i] = binaryReader.ReadByte();
				}
				int num = 240;
				byte[] array2 = new byte[6];
				for (int j = 0; j < 4; j++)
				{
					array2[j] = array[fileStream.Length - num];
					num--;
				}
				if (array2[0] != 0 && array2[1] != 85 && array2[2] != 83 && array2[3] != 83)
				{
					Console.WriteLine("NFC_MSG_OTA_FORMAT_ILLEGAl");
					return NFCError.NFC_MSG_OTA_FORMAT_ILLEGAl;
				}
				fWinfo.Getdata_info(array, type);
				api.bigdata(fWinfo, DrawImageMethod.DIMethod_Normal, this);
			}
			catch (Exception value)
			{
				Console.WriteLine(value);
			}
			finally
			{
				binaryReader?.Close();
				fileStream?.Close();
			}
			return NFCError.NFC_MSG_SUCCESS;
		}

		private void checkSize(out int width, out int height, out int page)
		{
			if (api == null)
			{
				OpenNFC();
			}
			string platformName = api.GetPlatformName();
			string version = api.GetVersion();
			if (platformName.Equals("EPD-210--TC2") || platformName.Equals("D30-ED29-TC2"))
			{
				model = "EPD-210";
				width = Epd29.iwidthedge;
				height = Epd29.iheightedge;
				page = 1;
			}
			else if (platformName.Equals("EPD-302--TC2"))
			{
				model = "EPD-302";
				width = EPD_BW_37.iwidthedge;
				height = EPD_BW_37.iheightedge;
				page = 1;
			}
			else if (platformName.Equals("EPD-303--TC2"))
			{
				model = "EPD-303";
				width = Epd37.iwidthedge;
				height = Epd37.iheightedge;
				page = 2;
			}
			else if (platformName.Equals("EPD-304--TC2"))
			{
				model = "EPD-304";
				width = EPD_BWYR_37.iwidthedge;
				height = EPD_BWYR_37.iheightedge;
				page = 2;
			}
			else
			{
				width = Epd29.iwidthedge;
				height = Epd29.iheightedge;
				page = 1;
			}
		}

		private static async Task<string> checkDrawImageState()
		{
			Task subThreadTask = Task.Run(delegate
			{
				Thread.Sleep(1000);
			});
			await subThreadTask;
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

		/// <summary>
		/// Callback function while tag state changed
		/// </summary>
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

		/// <summary>
		/// Callback function while refresh image
		/// </summary>
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

		/// <summary>
		/// Callback function while Big Data for OTA
		/// </summary>
		public void onProgress(SendDataState state, object data)
		{
			if (state != SendDataState.SDState_SendData || (int)data == 0)
			{
				log.WriteLog("On OTA onProgress [" + state + "] data [" + data + "]");
			}
			try
			{
				mSendDataState = (nOTAState)state;
				bool flag = false;
				switch (state)
				{
				case SendDataState.SDState_Unlock:
				case SendDataState.SDState_Checksum_APP:
					break;
				case SendDataState.SDState_Erase:
					_OTAProcessState.onOTAProcessState((nOTAState)state, data);
					break;
				case SendDataState.SDState_SendData:
					_OTAProcessState.onOTAProcessState((nOTAState)state, data);
					break;
				case SendDataState.SDState_Finish:
					_OTAProcessState.onOTAProcessState((nOTAState)state, data);
					break;
				case SendDataState.SDState_Error:
				case SendDataState.SDState_Erase_Error:
				case SendDataState.SDState_NOAPP_Error:
				case SendDataState.SDState_UPGRADEAPP_Error:
				case SendDataState.SDState_BLTAPP_Error:
				case SendDataState.SDState_DEV_VOLT_Error:
				case SendDataState.SDState_Checksum_Error:
				case SendDataState.SDState_Compare_Error:
					_OTAProcessState.onOTAProcessState(nOTAState.SDState_Error, 0);
					break;
				}
			}
			catch (Exception ex)
			{
				_OTAProcessState.onOTAProcessState(nOTAState.SDState_Error, 0);
				log.WriteLog(ex.ToString());
				Console.WriteLine(ex.Message);
			}
		}

		private EinkImage newEinkImage(int width, int height, int pages, Bitmap bitmap)
		{
			string platformName = GetPlatformName();
			if (!platformName.StartsWith("EPD-210") || !isFWSupport("3.0.0"))
			{
				if (!platformName.StartsWith("EPD-303"))
				{
					if (!platformName.Equals("EPD-302--TC2"))
					{
						if (!platformName.Equals("EPD-304--TC2"))
						{
							return new EinkImage(width, height, pages, bitmap);
						}
						return new EinkImage(width, height, pages, bitmap, 1, 5120, platformName);
					}
					return new EinkImage(width, height, pages, bitmap, 1, 5120, platformName);
				}
				return new EinkImage(width, height, pages, bitmap, 1, 5120, platformName);
			}
			return new EinkImage(width, height, pages, bitmap, 1, 1024, platformName);
		}

		/// <summary>
		/// Get user data from tag
		/// </summary>
		/// <returns>Tag data/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetTagData();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string GetTagData()
		{
			string text = "";
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

		/// <summary>
		/// Write user data to tag
		/// </summary>
		/// <param name="strData">tag data</param>
		/// <returns>Result/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetTagData(strData);
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string WriteTagData(string strData)
		{
			string text = "";
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

		/// <summary>
		/// Get user data from tag to flash <br />
		/// <b>Only supprot upper FW version <span style="color:red;">3.0.0</span></b> 
		/// </summary>
		/// <returns>Tag data/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetTagData();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string GetTagDataFlash()
		{
			string platformName = GetPlatformName();
			if (!platformName.Contains("EPD-210") || isFWSupport("3.0.0"))
			{
				string text = "";
				return GetTagDataFlashDo();
			}
			Constants.FW_SUPPORT = "3.0.0";
			return NFCError.NFC_MSG_TAG_FW_NO_SUPPORT;
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
					if (num >= 250)
					{
						flag = true;
					}
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
			}
			Thread.Sleep(200);
			return text;
		}

		/// <summary>
		/// Write user data to tag to flash <br />
		/// <b>Only supprot upper FW version <span style="color:red;">3.0.0</span></b> 
		/// </summary>
		/// <param name="strData">tag data</param>
		/// <returns>Result/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetTagData(strData);
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string WriteTagDataFlash(string strData)
		{
			string platformName = GetPlatformName();
			if (!platformName.Contains("EPD-210") || isFWSupport("3.0.0"))
			{
				string text = "";
				return WriteTagDataFlashDo(strData);
			}
			Constants.FW_SUPPORT = "3.0.0";
			return NFCError.NFC_MSG_TAG_FW_NO_SUPPORT;
		}

		private string WriteTagDataFlashDo(string strData)
		{
			string text = "";
			if (_TagReady)
			{
				int num = 0;
				string text2 = ToHex(strData, "utf-8", false);
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
			int num = 0;
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

		public void CloseAntenna()
		{
			manager.setNFCCommand(null);
			nfc.closeNFC();
			nfc = null;
		}

		public void OpenAntenna()
		{
			Console.WriteLine("comPort : " + comPort);
			nfc = new D30Command(comPort);
			if (nfc.openNFC())
			{
				manager.setNFCCommand(nfc);
				api = manager.getNfcAPI();
			}
			else
			{
				nfc = null;
			}
		}

		public void WriteLog(string msg)
		{
			log.WriteLog(msg);
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
				int num = 0;
				num = NFC_API.RFID_OpenReader(comPort);
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
			Encoding encoding = Encoding.GetEncoding(charset);
			byte[] bytes = encoding.GetBytes(s);
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
			Encoding encoding = Encoding.GetEncoding(charset);
			return encoding.GetString(array);
		}

		/// <summary>
		/// Get user data from tag
		/// </summary>
		/// <returns>Tag data/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetTagData();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public Bitmap DoDithering(Bitmap oImage, Size size)
		{
			return DoDithering(oImage, size, false);
		}

		public Bitmap DoDithering(Bitmap oImage, Size size, bool reduce)
		{
			if (api == null)
			{
				OpenNFC();
			}
			checkSize(out int _, out int _, out int _);
			ImageGenerator imageGenerator = new ImageGenerator(model, size);
			return imageGenerator.dithering(oImage);
		}

		private Bitmap Dithering(Bitmap oImage)
		{
			if (model == null || (!model.Contains("EPD-302") && !model.Contains("EPD-303") && !model.Contains("EPD-304")))
			{
				Bitmap bitmap = myCopy(oImage);
				bitmap.Save("dithered_not.png");
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
				Bitmap bitmap2 = ToBitmap(pixelsFrom32BitArgbImage, size);
				Console.WriteLine("Size Width: " + size.Width + ";Height:" + size.Height);
				bitmap2.Save("dithered_image.png");
				return bitmap2;
			}
			ImageGenerator imageGenerator = new ImageGenerator(model);
			bool reduce = false;
			return imageGenerator.dithering(oImage, reduce);
		}

		public static Bitmap resizeImage(Bitmap imgToResize, Size size)
		{
			if (imgToResize.Width < imgToResize.Height)
			{
				imgToResize.RotateFlip(RotateFlipType.Rotate270FlipNone);
			}
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
			}
			return bitmap;
		}

		public unsafe static myColor[] GetPixelsFrom32BitArgbImage(Bitmap bitmap)
		{
			if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
			{
				throw new ArgumentException("The bitmap must be in 32bpp ARGB format.");
			}
			int width = bitmap.Width;
			int height = bitmap.Height;
			myColor[] array = new myColor[width * height];
			BitmapData bitmapData = null;
			try
			{
				bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
				byte* ptr = (byte*)(void*)bitmapData.Scan0;
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++)
					{
						int num = (i * width + j) * 4;
						myColor myColor = default(myColor);
						myColor.B = ptr[num];
						myColor.G = ptr[num + 1];
						myColor.R = ptr[num + 2];
						myColor.A = ptr[num + 3];
						myColor myColor2 = array[i * width + j] = myColor;
					}
				}
			}
			catch (Exception innerException)
			{
				throw new ApplicationException("Failed to extract pixel data from bitmap.", innerException);
			}
			finally
			{
				if (bitmapData != null)
				{
					bitmap.UnlockBits(bitmapData);
				}
			}
			return array;
		}

		private static myColor TransformPixel(myColor pixel)
		{
			byte b = (byte)(0.299 * (double)(int)pixel.R + 0.587 * (double)(int)pixel.G + 0.114 * (double)(int)pixel.B);
			return (pixel.R > 200 && pixel.G < 50 && pixel.B < 50) ? new myColor(byte.MaxValue, byte.MaxValue, 0, 0) : ((b < 127) ? new myColor(byte.MaxValue, 0, 0, 0) : new myColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
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
			int num = VersionComparer.CompareVersions(GetVersion(), v);
			if (num < 0)
			{
				return false;
			}
			return true;
		}
	}
}
