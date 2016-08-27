
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.Convert"/>.cs</para>
    /// <para><see cref="SilDev.Crypt"/>.cs</para>
    /// <para><see cref="SilDev.Log"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class Drawing
    {
        [SuppressUnmanagedCodeSecurity]
        internal static class SafeNativeMethods
        {
            [DllImport("shell32.dll", BestFitMapping = false, SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Ansi)]
            internal extern static int ExtractIconEx([MarshalAs(UnmanagedType.LPStr)]string libName, int iconIndex, IntPtr[] largeIcon, IntPtr[] smallIcon, int nIcons);

            [DllImport("user32.dll", SetLastError = true)]
            internal static extern bool DestroyIcon(IntPtr hIcon);
        }

        public static Color ColorFromHtml(string code, Color defaultColor) =>
            code.StartsWith("#") && code.Length == 7 ? ColorTranslator.FromHtml(code) : defaultColor;

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

        private static Dictionary<object, Image> OriginalImages = new Dictionary<object, Image>();
        public static Image ImageGrayScaleSwitch(object key, Image image)
        {
            try
            {
                if (!OriginalImages.ContainsKey(key))
                    OriginalImages.Add(key, image);
                return OriginalImages[key] == image ? ImageToGrayScale(image) : OriginalImages[key];
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

        public static Icon IconResourceFromFile(string path, int index = 0, bool large = false)
        {
            try
            {
                IntPtr[] ptrs = new IntPtr[1];
                SafeNativeMethods.ExtractIconEx(path, index, large ? ptrs : new IntPtr[1], !large ? ptrs : new IntPtr[1], 1);
                IntPtr ptr = ptrs[0];
                if (ptr == IntPtr.Zero)
                    throw new ArgumentNullException();
                return Icon.FromHandle(ptr);
            }
            catch
            {
                return null;
            }
        }

        public static Image IconResourceFromFileAsImage(string path, int index = 0, bool large = false)
        {
            try
            {
                Icon ico = IconResourceFromFile(path, index, large);
                return new Bitmap(ico.ToBitmap(), ico.Width, ico.Height);
            }
            catch
            {
                return null;
            }
        }

        public enum SystemIconKey : uint
        {
            Asterisk = 76,
            Camera = 41,
            CD = 56,
            CD_R = 56,
            CD_ROM = 57,
            CD_RW = 58,
            Chip = 29,
            CommandPrompt = 256,
            Computer = 104,
            Defrag = 106,
            Desktop = 105,
            DiscDrive = 25,
            DVD = 51,
            DVD_R = 33,
            DVD_RAM = 34,
            DVD_ROM = 35,
            DVD_RW = 36,
            DVDDrive = 32,
            DynamicLinkLibrary = 62,
            Error = 93,
            Executable = 11,
            Explorer = 203,
            Favorite = 204,
            FloppyDrive = 23,
            Folder = 3,
            Games = 10,
            HardDrive = 30,
            Help = 94,
            HelpShield = 99,
            Network = 170,
            OneDrive = 220,
            Pin = 228,
            Printer = 46,
            Question = 94,
            RecycleBinEmpty = 50,
            RecycleBinFull = 49,
            Run = 95,
            Screensaver = 96,
            Search = 13,
            Security = 54,
            Sharing = 83,
            Shortcut = 154,
            Stop = 207,
            SystemControl = 22,
            SystemDrive = 31,
            TaskManager = 144,
            Undo = 249,
            Unpin = 227,
            User = 208,
            UserAccountControl = 73,
            Warning = 79
        }

        public static Icon SystemIcon(SystemIconKey key, bool large = false)
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "imageres.dll");
                Icon ico = IconResourceFromFile(path, (int)key);
                return ico;
            }
            catch
            {
                return null;
            }
        }

        public static Image SystemIconAsImage(SystemIconKey key, bool large = false)
        {
            try
            {
                Icon ico = SystemIcon(key, large);
                Image img = new Bitmap(ico.ToBitmap(), ico.Width, ico.Height);
                return img;
            }
            catch
            {
                return null;
            }
        }

        public static void ContextMenuStrip_SetFixedSingle(ContextMenuStrip sender, PaintEventArgs e, Color? borderColor = null)
        {
            try
            {
                using (GraphicsPath gp = new GraphicsPath())
                {
                    sender.Region = new Region(new RectangleF(2, 2, sender.Width - 4, sender.Height - 4));
                    gp.AddRectangle(new RectangleF(2, 2, sender.Width - 5, sender.Height - 5));
                    using (Brush b = new SolidBrush(sender.BackColor))
                        e.Graphics.FillPath(b, gp);
                    using (Pen p = new Pen(borderColor == null ? SystemColors.ControlDark : (Color)borderColor, 1))
                        e.Graphics.DrawPath(p, gp);
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }
    }
}

#endregion
