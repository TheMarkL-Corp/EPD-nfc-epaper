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
					num = (((num & -2147483648) == 0) ? (num << 1) : ((num << 1) ^ POLYNOMIAL));
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
			if (!reflect_data)
			{
				return x;
			}
			return (byte)reflect(x, 8);
		}

		private uint REFLECT_REMAINDER(uint x)
		{
			if (!reflect_remainer)
			{
				return x;
			}
			return reflect(x, 32);
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
}
