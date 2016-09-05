
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.Convert"/>.cs</para>
    /// <para><see cref="SilDev.Log"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class Drawing
    {
        public static Color ColorFromHtml(string code, Color defaultColor)
        {
            try
            {
                code = code.ToUpper();
                if (!code.StartsWith("#") || code.Length < 4 || code.Substring(1).Count(c => !("0123456789ABCDEF").Contains(c)) > 0)
                    throw new ArgumentException();
                if (code.Length < 7)
                {
                    char c = code[code.Length - 1];
                    while (code.Length < 7)
                        code += c;
                }
                return ColorTranslator.FromHtml(code);
            }
            catch
            {
                return defaultColor;
            }
        }

        private static Image dimEmpty = null;
        public static Image DimEmpty
        {
            get
            {
                if (dimEmpty == null)
                {
                    dimEmpty = new Bitmap(1, 1);
                    using (Graphics gr = Graphics.FromImage(dimEmpty))
                    {
                        using (Brush b = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
                            gr.FillRectangle(b, 0, 0, 1, 1);
                    }
                }
                return dimEmpty;
            }
        }

        public static Image ImageFilter(Image image, int width, int heigth, SmoothingMode quality = SmoothingMode.HighQuality)
        {
            try
            {
                Bitmap bmp = new Bitmap(width, heigth);
                bmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                using (Graphics gr = Graphics.FromImage(bmp))
                {
                    gr.CompositingMode = CompositingMode.SourceCopy;
                    switch (quality)
                    {
                        case SmoothingMode.AntiAlias:
                            gr.CompositingQuality = CompositingQuality.HighQuality;
                            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            gr.SmoothingMode = SmoothingMode.AntiAlias;
                            break;
                        case SmoothingMode.HighQuality:
                            gr.CompositingQuality = CompositingQuality.HighQuality;
                            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            gr.SmoothingMode = SmoothingMode.HighQuality;
                            break;
                        case SmoothingMode.HighSpeed:
                            gr.CompositingQuality = CompositingQuality.HighSpeed;
                            gr.InterpolationMode = InterpolationMode.NearestNeighbor;
                            gr.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                            gr.SmoothingMode = SmoothingMode.HighSpeed;
                            break;
                    }
                    using (ImageAttributes attr = new ImageAttributes())
                    {
                        attr.SetWrapMode(WrapMode.TileFlipXY);
                        gr.DrawImage(image, new Rectangle(0, 0, width, heigth), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attr);
                    }
                }
                return bmp;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return image;
            }
        }

        public static Image ImageFilter(Image image, SmoothingMode quality = SmoothingMode.HighQuality)
        {
            int[] size = new int[]
            {
                image.Width,
                image.Height
            };
            for (int i = 0; i < size.Length; i++)
            {
                if (size[i] > 2048)
                {
                    int percent = (int)Math.Round(100f / size[i] * 2048);
                    size[i] = (int)(size[i] * (percent / 100f));
                    size[i == 0 ? 1 : 0] = (int)(size[i == 0 ? 1 : 0] * (percent / 100f));
                    break;
                }
            }
            return ImageFilter(image, size[0], size[1], quality);
        }

        public static Image ImageInvertColors(Image image)
        {
            try
            {
                Bitmap bmp = new Bitmap(image.Width, image.Height);
                using (Graphics gr = Graphics.FromImage(bmp))
                {
                    ColorMatrix cm = new ColorMatrix(new float[][]
                    {
                        new float[] { -1,  0,  0,  0,  0 },
                        new float[] {  0, -1,  0,  0,  0 },
                        new float[] {  0,  0, -1,  0,  0 },
                        new float[] {  0,  0,  0,  1,  0 },
                        new float[] {  1,  1,  1,  0,  1 }
                    });
                    using (ImageAttributes attr = new ImageAttributes())
                    {
                        attr.SetColorMatrix(cm);
                        gr.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attr);
                    }
                }
                return bmp;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return image;
            }
        }

        public static Image ImageToGrayScale(Image image)
        {
            try
            {
                Bitmap bmp = new Bitmap(image.Width, image.Height);
                using (Graphics gr = Graphics.FromImage(bmp))
                {
                    ColorMatrix cm = new ColorMatrix(new float[][]
                    {
                        new float[] { 0.30f, 0.30f, 0.30f, 0.00f, 0.00f },
                        new float[] { 0.59f, 0.59f, 0.59f, 0.00f, 0.00f },
                        new float[] { 0.11f, 0.11f, 0.11f, 0.00f, 0.00f },
                        new float[] { 0.00f, 0.00f, 0.00f, 1.00f, 0.00f },
                        new float[] { 0.00f, 0.00f, 0.00f, 0.00f, 1.00f }
                    });
                    using (ImageAttributes attr = new ImageAttributes())
                    {
                        attr.SetColorMatrix(cm);
                        gr.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attr);
                    }
                }
                return bmp;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return image;
            }
        }

        private static Dictionary<object, Image> originalImages = new Dictionary<object, Image>();
        public static Image ImageGrayScaleSwitch(object key, Image image)
        {
            try
            {
                if (!originalImages.ContainsKey(key))
                    originalImages.Add(key, image);
                return originalImages[key] == image ? ImageToGrayScale(image) : originalImages[key];
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return image;
            }
        }

        public static Image ImageReColorPixels(Image image, Color from, Color to)
        {
            try
            {
                Bitmap bmp = (Bitmap)image;
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        Color px = bmp.GetPixel(x, y);
                        if (Color.FromArgb(0, px.R, px.G, px.B) == Color.FromArgb(0, from.R, from.G, from.B))
                            bmp.SetPixel(x, y, Color.FromArgb(px.A, to.R, to.G, to.B));
                    }
                }
                return bmp;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return image;
            }
        }
    }
}

#endregion
