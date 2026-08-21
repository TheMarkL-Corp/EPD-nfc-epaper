using System.Linq;

namespace com.advantech.nfc.api
{
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
}
