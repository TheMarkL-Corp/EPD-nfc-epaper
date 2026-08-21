namespace AdvNFCWrap
{
	public class RGBTriple
	{
		public int[] channels;

		public RGBTriple()
		{
			channels = new int[3];
		}

		public RGBTriple(int R, int G, int B)
		{
			channels = new int[3]
			{
				R,
				G,
				B
			};
		}
	}
}
