namespace com.advantech.nfc
{
	public interface IDrawImageCallback
	{
		void onProgress(DrawImageState state, object data);
	}
}
