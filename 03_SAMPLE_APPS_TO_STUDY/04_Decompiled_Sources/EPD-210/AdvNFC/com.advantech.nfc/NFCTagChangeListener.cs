namespace com.advantech.nfc
{
	public interface NFCTagChangeListener
	{
		void onTagStateChange(NFCTagState state);
	}
}
