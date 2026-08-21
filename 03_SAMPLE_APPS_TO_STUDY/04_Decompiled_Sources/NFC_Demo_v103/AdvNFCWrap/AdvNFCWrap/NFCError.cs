using System.Collections.Generic;

namespace AdvNFCWrap
{
	/// <summary>
	/// Error message of send command to tag
	/// </summary>
	public class NFCError
	{
		/// <summary>
		/// Success
		/// </summary>
		/// <value>
		///  0000
		/// </value>
		public static string NFC_MSG_SUCCESS = "0000";

		/// <summary>
		/// Port is empty
		/// </summary>
		/// <value>
		///  0101
		/// </value>
		public static string NFC_MSG_PORT_EMPTY = "0101";

		/// <summary>
		/// Tag not ready
		/// </summary>
		/// <value>
		///  0201
		/// </value>
		public static string NFC_MSG_TAG_NOREADY = "0201";

		/// <summary>
		/// Tag command error
		/// </summary>
		/// <value>
		///  0202
		/// </value>
		public static string NFC_MSG_TAG_COMMAND_ERROR = "0202";

		/// <summary>
		/// Tag firmware version not support
		/// </summary>
		/// <value>
		///  0203
		/// </value>
		public static string NFC_MSG_TAG_FW_NO_SUPPORT = "0203";

		/// <summary>
		/// Data length too large
		/// </summary>
		/// <value>
		///  0301
		/// </value>
		public static string NFC_MSG_DATA_LENGTH_TOOLARGE = "0301";

		/// <summary>
		/// Data length too large
		/// </summary>
		/// <value>
		///  0301
		/// </value>
		public static string NFC_MSG_OTA_FILE_EMPTY = "0401";

		/// <summary>
		/// Data length too large
		/// </summary>
		/// <value>
		///  0301
		/// </value>
		public static string NFC_MSG_OTA_FORMAT_ILLEGAl = "0402";

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
			},
			{
				NFC_MSG_OTA_FILE_EMPTY,
				"Please select an FW image to upload first"
			},
			{
				NFC_MSG_OTA_FORMAT_ILLEGAl,
				"FW image is illegal ,please reupload again"
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
}
