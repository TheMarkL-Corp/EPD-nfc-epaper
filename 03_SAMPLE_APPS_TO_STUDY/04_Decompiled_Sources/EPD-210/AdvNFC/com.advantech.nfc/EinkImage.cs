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
				case EinkImageTemplate.EINK_IMAGE_RANDOM:
					data[i] = (byte)(random.Next(256) & 0xFF);
					if (pages != 1)
					{
						data[i + num] = data[i];
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_VERTICAL_0:
					data[i] = 85;
					if (pages != 1)
					{
						data[i + num] = data[i];
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_VERTICAL_1:
					data[i] = 170;
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

		public unsafe EinkImage(int width, int height, int pages, EinkImageTemplate type, int lz4flag, int packsize)
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
				case EinkImageTemplate.EINK_IMAGE_RANDOM:
					data[i] = (byte)(random.Next(256) & 0xFF);
					if (pages != 1)
					{
						data[i + num] = data[i];
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_VERTICAL_0:
					data[i] = 85;
					if (pages != 1)
					{
						data[i + num] = data[i];
					}
					break;
				case EinkImageTemplate.EINK_IMAGE_VERTICAL_1:
					data[i] = 170;
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
			if (lz4flag == 0)
			{
				lz4data = new byte[1];
				lz4data[0] = 0;
				lz4size = 0;
			}
			else
			{
				int num3 = 0;
				int num4 = 0;
				int num5 = width * height / 8;
				int num6 = (num5 / packsize + 1) * 2;
				byte[] array = new byte[Lz4.LZ4_compressBound(packsize)];
				int num7 = num5 / packsize + 1;
				int isize = array.Length * num7;
				lz4data = new byte[Lz4.LZ4_compressBound(isize) + num6];
				bool flag = true;
				int num10;
				int num8;
				int num9;
				int num11 = num10 = (num9 = (num8 = 0));
				Console.WriteLine("buffer length=" + array.Length);
				Console.WriteLine("newimage=" + lz4data.Length);
				while (flag)
				{
					fixed (byte* source = &data[num8])
					{
						fixed (byte* destination = &array[0])
						{
							if (num4 == 0)
							{
								num9 = Lz4.LZ4_compressHC(source, destination, packsize);
								Console.WriteLine("finflag=" + num4);
							}
							else
							{
								num9 = Lz4.LZ4_compressHC(source, destination, num3);
								Console.WriteLine("finflag=" + num4);
							}
							if (num10 == 0)
							{
								lz4data[num11] = (byte)num9;
								lz4data[num11 + 1] = (byte)(num9 >> 8);
							}
							else
							{
								lz4data[num11 + 1] = (byte)num9;
								lz4data[num11 + 2] = (byte)(num9 >> 8);
							}
							Console.WriteLine("sz=" + num9);
							Console.WriteLine("start pos=" + num11);
							if (num10 == 0)
							{
								Array.Copy(array, 0, lz4data, num11 + 2, num9);
							}
							else
							{
								Array.Copy(array, 0, lz4data, num11 + 3, num9);
							}
							num10 += num9 + 2;
							num11 = num10;
							Console.WriteLine("end pos=" + num11);
							if (num5 > packsize)
							{
								num8 += packsize;
								num5 -= packsize;
								Console.WriteLine("1_i=" + num8);
								Console.WriteLine("1_datasz=" + num5);
								if (num5 < packsize)
								{
									num3 = num5;
									Console.WriteLine("final_packet=" + num3);
									num4 = 1;
								}
							}
							else if (num5 < packsize)
							{
								num8 += num5;
								num5 -= num5;
								Console.WriteLine("2_i=" + num8);
								Console.WriteLine("2_datasz=" + num5);
								if (num5 == 0)
								{
									lz4data[num11 + 1] = 13;
									lz4data[num11 + 2] = 10;
									num10 += 2;
									flag = false;
								}
							}
							Console.WriteLine("total size=" + num10);
							Console.WriteLine("while state=" + flag.ToString());
						}
					}
				}
				lz4size = num10;
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
					return bitmap;
				}
			}
		}

		public EinkImage(int width, int height, int pages, Bitmap bitmap_old)
		{
			this.width = width;
			this.height = height;
			this.pages = pages;
			Bitmap bitmap = ResizeImage(bitmap_old, 296, 128);
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

		public unsafe EinkImage(int width, int height, int pages, Bitmap bitmap_old, int lz4flag, int packsize)
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
			if (lz4flag == 0)
			{
				lz4data = new byte[1];
				lz4data[0] = 0;
				lz4size = 0;
			}
			else
			{
				int size = 0;
				int num4 = 0;
				int num5 = 0;
				int num6 = width * height / 8;
				int num7 = (num6 / packsize + 1) * 2;
				byte[] array = new byte[Lz4.LZ4_compressBound(packsize)];
				int num8 = num6 / packsize + 1;
				num5 = array.Length * num8;
				lz4data = new byte[Lz4.LZ4_compressBound(num5) + num7];
				bool flag = true;
				int num11;
				int num9;
				int num10;
				int num12 = num11 = (num10 = (num9 = 0));
				num6 = width * height / 8;
				while (flag)
				{
					fixed (byte* source = &data[num9])
					{
						fixed (byte* destination = &array[0])
						{
							num10 = ((num4 != 0) ? Lz4.LZ4_compressHC(source, destination, size) : Lz4.LZ4_compressHC(source, destination, packsize));
							if (num11 == 0)
							{
								lz4data[num12] = (byte)num10;
								lz4data[num12 + 1] = (byte)(num10 >> 8);
							}
							else
							{
								lz4data[num12 + 1] = (byte)num10;
								lz4data[num12 + 2] = (byte)(num10 >> 8);
							}
							if (num11 == 0)
							{
								Array.Copy(array, 0, lz4data, num12 + 2, num10);
							}
							else
							{
								Array.Copy(array, 0, lz4data, num12 + 3, num10);
							}
							num11 += num10 + 2;
							num12 = num11;
							if (num6 > packsize)
							{
								num9 += packsize;
								num6 -= packsize;
								if (num6 < packsize)
								{
									size = num6;
									num4 = 1;
								}
							}
							else if (num6 < packsize)
							{
								num9 += num6;
								num6 -= num6;
								if (num6 == 0)
								{
									lz4data[num12 + 1] = 13;
									lz4data[num12 + 2] = 10;
									num11 += 2;
									flag = false;
								}
							}
						}
					}
				}
				lz4size = num11;
			}
		}

		private bool ConvertPixel(Color color)
		{
			int r = color.R;
			int g = color.G;
			int b = color.B;
			int num = (r - 255) * (r - 255) + (g - 255) * (g - 255) + (b - 255) * (b - 255);
			if (r * r + g * g + b * b < num)
			{
				return false;
			}
			return true;
		}

		public void DrawText(int x, int y, string s)
		{
			for (int i = 0; i < s.Length; i++)
			{
				byte b = (byte)(s[i] & 0x7F);
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
			for (int i = 0; i < s.Length; i++)
			{
				byte b = (byte)(s[i] & 0x7F);
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
			int num4 = height / 8 * (width - x - 1) + num2;
			byte b = data[num4];
			if (color == 0)
			{
				b = (byte)(b & ~(1 << 7 - num3));
				data[num4] = b;
				if (pages == 2)
				{
					data[num4 + num] = b;
				}
			}
			else
			{
				b = (byte)(b | (1 << 7 - num3));
				data[num4] = b;
				if (pages == 2)
				{
					data[num4 + num] = b;
				}
			}
		}
	}
}
