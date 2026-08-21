using System;

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
}
