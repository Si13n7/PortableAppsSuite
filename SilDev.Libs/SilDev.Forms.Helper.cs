
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Drawing;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.Convert"/>.cs</para>
    /// <para><see cref="SilDev.Crypt"/>.cs</para>
    /// <para><see cref="SilDev.Log"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class Forms
    {
        public static class LinkLabel
        {
            public static void LinkText(System.Windows.Forms.LinkLabel linkLabel, string text, Uri uri)
            {
                try
                {
                    if (text.Length == 0)
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

        public static class RichTextBox
        {
            public static void MarkText(System.Windows.Forms.RichTextBox richTextBox, string text, Color foreColor, Color? backColor = null, Font font = null)
            {
                try
                {
                    if (text.Length == 0)
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
    }
}

#endregion
