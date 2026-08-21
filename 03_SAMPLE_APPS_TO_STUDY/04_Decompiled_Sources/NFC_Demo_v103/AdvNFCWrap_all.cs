using AdvNFCWrap.model;
using com.advantech.nfc;
using com.advantech.nfc.cmd;
using J_RFID;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: AssemblyTitle("AdvNFCWrap")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("AdvNFCWrap")]
[assembly: AssemblyCopyright("Copyright ©  2021")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("edeaa656-624f-4165-9f23-291560aca64e")]
[assembly: AssemblyFileVersion("1.0.1.0")]
[assembly: TargetFramework(".NETFramework,Version=v4.6.1", FrameworkDisplayName = ".NET Framework 4.6.1")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion("1.0.1.0")]
namespace AdvNFCWrap
{
	/// <summary>
	/// Error message of send command to tag
	/// </summary>
	public static class Constants
	{
		public static string FW_SUPPORT = "";
	}
	public static class BSCAdjuster
	{
		public static DirectBitmap transform(DirectBitmap src, double saturation, double brightness, double contrast)
		{
			saturation = 1.0 + saturation / 100.0;
			brightness = 1.0 + brightness / 100.0;
			contrast = 1.0 + contrast / 100.0;
			int width = src.Width;
			int height = src.Height;
			DirectBitmap directBitmap = new DirectBitmap(width, height);
			for (int i = 0; i < src.Height; i++)
			{
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				for (int j = 0; j < src.Width; j++)
				{
					Color pixel = src.GetPixel(j, i);
					num = pixel.A;
					num2 = pixel.R;
					num3 = pixel.G;
					num4 = pixel.B;
					double[] array = rgb2hsl(new int[3]
					{
						num2,
						num3,
						num4
					});
					array[1] *= saturation;
					if (array[1] < 0.0)
					{
						array[1] = 0.0;
					}
					if (array[1] > 255.0)
					{
						array[1] = 255.0;
					}
					array[2] *= brightness;
					if (array[2] < 0.0)
					{
						array[2] = 0.0;
					}
					if (array[2] > 255.0)
					{
						array[2] = 255.0;
					}
					int[] array2 = hsl2rgb(array);
					num2 = clamp(array2[0]);
					num3 = clamp(array2[1]);
					num4 = clamp(array2[2]);
					double num5 = ((double)num2 / 255.0 - 0.5) * contrast;
					double num6 = ((double)num3 / 255.0 - 0.5) * contrast;
					double num7 = ((double)num4 / 255.0 - 0.5) * contrast;
					num2 = (int)((num5 + 0.5) * 255.0);
					num3 = (int)((num6 + 0.5) * 255.0);
					num4 = (int)((num7 + 0.5) * 255.0);
					if (num2 < 0)
					{
						num2 = 0;
					}
					if (num2 > 255)
					{
						num2 = 255;
					}
					if (num3 < 0)
					{
						num3 = 0;
					}
					if (num3 > 255)
					{
						num3 = 255;
					}
					if (num4 < 0)
					{
						num4 = 0;
					}
					if (num4 > 255)
					{
						num4 = 255;
					}
					Color color = default(Color);
					color = Color.FromArgb(num, num2, num3, num4);
					directBitmap.SetPixel(j, i, color);
				}
			}
			src.Dispose();
			return directBitmap;
		}

		public static int clamp(int value)
		{
			return (value > 255) ? 255 : ((value >= 0) ? value : 0);
		}

		public static double[] rgb2hsl(int[] rgb)
		{
			double num = (double)Math.Max(Math.Max(rgb[0], rgb[1]), rgb[2]);
			double num2 = num - (double)Math.Min(Math.Min(rgb[0], rgb[1]), rgb[2]);
			double num3 = 0.0;
			int num4 = 0;
			int num5 = (int)Math.Round(num * 100.0 / 255.0);
			if (num != 0.0)
			{
				num4 = (int)Math.Round(num2 * 100.0 / num);
				num3 = ((num == (double)rgb[0]) ? ((double)(rgb[1] - rgb[2]) / num2) : ((num != (double)rgb[1]) ? ((double)(rgb[0] - rgb[1]) / num2 + 4.0) : ((double)(rgb[2] - rgb[0]) / num2 + 2.0)));
				num3 = Math.Min(Math.Round(num3 * 60.0), 360.0);
				if (num3 < 0.0)
				{
					num3 += 360.0;
				}
			}
			return new double[3]
			{
				num3,
				(double)num4,
				(double)num5
			};
		}

		public static int[] hsl2rgb(double[] hsl)
		{
			double num = hsl[0] / 360.0;
			double num2 = hsl[1] / 100.0;
			double num3 = hsl[2] / 100.0;
			double a = 0.0;
			double a2 = 0.0;
			if (!(num2 > 0.0))
			{
				num3 = Math.Round(num3 * 255.0);
				return new int[3]
				{
					(int)num3,
					(int)num3,
					(int)num3
				};
			}
			if (num >= 1.0)
			{
				num = 0.0;
			}
			num *= 6.0;
			double num4 = num - Math.Floor(num);
			double num5 = Math.Round(num3 * 255.0 * (1.0 - num2));
			double num6 = Math.Round(num3 * 255.0 * (1.0 - num2 * num4));
			double num7 = Math.Round(num3 * 255.0 * (1.0 - num2 * (1.0 - num4)));
			num3 = Math.Round(num3 * 255.0);
			switch ((int)Math.Floor(num))
			{
			case 0:
				a = num3;
				a2 = num7;
				num6 = num5;
				break;
			case 1:
				a = num6;
				a2 = num3;
				num6 = num5;
				break;
			case 2:
				a = num5;
				a2 = num3;
				num6 = num7;
				break;
			case 3:
				a = num5;
				a2 = num6;
				num6 = num3;
				break;
			case 4:
				a = num7;
				a2 = num5;
				num6 = num3;
				break;
			case 5:
				a = num3;
				a2 = num5;
				break;
			}
			return new int[3]
			{
				(int)Math.Round(a),
				(int)Math.Round(a2),
				(int)Math.Round(num6)
			};
		}
	}
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
	internal class ImageGenerator
	{
		private int width;

		private int height;

		private string color;

		private RGBTriple[] rGBTriples;

		private string model;

		private bool image_reverse;

		public ImageGenerator(string model)
		{
			this.model = model;
			Dictionary<string, object> property = EPDModel.Instance.getProperty(model);
			if (property == null)
			{
				width = 1600;
				height = 1200;
				color = "RBW";
				image_reverse = true;
				rGBTriples = Dithering.bwr;
			}
			else
			{
				width = (int)property["width"];
				height = (int)property["height"];
				color = (string)property["color"];
				image_reverse = (bool)property["image_reverse"];
				if (EPDModel.COLOR_RBW.Equals(color))
				{
					rGBTriples = Dithering.bwr;
				}
				else if (EPDModel.COLOR_GRAY.Equals(color))
				{
					rGBTriples = Dithering.grayScale;
				}
				else if (EPDModel.COLOR_FOUR.Equals(color))
				{
					rGBTriples = Dithering.fourColor;
				}
				else if (EPDModel.COLOR_FULL.Equals(color))
				{
					rGBTriples = Dithering.sevenColor;
				}
				else
				{
					rGBTriples = Dithering.sevenColor;
				}
			}
		}

