
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.Convert"/>.cs</para>
    /// <para><see cref="SilDev.Log"/>.cs</para>
    /// <para><see cref="SilDev.Run"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class Resource
    {
        [SuppressUnmanagedCodeSecurity]
        internal static class SafeNativeMethods
        {
            [DllImport("shell32.dll", BestFitMapping = false, SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Ansi)]
            internal extern static int ExtractIconEx([MarshalAs(UnmanagedType.LPStr)]string libName, int iconIndex, IntPtr[] largeIcon, IntPtr[] smallIcon, int nIcons);

            [DllImport("user32.dll", SetLastError = true)]
            internal static extern bool DestroyIcon(IntPtr hIcon);
        }

        public static Icon IconFromFile(string path, int index = 0, bool large = false)
        {
            try
            {
                IntPtr[] ptrs = new IntPtr[1];
                SafeNativeMethods.ExtractIconEx(Run.EnvVarFilter(path), index, large ? ptrs : new IntPtr[1], !large ? ptrs : new IntPtr[1], 1);
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

        public static Image IconFromFileAsImage(string path, int index = 0, bool large = false)
        {
            try
            {
                Icon ico = IconFromFile(path, index, large);
                return new Bitmap(ico.ToBitmap(), ico.Width, ico.Height);
            }
            catch
            {
                return null;
            }
        }

        #region ICON BROWSER DIALOG

        public sealed class IconBrowserDialog : Form
        {
            private IContainer components = null;

            protected override void Dispose(bool disposing)
            {
                if (disposing && components != null)
                    components.Dispose();
                base.Dispose(disposing);
            }

            private Panel panel;
            private TextBox textBox;
            private Button button;

            public IconBrowserDialog(string path = "%system%\\imageres.dll", Color? backColor = null, Color? foreColor = null, Color? buttonFace = null, Color? buttonText = null, Color? buttonHighlight = null)
            {
                SuspendLayout();

                string sysIconResPath = path.EndsWith("imageres.dll", StringComparison.OrdinalIgnoreCase) ? path : "%system%\\imageres.dll";
                BackColor = backColor == null ? SystemColors.Control : (Color)backColor;
                ForeColor = foreColor == null ? SystemColors.ControlText : (Color)foreColor;
                Font = new Font("Consolas", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
                Icon = SystemIcon(SystemIconKey.DIRECTORY_SEARCH, true, sysIconResPath);
                MaximizeBox = false;
                MaximumSize = new Size(680, Screen.FromHandle(Handle).WorkingArea.Height);
                MinimizeBox = false;
                MinimumSize = new Size(680, 448);
                Name = "IconBrowserForm";
                Size = MinimumSize;
                SizeGripStyle = SizeGripStyle.Hide;
                StartPosition = FormStartPosition.CenterScreen;
                Text = "Icon Resource Browser";

                TableLayoutPanel tableLayoutPanel = new TableLayoutPanel()
                {
                    BackColor = Color.Transparent,
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                    Dock = DockStyle.Fill,
                    Name = "tableLayoutPanel",
                    RowCount = 2
                };
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
                Controls.Add(tableLayoutPanel);

                panel = new Panel()
                {
                    AutoScroll = true,
                    BackColor = buttonFace == null ? SystemColors.ButtonFace : (Color)buttonFace,
                    BorderStyle = BorderStyle.FixedSingle,
                    ForeColor = buttonText == null ? SystemColors.ControlText : (Color)buttonText,
                    Dock = DockStyle.Fill,
                    Name = "panel",
                    TabIndex = 0,

                };
                panel.Scroll += new ScrollEventHandler((s, e) => ((Panel)s).Update());
                tableLayoutPanel.Controls.Add(panel, 0, 0);

                TableLayoutPanel innerTableLayoutPanel = new TableLayoutPanel()
                {
                    BackColor = Color.Transparent,
                    ColumnCount = 2,
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                    Dock = DockStyle.Fill,
                    Name = "innerTableLayoutPanel",
                };
                innerTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                innerTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 24));
                tableLayoutPanel.Controls.Add(innerTableLayoutPanel, 0, 1);

                textBox = new TextBox()
                {
                    BorderStyle = BorderStyle.FixedSingle,
                    Dock = DockStyle.Top,
                    Font = Font,
                    Name = "textBox",
                    TabIndex = 1
                };
                textBox.TextChanged += new EventHandler(textBox_TextChanged);
                innerTableLayoutPanel.Controls.Add(textBox, 0, 0);

                Panel buttonPanel = new Panel()
                {
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    BackColor = Color.Transparent,
                    BorderStyle = BorderStyle.FixedSingle,
                    Name = "buttonPanel",
                    Size = new Size(20, 20)
                };
                innerTableLayoutPanel.Controls.Add(buttonPanel, 1, 0);

                button = new Button()
                {
                    BackColor = buttonFace == null ? SystemColors.ButtonFace : (Color)buttonFace,
                    BackgroundImage = SystemIconAsImage(SystemIconKey.DIRECTORY, false, sysIconResPath),
                    BackgroundImageLayout = ImageLayout.Zoom,
                    Dock = DockStyle.Fill,
                    FlatStyle = FlatStyle.Flat,
                    Font = Font,
                    ForeColor = buttonText == null ? SystemColors.ControlText : (Color)buttonText,
                    Name = "button",
                    TabIndex = 2,
                    UseVisualStyleBackColor = false
                };
                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.MouseOverBackColor = buttonHighlight == null ? ProfessionalColors.ButtonSelectedHighlight : (Color)buttonHighlight;
                button.Click += new EventHandler(button_Click);
                buttonPanel.Controls.Add(button);

                ResumeLayout(false);
                PerformLayout();

                textBox.Text = path;
                if (File.Exists(textBox.Text))
                    ShowIconResources(textBox.Text);
            }

            private void textBox_TextChanged(object sender, EventArgs e)
            {
                string path = Run.EnvVarFilter(((TextBox)sender).Text);
                if ((path.EndsWith(".ico", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) && File.Exists(path))
                    ShowIconResources(path);
            }

            private void button_Click(object sender, EventArgs e)
            {
                using (OpenFileDialog dialog = new OpenFileDialog() { Multiselect = false, InitialDirectory = Application.StartupPath, RestoreDirectory = false })
                {
                    dialog.ShowDialog(new Form() { ShowIcon = false, TopMost = true });
                    if (!string.IsNullOrWhiteSpace(dialog.FileName))
                        textBox.Text = dialog.FileName;
                }
                if (!panel.Focus())
                    panel.Select();
            }

            private void ShowIconResources(string path)
            {
                try
                {
                    IconBox[] boxes = new IconBox[short.MaxValue];
                    if (panel.Controls.Count > 0)
                        panel.Controls.Clear();
                    for (int i = 0; i < short.MaxValue; i++)
                    {
                        try
                        {
                            boxes[i] = new IconBox(path, i, button.BackColor, button.ForeColor, button.FlatAppearance.MouseOverBackColor);
                            panel.Controls.Add(boxes[i]);
                        }
                        catch
                        {
                            break;
                        }
                    }
                    if (boxes[0] == null)
                        return;
                    int max = panel.Width / boxes[0].Width;
                    int line = 0;
                    int column = 0;
                    for (int i = 0; i < boxes.Length; i++)
                    {
                        if (boxes[i] == null)
                            continue;
                        line = i / max;
                        column = i - line * max;
                        boxes[i].Location = new Point(column * boxes[i].Width, line * boxes[i].Height);
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                }
            }

            #region ICON BOX

            private sealed class IconBox : UserControl
            {
                private IContainer components = null;

                protected override void Dispose(bool disposing)
                {
                    if (disposing && components != null)
                        components.Dispose();
                    base.Dispose(disposing);
                }

                private static IntPtr[] icons;
                private static string file;

                private Button button;

                public IconBox(string path, int index, Color? buttonFace = null, Color? buttonText = null, Color? buttonHighlight = null)
                {
                    SuspendLayout();

                    BackColor = buttonFace == null ? SystemColors.ButtonFace : (Color)buttonFace;
                    ForeColor = buttonText == null ? SystemColors.ControlText : (Color)buttonText;
                    Name = "IconBox";
                    Size = new Size(58, 62);

                    button = new Button()
                    {
                        BackColor = BackColor,
                        FlatStyle = FlatStyle.Flat,
                        ForeColor = ForeColor,
                        ImageAlign = ContentAlignment.TopCenter,
                        Location = new Point(3, 3),
                        Name = "button",
                        Size = new Size(52, 56),
                        TabIndex = 0,
                        TextAlign = ContentAlignment.BottomCenter,
                        UseVisualStyleBackColor = false
                    };
                    button.FlatAppearance.BorderSize = 0;
                    button.FlatAppearance.MouseOverBackColor = buttonHighlight == null ? ProfessionalColors.ButtonSelectedHighlight : (Color)buttonHighlight;
                    button.Click += new EventHandler(button_Click);
                    Controls.Add(button);

                    ResumeLayout(false);

                    if (file != null && file != path)
                        icons = null;
                    file = path;

                    Icon myIcon = GetIcons(index);
                    button.Image = new Bitmap(myIcon.ToBitmap(), myIcon.Width, myIcon.Height);
                    button.Text = index.ToString();
                }

                private static Icon GetIcons(int index)
                {
                    if (icons == null)
                    {
                        icons = new IntPtr[short.MaxValue];
                        SafeNativeMethods.ExtractIconEx(file, 0, icons, new IntPtr[short.MaxValue], short.MaxValue);
                    }
                    if (index > icons.Length - 1)
                        return null;
                    return Icon.FromHandle(icons[index]);
                }

                private void button_Click(object sender, EventArgs e)
                {
                    ParentForm.Text = File.Exists(file) ? $"{file},{button.Text}" : string.Empty;
                    ParentForm.Close();
                }
            }

            #endregion
        }

        #endregion

        #region SYSTEM ICONS

        public enum SystemIconKey : uint
        {
            ASTERISK = 76,
            BARRIER = 81,
            BMP = 66,
            CAM = 41,
            CD = 56,
            CD_R = 57,
            CD_ROM = 58,
            CD_RW = 59,
            CHIP = 29,
            CLIPBOARD = 241,
            CLOSE = 235,
            CMD = 262,
            COMPUTER = 104,
            DEFRAG = 106,
            DESKTOP = 105,
            DIRECTORY = 3,
            DIRECTORY_SEARCH = 13,
            DISC_DRIVE = 25,
            DLL = 62,
            DVD = 51,
            DVD_DRIVE = 32,
            DVD_R = 33,
            DVD_RAM = 34,
            DVD_ROM = 35,
            DVD_RW = 36,
            EJECT = 167,
            ERROR = 93,
            EXE = 11,
            EXPLORER = 203,
            FAVORITE = 204,
            FLOPPY_DRIVE = 23,
            GAMES = 10,
            HARD_DRIVE = 30,
            HELP = 94,
            HELP_SHIELD = 99,
            INF = 64,
            INSTALL = 82,
            JPG = 67,
            KEY = 77,
            NETWORK = 170,
            ONE_DRIVE = 220,
            PLAY = 280,
            PIN = 234,
            PNG = 78,
            PRINTER = 46,
            QUESTION = 94,
            RECYCLE_BIN_EMPTY = 50,
            RECYCLE_BIN_FULL = 49,
            RETRY = 251,
            RUN = 95,
            SCREENSAVER = 96,
            SEARCH = 168,
            SECURITY = 54,
            SHARED_MARKER = 155,
            SHARING = 83,
            SHORTCUT_MARKER = 154,
            STOP = 207,
            SYSTEM_CONTROL = 22,
            SYSTEM_DRIVE = 31,
            TASK_MANAGER = 144,
            UNDO = 255,
            UNKNOWN_DRIVE = 70,
            UNPIN = 233,
            USER = 208,
            USER_DIR = 117,
            UAC = 73,
            WARNING = 79,
            ZIP = 165
        }

        public static Icon SystemIcon(SystemIconKey key, bool large = false, string path = "%system%\\imageres.dll")
        {
            try
            {
                path = Run.EnvVarFilter(path);
                if (!File.Exists(path))
                    path = Run.EnvVarFilter("%system%\\imageres.dll");
                if (!File.Exists(path))
                    throw new FileNotFoundException();
                Icon ico = IconFromFile(path, (int)key, large);
                return ico;
            }
            catch (FileNotFoundException ex)
            {
                Log.Debug(ex);
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static Icon SystemIcon(SystemIconKey key, string path) =>
            SystemIcon(key, false, path);

        public static Image SystemIconAsImage(SystemIconKey key, bool large = false, string path = "%system%\\imageres.dll")
        {
            try
            {
                Icon ico = SystemIcon(key, large, path);
                Image img = new Bitmap(ico.ToBitmap(), ico.Width, ico.Height);
                return img;
            }
            catch
            {
                return null;
            }
        }

        public static Image SystemIconAsImage(SystemIconKey key, string path) =>
            SystemIconAsImage(key, false, path);

        #endregion

        #region EXTRACT DATA

        public static void ExtractConvert(byte[] resData, string destPath, bool reverseBytes = true)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(resData))
                {
                    byte[] data = ms.ToArray();
                    if (reverseBytes)
                        data = data.Reverse().ToArray();
                    using (FileStream fs = new FileStream(destPath, FileMode.CreateNew, FileAccess.Write))
                        fs.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        public static void Extract(byte[] resData, string destPath) =>
            ExtractConvert(resData, destPath, false);

        #endregion

        #region PLAY WAVE

        public static void PlayWave(Stream resData)
        {
            try
            {
                using (Stream audio = resData)
                {
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(audio);
                    player.Play();
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        public static void PlayWaveAsync(Stream resData) =>
            new Thread(() => PlayWave(resData)).Start();

        #endregion
    }
}

#endregion
