using System.Drawing;

namespace AdvNFCWrap
{
	internal class ImageConverter
	{
		private static int MIN = 0;

		private static int MAX = 255;

		private static int CENTER = 128;

		public static char[,] img2rgb_forEPD562(Bitmap image)
		{
			char[,] array = new char[image.Width, image.Height];
			for (int i = 0; i < image.Width; i++)
			{
				for (int j = 0; j < image.Height; j++)
				{
					Color pixel = image.GetPixel(i, j);
					int b = pixel.B;
					int g = pixel.G;
					int r = pixel.R;
					if (r == MAX && g == MAX && b == MAX)
					{
						array[i, j] = 'w';
					}
					else if (r == MIN && g == MIN && b == MIN)
					{
						array[i, j] = 'b';
					}
					else if (r == MAX && g == MIN && b == MIN)
					{
						array[i, j] = 'r';
					}
					else if (r > 200 && 50 < r - g && 50 < r - b)
					{
						array[i, j] = 'r';
					}
					else if (r > 150 && 10 > g && 10 > b)
					{
						array[i, j] = 'r';
					}
					else
					{
						double num = (double)r * 0.299 + (double)g * 0.587 + (double)b * 0.144;
						if (num > 192.0)
						{
							array[i, j] = 'w';
						}
						else
						{
							array[i, j] = 'b';
						}
					}
				}
			}
			image.Dispose();
			return array;
		}

		public static char[,] img2rgb_forEPD660(Bitmap image)
		{
			char[,] array = new char[image.Width, image.Height];
			for (int i = 0; i < image.Width; i++)
			{
				for (int j = 0; j < image.Height; j++)
				{
					Color pixel = image.GetPixel(i, j);
					int b = pixel.B;
					int g = pixel.G;
					int r = pixel.R;
					if (r == MAX && g == MAX && b == MAX)
					{
						array[i, j] = 'p';
					}
					else if (r == MIN && g == MIN && b == MIN)
					{
						array[i, j] = 'a';
					}
					else if (r == MAX && g == MIN && b == MIN)
					{
						array[i, j] = 'r';
					}
					else if (r > 200 && 50 < r - g && 50 < r - b)
					{
						array[i, j] = 'r';
					}
					else if (r > 150 && 10 > g && 10 > b)
					{
						array[i, j] = 'r';
					}
					else
					{
						double num = (double)r * 0.299 + (double)g * 0.587 + (double)b * 0.144;
						if (num < 16.0)
						{
							array[i, j] = 'a';
						}
						else if (num < 32.0)
						{
							array[i, j] = 'b';
						}
						else if (num < 48.0)
						{
							array[i, j] = 'c';
						}
						else if (num < 64.0)
						{
							array[i, j] = 'd';
						}
						else if (num < 80.0)
						{
							array[i, j] = 'e';
						}
						else if (num < 96.0)
						{
							array[i, j] = 'f';
						}
						else if (num < 112.0)
						{
							array[i, j] = 'g';
						}
						else if (num < 128.0)
						{
							array[i, j] = 'h';
						}
						else if (num < 144.0)
						{
							array[i, j] = 'i';
						}
						else if (num < 160.0)
						{
							array[i, j] = 'j';
						}
						else if (num < 176.0)
						{
							array[i, j] = 'k';
						}
						else if (num < 192.0)
						{
							array[i, j] = 'l';
						}
						else if (num < 208.0)
						{
							array[i, j] = 'm';
						}
						else if (num < 224.0)
						{
							array[i, j] = 'n';
						}
						else if (num < 240.0)
						{
							array[i, j] = 'o';
						}
						else
						{
							array[i, j] = 'p';
						}
					}
				}
			}
			image.Dispose();
			return array;
		}

		public static char[,] img2rgb_forEPD662(Bitmap image)
		{
			char[,] array = new char[image.Width, image.Height];
			for (int i = 0; i < image.Width; i++)
			{
				for (int j = 0; j < image.Height; j++)
				{
					Color pixel = image.GetPixel(i, j);
					int b = pixel.B;
					int g = pixel.G;
					int r = pixel.R;
					if (r == MAX && g == MAX && b == MAX)
					{
						array[i, j] = 'w';
					}
					else if (r == MIN && g == MIN && b == MIN)
					{
						array[i, j] = 'b';
					}
					else if (r == MAX && g == MIN && b == MIN)
					{
						array[i, j] = 'r';
					}
					else if (r > 200 && 50 < r - g && 50 < r - b)
					{
						array[i, j] = 'r';
					}
					else if (r > 150 && 10 > g && 10 > b)
					{
						array[i, j] = 'r';
					}
					else
					{
						double num = (double)r * 0.299 + (double)g * 0.587 + (double)b * 0.144;
						if (num > 192.0)
						{
							array[i, j] = 'w';
						}
						else
						{
							array[i, j] = 'b';
						}
					}
				}
			}
			image.Dispose();
			return array;
		}