		public ImageGenerator(string model, Size size)
		{
			this.model = model;
			Dictionary<string, object> property = EPDModel.Instance.getProperty(model);
			if (property == null)
			{
				width = size.Width;
				height = size.Height;
				color = "RBW";
				image_reverse = true;
				rGBTriples = Dithering.bwr;
			}
			else
			{
				width = size.Width;
				height = size.Height;
				color = (string)property["color"];
				image_reverse = (bool)property["image_reverse"];
				if (EPDModel.COLOR_RBW.Equals(color))
				{
					rGBTriples = Dithering.bwr;
				}
				else if (EPDModel.COLOR_GRAY.Equals(color))
				{
					rGBTriples = Dithering.grayScale;
				}
				else if (EPDModel.COLOR_FOUR.Equals(color))
				{
					rGBTriples = Dithering.fourColor;
				}
				else if (EPDModel.COLOR_FULL.Equals(color))
				{
					rGBTriples = Dithering.sevenColor;
				}
				else
				{
					rGBTriples = Dithering.sevenColor;
				}
			}
		}

		public Bitmap generateImage(Bitmap image)
		{
			Bitmap bitmap = resizeImage(image, new Size(width, height));
			if (!image_reverse)
			{
				return bitmap;
			}
			return rotateImage(bitmap, RotateFlipType.Rotate180FlipNone);
		}

		public Bitmap bscAdjust(Bitmap image)
		{
			DirectBitmap directBitmap = new DirectBitmap(width, height);
			using (Graphics graphics = Graphics.FromImage(directBitmap.Bitmap))
			{
				graphics.DrawImage(image, 0, 0, width, height);
				image.Dispose();
			}
			DirectBitmap directBitmap2 = null;
			try
			{
				directBitmap2 = BSCAdjuster.transform(directBitmap, 35.0, 15.0, 25.0);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.StackTrace);
			}
			Bitmap bitmap = new Bitmap(width, height);
			using (Graphics graphics2 = Graphics.FromImage(bitmap))
			{
				graphics2.DrawImage(directBitmap2.Bitmap, 0, 0, width, height);
				directBitmap2.Dispose();
			}
			return bitmap;
		}

		public Bitmap dithering(Bitmap image)
		{
			return dithering(image, false);
		}

		public Bitmap dithering(Bitmap image, bool reduce)
		{
			DirectBitmap directBitmap = new DirectBitmap(width, height);
			using (Graphics graphics = Graphics.FromImage(directBitmap.Bitmap))
			{
				graphics.DrawImage(image, 0, 0, width, height);
				image.Dispose();
			}
			Dithering.applyFloydSteinbergDithering(directBitmap, rGBTriples, reduce);
			Bitmap bitmap = new Bitmap(width, height);
			using (Graphics graphics2 = Graphics.FromImage(bitmap))
			{
				graphics2.DrawImage(directBitmap.Bitmap, 0, 0, width, height);
				directBitmap.Dispose();
			}
			return bitmap;
		}

