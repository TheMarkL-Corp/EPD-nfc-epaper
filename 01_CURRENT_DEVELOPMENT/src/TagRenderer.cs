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
        public const int HEADER_HEIGHT = 42;

        public static Bitmap RenderTag(string line1, string line2, bool showBorder, TagStyle style)
        {
            Bitmap bmp = new Bitmap(TAG_WIDTH, TAG_HEIGHT, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // 1. Base White Background
                g.Clear(Color.White);

                // 2. Draw Style A Black Header Banner if selected
                if (style == TagStyle.StyleA_BlackHeader)
                {
                    if (showBorder)
                    {
                        // In border mode, clip the black header within the top of the rounded rect
                        int borderMargin = 4;
                        int cornerRadius = 14;
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
                        // Edge-to-edge black header banner
                        g.FillRectangle(Brushes.Black, 0, 0, TAG_WIDTH, HEADER_HEIGHT);
                    }
                }

                // 3. Draw Rounded Border if enabled
                if (showBorder)
                {
                    int borderMargin = 4;
                    int cornerRadius = 14;
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

                int leftPadding = showBorder ? 18 : 12;
                int rightPadding = showBorder ? 18 : 12;
                int maxContentWidth = TAG_WIDTH - leftPadding - rightPadding;

                // 4. Draw Line 2 (Subtitle at Upper-Left)
                if (!string.IsNullOrEmpty(line2) && !string.IsNullOrWhiteSpace(line2))
                {
                    string text2 = line2.Trim();
                    float line2FontSize = FindOptimalFontSize(g, text2, "Segoe UI", FontStyle.Regular, 19f, 10f, maxContentWidth);
                    Color subtitleColor = (style == TagStyle.StyleA_BlackHeader) ? Color.White : Color.Black;

                    using (Font font2 = new Font("Segoe UI", line2FontSize, FontStyle.Regular))
                    using (SolidBrush brush2 = new SolidBrush(subtitleColor))
                    {
                        StringFormat format2 = new StringFormat
                        {
                            Alignment = StringAlignment.Near,
                            LineAlignment = StringAlignment.Center,
                            Trimming = StringTrimming.EllipsisCharacter,
                            FormatFlags = StringFormatFlags.NoWrap
                        };

                        float yPos2 = showBorder ? 7f : 4f;
                        RectangleF layoutRect2 = new RectangleF(leftPadding, yPos2, maxContentWidth, HEADER_HEIGHT - 6);
                        g.DrawString(text2, font2, brush2, layoutRect2, format2);
                    }
                }

                // 5. Draw Line 1 (Main Title at Lower Part with Larger Font)
                if (!string.IsNullOrEmpty(line1) && !string.IsNullOrWhiteSpace(line1))
                {
                    string text1 = line1.Trim();
                    float line1FontSize = FindOptimalFontSize(g, text1, "Segoe UI", FontStyle.Regular, 28f, 12f, maxContentWidth);
                    using (Font font1 = new Font("Segoe UI", line1FontSize, FontStyle.Regular))
                    using (SolidBrush brush1 = new SolidBrush(Color.Black))
                    {
                        StringFormat format1 = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center,
                            Trimming = StringTrimming.EllipsisCharacter,
                            FormatFlags = StringFormatFlags.NoWrap
                        };

                        float yPos1 = HEADER_HEIGHT + 6;
                        float height1 = TAG_HEIGHT - yPos1 - (showBorder ? 8 : 4);
                        RectangleF layoutRect1 = new RectangleF(leftPadding, yPos1, maxContentWidth, height1);
                        g.DrawString(text1, font1, brush1, layoutRect1, format1);
                    }
                }
            }
            return bmp;
        }

        private static float FindOptimalFontSize(Graphics g, string text, string fontFamily, FontStyle style, float maxSize, float minSize, float targetWidth)
        {
            for (float size = maxSize; size >= minSize; size -= 0.5f)
            {
                using (Font testFont = new Font(fontFamily, size, style))
                {
                    SizeF measured = g.MeasureString(text, testFont);
                    if (measured.Width <= targetWidth)
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
