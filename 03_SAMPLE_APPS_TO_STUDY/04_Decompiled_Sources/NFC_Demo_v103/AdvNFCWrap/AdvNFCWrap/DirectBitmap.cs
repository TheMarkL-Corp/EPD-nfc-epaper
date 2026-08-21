using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AdvNFCWrap
{
	public class DirectBitmap
	{
		public Bitmap Bitmap
		{
			get;
			private set;
		}

		public int[] Bits
		{
			get;
			private set;
		}

		public bool Disposed
		{
			get;
			private set;
		}

		public int Height
		{
			get;
			private set;
		}

		public int Width
		{
			get;
			private set;
		}

		public GCHandle BitsHandle
		{
			get;
			private set;
		}

		public DirectBitmap(int width, int height)
		{
			Width = width;
			Height = height;
			Bits = new int[width * height];
			BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
			Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
		}

		public void SetPixel(int x, int y, Color colour)
		{
			int num = x + y * Width;
			int num2 = colour.ToArgb();
			Bits[num] = num2;
		}

		public Color GetPixel(int x, int y)
		{
			int num = x + y * Width;
			int argb = Bits[num];
			return Color.FromArgb(argb);
		}

		public void Dispose()
		{
			if (!Disposed)
			{
				Disposed = true;
				Bitmap.Dispose();
				BitsHandle.Free();
			}
		}
	}
}
