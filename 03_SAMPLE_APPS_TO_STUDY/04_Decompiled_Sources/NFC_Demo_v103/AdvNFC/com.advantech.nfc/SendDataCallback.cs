namespace com.advantech.nfc
{
	public interface SendDataCallback
	{
		void onProgress(SendDataState state, object data);
	}
}
