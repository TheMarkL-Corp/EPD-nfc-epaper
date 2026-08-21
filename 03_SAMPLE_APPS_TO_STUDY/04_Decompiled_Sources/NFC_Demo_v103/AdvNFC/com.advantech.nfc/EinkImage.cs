using Lz4Net;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace com.advantech.nfc
{
	public class EinkImage
	{
		private int width;

		private int height;

		private int pages;

		private byte[] data;

		private int lz4size;

		private byte[] lz4data;

		private static readonly byte[] font5x5 = new byte[80]
		{
			124,
			76,
			84,
			100,
			124,
			16,
			48,
			16,
			16,
			56,
			120,
			4,
			56,
			64,
			124,
			124,
			4,
			56,
			4,
			124,
			64,
			64,
			80,
			124,
			16,
			124,
			64,
			120,
			4,
			120,
			124,
			64,
			124,
			68,
			124,
			124,
			4,
			8,
			16,
			16,
			124,
			68,
			124,
			68,
			124,
			124,
			68,
			124,
			4,
			124,
			124,
			68,
			68,
			124,
			68,
			124,
			68,
			120,
			68,
			124,
			124,
			64,
			64,
			64,
			124,
			120,
			68,
			68,
			68,
			120,
			124,
			64,
			120,
			64,
			124,
			124,
			64,
			112,
			64,
			64
		};

		private static readonly byte[] font8x8 = new byte[1024]
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			24,
			60,
			60,
			24,
			24,
			0,
			24,
			0,
			54,
			54,
			0,
			0,
			0,
			0,
			0,
			0,
			54,
			54,
			127,
			54,
			127,
			54,
			54,
			0,
			12,
			62,
			3,
			30,
			48,
			31,
			12,
			0,
			0,
			99,
			51,
			24,
			12,
			102,
			99,
			0,
			28,
			54,
			28,
			110,
			59,
			51,
			110,
			0,
			6,
			6,
			3,
			0,
			0,
			0,
			0,
			0,
			24,
			12,
			6,
			6,
			6,
			12,
			24,
			0,
			6,
			12,
			24,
			24,
			24,
			12,
			6,
			0,
			0,
			102,
			60,
			byte.MaxValue,
			60,
			102,
			0,
			0,
			0,
			12,
			12,
			63,
			12,
			12,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			12,
			12,
			6,
			0,
			0,
			0,
			63,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			12,
			12,
			0,
			96,
			48,
			24,
			12,
			6,
			3,
			1,
			0,
			62,
			99,
			115,
			123,
			111,
			103,
			62,
			0,
			12,
			14,
			12,
			12,
			12,
			12,
			63,
			0,
			30,
			51,
			48,
			28,
			6,
			51,
			63,
			0,
			30,
			51,
			48,
			28,
			48,
			51,
			30,
			0,
			56,
			60,
			54,
			51,
			127,
			48,
			120,
			0,
			63,
			3,
			31,
			48,
			48,
			51,
			30,
			0,
			28,
			6,
			3,
			31,
			51,
			51,
			30,
			0,
			63,
			51,
			48,
			24,
			12,
			12,
			12,
			0,
			30,
			51,
			51,
			30,
			51,
			51,
			30,
			0,
			30,
			51,
			51,
			62,
			48,
			24,
			14,
			0,
			0,
			12,
			12,
			0,
			0,
			12,
			12,
			0,
			0,
			12,
			12,
			0,
			0,
			12,
			12,
			6,
			24,
			12,
			6,
			3,
			6,
			12,
			24,
			0,
			0,
			0,
			63,
			0,
			0,
			63,
			0,
			0,
			6,
			12,
			24,
			48,
			24,
			12,
			6,
			0,
			30,
			51,
			48,
			24,
			12,
			0,
			12,
			0,
			62,
			99,
			123,
			123,
			123,
			3,
			30,
			0,
			12,
			30,
			51,
			51,
			63,
			51,
			51,
			0,
			63,
			102,
			102,
			62,
			102,
			102,
			63,
			0,
			60,
			102,
			3,
			3,
			3,
			102,
			60,
			0,
			31,
			54,
			102,
			102,
			102,
			54,
			31,
			0,
			127,
			70,
			22,
			30,
			22,
			70,
			127,
			0,
			127,
			70,
			22,
			30,
			22,
			6,
			15,
			0,
			60,
			102,
			3,
			3,
			115,
			102,
			124,
			0,
			51,
			51,
			51,
			63,
			51,
			51,
			51,
			0,
			30,
			12,
			12,
			12,
			12,
			12,
			30,
			0,
			120,
			48,
			48,
			48,
			51,
			51,
			30,
			0,
			103,
			102,
			54,
			30,
			54,
			102,
			103,
			0,
			15,
			6,
			6,
			6,
			70,
			102,
			127,
			0,
			99,
			119,
			127,
			127,
			107,
			99,
			99,
			0,
			99,
			103,
			111,
			123,
			115,
			99,
			99,
			0,
			28,
			54,
			99,
			99,
			99,
			54,
			28,
			0,
			63,
			102,
			102,
			62,
			6,
			6,
			15,
			0,
			30,
			51,
			51,
			51,
			59,
			30,
			56,
			0,
			63,
			102,
			102,
			62,
			54,
			102,
			103,
			0,
			30,
			51,
			7,
			14,
			56,
			51,
			30,
			0,
			63,
			45,
			12,
			12,
			12,
			12,
			30,
			0,
			51,
			51,
			51,
			51,
			51,
			51,
			63,
			0,
			51,
			51,
			51,
			51,
			51,
			30,
			12,
			0,
			99,
			99,
			99,
			107,
			127,
			119,
			99,
			0,
			99,
			99,
			54,
			28,
			28,
			54,
			99,
			0,
			51,
			51,
			51,
			30,
			12,
			12,
			30,
			0,
			127,
			99,
			49,
			24,
			76,
			102,
			127,
			0,
			30,
			6,
			6,
			6,
			6,
			6,
			30,
			0,
			3,
			6,
			12,
			24,
			48,
			96,
			64,
			0,
			30,
			24,
			24,
			24,
			24,
			24,
			30,
			0,
			8,
			28,
			54,
			99,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			byte.MaxValue,
			12,
			12,
			24,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			30,
			48,
			62,
			51,
			110,
			0,
			7,
			6,
			6,
			62,
			102,
			102,
			59,
			0,
			0,
			0,
			30,
			51,
			3,
			51,
			30,
			0,
			56,
			48,
			48,
			62,
			51,
			51,
			110,
			0,
			0,
			0,
			30,
			51,
			63,
			3,
			30,
			0,
			28,
			54,
			6,
			15,
			6,
			6,
			15,
			0,
			0,
			0,
			110,
			51,
			51,
			62,
			48,
			31,
			7,
			6,
			54,
			110,
			102,
			102,
			103,
			0,
			12,
			0,
			14,
			12,
			12,
			12,
			30,
			0,
			48,
			0,
			48,
			48,
			48,
			51,
			51,
			30,
			7,
			6,
			102,
			54,
			30,
			54,
			103,
			0,
			14,
			12,
			12,
			12,
			12,
			12,
			30,
			0,
			0,
			0,
			51,
			127,
			127,
			107,
			99,
			0,
			0,
			0,
			31,
			51,
			51,
			51,
			51,
			0,
			0,
			0,
			30,
			51,
			51,
			51,
			30,
			0,
			0,
			0,
			59,
			102,
			102,
			62,
			6,
			15,
			0,
			0,
			110,
			51,
			51,
			62,
			48,
			120,
			0,
			0,
			59,
			110,
			102,
			6,
			15,
			0,
			0,
			0,
			62,
			3,
			30,
			48,
			31,
			0,
			8,
			12,
			62,
			12,
			12,
			44,
			24,
			0,
			0,
			0,
			51,
			51,
			51,
			51,
			110,
			0,
			0,
			0,
			51,
			51,
			51,
			30,
			12,
			0,
			0,
			0,
			99,
			107,
			127,
			127,
			54,
			0,
			0,
			0,
			99,
			54,
			28,
			54,
			99,
			0,
			0,
			0,
			51,
			51,
			51,
			62,
			48,
			31,
			0,
			0,
			63,
			25,
			12,
			38,
			63,
			0,
			56,
			12,
			12,
			7,
			12,
			12,
			56,
			0,
			24,
			24,
			24,
			0,
			24,
			24,
			24,
			0,
			7,
			12,
			12,
			56,
			12,
			12,
			7,
			0,
			110,
			59,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0
		};

		public int getWidth()
		{
			return width;
		}

		public int getHeight()
		{
			return height;
		}

		public int getPages()
		{
			return pages;
		}

		public byte[] getData()
		{
			return data;
		}

		public byte[] getlz4Data()
		{
			return lz4data;
		}

		public int getlz4()
		{
			return lz4size;
		}

		public int findNearestColor(Color color)
		{
			int[,] array = new int[4, 3]
			{
				{
					255,
					255,
					255
				},
				{
					0,
					0,
					0
				},
				{
					255,
					0,
					0
				},
				{
					255,
					255,
					0
				}
			};
			int result = 0;
			int r = color.R;
			int b = color.B;
			int g = color.G;
			int num = 195076;
			int num3;
			int num2;
			int num4 = num3 = (num2 = 255);
			int num5 = 0;
			for (int i = 0; i < 4; i++)
			{
				num4 = r - array[i, 0];
				num3 = g - array[i, 1];
				num2 = b - array[i, 2];
				num5 = num4 * num4 + num3 * num3 + num2 * num2;
				if (num5 < num)
				{
					num = num5;
					result = i;
				}
			}
			return result;
		}

		public char[,] img2rgb_forEPD37(Bitmap image)
		{
			int num = 255;
			int num2 = 0;
			char[,] array = new char[image.Width, image.Height];
			for (int i = 0; i < image.Width; i++)
			{
				for (int j = 0; j < image.Height; j++)
				{
					Color pixel = image.GetPixel(i, image.Height - j - 1);
					int b = pixel.B;
					int g = pixel.G;
					int r = pixel.R;
					if (r == num && g == num && b == num)
					{
						array[i, j] = 'w';
					}
					else if (r == num2 && g == num2 && b == num2)
					{
						array[i, j] = 'b';
					}
					else if (r == num && g == num2 && b == num2)
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
						double num3 = (double)r * 0.299 + (double)g * 0.587 + (double)b * 0.144;
						if (num3 > 192.0)
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

		private static char GetClosestColor(int r, int g, int b)
		{
			int[] color = new int[3];
			int[] color2 = new int[3]
			{
				255,
				255,
				255
			};
			int[] color3 = new int[3]
			{
				255,
				0,
				0
			};
			int[] array = new int[3]
			{
				255,
				255,
				0
			};
			double colorDistance = GetColorDistance(r, g, b, color);
			double colorDistance2 = GetColorDistance(r, g, b, color2);
			double colorDistance3 = GetColorDistance(r, g, b, color3);
			double num = Math.Min(Math.Min(colorDistance, colorDistance2), colorDistance3);
			if (num != colorDistance)
			{
				if (num != colorDistance2)
				{
					return 'r';
				}
				return 'w';
			}
			return 'b';
		}

		private static double GetColorDistance(int r, int g, int b, int[] color)
		{
			return Math.Sqrt(Math.Pow((double)(r - color[0]), 2.0) + Math.Pow((double)(g - color[1]), 2.0) + Math.Pow((double)(b - color[2]), 2.0));
		}

		public byte[] rgb2_forEPD37(char[,] rgbTable, int page)
		{
			byte[] array = (page <= 1) ? new byte[rgbTable.GetLength(1) * rgbTable.GetLength(0) / 8] : new byte[rgbTable.GetLength(1) * rgbTable.GetLength(0) / 4];
			int num = 0;
			for (int i = 0; i < rgbTable.GetLength(0); i++)
			{
				for (int j = 0; j < rgbTable.GetLength(1); j += 8)
				{
					byte b = 0;
					int num2 = 0;
					for (int k = 0; k < 8; k++)
					{
						switch (rgbTable[rgbTable.GetLength(0) - i - 1, j + k])
						{
						case 'w':
							num2 = 1 << 7 - k;
							break;
						case 'r':
							num2 = 1 << 7 - k;
							break;
						case 'b':
							num2 = 0;
							break;
						}
						b = (byte)(b + (byte)num2);
					}
					array[num] = b;
					num++;
				}
			}
			if (page > 1)
			{
				for (int l = 0; l < rgbTable.GetLength(0); l++)
				{
					for (int m = 0; m < rgbTable.GetLength(1); m += 8)
					{
						byte b2 = 0;
						for (int n = 0; n < 8; n++)
						{
							char c = rgbTable[rgbTable.GetLength(0) - l - 1, m + n];
							if (c == 'r')
							{
								int num3 = 1 << 7 - n;
								b2 = (byte)(b2 + (byte)num3);
							}
							else
							{
								int num4 = 0;
								b2 = (byte)(b2 + (byte)num4);
							}
						}
						array[num] = b2;
						num++;
					}
				}
			}
			return array;
		}

		public byte[] img_forEPD_BW(Bitmap image)
		{
			int num = image.Width;
			int num2 = image.Height;
			int num3 = 0;
			int num4 = num * num2 / 8;
			byte[] array = new byte[num4];
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j += 8)
				{
					int num5 = 0;
					for (int k = 0; k < 8; k++)
					{
						Color pixel = image.GetPixel(num - i - 1, j + k);
						num5 *= 2;
						if (ConvertPixel(pixel))
						{
							num5 |= 1;
						}
					}
					array[num3] = (byte)(num5 & 0xFF);
					num3++;
				}
			}
			return array;
		}

		public byte[] img_forEPD37_BW(Bitmap image)
		{
			int num = image.Width;
			int num2 = image.Height;
			int num3 = 0;
			int num4 = num * num2 / 8;
			byte[] array = new byte[num4];
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j += 8)
				{
					int num5 = 0;
					for (int k = 0; k < 8; k++)
					{
						Color pixel = image.GetPixel(i, j + k);
						num5 *= 2;
						if (ConvertPixel(pixel))
						{
							num5 |= 1;
						}
					}
					array[num3] = (byte)(num5 & 0xFF);
					num3++;
				}
			}
			return array;
		}

		public char[,] img2rgbY_EPD37(Bitmap image)
		{
			int num = 255;
			int num2 = 0;
			Logfile logfile = new Logfile();
			char[,] array = new char[image.Width, image.Height];
			for (int i = 0; i < image.Width; i++)
			{
				for (int j = 0; j < image.Height; j++)
				{
					Color pixel = image.GetPixel(i, image.Height - j - 1);
					int b = pixel.B;
					int g = pixel.G;
					int r = pixel.R;
					if (r == num && g == num && b == num)
					{
						array[i, j] = 'w';
					}
					else if (r == num2 && g == num2 && b == num2)
					{
						array[i, j] = 'b';
					}
					else if (r == num && g == num2 && b == num2)
					{
						array[i, j] = 'r';
					}
					else if (r == num && g == num && b == num2)
					{
						array[i, j] = 'y';
					}
					else if (r > 127 && 100 < r - g && 100 < r - b)
					{
						array[i, j] = 'r';
					}
					else if (r > 180 && g >= 158 && b < 169)
					{
						array[i, j] = 'y';
					}
					else
					{
						double num3 = (double)r * 0.299 + (double)g * 0.587 + (double)b * 0.144;
						if (num3 > 184.0)
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

		public byte[] rgbY2bin_EPD37(char[,] rgbTable)
		{
			byte[] array = new byte[rgbTable.GetLength(1) * rgbTable.GetLength(0) / 4];
			int num = 0;
			for (int i = 0; i < rgbTable.GetLength(0); i++)
			{
				for (int j = 0; j < rgbTable.GetLength(1); j += 4)
				{
					byte b = 0;
					for (int k = 0; k < 4; k++)
					{
						int num2 = 0;
						switch (rgbTable[rgbTable.GetLength(0) - i - 1, j + k])
						{
						case 'w':
							num2 = 1;
							break;
						case 'r':
							num2 = 3;
							break;
						case 'y':
							num2 = 2;
							break;
						case 'b':
							num2 = 0;
							break;
						}
						b = (byte)(b + (byte)num2);
						if (k < 3)
						{
							b = (byte)(b << 2);
						}
					}
					array[num] = b;
					num++;
				}
			}
			return array;
		}

		public EinkImage(int width, int height, int pages)
		{
			this.width = width;
			this.height = height;
			this.pages = pages;
			int num = width * height / 8;
			data = new byte[num * pages];
			for (int i = 0; i < num; i++)
			{
				data[i] = 0;
			}
		}

		public EinkImage(int width, int height, int pages, EinkImageTemplate type)
		{
			this.width = width;
			this.height = height;
			this.pages = pages;
			Random random = new Random();
			int num = width * height / 8;
			data = new byte[num * pages];
			int num2 = height / 8;
			for (int i = 0; i < num; i++)
			{
				switch (type)
				{
				case EinkImageTemplate.EINK_IMAGE_BLACK:
					data[i] = 0;
					if (pages != 1)
					{
						data[i + num] = 0;
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_WHITE:
					data[i] = byte.MaxValue;
					if (pages != 1)
					{
						data[i + num] = byte.MaxValue;
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_YELLOW:
					data[i] = 170;
					if (pages != 1)
					{
						data[i + num] = 170;
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_RANDOM:
					data[i] = (byte)(random.Next(256) & 0xFF);
					if (pages != 1)
					{
						data[i + num] = data[i];
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_VERTICAL_0:
					data[i] = 60;
					if (pages != 1)
					{
						data[i + num] = data[i];
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_VERTICAL_1:
					data[i] = 204;
					if (pages != 1)
					{
						data[i + num] = data[i];
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_HORIZOTAL_0:
					if (i / num2 % 2 == 0)
					{
						data[i] = byte.MaxValue;
					}
					else
					{
						data[i] = 0;
					}
					if (pages != 1)
					{
						data[i + num] = data[i];
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_HORIZOTAL_1:
					if (i / num2 % 2 == 1)
					{
						data[i] = byte.MaxValue;
					}
					else
					{
						data[i] = 0;
					}
					if (pages != 1)
					{
						data[i + num] = data[i];
					}
					break;
				}
			}
		}

		public EinkImage(int width, int height, int pages, EinkImageTemplate type, int lz4flag, int packsize, string epdname)
		{
			this.width = width;
			this.height = height;
			this.pages = pages;
			Random random = new Random();
			int num = width * height / 8;
			data = new byte[num * pages];
			lz4data = new byte[num * pages];
			int num2 = height / 8;
			for (int i = 0; i < num; i++)
			{
				switch (type)
				{
				case EinkImageTemplate.EINK_IMAGE_BLACK:
					data[i] = 0;
					if (pages != 1)
					{
						data[i + num] = 0;
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_WHITE:
					if (epdname.Equals("EPD-304--TC2"))
					{
						data[i] = 85;
						if (pages != 1)
						{
							data[i + num] = 85;
						}
					}
					else
					{
						data[i] = byte.MaxValue;
						if (pages != 1)
						{
							data[i + num] = 0;
						}
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_RED:
					data[i] = byte.MaxValue;
					if (pages != 1)
					{
						data[i + num] = byte.MaxValue;
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_YELLOW:
					data[i] = 170;
					if (pages != 1)
					{
						data[i + num] = 170;
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_RANDOM:
					data[i] = (byte)(random.Next(256) & 0xFF);
					if (pages != 1)
					{
						data[i + num] = data[i];
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_VERTICAL_0:
					if (epdname.Equals("EPD-304--TC2"))
					{
						data[i] = 75;
						if (pages != 1)
						{
							data[i + num] = 75;
						}
					}
					else
					{
						data[i] = 60;
						if (pages != 1)
						{
							data[i + num] = 12;
						}
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_VERTICAL_1:
					if (epdname.Equals("EPD-304--TC2"))
					{
						data[i] = 180;
						if (pages != 1)
						{
							data[i + num] = 180;
						}
					}
					else
					{
						data[i] = 204;
						if (pages != 1)
						{
							data[i + num] = 192;
						}
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_HORIZOTAL_0:
					if (epdname.Equals("EPD-304--TC2"))
					{
						num2 = height / 4;
						if (i / num2 % 10 == 0 || i / num2 % 10 == 1)
						{
							data[i] = 85;
						}
						else if (i / num2 % 10 == 2 || i / num2 % 10 == 3)
						{
							data[i] = 0;
						}
						else if (i / num2 % 10 == 4 || i / num2 % 10 == 5)
						{
							data[i] = 170;
						}
						else
						{
							data[i] = byte.MaxValue;
						}
						if (pages != 1)
						{
							data[i + num] = data[i];
						}
					}
					else
					{
						if (i / num2 % 4 == 0)
						{
							data[i] = byte.MaxValue;
						}
						else
						{
							data[i] = 0;
						}
						if (pages != 1)
						{
							data[i + num] = data[i];
						}
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_HORIZOTAL_1:
					if (epdname.Equals("EPD-304--TC2"))
					{
						num2 = height / 4;
						if (i / num2 % 10 == 0 || i / num2 % 10 == 1)
						{
							data[i] = byte.MaxValue;
						}
						else if (i / num2 % 10 == 2 || i / num2 % 10 == 3)
						{
							data[i] = 0;
						}
						else if (i / num2 % 10 == 4 || i / num2 % 10 == 5)
						{
							data[i] = 170;
						}
						else
						{
							data[i] = 85;
						}
						if (pages != 1)
						{
							data[i + num] = data[i];
						}
					}
					else
					{
						if (i / num2 % 4 == 1)
						{
							data[i] = byte.MaxValue;
						}
						else
						{
							data[i] = 0;
						}
						if (pages != 1)
						{
							data[i + num] = data[i];
						}
					}
					break;
				}
			}
			if (lz4flag == 0)
			{
				lz4data = new byte[1];
				lz4data[0] = 0;
				lz4size = 0;
			}
			else
			{
				int page = 1;
				Lz4comp_segment(width, height, page, packsize);
				if (pages > 1)
				{
					Lz4comp_segment(width, height, pages, packsize);
				}
			}
		}

		public static Bitmap ResizeImage(Image image, int width, int height)
		{
			Rectangle destRect = new Rectangle(0, 0, width, height);
			Bitmap bitmap = new Bitmap(width, height);
			bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.CompositingMode = CompositingMode.SourceCopy;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
				using (ImageAttributes imageAttributes = new ImageAttributes())
				{
					imageAttributes.SetWrapMode(WrapMode.TileFlipXY);
					graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
				}
			}
			return bitmap;
		}

		public EinkImage(int width, int height, int pages, Bitmap bitmap_old)
		{
			this.width = width;
			this.height = height;
			this.pages = pages;
			Bitmap bitmap = ResizeImage(bitmap_old, width, height);
			int num = width * height / 8;
			data = new byte[num * pages];
			int num2 = 0;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j += 8)
				{
					int num3 = 0;
					for (int k = 0; k < 8; k++)
					{
						Color pixel = bitmap.GetPixel(width - i - 1, j + k);
						num3 *= 2;
						if (ConvertPixel(pixel))
						{
							num3 |= 1;
						}
					}
					data[num2] = (byte)(num3 & 0xFF);
					if (pages != 1)
					{
						data[num2 + num] = data[num2];
					}
					num2++;
				}
			}
		}

		public EinkImage(int width, int height, int pages, Bitmap bitmap_old, int lz4flag, int lz4packsize, string epdname)
		{
			this.width = width;
			this.height = height;
			this.pages = pages;
			if (bitmap_old.Width < bitmap_old.Height)
			{
				bitmap_old.RotateFlip(RotateFlipType.Rotate270FlipNone);
			}
			Bitmap image = ResizeImage(bitmap_old, width, height);
			int num = width * height / 8;
			data = new byte[num * pages];
			lz4data = new byte[num * pages];
			if (num == Epd29.size)
			{
				data = img_forEPD_BW(image);
			}
			else if (epdname.Equals("EPD-302--TC2"))
			{
				char[,] rgbTable = img2rgb_forEPD37(image);
				data = rgb2_forEPD37(rgbTable, pages);
			}
			else if (epdname.Equals("EPD-303--TC2"))
			{
				char[,] rgbTable2 = img2rgb_forEPD37(image);
				data = rgb2_forEPD37(rgbTable2, pages);
			}
			else if (epdname.Equals("EPD-304--TC2"))
			{
				char[,] rgbTable3 = img2rgbY_EPD37(image);
				data = rgbY2bin_EPD37(rgbTable3);
			}
			if (lz4flag == 0)
			{
				lz4data = new byte[1];
				lz4data[0] = 0;
				lz4size = 0;
			}
			else
			{
				int page = 1;
				Lz4comp_segment(width, height, page, lz4packsize);
				if (pages > 1)
				{
					Lz4comp_segment(width, height, pages, lz4packsize);
				}
			}
		}

		private unsafe void Lz4comp_segment(int img_width, int img_height, int page, int packsize)
		{
			int size = 0;
			int num = 0;
			int num2 = 0;
			int num3 = img_width * img_height / 8;
			int num4 = (num3 / packsize + 1) * 2;
			byte[] array = new byte[Lz4.LZ4_compressBound(packsize)];
			int num5 = num3 / packsize + 1;
			num2 = array.Length * num5;
			byte[] array2 = new byte[Lz4.LZ4_compressBound(num2) + num4];
			bool flag = true;
			int num8;
			int num7;
			int num6;
			int num9 = num8 = (num7 = (num6 = 0));
			int num10 = (page != 1) ? (img_width * img_height / 8) : 0;
			while (flag)
			{
				fixed (byte* source = &data[num10])
				{
					fixed (byte* destination = &array[0])
					{
						num6 = ((num != 0) ? Lz4.LZ4_compressHC(source, destination, size) : Lz4.LZ4_compressHC(source, destination, packsize));
						if (num7 == 0)
						{
							array2[num8] = (byte)num6;
							array2[num8 + 1] = (byte)(num6 >> 8);
						}
						else
						{
							array2[num8 + 1] = (byte)num6;
							array2[num8 + 2] = (byte)(num6 >> 8);
						}
						if (num7 == 0)
						{
							Array.Copy(array, 0, array2, num8 + 2, num6);
						}
						else
						{
							Array.Copy(array, 0, array2, num8 + 3, num6);
						}
						num7 += num6 + 2;
						num8 = num7;
						if (num3 > packsize)
						{
							num10 += packsize;
							num3 -= packsize;
							if (num3 <= packsize)
							{
								size = num3;
								num = 1;
							}
						}
						else if (num3 <= packsize)
						{
							num10 += num3;
							num3 -= num3;
							if (num3 == 0)
							{
								array2[num8 + 1] = 13;
								array2[num8 + 2] = 10;
								num7 += 2;
								flag = false;
							}
						}
						Console.WriteLine("total size=" + num7);
						Console.WriteLine("while state=" + flag.ToString());
					}
				}
			}
			if (page == 1)
			{
				int num11 = img_width * img_height / 8;
				if (num7 < num11)
				{
					Array.Copy(array2, 0, lz4data, 0, num7 + 1);
				}
			}
			else if (lz4size + num7 < data.Length)
			{
				Array.Copy(array2, 0, lz4data, lz4size, num7 + 1);
			}
			lz4size += num7 + 1;
		}

		private bool ConvertPixel(Color color)
		{
			int r = color.R;
			int g = color.G;
			int b = color.B;
			int num = (r - 255) * (r - 255) + (g - 255) * (g - 255) + (b - 255) * (b - 255);
			int num2 = r * r + g * g + b * b;
			if (num2 >= num)
			{
				return true;
			}
			return false;
		}

		public void DrawText(int x, int y, string s)
		{
			foreach (char c in s)
			{
				byte b = (byte)(c & 0x7F);
				for (int j = 0; j < 8; j++)
				{
					byte b2 = font8x8[b * 8 + j];
					for (int k = 0; k < 8; k++)
					{
						int color = ((b2 & 1) == 0) ? 1 : 0;
						DrawPixel(x + k, y + j, color);
						b2 = (byte)(b2 >> 1);
					}
				}
				x += 8;
			}
		}

		public void DrawText5x5Digit(int x, int y, string s)
		{
			foreach (char c in s)
			{
				byte b = (byte)(c & 0x7F);
				b = ((b < 65) ? ((byte)(b - 48)) : ((byte)(b - 65 + 10)));
				for (int j = 0; j < 5; j++)
				{
					byte b2 = font5x5[b * 5 + j];
					for (int k = 0; k < 6; k++)
					{
						int color = ((b2 & 0x80) == 0) ? 1 : 0;
						DrawPixel(x + k, y + j, color);
						b2 = (byte)(b2 << 1);
					}
				}
				x += 6;
			}
		}

		public void DrawPixel(int x, int y, int color)
		{
			int num = width * height / 8;
			int num2 = y / 8;
			int num3 = y % 8;
			int num4 = height / 8 * (width - x - 1);
			int num5 = num4 + num2;
			byte b = data[num5];
			if (color == 0)
			{
				b = (byte)(b & ~(1 << 7 - num3));
				data[num5] = b;
				if (pages == 2)
				{
					data[num5 + num] = b;
				}
			}
			else
			{
				b = (byte)(b | (1 << 7 - num3));
				data[num5] = b;
				if (pages == 2)
				{
					data[num5 + num] = b;
				}
			}
		}
	}
}
