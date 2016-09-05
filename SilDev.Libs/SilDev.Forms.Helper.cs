
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.Convert"/>.cs</para>
    /// <para><see cref="SilDev.Drawing"/>.cs</para>
    /// <para><see cref="SilDev.Log"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class Forms
    {
        public static class Button
        {
            public static void DrawSplit(System.Windows.Forms.Button button, Color? buttonText = null)
            {
                try
                {
                    if (button.FlatStyle != System.Windows.Forms.FlatStyle.Flat)
                    {
                        button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
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
                    Log.Debug(ex);
                }
            }

            public static bool Split_Click(System.Windows.Forms.Button button, System.Windows.Forms.ContextMenuStrip contextMenuStrip, Point pointToClientMousePosition)
            {
                if (pointToClientMousePosition.X >= (button.Width - 6))
                {
                    contextMenuStrip.Show(button, new Point(0, button.Height), System.Windows.Forms.ToolStripDropDownDirection.BelowRight);
                    return true;
                }
                return false;
            }

            public static void Split_MouseMove(System.Windows.Forms.Button button, Point pointToClientMousePosition, Color? backColor = null, Color? hoverColor = null)
            {
                Split_MouseLeave(button);
                try
                {
                    if (backColor == null)
                        backColor = button.BackColor;
                    if (hoverColor == null)
                        hoverColor = button.FlatAppearance.MouseOverBackColor;
                    if (hoverColor == null)
                        hoverColor = SystemColors.Highlight;
                    if (pointToClientMousePosition.X >= (button.Width - 6))
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
                    Log.Debug(ex);
                }
            }

            public static void Split_MouseLeave(System.Windows.Forms.Button button)
            {
                if (button.BackgroundImage != null)
                    button.BackgroundImage = null;
            }
        }

        public static class ContextMenuStrip
        {
            public static void SetFixedSingle(System.Windows.Forms.ContextMenuStrip contextMenuStrip, System.Windows.Forms.PaintEventArgs paintEventArgs, Color? borderColor = null)
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
                    Log.Debug(ex);
                }
            }
        }

        public static class LinkLabel
        {
            public static void LinkText(System.Windows.Forms.LinkLabel linkLabel, string text, Uri uri)
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
                    Log.Debug(ex);
                }
            }

            public static void LinkText(System.Windows.Forms.LinkLabel linkLabel, string text, string uri)
            {
                try
                {
                    LinkText(linkLabel, text, new Uri(uri));
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                }
            }
        }

        public static class ProgressBar
        {
            public static void JumpToEnd(System.Windows.Forms.ProgressBar progressBar)
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
                    Log.Debug(ex);
                }
            }
        }

        public static class RichTextBox
        {
            public static void MarkText(System.Windows.Forms.RichTextBox richTextBox, string text, Color foreColor, Color? backColor = null, Font font = null)
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
                    Log.Debug(ex);
                }
            }
        }

        public static class TextBox
        {
            public static void DrawSearchSymbol(System.Windows.Forms.TextBox textBox, Color? color = null)
            {
                try
                {
                    string[] sa = new string[]
                    {
                        "89504e470d0a1a0a0000000d494844520000000d0000000d0806",
                        "00000072ebe47c0000000467414d410000b18f0bfc6105000000",
                        "097048597300000b1100000b11017f645f910000000774494d45",
                        "07e0081f120d0c4120f852000000d84944415428537dd1b16ac2",
                        "5018c5f1e82c38e813742e4e7d0d411d1cb58b42a18b4bc15570",
                        "f501dc04272711fa08ae22bab44d870ea520940c2e8282f17fc4",
                        "0fae37d103bf0b39f96e127283388e6fa9618d0dfe30421141da",
                        "b0b4a18cd144079ff846216dc3038ee8399de4f083895b1a3df5",
                        "1719a733fa82c82fa58f85d7990afed36e94b1c3a3d3992142bf",
                        "34213ef074b9cee2154ac31fae628a25beb0c51c2b286fb8fae5",
                        "2deca1bc230f9dd5005d94709eb50d3a0b377aa3dd4bd052c701",
                        "961724065d5a2258740e89219f9619b4f1d9cafbe2e004e0e328",
                        "7fbb6e47ff0000000049454e44ae426082"
                    };
                    string s = string.Join("", sa);
                    Image img = null;
                    byte[] ba = Enumerable.Range(0, s.Length).Where(x => x % 2 == 0).Select(x => System.Convert.ToByte(s.Substring(x, 2), 16)).ToArray();
                    if (ba != null)
                    {
                        using (MemoryStream ms = new MemoryStream(ba))
                            img = Image.FromStream(ms);
                    }
                    if (img != null)
                    {
                        if (color == null)
                            color = textBox.ForeColor;
                        if (color != Color.White)
                            img = Drawing.ImageReColorPixels(img, Color.White, (Color)color);

                        System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel();
                        panel.Anchor = textBox.Anchor;
                        panel.BackColor = textBox.BackColor;
                        panel.BorderStyle = textBox.BorderStyle;
                        panel.Dock = textBox.Dock;
                        panel.ForeColor = textBox.ForeColor;
                        panel.Location = textBox.Location;
                        panel.Name = $"{textBox.Name}Panel";
                        panel.Parent = textBox.Parent;
                        panel.Size = textBox.Size;
                        panel.TabIndex = textBox.TabIndex;

                        System.Windows.Forms.PictureBox pbox = new System.Windows.Forms.PictureBox();
                        pbox.BackColor = textBox.BackColor;
                        pbox.BackgroundImage = img;
                        pbox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
                        pbox.Dock = System.Windows.Forms.DockStyle.Right;
                        pbox.ForeColor = textBox.ForeColor;
                        pbox.Location = new Point(0, 0);
                        pbox.Name = $"{textBox.Name}PictureBox";
                        pbox.Size = new Size(16, 16);
                        panel.Controls.Add(pbox);

                        textBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
                        textBox.Dock = System.Windows.Forms.DockStyle.Fill;
                        textBox.MinimumSize = panel.Size;
                        textBox.Parent = panel;

                        panel.Parent.Update();
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                }
            }
        }
    }
}

#endregion
