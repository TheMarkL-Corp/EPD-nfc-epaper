using System;
using System.Drawing;

namespace AdvNFCWrap
{
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
}
