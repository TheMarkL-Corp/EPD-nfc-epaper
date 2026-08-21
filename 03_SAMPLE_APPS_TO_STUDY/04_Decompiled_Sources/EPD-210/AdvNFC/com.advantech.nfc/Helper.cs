using System;

namespace com.advantech.nfc
{
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
}
