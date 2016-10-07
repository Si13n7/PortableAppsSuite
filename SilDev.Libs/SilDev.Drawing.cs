
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class DRAWING
    {
        public static Color FromHtmlToColor(this string code, Color defaultColor)
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

        public static Color InvertColor(Color color, byte? alpha = null) =>
            Color.FromArgb(alpha ?? color.A, (byte)~color.R, (byte)~color.G, (byte)~color.B);

        public static Image ToImage(this Color color)
        {
            Bitmap img = new Bitmap(1, 1);
            try
            {
                using (Graphics gr = Graphics.FromImage(img))
                {
                    using (Brush b = new SolidBrush(color))
                        gr.FillRectangle(b, 0, 0, 1, 1);
                }
                return img;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return img;
            }
        }

        public static Color ToColor(this Image image, bool disposeImage = true)
        {
            try
            {
                Color c;
                using (Bitmap bmp = (Bitmap)image)
                    c = bmp.GetPixel(0, 0);
                if (disposeImage)
                    image.Dispose();
                return c;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return Color.Empty;
            }
        }

        public static Color InvertColors(this Color color) =>
            color.ToImage().InvertColors().ToColor();

        public static Color ToGrayScale(this Color color) =>
            color.ToImage().ToGrayScale().ToColor();

        private static Image dimEmpty;
        public static Image DimEmpty
        {
            get
            {
                if (dimEmpty == null)
                    dimEmpty = Color.FromArgb(140, 0, 0, 0).ToImage();
                return dimEmpty;
            }
        }

        private static Image defaultSearchSymbol;
        public static Image DefaultSearchSymbol
        {
            get
            {
                if (defaultSearchSymbol == null)
                    defaultSearchSymbol = CONVERT.FromHexStringToImage(
                    "89504e470d0a1a0a0000000d494844520000000d0000000d0806000" +
                    "00072ebe47c0000000467414d410000b18f0bfc6105000000097048" +
                    "597300000b1100000b11017f645f910000000774494d4507e0081f1" +
                    "20d0c4120f852000000d84944415428537dd1b16ac25018c5f1e82c" +
                    "38e813742e4e7d0d411d1cb58b42a18b4bc15570f501dc04272711f" +
                    "a08ae22bab44d870ea520940c2e8282f17fc40fae37d103bf0b39f9" +
                    "6e127283388e6fa9618d0dfe30421141dab0b4a18cd144079ff8462" +
                    "16dc3038ee8399de4f083895b1a3df51719a733fa82c82fa58f85d7" +
                    "990afed36e94b1c3a3d3992142bf34213ef074b9cee2154ac31fae6" +
                    "28a25beb0c51c2b286fb8fae52deca1bc230f9dd5005d94709eb50d" +
                    "3a0b377aa3dd4bd052c701961724065d5a2258740e89219f9619b4f" +
                    "1d9cafbe2e004e0e3287fbb6e47ff0000000049454e44ae426082");
                return defaultSearchSymbol;
            }
        }

        public static Image Redraw(this Image image, int width, int heigth, SmoothingMode quality = SmoothingMode.HighQuality)
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
                LOG.Debug(ex);
                return image;
            }
        }

        public static Image Redraw(this Image image, SmoothingMode quality = SmoothingMode.HighQuality, int maxLength = 1024)
        {
            int[] size = new int[]
            {
                image.Width,
                image.Height
            };
            if (maxLength > 0 && (maxLength < size[0] || maxLength < size[1]))
            {
                for (int i = 0; i < size.Length; i++)
                {
                    if (size[i] > maxLength)
                    {
                        int percent = (int)Math.Round(100f / size[i] * maxLength);
                        size[i] = (int)(size[i] * (percent / 100f));
                        size[i == 0 ? 1 : 0] = (int)(size[i == 0 ? 1 : 0] * (percent / 100f));
                        break;
                    }
                }
            }
            return image.Redraw(size[0], size[1], quality);
        }

        public static Image Redraw(this Image image, int maxLength) =>
            image.Redraw(SmoothingMode.HighQuality, maxLength);

        public static Image InvertColors(this Image image)
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
                LOG.Debug(ex);
                return image;
            }
        }

        public static Image ToGrayScale(this Image image)
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
                LOG.Debug(ex);
                return image;
            }
        }

        private static Dictionary<object, Image> originalImages = new Dictionary<object, Image>();
        public static Image SwitchGrayScale(this Image image, object key)
        {
            try
            {
                if (!originalImages.ContainsKey(key))
                    originalImages.Add(key, image);
                return originalImages[key] == image ? image.ToGrayScale() : originalImages[key];
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return image;
            }
        }

        public static Image RecolorPixels(this Image image, Color from, Color to)
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
                LOG.Debug(ex);
                return image;
            }
        }
    }
}
