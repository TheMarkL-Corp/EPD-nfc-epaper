namespace com.advantech.nfc
{
	public interface INFCCommand
	{
		bool openNFC();

		void closeNFC();

		byte[] transferRF(byte[] data);

		bool isResponseOK(byte[] data);

		bool isValid();

		int getMaxNFCLength();
	}
}