		public byte[] toByteArrayRaw(Bitmap image)
		{
			byte[] result = null;
			if ("EPD-662".Equals(model))
			{
				char[,] rgbTable = ImageConverter.img2rgb_forEPD662(image);
				result = ImageConverter.rgb2_forEPD662(rgbTable);
			}
			else if ("EPD-660".Equals(model))
			{
				char[,] rgbTable2 = ImageConverter.img2rgb_forEPD660(image);
				result = ImageConverter.rgb2_forEPD660(rgbTable2);
			}
			else if ("EPD-562".Equals(model))
			{
				char[,] rgbTable3 = ImageConverter.img2rgb_forEPD562(image);
				result = ImageConverter.rgb2_forEPD562(rgbTable3);
			}
			else if ("EPD-763".Equals(model))
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					image.Save(memoryStream, ImageFormat.Bmp);
					result = memoryStream.ToArray();
				}
			}
			else if ("EPD-765".Equals(model))
			{
				using (MemoryStream memoryStream2 = new MemoryStream())
				{
					image.Save(memoryStream2, ImageFormat.Bmp);
					result = memoryStream2.ToArray();
				}
			}
			else
			{
				char[,] rgbTable4 = ImageConverter.img2rgb_forEPD662(image);
				result = ImageConverter.rgb2_forEPD662(rgbTable4);
			}
			return result;
		}

		public Bitmap resizeImage(Bitmap imgToResize, Size size)
		{
			int num = imgToResize.Width;
			int num2 = imgToResize.Height;
			int num3 = size.Width;
			int num4 = size.Height;
			Bitmap bitmap = new Bitmap(num3, num4);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.DrawImage(imgToResize, 0, 0, num3, num4);
			graphics.Dispose();
			return bitmap;
		}

		public Bitmap rotateImage(Bitmap image, RotateFlipType type)
		{
			image.RotateFlip(type);
			return image;
		}

		public byte[] bitmapToByteArray(Bitmap bitmap)
		{
			MemoryStream memoryStream = new MemoryStream();
			bitmap.Save(memoryStream, ImageFormat.Png);
			return memoryStream.GetBuffer();
		}

		public void ApplySmoothing(Bitmap originalImage, Bitmap resultImage)
		{
			int num = 3;
			for (int i = 1; i < originalImage.Height - 1; i++)
			{
				for (int j = 1; j < originalImage.Width - 1; j++)
				{
					int num2 = 0;
					int num3 = 0;
					int num4 = 0;
					for (int k = -1; k <= 1; k++)
					{
						for (int l = -1; l <= 1; l++)
						{
							Color pixel = originalImage.GetPixel(j + l, i + k);
							num2 += pixel.R;
							num3 += pixel.G;
							num4 += pixel.B;
						}
					}
					num2 /= num * num;
					num3 /= num * num;
					num4 /= num * num;
					resultImage.SetPixel(j, i, Color.FromArgb(num2, num3, num4));
				}
			}
		}
	}
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
	/// <summary>
	/// Error message of send command to tag
	/// </summary>
	public class NFCError
	{
		/// <summary>
		/// Success
		/// </summary>
		/// <value>
		///  0000
		/// </value>
		public static string NFC_MSG_SUCCESS = "0000";

		/// <summary>
		/// Port is empty
		/// </summary>
		/// <value>
		///  0101
		/// </value>
		public static string NFC_MSG_PORT_EMPTY = "0101";

		/// <summary>
		/// Tag not ready
		/// </summary>
		/// <value>
		///  0201
		/// </value>
		public static string NFC_MSG_TAG_NOREADY = "0201";

		/// <summary>
		/// Tag command error
		/// </summary>
		/// <value>
		///  0202
		/// </value>
		public static string NFC_MSG_TAG_COMMAND_ERROR = "0202";

		/// <summary>
		/// Tag firmware version not support
		/// </summary>
		/// <value>
		///  0203
		/// </value>
		public static string NFC_MSG_TAG_FW_NO_SUPPORT = "0203";

		/// <summary>
		/// Data length too large
		/// </summary>
		/// <value>
		///  0301
		/// </value>
		public static string NFC_MSG_DATA_LENGTH_TOOLARGE = "0301";

		/// <summary>
		/// Data length too large
		/// </summary>
		/// <value>
		///  0301
		/// </value>
		public static string NFC_MSG_OTA_FILE_EMPTY = "0401";

		/// <summary>
		/// Data length too large
		/// </summary>
		/// <value>
		///  0301
		/// </value>
		public static string NFC_MSG_OTA_FORMAT_ILLEGAl = "0402";

		private static Dictionary<string, string> defaultMessages = new Dictionary<string, string>
		{
			{
				NFC_MSG_SUCCESS,
				"Success"
			},
			{
				NFC_MSG_PORT_EMPTY,
				"Port is empty"
			},
			{
				NFC_MSG_TAG_NOREADY,
				"Tag not ready"
			},
			{
				NFC_MSG_TAG_COMMAND_ERROR,
				"Tag command error"
			},
			{
				NFC_MSG_TAG_FW_NO_SUPPORT,
				"Tag firmware version not support"
			},
			{
				NFC_MSG_DATA_LENGTH_TOOLARGE,
				"Data length too large"
			},
			{
				NFC_MSG_OTA_FILE_EMPTY,
				"Please select an FW image to upload first"
			},
			{
				NFC_MSG_OTA_FORMAT_ILLEGAl,
				"FW image is illegal ,please reupload again"
			}
		};

		public string Code
		{
			get;
			set;
		}

		public string Content
		{
			get;
			set;
		}

		public NFCError(string code)
		{
			if (code == NFC_MSG_TAG_FW_NO_SUPPORT)
			{
				setFWSupport();
			}
			Code = code;
			if (defaultMessages.ContainsKey(Code))
			{
				Content = defaultMessages[Code];
			}
			else
			{
				Content = code;
				Code = "0000";
			}
		}

		public override string ToString()
		{
			return "Code: " + Code + ", Message: " + Content;
		}

		public void setFWSupport()
		{
			defaultMessages[NFC_MSG_TAG_FW_NO_SUPPORT] = $"Tag firmware version not support, the minimum support version is [ {Constants.FW_SUPPORT} ]";
		}
	}
	/// <summary>
	/// Encapsulated NFC dll library, read / write tag data and refresh images
	/// </summary>
	public class NFCWrap : NFCTagChangeListener, IDrawImageCallback
	{
		public enum nTagState
		{
			/// <summary>
			/// Tag is offline
			/// </summary>
			NFC_TAG_STATE_TAG_OFF,
			/// <summary>
			/// Tag is connect on reader 
			/// </summary>
			NFC_TAG_STATE_TAG_ON,
			/// <summary>
			/// Tag is ready
			/// </summary>
			NFC_TAG_STATE_COMM_ON
		}

		public interface TagState
		{
			void onTagState(nTagState state);
		}

		public enum nImageState
		{
			DIState_Erase,
			DIState_SendData,
			DIState_WriteToEPD,
			DIState_Finish,
			DIState_Error
		}

		public enum nOTAState
		{
			SDState_Getinfo,
			SDState_Reboot_FW_App,
			SDState_Erase,
			SDState_Unlock,
			SDState_SendData,
			SDState_Checksum_APP,
			SDState_Finish,
			SDState_Error,
			SDState_Erase_Error,
			SDState_NOAPP_Error,
			SDState_UPGRADEAPP_Error,
			SDState_BLTAPP_Error,
			SDState_DEV_VOLT_Error,
			SDState_Checksum_Error,
			SDState_Compare_Error
		}

		public interface ProcessState
		{
			void onProcessState(nImageState state, object data);
		}

		public interface OTAProcessState
		{
			void onOTAProcessState(nOTAState state, object data);
		}

		public struct myColor
		{
			public byte A;

			public byte R;

			public byte G;

			public byte B;

			public myColor(byte a, byte r, byte g, byte b)
			{
				A = a;
				R = r;
				G = g;
				B = b;
			}
		}

		public class VersionComparer
		{
			public static int CompareVersions(string version1, string version2)
			{
				Version version3 = new Version(version1);
				Version value = new Version(version2);
				return version3.CompareTo(value);
			}
		}

		private static NFCManager manager = null;

		private static INFCEDPAPI api = null;

		private static INFCCommand nfc = null;

		private static TagState _tagState = null;

		private static ProcessState _processState = null;

		private static OTAProcessState _OTAProcessState = null;

		private string comPort = "";

		private static RFIDAPI NFC_API = new RFIDAPI();

		private bool _TagReady = false;

		private static bool chkTagConnected = false;

		private bool _bNFC = false;

		private bool _bNFCData = false;

		private int nTryCount = 100;

		private static nImageState mImageState;

		private static nOTAState mSendDataState;

		private string model;

		private Logfile log;

		/// <summary>
		/// Callback function when the tag status changes
		/// </summary>
		public TagState TagStateListener
		{
			get
			{
				return _tagState;
			}
			set
			{
				_tagState = value;
			}
		}

		/// <summary>
		/// Callback function while refresh image
		/// </summary>
		public ProcessState ProcessStateListener
		{
			get
			{
				return _processState;
			}
			set
			{
				_processState = value;
			}
		}

		public OTAProcessState OTAProcessStateListener
		{
			get
			{
				return _OTAProcessState;
			}
			set
			{
				_OTAProcessState = value;
			}
		}

		/// <summary>
		/// Empty constructor, from which to call functions
		/// </summary>
		public NFCWrap()
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Expected O, but got Unknown
			log = new Logfile();
			comPort = "";
			CloseNFC();
			CloseNFCData();
		}

		/// <summary>
		/// Connect the NFC reader from the given port
		/// </summary>
		/// <param name="strPort">the port name of reader connected</param>
		public NFCWrap(string strPort)
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Expected O, but got Unknown
			log = new Logfile();
			comPort = strPort;
			CloseNFC();
			CloseNFCData();
		}

		/// <summary>
		/// Detect the port connected to the NFC reader
		/// </summary>
		/// <returns>Port string/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetPort();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string GetPort()
		{
			CloseNFC();
			CloseNFCData();
			string text = NFCError.NFC_MSG_PORT_EMPTY;
			string[] portNames = SerialPort.GetPortNames();
			int num = 0;
			for (int i = 0; i < portNames.Length; i++)
			{
				string text2 = portNames[i];
				num = NFC_API.RFID_OpenReader(text2);
				Console.WriteLine("RFID_OpenReader : " + num + " Port [" + text2 + "]");
				string FirmwareVer = "";
				num = NFC_API.RFID_FWVersion(out FirmwareVer);
				Console.WriteLine("RFID_FWVersion : " + num);
				if (num == 0)
				{
					NFC_API.RFID_CloseReader(comPort);
					text = portNames[i];
					break;
				}
				NFC_API.RFID_CloseReader(comPort);
			}
			Console.WriteLine("GetPort strResult : " + text);
			return text;
		}

		/// <summary>
		/// Connect tag
		/// </summary>
		/// <returns>
		/// <see cref="T:AdvNFCWrap.NFCError" />
		/// </returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.ConnectTag();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string ConnectTag()
		{
			string result = NFCError.NFC_MSG_SUCCESS;
			if (comPort != "")
			{
				OpenNFC();
			}
			else
			{
				result = NFCError.NFC_MSG_PORT_EMPTY;
			}
			return result;
		}

		/// <summary>
		/// Connect tag and return connect status
		/// </summary>
		/// <returns>
		/// <see cref="T:AdvNFCWrap.NFCError" />
		/// </returns>
		/// <example>
		/// <code>
		/// String strReturn = await oNFC.ConnectTagAsync();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public async Task<string> ConnectTagAsync()
		{
			string strResult;
			if (comPort != "")
			{
				OpenNFC();
				strResult = await chkTagState();
			}
			else
			{
				strResult = NFCError.NFC_MSG_PORT_EMPTY;
			}
			Console.WriteLine("ConnectTagAsync : [" + strResult + "]");
			return strResult;
		}

		private static async Task<string> chkTagState()
		{
			string result = NFCError.NFC_MSG_TAG_NOREADY;
			Task subThreadTask = Task.Run(delegate
			{
				Thread.Sleep(3000);
				result = (chkTagConnected ? NFCError.NFC_MSG_SUCCESS : NFCError.NFC_MSG_TAG_NOREADY);
			});
			await subThreadTask;
			return result;
		}

		/// <summary>
		/// Disconnect tag
		/// </summary>
		/// <returns>
		/// <see cref="T:AdvNFCWrap.NFCError" />
		/// </returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.DisconnectTag();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string DisconnectTag()
		{
			string text = NFCError.NFC_MSG_SUCCESS;
			if (comPort != "")
			{
				CloseNFC();
				CloseNFCData();
			}
			else
			{
				text = NFCError.NFC_MSG_PORT_EMPTY;
			}
			Console.WriteLine("DisconnectTag : [" + text + "]");
			return text;
		}

		/// <summary>
		/// Get tag ID
		/// </summary>
		/// <returns>ID/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetTagID();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string GetTagID()
		{
			if (api == null)
			{
				OpenNFC();
			}
			string text = "";
			if (_TagReady)
			{
				byte[] tagID = api.getTagID();
				if (tagID == null)
				{
					text = NFCError.NFC_MSG_TAG_COMMAND_ERROR;
				}
				else
				{
					byte[] array = tagID;
					foreach (byte b in array)
					{
						text += $"{b:X2}";
					}
				}
			}
			else
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			Console.WriteLine("GetTagID : [" + text + "]");
			return text;
		}

		/// <summary>
		/// Get tag version
		/// </summary>
		/// <returns>Version/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetVersion();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string GetVersion()
		{
			if (api == null)
			{
				OpenNFC();
			}
			string text = "";
			text = ((!_TagReady) ? NFCError.NFC_MSG_TAG_NOREADY : api.GetVersion());
			Console.WriteLine("GetVersion : [" + text + "]");
			return text;
		}

		/// <summary>
		/// Get tag platform name
		/// </summary>
		/// <returns>platform name/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetPlatformName();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string GetPlatformName()
		{
			if (api == null)
			{
				OpenNFC();
			}
			string text = "";
			text = ((!_TagReady) ? NFCError.NFC_MSG_TAG_NOREADY : api.GetPlatformName());
			Console.WriteLine("GetPlatformName : [" + text + "]");
			return text;
		}

		/// <summary>
		/// Get tag serial number
		/// </summary>
		/// <returns>Serial number/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetSN();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string GetSN()
		{
			if (api == null)
			{
				OpenNFC();
			}
			string text = "";
			text = ((!_TagReady) ? NFCError.NFC_MSG_TAG_NOREADY : api.GetSN());
			Console.WriteLine("GetSN : [" + text + "]");
			return text;
		}

		/// <summary>
		/// Set a new ping code of tag, rember to ulock first
		/// </summary>
		/// <param name="strData"> new ping code</param>
		/// <returns><see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.SetPingCode("0000");
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string SetPingCode(string strData)
		{
			if (api == null)
			{
				OpenNFC();
			}
			string text = "";
			if (_TagReady)
			{
				byte[] pinCode = Hex2Byte(strData);
				text = ((!api.SetPinCode(pinCode)) ? NFCError.NFC_MSG_TAG_COMMAND_ERROR : NFCError.NFC_MSG_SUCCESS);
			}
			else
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			Console.WriteLine("SetPingCode : [" + text + "]");
			return text;
		}

		/// <summary>
		/// Get ping code status
		/// </summary>
		/// <returns>Port string/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetPinCodeStatus();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string GetPinCodeStatus()
		{
			if (api == null)
			{
				OpenNFC();
			}
			string text = "";
			if (_TagReady)
			{
				int pinCodeStatus = api.GetPinCodeStatus();
				int num = pinCodeStatus & 0xF;
				string arg;
				switch (pinCodeStatus >> 4)
				{
				case 0:
					arg = "UNLOCKED";
					break;
				case 1:
					arg = "LOCKED";
					break;
				case 2:
					arg = "BLOCKED";
					break;
				default:
					arg = "??";
					break;
				}
				text = $"{arg} ({num})";
			}
			else
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			Console.WriteLine("GetPinCodeStatus : [" + text + "]");
			return text;
		}

		/// <summary>
		/// Unlock tag with ping code
		/// </summary>
		/// <param name="strData">ping code</param>
		/// <returns><see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.UnlockPinCode("0000");
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string UnlockPinCode(string strData)
		{
			if (api == null)
			{
				OpenNFC();
			}
			string text = "";
			if (_TagReady)
			{
				byte[] data = Hex2Byte(strData);
				text = ((!api.UnlockPinCode(data)) ? NFCError.NFC_MSG_TAG_COMMAND_ERROR : NFCError.NFC_MSG_SUCCESS);
			}
			else
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			Console.WriteLine("UnlockPinCode : [" + text + "]");
			return text;
		}

		/// <summary>
		/// Send a image to tag and refresh
		/// </summary>
		/// <param name="oImage">image object</param>
		/// <returns><see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.DrawImage(oImage);
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string DrawImage(Bitmap oImage)
		{
			if (api == null)
			{
				OpenNFC();
			}
			checkSize(out int width, out int height, out int page);
			Bitmap bitmap = resizeImage(oImage, new Size(width, height));
			string text = "";
			if (_TagReady)
			{
				EinkImage image = newEinkImage(width, height, page, bitmap);
				api.DrawImage(image, DrawImageMethod.DIMethod_Normal, this);
			}
			else
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			Console.WriteLine("DrawImage : [" + text + "]");
			return text;
		}

		/// <summary>
		/// Send a image to tag and refresh
		/// </summary>
		/// <param name="oImage">image object</param>
		/// <param name="bDithering">dithering</param>
		/// <returns><see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.DrawImage(oImage, bDithering);
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string DrawImage(Bitmap oImage, bool bDithering)
		{
			if (api == null)
			{
				OpenNFC();
			}
			checkSize(out int width, out int height, out int page);
			Bitmap bitmap = resizeImage(oImage, new Size(width, height));
			string text = "";
			if (_TagReady)
			{
				if (bDithering)
				{
					Console.WriteLine("Dithering");
					Bitmap bitmap2 = Dithering(bitmap);
					EinkImage image = newEinkImage(width, height, page, bitmap2);
					api.DrawImage(image, DrawImageMethod.DIMethod_Normal, this);
				}
				else
				{
					Console.WriteLine("No Dithering");
					EinkImage image2 = newEinkImage(width, height, page, bitmap);
					api.DrawImage(image2, DrawImageMethod.DIMethod_Normal, this);
				}
			}
			else
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			Console.WriteLine("DrawImage : [" + text + "]");
			return text;
		}

		/// <summary>
		/// Send a image to tag and refresh and return display image status
		/// </summary>
		/// <param name="oImage">image object</param>
		/// <returns>OK/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = await oNFC.DrawImageAsync(oImage);
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public async Task<string> DrawImageAsync(Bitmap oImage)
		{
			if (api == null)
			{
				OpenNFC();
			}
			checkSize(out int iwidth, out int iheight, out int ipage);
			Bitmap dImage = resizeImage(oImage, new Size(iwidth, iheight));
			string strResult = "";
			if (_TagReady)
			{
				EinkImage tImage = newEinkImage(iwidth, iheight, ipage, dImage);
				api.DrawImage(tImage, DrawImageMethod.DIMethod_Normal, this);
			}
			else
			{
				strResult = NFCError.NFC_MSG_TAG_NOREADY;
			}
			if (strResult.Equals(""))
			{
				strResult = await checkDrawImageState();
			}
			Console.WriteLine("DrawImageAsync strResult : [" + strResult + "]");
			return strResult;
		}

		/// <summary>
		/// Send a image to tag and refresh and return display image status
		/// </summary>
		/// <param name="oImage">image object</param>
		/// <param name="bDithering">dithering</param>
		/// <returns>OK/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.SystemRest();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public async Task<string> DrawImageAsync(Bitmap oImage, bool bDithering)
		{
			if (api == null)
			{
				OpenNFC();
			}
			checkSize(out int iwidth, out int iheight, out int ipage);
			Bitmap rImage = resizeImage(oImage, new Size(iwidth, iheight));
			string strResult = "";
			if (_TagReady)
			{
				if (bDithering)
				{
					Console.WriteLine("Dithering");
					EinkImage tImage2 = newEinkImage(bitmap: Dithering(rImage), width: iwidth, height: iheight, pages: ipage);
					api.DrawImage(tImage2, DrawImageMethod.DIMethod_Normal, this);
				}
				else
				{
					Console.WriteLine("No Dithering");
					EinkImage tImage = newEinkImage(iwidth, iheight, ipage, rImage);
					api.DrawImage(tImage, DrawImageMethod.DIMethod_Normal, this);
				}
			}
			else
			{
				strResult = NFCError.NFC_MSG_TAG_NOREADY;
			}
			if (strResult.Equals(""))
			{
				strResult = await checkDrawImageState();
			}
			Console.WriteLine("DrawImageAsync strResult : [" + strResult + "]");
			return strResult;
		}

		/// <summary>
		/// reset Pin code to default (0000)
		/// </summary>
		/// <returns>OK/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = await oNFC.DrawImageAsync(oImage, bDithering);
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string SystemRest()
		{
			byte[] array = api.SystemRest();
			string str = "";
			if (array != null)
			{
				if (array.Length != 7)
				{
					str = "Not support error";
					return NFCError.NFC_MSG_TAG_FW_NO_SUPPORT;
				}
				for (int i = 2; i < 6; i++)
				{
					str += $"{array[i]:X2}";
				}
				return NFCError.NFC_MSG_SUCCESS;
			}
			str = "??";
			Console.WriteLine("Crc : [" + str + "]");
			return NFCError.NFC_MSG_TAG_COMMAND_ERROR;
		}

		/// <summary>
		/// FW Upgrade (0000)
		/// </summary>
		/// <returns>OK/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = await oNFC.DrawImageAsync(oImage, bDithering);
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string FWUpgrade(string FileName, int type)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Expected O, but got Unknown
			FileStream fileStream = null;
			BinaryReader binaryReader = null;
			try
			{
				fileStream = new FileStream(FileName, FileMode.Open);
				Console.WriteLine(fileStream);
				Console.WriteLine(fileStream.Length);
				byte[] array = new byte[fileStream.Length];
				binaryReader = new BinaryReader(fileStream);
				FWinfo val = new FWinfo();
				for (int i = 0; i < fileStream.Length; i++)
				{
					array[i] = binaryReader.ReadByte();
				}
				int num = 240;
				byte[] array2 = new byte[6];
				for (int j = 0; j < 4; j++)
				{
					array2[j] = array[fileStream.Length - num];
					num--;
				}
				if (array2[0] != 0 && array2[1] != 85 && array2[2] != 83 && array2[3] != 83)
				{
					Console.WriteLine("NFC_MSG_OTA_FORMAT_ILLEGAl");
					return NFCError.NFC_MSG_OTA_FORMAT_ILLEGAl;
				}
				val.Getdata_info(array, type);
				api.bigdata(val, DrawImageMethod.DIMethod_Normal, this);
			}
			catch (Exception value)
			{
				Console.WriteLine(value);
			}
			finally
			{
				binaryReader?.Close();
				fileStream?.Close();
			}
			return NFCError.NFC_MSG_SUCCESS;
		}

		private void checkSize(out int width, out int height, out int page)
		{
			if (api == null)
			{
				OpenNFC();
			}
			string platformName = api.GetPlatformName();
			string version = api.GetVersion();
			if (platformName.Equals("EPD-210--TC2") || platformName.Equals("D30-ED29-TC2"))
			{
				model = "EPD-210";
				width = Epd29.iwidthedge;
				height = Epd29.iheightedge;
				page = 1;
			}
			else if (platformName.Equals("EPD-302--TC2"))
			{
				model = "EPD-302";
				width = EPD_BW_37.iwidthedge;
				height = EPD_BW_37.iheightedge;
				page = 1;
			}
			else if (platformName.Equals("EPD-303--TC2"))
			{
				model = "EPD-303";
				width = Epd37.iwidthedge;
				height = Epd37.iheightedge;
				page = 2;
			}
			else if (platformName.Equals("EPD-304--TC2"))
			{
				model = "EPD-304";
				width = EPD_BWYR_37.iwidthedge;
				height = EPD_BWYR_37.iheightedge;
				page = 2;
			}
			else
			{
				width = Epd29.iwidthedge;
				height = Epd29.iheightedge;
				page = 1;
			}
		}

		private static async Task<string> checkDrawImageState()
		{
			Task subThreadTask = Task.Run(delegate
			{
				Thread.Sleep(1000);
			});
			await subThreadTask;
			if (mImageState == nImageState.DIState_Finish)
			{
				return NFCError.NFC_MSG_SUCCESS;
			}
			if (mImageState == nImageState.DIState_Error)
			{
				return NFCError.NFC_MSG_TAG_COMMAND_ERROR;
			}
			return await checkDrawImageState();
		}

		/// <summary>
		/// Callback function while tag state changed
		/// </summary>
		public void onTagStateChange(NFCTagState state)
		{
			try
			{
				_TagReady = (state == NFCTagState.NFC_TAG_STATE_COMM_ON);
				chkTagConnected = _TagReady;
				_tagState.onTagState((nTagState)state);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		/// <summary>
		/// Callback function while refresh image
		/// </summary>
		public void onProgress(DrawImageState state, object data)
		{
			try
			{
				mImageState = (nImageState)state;
				_processState.onProcessState((nImageState)state, data);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		/// <summary>
		/// Callback function while Big Data for OTA
		/// </summary>
		public void onProgress(SendDataState state, object data)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Invalid comparison between Unknown and I4
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Expected I4, but got Unknown
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Expected I4, but got Unknown
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Expected I4, but got Unknown
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Expected I4, but got Unknown
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Expected I4, but got Unknown
			if ((int)state != 4 || (int)data == 0)
			{
				log.WriteLog("On OTA onProgress [" + state + "] data [" + data + "]");
			}
			try
			{
				mSendDataState = (nOTAState)state;
				bool flag = false;
				switch (state - 2)
				{
				case 1:
				case 3:
					break;
				case 0:
					_OTAProcessState.onOTAProcessState((nOTAState)state, data);
					break;
				case 2:
					_OTAProcessState.onOTAProcessState((nOTAState)state, data);
					break;
				case 4:
					_OTAProcessState.onOTAProcessState((nOTAState)state, data);
					break;
				case 5:
				case 6:
				case 7:
				case 8:
				case 9:
				case 10:
				case 11:
				case 12:
					_OTAProcessState.onOTAProcessState(nOTAState.SDState_Error, 0);
					break;
				}
			}
			catch (Exception ex)
			{
				_OTAProcessState.onOTAProcessState(nOTAState.SDState_Error, 0);
				log.WriteLog(ex.ToString());
				Console.WriteLine(ex.Message);
			}
		}

		private EinkImage newEinkImage(int width, int height, int pages, Bitmap bitmap)
		{
			string platformName = GetPlatformName();
			if (!platformName.StartsWith("EPD-210") || !isFWSupport("3.0.0"))
			{
				if (!platformName.StartsWith("EPD-303"))
				{
					if (!platformName.Equals("EPD-302--TC2"))
					{
						if (!platformName.Equals("EPD-304--TC2"))
						{
							return new EinkImage(width, height, pages, bitmap);
						}
						return new EinkImage(width, height, pages, bitmap, 1, 5120, platformName);
					}
					return new EinkImage(width, height, pages, bitmap, 1, 5120, platformName);
				}
				return new EinkImage(width, height, pages, bitmap, 1, 5120, platformName);
			}
			return new EinkImage(width, height, pages, bitmap, 1, 1024, platformName);
		}

		/// <summary>
		/// Get user data from tag
		/// </summary>
		/// <returns>Tag data/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetTagData();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string GetTagData()
		{
			string text = "";
			return GetTagDataDo();
		}

		private string GetTagDataDo()
		{
			if (!_bNFCData)
			{
				OpenNFCData();
			}
			string text = "";
			if (_bNFCData)
			{
				string text2 = "";
				string Data = "";
				int num = 0;
				num = NFC_API.RFID_WorkingType(0, 1);
				for (int i = 0; i < 80; i++)
				{
					string text3 = i.ToString().PadLeft(2, '0') + "00";
					int num2 = 0;
					Console.WriteLine("strBlock : [" + text3 + "]");
					while (num2 < nTryCount)
					{
						num = NFC_API.RFID_ST25DVRead("00", "", text3, out Data);
						Console.WriteLine("Data : [" + Data + "] nErr=" + num);
						num2++;
						if (num == 0)
						{
							break;
						}
						Thread.Sleep(100);
					}
					if (num2 >= nTryCount)
					{
						break;
					}
					if (Data.Contains("00"))
					{
						text2 += Data.Substring(0, Data.IndexOf("00"));
						break;
					}
					text2 += Data;
				}
				text = UnHex(text2, "utf-8");
				text = text.Replace("\0", string.Empty);
				if (text == "")
				{
					text = NFCError.NFC_MSG_TAG_COMMAND_ERROR;
				}
			}
			else
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			Console.WriteLine("GetTagData : [" + text + "]");
			return text;
		}

		/// <summary>
		/// Write user data to tag
		/// </summary>
		/// <param name="strData">tag data</param>
		/// <returns>Result/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetTagData(strData);
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string WriteTagData(string strData)
		{
			string text = "";
			return WriteTagDataDo(strData);
		}

		private string WriteTagDataDo(string strData)
		{
			if (!_bNFCData)
			{
				OpenNFCData();
			}
			string text = NFCError.NFC_MSG_SUCCESS;
			int num = 0;
			if (_bNFCData)
			{
				Console.WriteLine("WriteTagDataDo strData : [" + strData + "]");
				string text2 = ToHex(strData, "utf-8", false) + "00";
				int num2 = text2.Length / 8 + 1;
				int num3 = 0;
				num3 = NFC_API.RFID_WorkingType(0, 1);
				for (int i = 0; i < num2; i++)
				{
					string str = i.ToString().PadLeft(2, '0');
					str += "00";
					int num4 = 8;
					if (text2.Length - i * 8 < num4)
					{
						num4 = text2.Length - i * 8;
					}
					string text3 = text2.Substring(i * 8, num4);
					text3 = text3.PadRight(8, '0');
					int num5 = 0;
					while (num5 < nTryCount)
					{
						Console.WriteLine("strBlock : [" + str + "] strTemp : [" + text3 + "] nErr" + num3);
						num3 = NFC_API.RFID_ST25DVWrite("00", "", str, text3);
						num5++;
						if (num3 == 0)
						{
							break;
						}
						num++;
						Thread.Sleep(100);
					}
					if (num5 >= nTryCount)
					{
						text = NFCError.NFC_MSG_TAG_COMMAND_ERROR;
						break;
					}
				}
				Console.WriteLine("Err  retry " + num);
			}
			else
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			Console.WriteLine("WriteTagData : [" + text + "]");
			return text;
		}

		/// <summary>
		/// Get user data from tag to flash <br />
		/// <b>Only supprot upper FW version <span style="color:red;">3.0.0</span></b> 
		/// </summary>
		/// <returns>Tag data/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetTagData();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string GetTagDataFlash()
		{
			string platformName = GetPlatformName();
			if (!platformName.Contains("EPD-210") || isFWSupport("3.0.0"))
			{
				string text = "";
				return GetTagDataFlashDo();
			}
			Constants.FW_SUPPORT = "3.0.0";
			return NFCError.NFC_MSG_TAG_FW_NO_SUPPORT;
		}

		private string GetTagDataFlashDo()
		{
			string text = "";
			if (_TagReady)
			{
				bool flag = false;
				int num = 0;
				DateTime now = DateTime.Now;
				int num2 = now.Second * 1000;
				now = DateTime.Now;
				int num3 = num2 + now.Millisecond;
				while (!flag)
				{
					byte[] array = api.ReadUserData(num);
					if (array == null)
					{
						return NFCError.NFC_MSG_TAG_COMMAND_ERROR;
					}
					if (array[0] == 9)
					{
						return NFCError.NFC_MSG_TAG_NOREADY;
					}
					int num4 = array[1];
					int num5 = 2;
					if (num >= 250)
					{
						flag = true;
					}
					if (array[num5 + num4 - 2] == 13 && array[num5 + num4 - 1] == 10)
					{
						flag = true;
						num4 -= 2;
					}
					else
					{
						num += num4;
					}
					byte[] array2 = new byte[num4];
					Array.Copy(array, 2, array2, 0, num4);
					text += Encoding.UTF8.GetString(array2);
					Thread.Sleep(100);
				}
				now = DateTime.Now;
				int num6 = now.Second * 1000;
				now = DateTime.Now;
				int num7 = num6 + now.Millisecond;
			}
			Thread.Sleep(200);
			return text;
		}

		/// <summary>
		/// Write user data to tag to flash <br />
		/// <b>Only supprot upper FW version <span style="color:red;">3.0.0</span></b> 
		/// </summary>
		/// <param name="strData">tag data</param>
		/// <returns>Result/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetTagData(strData);
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public string WriteTagDataFlash(string strData)
		{
			string platformName = GetPlatformName();
			if (!platformName.Contains("EPD-210") || isFWSupport("3.0.0"))
			{
				string text = "";
				return WriteTagDataFlashDo(strData);
			}
			Constants.FW_SUPPORT = "3.0.0";
			return NFCError.NFC_MSG_TAG_FW_NO_SUPPORT;
		}

		private string WriteTagDataFlashDo(string strData)
		{
			string text = "";
			if (_TagReady)
			{
				int num = 0;
				string text2 = ToHex(strData, "utf-8", false);
				byte[] bytes = Encoding.ASCII.GetBytes(strData);
				if (bytes.Length > 250)
				{
					return NFCError.NFC_MSG_DATA_LENGTH_TOOLARGE;
				}
				byte[] array = new byte[bytes.Length + 2];
				Array.Copy(bytes, 0, array, 0, bytes.Length);
				array[bytes.Length] = 13;
				array[bytes.Length + 1] = 10;
				text = api.WriteUserData(array);
			}
			Thread.Sleep(200);
			if ("Write Image Fail".Equals(text))
			{
				text = NFCError.NFC_MSG_TAG_COMMAND_ERROR;
			}
			else if ("NFC Card Off".Equals(text))
			{
				text = NFCError.NFC_MSG_TAG_NOREADY;
			}
			else if ("Success".Equals(text))
			{
				text = NFCError.NFC_MSG_SUCCESS;
			}
			return text;
		}

		public string getFWData()
		{
			string FirmwareVer = "";
			if (!_bNFCData)
			{
				OpenNFCData();
			}
			int num = 0;
			if (_bNFCData)
			{
				NFC_API.RFID_FWVersion(out FirmwareVer);
			}
			Console.WriteLine(FirmwareVer);
			return FirmwareVer;
		}

		private void OpenNFC()
		{
			_bNFC = true;
			if (_bNFCData)
			{
				CloseNFCData();
			}
			if (comPort != "")
			{
				nfc = new D30Command(comPort);
				if (nfc.openNFC())
				{
					manager = NFCManager.getInstance();
					manager.TagChange = this;
					manager.setNFCCommand(nfc);
					api = manager.getNfcAPI();
				}
			}
		}

		private void CloseNFC()
		{
			if (api != null)
			{
				api = null;
				manager.setNFCCommand(null);
				nfc.closeNFC();
				nfc = null;
			}
			_bNFC = false;
			_TagReady = false;
		}

		public void CloseAntenna()
		{
			manager.setNFCCommand(null);
			nfc.closeNFC();
			nfc = null;
		}

		public void OpenAntenna()
		{
			Console.WriteLine("comPort : " + comPort);
			nfc = new D30Command(comPort);
			if (nfc.openNFC())
			{
				manager.setNFCCommand(nfc);
				api = manager.getNfcAPI();
			}
			else
			{
				nfc = null;
			}
		}

		public void WriteLog(string msg)
		{
			log.WriteLog(msg);
		}

		private void OpenNFCData()
		{
			_bNFCData = true;
			if (_bNFC)
			{
				CloseNFC();
			}
			if (comPort != "")
			{
				int num = 0;
				num = NFC_API.RFID_OpenReader(comPort);
			}
		}

		private void CloseNFCData()
		{
			NFC_API.RFID_CloseReader(comPort);
			_bNFCData = false;
		}

		private static byte[] Hex2Byte(string asciiString)
		{
			byte[] array = new byte[asciiString.Length];
			for (int i = 0; i < asciiString.Length; i++)
			{
				array[i] = (byte)asciiString[i];
			}
			return array;
		}

		public static string ToHex(string s, string charset, bool fenge)
		{
			if (s.Length % 2 != 0)
			{
				s += " ";
			}
			Encoding encoding = Encoding.GetEncoding(charset);
			byte[] bytes = encoding.GetBytes(s);
			string text = "";
			for (int i = 0; i < bytes.Length; i++)
			{
				text += $"{bytes[i]:X}";
				if (fenge && i != bytes.Length - 1)
				{
					text += string.Format("{0}", ",");
				}
			}
			return text.ToLower();
		}

		public static string UnHex(string hex, string charset)
		{
			if (hex == null)
			{
				throw new ArgumentNullException("hex");
			}
			hex = hex.Replace(",", "");
			hex = hex.Replace("\n", "");
			hex = hex.Replace("\\", "");
			hex = hex.Replace(" ", "");
			if (hex.Length % 2 != 0)
			{
				hex += "0";
			}
			byte[] array = new byte[hex.Length / 2];
			for (int i = 0; i < array.Length; i++)
			{
				try
				{
					array[i] = byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber);
				}
				catch
				{
					throw new ArgumentException("hex is not a valid hex number!", "hex");
				}
			}
			Encoding encoding = Encoding.GetEncoding(charset);
			return encoding.GetString(array);
		}

		/// <summary>
		/// Get user data from tag
		/// </summary>
		/// <returns>Tag data/<see cref="T:AdvNFCWrap.NFCError" /></returns>
		/// <example>
		/// <code>
		/// String strReturn = oNFC.GetTagData();
		/// Console.WriteLine(new NFCError(strResult).Content);
		/// </code>
		/// </example>
		public Bitmap DoDithering(Bitmap oImage, Size size)
		{
			return DoDithering(oImage, size, false);
		}

		public Bitmap DoDithering(Bitmap oImage, Size size, bool reduce)
		{
			if (api == null)
			{
				OpenNFC();
			}
			checkSize(out int _, out int _, out int _);
			ImageGenerator imageGenerator = new ImageGenerator(model, size);
			return imageGenerator.dithering(oImage);
		}

		private Bitmap Dithering(Bitmap oImage)
		{
			if (model == null || (!model.Contains("EPD-302") && !model.Contains("EPD-303") && !model.Contains("EPD-304")))
			{
				Bitmap bitmap = myCopy(oImage);
				bitmap.Save("dithered_not.png");
				Size size = bitmap.Size;
				myColor[] pixelsFrom32BitArgbImage = GetPixelsFrom32BitArgbImage(bitmap);
				for (int i = 0; i < size.Height; i++)
				{
					for (int j = 0; j < size.Width; j++)
					{
						int num = i * size.Width + j;
						myColor myColor = pixelsFrom32BitArgbImage[num];
						FloydSteinbergDithering(pixelsFrom32BitArgbImage, myColor, pixelsFrom32BitArgbImage[num] = TransformPixel(myColor), j, i, size.Width, size.Height);
					}
				}
				Bitmap bitmap2 = ToBitmap(pixelsFrom32BitArgbImage, size);
				Console.WriteLine("Size Width: " + size.Width + ";Height:" + size.Height);
				bitmap2.Save("dithered_image.png");
				return bitmap2;
			}
			ImageGenerator imageGenerator = new ImageGenerator(model);
			bool reduce = false;
			return imageGenerator.dithering(oImage, reduce);
		}

		public static Bitmap resizeImage(Bitmap imgToResize, Size size)
		{
			if (imgToResize.Width < imgToResize.Height)
			{
				imgToResize.RotateFlip(RotateFlipType.Rotate270FlipNone);
			}
			return new Bitmap(imgToResize, size);
		}

		public static Bitmap myCopy(Image image)
		{
			Size size = image.Size;
			int width = size.Width;
			size = image.Size;
			Bitmap bitmap = new Bitmap(width, size.Height, PixelFormat.Format32bppArgb);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.Clear(Color.Transparent);
				graphics.PageUnit = GraphicsUnit.Pixel;
				graphics.DrawImage(image, new Rectangle(Point.Empty, image.Size));
			}
			return bitmap;
		}

		public unsafe static myColor[] GetPixelsFrom32BitArgbImage(Bitmap bitmap)
		{
			if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
			{
				throw new ArgumentException("The bitmap must be in 32bpp ARGB format.");
			}
			int width = bitmap.Width;
			int height = bitmap.Height;
			myColor[] array = new myColor[width * height];
			BitmapData bitmapData = null;
			try
			{
				bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
				byte* ptr = (byte*)(void*)bitmapData.Scan0;
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++)
					{
						int num = (i * width + j) * 4;
						myColor myColor = default(myColor);
						myColor.B = ptr[num];
						myColor.G = ptr[num + 1];
						myColor.R = ptr[num + 2];
						myColor.A = ptr[num + 3];
						myColor myColor2 = array[i * width + j] = myColor;
					}
				}
			}
			catch (Exception innerException)
			{
				throw new ApplicationException("Failed to extract pixel data from bitmap.", innerException);
			}
			finally
			{
				if (bitmapData != null)
				{
					bitmap.UnlockBits(bitmapData);
				}
			}
			return array;
		}

		private static myColor TransformPixel(myColor pixel)
		{
			byte b = (byte)(0.299 * (double)(int)pixel.R + 0.587 * (double)(int)pixel.G + 0.114 * (double)(int)pixel.B);
			return (pixel.R > 200 && pixel.G < 50 && pixel.B < 50) ? new myColor(byte.MaxValue, byte.MaxValue, 0, 0) : ((b < 127) ? new myColor(byte.MaxValue, 0, 0, 0) : new myColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		}

		private unsafe static Bitmap ToBitmap(myColor[] data, Size size)
		{
			Bitmap bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
			int width = bitmap.Width;
			int height = bitmap.Height;
			BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			myColor* ptr = (myColor*)(void*)bitmapData.Scan0;
			for (int i = 0; i < size.Height; i++)
			{
				for (int j = 0; j < size.Width; j++)
				{
					int num = i * size.Width + j;
					myColor myColor = *ptr = data[num];
					ptr++;
				}
			}
			bitmap.UnlockBits(bitmapData);
			return bitmap;
		}

		private static byte myToByte(int value)
		{
			if (value < 0)
			{
				value = 0;
			}
			else if (value > 255)
			{
				value = 255;
			}
			return (byte)value;
		}

		private static void FloydSteinbergDithering(myColor[] original, myColor originalPixel, myColor transformedPixel, int x, int y, int width, int height)
		{
			int num = y * width + x;
			int num2 = originalPixel.R - transformedPixel.R;
			int num3 = originalPixel.G - transformedPixel.G;
			int num4 = originalPixel.B - transformedPixel.B;
			myColor myColor;
			if (x + 1 < width)
			{
				int num5 = num + 1;
				myColor = original[num5];
				myColor.R = myToByte(myColor.R + (num2 * 7 >> 4));
				myColor.G = myToByte(myColor.G + (num4 * 7 >> 4));
				myColor.B = myToByte(myColor.B + (num3 * 7 >> 4));
				original[num5] = myColor;
			}
			if (y + 1 < height)
			{
				int num5;
				if (x - 1 > 0)
				{
					num5 = num + width - 1;
					myColor = original[num5];
					myColor.R = myToByte(myColor.R + (num2 * 3 >> 4));
					myColor.G = myToByte(myColor.G + (num4 * 3 >> 4));
					myColor.B = myToByte(myColor.B + (num3 * 3 >> 4));
					original[num5] = myColor;
				}
				num5 = num + width;
				myColor = original[num5];
				myColor.R = myToByte(myColor.R + (num2 * 5 >> 4));
				myColor.G = myToByte(myColor.G + (num4 * 5 >> 4));
				myColor.B = myToByte(myColor.B + (num3 * 5 >> 4));
				original[num5] = myColor;
				if (x + 1 < width)
				{
					num5 = num + width + 1;
					myColor = original[num5];
					myColor.R = myToByte(myColor.R + (num2 >> 4));
					myColor.G = myToByte(myColor.G + (num4 >> 4));
					myColor.B = myToByte(myColor.B + (num3 >> 4));
					original[num5] = myColor;
				}
			}
		}

		private bool isFWSupport(string v)
		{
			int num = VersionComparer.CompareVersions(GetVersion(), v);
			if (num < 0)
			{
				return false;
			}
			return true;
		}
	}
}
namespace AdvNFCWrap.model
{
	internal sealed class EPDModel
	{
		private static volatile EPDModel instance = null;

