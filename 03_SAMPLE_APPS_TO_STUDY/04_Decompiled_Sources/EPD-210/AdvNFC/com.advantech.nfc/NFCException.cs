using System;

namespace com.advantech.nfc
{
	public class NFCException : Exception
	{
		public NFCExceptionType type;

		public NFCException(NFCExceptionType type)
		{
			this.type = type;
		}
	}
}
