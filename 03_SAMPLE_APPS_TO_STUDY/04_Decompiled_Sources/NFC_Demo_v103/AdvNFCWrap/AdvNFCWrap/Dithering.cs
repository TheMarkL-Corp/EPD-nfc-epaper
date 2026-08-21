using System.Drawing;

namespace AdvNFCWrap
{
	public static class Dithering
	{
		public static RGBTriple[] bw = new RGBTriple[2]
		{
			new RGBTriple(255, 255, 255),
			new RGBTriple(0, 0, 0)
		};

		public static RGBTriple[] bwr = new RGBTriple[3]
		{
			new RGBTriple(255, 255, 255),
			new RGBTriple(0, 0, 0),
			new RGBTriple(255, 0, 0)
		};

		public static RGBTriple[] sevenColor = new RGBTriple[7]
		{
			new RGBTriple(0, 0, 0),
			new RGBTriple(0, 0, 255),
			new RGBTriple(0, 255, 0),
			new RGBTriple(255, 0, 0),
			new RGBTriple(255, 128, 0),
			new RGBTriple(255, 255, 0),
			new RGBTriple(255, 255, 255)
		};

		public static RGBTriple[] grayScale = new RGBTriple[16]
		{
			new RGBTriple(0, 0, 0),
			new RGBTriple(17, 17, 17),
			new RGBTriple(34, 34, 34),
			new RGBTriple(51, 51, 51),
			new RGBTriple(68, 68, 68),
			new RGBTriple(85, 85, 85),
			new RGBTriple(102, 102, 102),
			new RGBTriple(119, 119, 119),
			new RGBTriple(136, 136, 136),
			new RGBTriple(153, 153, 153),
			new RGBTriple(170, 170, 170),
			new RGBTriple(187, 187, 187),
			new RGBTriple(204, 204, 204),
			new RGBTriple(221, 221, 221),
			new RGBTriple(238, 238, 238),
			new RGBTriple(255, 255, 255)
		};

		public static RGBTriple[] fourColor = new RGBTriple[4]
		{
			new RGBTriple(0, 0, 0),
			new RGBTriple(255, 0, 0),
			new RGBTriple(255, 255, 0),
			new RGBTriple(255, 255, 255)
		};

		public static int getARGB(DirectBitmap image, int x, int y)
		{
			Color pixel = image.GetPixel(x, y);
			return (pixel.A << 24) | (pixel.R << 16) | (pixel.G << 8) | pixel.B;
		}

		public static void setARGB(DirectBitmap image, int x, int y, int argb)
		{
			Color colour = Color.FromArgb(argb);
			image.SetPixel(x, y, colour);
		}

		public static void applyFloydSteinbergDithering(DirectBitmap image, RGBTriple[] palette, bool reduce)
		{
			int num = 1;
			int num2 = 1;
			if (reduce)
			{
				num = 2;
				num2 = 2;
			}
			for (int i = 0; i < image.Height; i += num2)
			{
				for (int j = 0; j < image.Width; j += num)
				{
					int aRGB = getARGB(image, j, i);
					RGBTriple rGBTriple = findNearestColor(aRGB, palette);
					int num3 = -16777216 | (rGBTriple.channels[0] << 16) | (rGBTriple.channels[1] << 8) | rGBTriple.channels[2];
					setARGB(image, j, i, num3);
					int num4 = (aRGB >> 24) & 0xFF;
					int num5 = (aRGB >> 16) & 0xFF;
					int num6 = (aRGB >> 8) & 0xFF;
					int num7 = aRGB & 0xFF;
					int num8 = (num3 >> 24) & 0xFF;
					int num9 = (num3 >> 16) & 0xFF;
					int num10 = (num3 >> 8) & 0xFF;
					int num11 = num3 & 0xFF;
					int errA = num4 - num8;
					int errR = num5 - num9;
					int errG = num6 - num10;
					int errB = num7 - num11;
					if (j + 1 < image.Width)
					{
						int argb = adjustPixel(getARGB(image, j + 1, i), errA, errR, errG, errB, 7);
						setARGB(image, j + 1, i, argb);
						if (i + 1 < image.Height)
						{
							argb = adjustPixel(getARGB(image, j + 1, i + 1), errA, errR, errG, errB, 1);
							setARGB(image, j + 1, i + 1, argb);
						}
					}
					if (i + 1 < image.Height)
					{
						int argb2 = adjustPixel(getARGB(image, j, i + 1), errA, errR, errG, errB, 5);
						setARGB(image, j, i + 1, argb2);
						if (j - 1 >= 0)
						{
							argb2 = adjustPixel(getARGB(image, j - 1, i + 1), errA, errR, errG, errB, 3);
							setARGB(image, j - 1, i + 1, argb2);
						}
					}
				}
			}
		}

		private static int adjustPixel(int argb, int errA, int errR, int errG, int errB, int mul)
		{
			int num = (argb >> 24) & 0xFF;
			int num2 = (argb >> 16) & 0xFF;
			int num3 = (argb >> 8) & 0xFF;
			int num4 = argb & 0xFF;
			num += errA * mul >> 4;
			num2 += errR * mul >> 4;
			num3 += errG * mul >> 4;
			num4 += errB * mul >> 4;
			if (num < 0)
			{
				num = 0;
			}
			else if (num > 255)
			{
				num = 255;
			}
			if (num2 < 0)
			{
				num2 = 0;
			}
			else if (num2 > 255)
			{
				num2 = 255;
			}
			if (num3 < 0)
			{
				num3 = 0;
			}
			else if (num3 > 255)
			{
				num3 = 255;
			}
			if (num4 < 0)
			{
				num4 = 0;
			}
			else if (num4 > 255)
			{
				num4 = 255;
			}
			return (num << 24) | (num2 << 16) | (num3 << 8) | num4;
		}

		public static RGBTriple findNearestColor(int argb, RGBTriple[] palette)
		{
			int num = (argb >> 24) & 0xFF;
			int num2 = (argb >> 16) & 0xFF;
			int num3 = (argb >> 8) & 0xFF;
			int num4 = argb & 0xFF;
			int num5 = 195076;
			int num6 = 0;
			for (byte b = 0; b < palette.Length; b = (byte)(b + 1))
			{
				int num7 = num2 - palette[b].channels[0];
				int num8 = num3 - palette[b].channels[1];
				int num9 = num4 - palette[b].channels[2];
				int num10 = num7 * num7 + num8 * num8 + num9 * num9;
				if (num10 < num5)
				{
					num5 = num10;
					num6 = b;
				}
			}
			return palette[num6];
		}
	}
}
