namespace com.advantech.nfc.api
{
	public class FTMEventRec
	{
		public FTMEventType type;

		public object data;

		public FTMEventRec(FTMEventType type, object data)
		{
			this.type = type;
			this.data = data;
		}
	}
}