		private static object syncObj = new object();

		private Dictionary<string, Dictionary<string, object>> config = new Dictionary<string, Dictionary<string, object>>();

		public static string COLOR_BW = "BW";

		public static string COLOR_RBW = "RBW";

		public static string COLOR_GRAY = "GRAY";

		public static string COLOR_FOUR = "FOUR";

		public static string COLOR_FULL = "FULL";

		public static string COLOR_SEVEN = "SEVEN";

		public static EPDModel Instance
		{
			get
			{
				if (instance == null)
				{
					lock (syncObj)
					{
						if (instance == null)
						{
							instance = new EPDModel();
						}
					}
				}
				return instance;
			}
		}

		private EPDModel()
		{
			Dictionary<string, object> value = new Dictionary<string, object>
			{
				{
					"width",
					296
				},
				{
					"height",
					128
				},
				{
					"color",
					COLOR_BW
				},
				{
					"max_page",
					1
				},
				{
					"image_reverse",
					true
				}
			};
			config.Add("EPD-210", value);
			value = new Dictionary<string, object>
			{
				{
					"width",
					416
				},
				{
					"height",
					240
				},
				{
					"color",
					COLOR_BW
				},
				{
					"max_page",
					1
				},
				{
					"image_reverse",
					true
				}
			};
			config.Add("EPD-302", value);
			value = new Dictionary<string, object>
			{
				{
					"width",
					416
				},
				{
					"height",
					240
				},
				{
					"color",
					COLOR_RBW
				},
				{
					"max_page",
					1
				},
				{
					"image_reverse",
					true
				}
			};
			config.Add("EPD-303", value);
			value = new Dictionary<string, object>
			{
				{
					"width",
					416
				},
				{
					"height",
					240
				},
				{
					"color",
					COLOR_FOUR
				},
				{
					"max_page",
					1
				},
				{
					"image_reverse",
					true
				}
			};
			config.Add("EPD-304", value);
		}

		public Dictionary<string, object> getProperty(string model)
		{
			Dictionary<string, object> value = null;
			config.TryGetValue(model, out value);
			return value;
		}
	}
}
