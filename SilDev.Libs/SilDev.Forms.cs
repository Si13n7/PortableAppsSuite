
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace SilDev.Forms
{
    #region BUTTON

    /// <summary>Requirements:
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.DRAWING"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class BUTTON
    {
        public static void Split(this Button button, Color? buttonText = null)
        {
            try
            {
                if (button.FlatStyle != FlatStyle.Flat)
                {
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.MouseOverBackColor = SystemColors.Highlight;
                }
                button.Image = new Bitmap(12, button.Height);
                button.ImageAlign = ContentAlignment.MiddleRight;
                using (Graphics gr = Graphics.FromImage(button.Image))
                {
                    Pen pen = new Pen(buttonText == null ? SystemColors.ControlText : (Color)buttonText, 1);
                    gr.DrawLine(pen, 0, 0, 0, button.Image.Height - 3);
                    Size size = new Size(button.Image.Width - 6, button.Image.Height - 12);
                    gr.DrawLine(pen, size.Width, size.Height, size.Width + 5, size.Height);
                    gr.DrawLine(pen, size.Width + 1, size.Height + 1, size.Width + 4, size.Height + 1);
                    gr.DrawLine(pen, size.Width + 2, size.Height + 2, size.Width + 3, size.Height + 2);
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }

        public static bool Split_ClickEvent(this Button button, ContextMenuStrip contextMenuStrip)
        {
            if (button.PointToClient(Cursor.Position).X >= (button.Width - 16))
            {
                contextMenuStrip.Show(button, new Point(0, button.Height), ToolStripDropDownDirection.BelowRight);
                return true;
            }
            return false;
        }

        public static void Split_MouseMoveEvent(this Button button, Color? backColor = null, Color? hoverColor = null)
        {
            Split_MouseLeaveEvent(button);
            try
            {
                if (backColor == null)
                    backColor = button.BackColor;
                if (hoverColor == null)
                    hoverColor = button.FlatAppearance.MouseOverBackColor;
                if (hoverColor == null)
                    hoverColor = SystemColors.Highlight;
                if (button.PointToClient(Cursor.Position).X >= (button.Width - 16))
                {
                    if (button.BackgroundImage == null)
                    {
                        button.BackgroundImage = new Bitmap(button.Width - 16 - button.FlatAppearance.BorderSize, button.Height);
                        using (Graphics g = Graphics.FromImage(button.BackgroundImage))
                        {
                            using (Brush b = new SolidBrush((Color)backColor))
                                g.FillRectangle(b, 0, 0, button.BackgroundImage.Width, button.BackgroundImage.Height);
                        }
                    }
                }
                else
                {
                    button.BackgroundImage = new Bitmap(button.Width, button.Height);
                    using (Graphics g = Graphics.FromImage(button.BackgroundImage))
                    {
                        using (Brush b = new SolidBrush((Color)backColor))
                            g.FillRectangle(b, 0, 0, button.BackgroundImage.Width, button.BackgroundImage.Height);
                        using (Brush b = new SolidBrush((Color)hoverColor))
                            g.FillRectangle(b, 0, 0, button.BackgroundImage.Width - 15 - button.FlatAppearance.BorderSize, button.BackgroundImage.Height);
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }

        public static void Split_MouseLeaveEvent(this Button button)
        {
            if (button.BackgroundImage != null)
                button.BackgroundImage = null;
        }
    }

    #endregion

    #region CONTROL

    /// <summary>Requirements:
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.DRAWING"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class CONTROL
    {
        public static void DoubleBuffering(this Control control, bool enable = true)
        {
            try
            {
                MethodInfo method = typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
                method.Invoke(control, new object[] { ControlStyles.OptimizedDoubleBuffer, enable });
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }

        public static void DrawSizeGrip(Control control, Color? color = null)
        {
            try
            {
                Image img = CONVERT.FromHexStringToImage(
                "89504e470d0a1a0a0000000d494844520000000c0000000c08" +
                "0600000056755ce70000000467414d410000b18f0bfc610500" +
                "0000097048597300000b1100000b11017f645f910000000774" +
                "494d4507e00908102912b9edb66f0000003d494441542853ad" +
                "8b0b0a00200c4277ff4b5b0c8410b78a121ef8c10070852d3b" +
                "6c2950997574509975dc62cb09a5fedfa1640d54e7df0e47d8" +
                "b2063100852bc6484e7044a50000000049454e44ae426082");
                if (img != null)
                {
                    if (color != Color.White)
                        img = img.RecolorPixels(Color.White, (Color)color);
                    control.BackgroundImage = img;
                    control.BackgroundImageLayout = ImageLayout.Center;
                    control.Size = new Size(12, 12);
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }
    }

    #endregion

    #region CONTEXTMENUSTRIP

    /// <summary>Requirements:
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.DRAWING"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class CONTEXTMENUSTRIP
    {
        public static void SetFixedSingle(this ContextMenuStrip contextMenuStrip, PaintEventArgs paintEventArgs, Color? borderColor = null)
        {
            try
            {
                using (GraphicsPath gp = new GraphicsPath())
                {
                    contextMenuStrip.Region = new Region(new RectangleF(2, 2, contextMenuStrip.Width - 4, contextMenuStrip.Height - 4));
                    gp.AddRectangle(new RectangleF(2, 2, contextMenuStrip.Width - 5, contextMenuStrip.Height - 5));
                    using (Brush b = new SolidBrush(contextMenuStrip.BackColor))
                        paintEventArgs.Graphics.FillPath(b, gp);
                    using (Pen p = new Pen(borderColor == null ? SystemColors.ControlDark : (Color)borderColor, 1))
                        paintEventArgs.Graphics.DrawPath(p, gp);
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }
    }

    #endregion

    #region LINKLABEL

    /// <summary>Requirements:
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.DRAWING"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class LINKLABEL
    {
        public static void LinkText(this LinkLabel linkLabel, string text, Uri uri)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    throw new ArgumentNullException();
                int startIndex = 0;
                int start;
                while ((start = linkLabel.Text.IndexOf(text, startIndex)) > -1)
                {
                    linkLabel.Links.Add(start, text.Length, uri);
                    startIndex = start + text.Length;
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }

        public static void LinkText(this LinkLabel linkLabel, string text, string uri)
        {
            try
            {
                LinkText(linkLabel, text, new Uri(uri));
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }
    }

    #endregion

    #region LISTVIEW

    /// <summary>Requirements:
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.DRAWING"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class LISTVIEW
    {
        public static ListViewItem ItemFromPoint(this ListView listView)
        {
            try
            {
                Point pos = listView.PointToClient(Cursor.Position);
                return listView.GetItemAt(pos.X, pos.Y);
            }
            catch
            {
                return null;
            }
        }

        public class AscendentAlphanumericComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                if (!(x is ListViewItem) || !(y is ListViewItem))
                    return 0;
                string s1 = ((ListViewItem)x).Text;
                string s2 = ((ListViewItem)y).Text;
                return new AscendentAlphanumericStringComparer().Compare(s1, s2);
            }
        }

        public class DescendentAlphanumericComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                if (!(x is ListViewItem) || !(y is ListViewItem))
                    return 0;
                string s1 = ((ListViewItem)x).Text;
                string s2 = ((ListViewItem)y).Text;
                return new DescendentAlphanumericStringComparer().Compare(s1, s2);
            }
        }
    }

    #endregion

    #region PROGRESSBAR

    /// <summary>Requirements:
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.DRAWING"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class PROGRESSBAR
    {
        public static void JumpToEnd(this ProgressBar progressBar)
        {
            try
            {
                int maximum = progressBar.Maximum;
                progressBar.Maximum = int.MaxValue;
                progressBar.Value = progressBar.Maximum;
                progressBar.Value--;
                progressBar.Maximum = maximum;
                progressBar.Value = progressBar.Maximum;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }
    }

    #endregion

    #region RICHTEXTBOX

    /// <summary>Requirements:
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.DRAWING"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class RICHTEXTBOX
    {
        public static void MarkText(this RichTextBox richTextBox, string text, Color foreColor, Color? backColor = null, Font font = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    throw new ArgumentNullException();
                Point selected = new Point(richTextBox.SelectionStart, richTextBox.SelectionLength);
                int startIndex = 0;
                int start;
                while ((start = richTextBox.Text.IndexOf(text, startIndex)) > -1)
                {
                    richTextBox.Select(start, text.Length);
                    richTextBox.SelectionColor = foreColor;
                    if (backColor != null)
                        richTextBox.SelectionBackColor = (Color)backColor;
                    if (font != null)
                        richTextBox.SelectionFont = font;
                    startIndex = start + text.Length;
                }
                richTextBox.SelectionStart = selected.X;
                richTextBox.SelectionLength = selected.Y;
                richTextBox.SelectionBackColor = richTextBox.BackColor;
                richTextBox.SelectionColor = richTextBox.ForeColor;
                richTextBox.SelectionFont = richTextBox.Font;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }
    }

    #endregion

    #region TEXTBOX

    /// <summary>Requirements:
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.DRAWING"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class TEXTBOX
    {
        public static void DrawSearchSymbol(this TextBox textBox, Color? color = null)
        {
            try
            {
                Image img = DRAWING.DefaultSearchSymbol;
                if (img != null)
                {
                    if (color == null)
                        color = textBox.ForeColor;
                    if (color != Color.White)
                        img = img.RecolorPixels(Color.White, (Color)color);

                    Panel panel = new Panel()
                    {
                        Anchor = textBox.Anchor,
                        BackColor = textBox.BackColor,
                        BorderStyle = textBox.BorderStyle,
                        Dock = textBox.Dock,
                        ForeColor = textBox.ForeColor,
                        Location = textBox.Location,
                        Name = $"{textBox.Name}Panel",
                        Parent = textBox.Parent,
                        Size = textBox.Size,
                        TabIndex = textBox.TabIndex
                    };

                    PictureBox pictureBox = new PictureBox()
                    {
                        BackColor = textBox.BackColor,
                        BackgroundImage = img,
                        BackgroundImageLayout = ImageLayout.Center,
                        Cursor = Cursors.IBeam,
                        Dock = DockStyle.Right,
                        ForeColor = textBox.ForeColor,
                        Name = $"{textBox.Name}PictureBox",
                        Size = new Size(16, 16)
                    };

                    pictureBox.Click += (sender, e) => textBox.Select();
                    panel.Controls.Add(pictureBox);

                    textBox.BorderStyle = BorderStyle.None;
                    textBox.Dock = DockStyle.Fill;
                    textBox.Parent = panel;

                    panel.Parent.Update();
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }
    }

    #endregion
}
