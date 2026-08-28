using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace AG_EPD_Tag
{
    public enum TagStyle
    {
        StyleA_BlackHeader = 0,
        StyleB_CleanWhite = 1
    }

    public static class TagRenderer
    {
        public const int TAG_WIDTH = 296;
        public const int TAG_HEIGHT = 128;
        public const int HEADER_HEIGHT = 38;

        private const string FONT_NAME = "Segoe UI";

        public static Bitmap RenderTag(string line1, string line2, bool showBorder, TagStyle style)
        {
            Bitmap bmp = new Bitmap(TAG_WIDTH, TAG_HEIGHT, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // 1. Base White Canvas
                g.Clear(Color.White);

                bool hasHeader = !string.IsNullOrEmpty(line2) && !string.IsNullOrWhiteSpace(line2);
                int borderMargin = 4;
                int cornerRadius = 14;

                // 2. Draw Style A Black Header Banner if selected
                if (style == TagStyle.StyleA_BlackHeader)
                {
                    if (showBorder)
                    {
                        Rectangle borderRect = new Rectangle(borderMargin, borderMargin, TAG_WIDTH - (borderMargin * 2), TAG_HEIGHT - (borderMargin * 2));
                        using (GraphicsPath path = CreateRoundedRectanglePath(borderRect, cornerRadius))
                        {
                            g.SetClip(path);
                            g.FillRectangle(Brushes.Black, 0, 0, TAG_WIDTH, HEADER_HEIGHT + borderMargin);
                            g.ResetClip();
                        }
                    }
                    else
                    {
                        g.FillRectangle(Brushes.Black, 0, 0, TAG_WIDTH, HEADER_HEIGHT);
                    }
                }

                // 3. Draw Rounded Border if enabled
                if (showBorder)
                {
                    using (Pen borderPen = new Pen(Color.Black, 3.5f))
                    {
                        borderPen.Alignment = PenAlignment.Inset;
                        Rectangle borderRect = new Rectangle(borderMargin, borderMargin, TAG_WIDTH - (borderMargin * 2), TAG_HEIGHT - (borderMargin * 2));
                        using (GraphicsPath path = CreateRoundedRectanglePath(borderRect, cornerRadius))
                        {
                            g.DrawPath(borderPen, path);
                        }
                    }
                }

                int leftPadding = showBorder ? 16 : 10;
                int rightPadding = showBorder ? 16 : 10;
                int maxContentWidth = TAG_WIDTH - leftPadding - rightPadding;

                // 4. Draw Line 2 (Subtitle at Upper-Left)
                if (hasHeader)
                {
                    string text2 = line2.Trim();
                    float maxHeaderSize = 17f;
                    float minHeaderSize = 9f;
                    float headerBoxHeight = HEADER_HEIGHT - 4;

                    float line2FontSize = FindOptimalFontSize(g, text2, FONT_NAME, FontStyle.Bold, maxHeaderSize, minHeaderSize, maxContentWidth, headerBoxHeight);
                    Color subtitleColor = (style == TagStyle.StyleA_BlackHeader) ? Color.White : Color.Black;

                    using (Font font2 = new Font(FONT_NAME, line2FontSize, FontStyle.Bold))
                    using (SolidBrush brush2 = new SolidBrush(subtitleColor))
                    {
                        StringFormat format2 = new StringFormat
                        {
                            Alignment = StringAlignment.Near,
                            LineAlignment = StringAlignment.Center,
                            Trimming = StringTrimming.EllipsisCharacter,
                            FormatFlags = StringFormatFlags.NoWrap
                        };

                        float yPos2 = showBorder ? 5f : 2f;
                        RectangleF layoutRect2 = new RectangleF(leftPadding, yPos2, maxContentWidth, headerBoxHeight);
                        g.DrawString(text2, font2, brush2, layoutRect2, format2);
                    }
                }

                // 5. Draw Line 1 (Main Body Text with Large Bold Auto-Scaling)
                if (!string.IsNullOrEmpty(line1) && !string.IsNullOrWhiteSpace(line1))
                {
                    string text1 = line1.Trim();
                    float yPos1;
                    float height1;

                    if (style == TagStyle.StyleA_BlackHeader || hasHeader)
                    {
                        // Header banner or subtitle present: Main body in lower section
                        float topMargin = showBorder ? 4f : 2f;
                        float bottomMargin = showBorder ? 10f : 4f;
                        yPos1 = HEADER_HEIGHT + topMargin;
                        height1 = TAG_HEIGHT - yPos1 - bottomMargin;
                    }
                    else
                    {
                        // Clean full-canvas single-line mode
                        yPos1 = showBorder ? 10f : 6f;
                        float bottomMargin = showBorder ? 12f : 6f;
                        height1 = TAG_HEIGHT - yPos1 - bottomMargin;
                    }

                    float maxBodySize = (hasHeader || style == TagStyle.StyleA_BlackHeader) ? 48f : 64f;
                    float minBodySize = 12f;

                    float line1FontSize = FindOptimalFontSize(g, text1, FONT_NAME, FontStyle.Bold, maxBodySize, minBodySize, maxContentWidth, height1);
                    using (Font font1 = new Font(FONT_NAME, line1FontSize, FontStyle.Bold))
                    using (SolidBrush brush1 = new SolidBrush(Color.Black))
                    {
                        StringFormat format1 = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center,
                            Trimming = StringTrimming.EllipsisCharacter,
                            FormatFlags = StringFormatFlags.NoWrap
                        };

                        RectangleF layoutRect1 = new RectangleF(leftPadding, yPos1, maxContentWidth, height1);
                        g.DrawString(text1, font1, brush1, layoutRect1, format1);
                    }
                }
            }
            return bmp;
        }

        private static float FindOptimalFontSize(Graphics g, string text, string fontFamily, FontStyle style, float maxSize, float minSize, float targetWidth, float targetHeight)
        {
            StringFormat sfMeasure = StringFormat.GenericTypographic;
            for (float size = maxSize; size >= minSize; size -= 0.5f)
            {
                using (Font testFont = new Font(fontFamily, size, style))
                {
                    SizeF measured = g.MeasureString(text, testFont, 1000, sfMeasure);
                    if (measured.Width <= targetWidth && measured.Height <= targetHeight)
                    {
                        return size;
                    }
                }
            }
            return minSize;
        }

        private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            Rectangle arc = new Rectangle(rect.X, rect.Y, diameter, diameter);

            // Top-left arc
            path.AddArc(arc, 180, 90);

            // Top-right arc
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom-right arc
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom-left arc
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}
