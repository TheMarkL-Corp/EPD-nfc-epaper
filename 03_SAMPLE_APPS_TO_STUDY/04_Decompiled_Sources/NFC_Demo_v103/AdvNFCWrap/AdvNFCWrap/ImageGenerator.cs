using AdvNFCWrap.model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace AdvNFCWrap
{
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
}
