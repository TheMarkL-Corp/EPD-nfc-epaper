using System.Linq;

namespace com.advantech.nfc.api
{
	public class NFCBuffer
	{
		private const int DEFAULT_READ_BUFFER_SIZE = 512;

		private const int DEFAULT_WRITE_BUFFER_SIZE = 512;

		private byte[] readBuffer = null;

		private byte[] writeBuffer = null;

		public NFCBuffer()
		{
			readBuffer = null;
			writeBuffer = null;
		}

		public bool putReadBuffer(byte[] data)
		{
			if (readBuffer == null)
			{
				readBuffer = data.ToArray();
				return true;
			}
			return false;
		}

		public int getReadBufferLength()
		{
			if (readBuffer != null)
			{
				return readBuffer.Length;
			}
			return 0;
		}

		public byte[] getDataReceived()
		{
			if (readBuffer != null)
			{
				byte[] result = readBuffer.ToArray();
				readBuffer = null;
				return result;
			}
			return null;
		}

		private void clearReadBuffer()
		{
			readBuffer = null;
		}

		public bool putWriteBuffer(byte[] data)
		{
			if (writeBuffer == null)
			{
				writeBuffer = data.ToArray();
				return true;
			}
			return false;
		}

		public int getWriteBufferLength()
		{
			if (writeBuffer != null)
			{
				return writeBuffer.Length;
			}
			return 0;
		}

		public byte[] getDataTransmitted()
		{
			if (writeBuffer != null)
			{
				byte[] result = writeBuffer.ToArray();
				writeBuffer = null;
				return result;
			}
			return null;
		}

		public void clearWriteBuffer()
		{
			writeBuffer = null;
		}
	}
}
