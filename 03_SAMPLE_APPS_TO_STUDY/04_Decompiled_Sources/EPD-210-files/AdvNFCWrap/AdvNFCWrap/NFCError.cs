using System.Collections.Generic;

namespace AdvNFCWrap
{
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
}