		public static byte[] rgb2_forEPD562(char[,] rgbTable)
		{
			byte[] array = new byte[rgbTable.GetLength(1) * rgbTable.GetLength(0) / 2];
			int num = 0;
			for (int i = 0; i < rgbTable.GetLength(1); i++)
			{
				for (int j = 0; j < rgbTable.GetLength(0); j += 2)
				{
					byte b = 0;
					for (int k = 0; k < 2; k++)
					{
						switch (rgbTable[j + k, i])
						{
						case 'r':
						{
							int num3 = (k == 1) ? 64 : 4;
							b = (byte)(b + (byte)num3);
							break;
						}
						case 'w':
						{
							int num2 = (k == 1) ? 240 : 15;
							b = (byte)(b + (byte)num2);
							break;
						}
						}
					}
					array[num] = b;
					num++;
				}
			}
			return array;
		}

		public static byte[] rgb2_forEPD660(char[,] rgbTable)
		{
			byte[] array = new byte[rgbTable.GetLength(1) * rgbTable.GetLength(0) / 2];
			int num = 0;
			for (int i = 0; i < rgbTable.GetLength(1); i++)
			{
				for (int j = 0; j < rgbTable.GetLength(0); j += 2)
				{
					byte b = 0;
					for (int k = 0; k < 2; k++)
					{
						switch (rgbTable[j + k, i])
						{
						case 'b':
						{
							int num15 = (k != 1) ? 1 : 16;
							b = (byte)(b + (byte)num15);
							break;
						}
						case 'c':
						{
							int num9 = (k == 1) ? 32 : 2;
							b = (byte)(b + (byte)num9);
							break;
						}
						case 'd':
						{
							int num16 = (k == 1) ? 48 : 3;
							b = (byte)(b + (byte)num16);
							break;
						}
						case 'e':
						{
							int num12 = (k == 1) ? 64 : 4;
							b = (byte)(b + (byte)num12);
							break;
						}
						case 'f':
						{
							int num7 = (k == 1) ? 80 : 5;
							b = (byte)(b + (byte)num7);
							break;
						}
						case 'g':
						{
							int num13 = (k == 1) ? 96 : 6;
							b = (byte)(b + (byte)num13);
							break;
						}
						case 'h':
						{
							int num11 = (k == 1) ? 112 : 7;
							b = (byte)(b + (byte)num11);
							break;
						}
						case 'i':
						{
							int num4 = (k == 1) ? 128 : 8;
							b = (byte)(b + (byte)num4);
							break;
						}
						case 'j':
						{
							int num5 = (k == 1) ? 144 : 9;
							b = (byte)(b + (byte)num5);
							break;
						}
						case 'k':
						{
							int num8 = (k == 1) ? 160 : 10;
							b = (byte)(b + (byte)num8);
							break;
						}
						case 'l':
						{
							int num3 = (k == 1) ? 176 : 11;
							b = (byte)(b + (byte)num3);
							break;
						}
						case 'm':
						{
							int num14 = (k == 1) ? 192 : 12;
							b = (byte)(b + (byte)num14);
							break;
						}
						case 'n':
						{
							int num10 = (k == 1) ? 208 : 13;
							b = (byte)(b + (byte)num10);
							break;
						}
						case 'o':
						{
							int num6 = (k == 1) ? 224 : 14;
							b = (byte)(b + (byte)num6);
							break;
						}
						case 'p':
						{
							int num2 = (k == 1) ? 240 : 15;
							b = (byte)(b + (byte)num2);
							break;
						}
						}
					}
					array[num] = b;
					num++;
				}
			}
			return array;
		}

		public static byte[] rgb2_forEPD662(char[,] rgbTable)
		{
			byte[] array = new byte[rgbTable.GetLength(1) * rgbTable.GetLength(0) / 2];
			int num = 0;
			for (int i = 0; i < rgbTable.GetLength(1); i++)
			{
				for (int j = 0; j < rgbTable.GetLength(0); j += 2)
				{
					byte b = 0;
					for (int k = 0; k < 2; k++)
					{
						switch (rgbTable[j + k, i])
						{
						case 'r':
						{
							int num3 = (k == 1) ? 64 : 4;
							b = (byte)(b + (byte)num3);
							break;
						}
						case 'w':
						{
							int num2 = (k == 1) ? 240 : 15;
							b = (byte)(b + (byte)num2);
							break;
						}
						}
					}
					array[num] = b;
					num++;
				}
			}
			return array;
		}
	}
}
