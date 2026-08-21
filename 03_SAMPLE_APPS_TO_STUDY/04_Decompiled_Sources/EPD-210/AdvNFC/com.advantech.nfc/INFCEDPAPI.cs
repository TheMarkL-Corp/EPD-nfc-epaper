namespace com.advantech.nfc
{
	public interface INFCEDPAPI
	{
		string GetVersion();

		string GetPlatformName();

		bool isValid();

		bool isBusy();

		byte[] getTagID();

		void TestAPI();

		bool CheckEPDStatus();

		void TxData(byte[] data);

		byte[] RxData();

		string WriteUserData(byte[] data);

		byte[] ReadUserData(int pos);

		void DrawImage(EinkImage image, DrawImageMethod method, IDrawImageCallback cb);

		string GetSN();

		byte GetPinCodeStatus();

		bool UnlockPinCode(byte[] data);

		bool SetPinCode(byte[] data);

		bool ResetPinCode(byte[] data);

		byte[] SystemRest();
	}
}
