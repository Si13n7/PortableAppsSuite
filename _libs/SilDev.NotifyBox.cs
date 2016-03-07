
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region Si13n7 Dev. ® created code

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SilDev
{
    public class NotifyBox
    {
        private static NotifyForm NotifyWindow { get; set; }
        private static System.Threading.Thread NotifyThread { get; set; }

        public enum NotifyBoxStartPosition
        {
            Center,
            CenterLeft,
            CenterRight,
            BottomLeft,
            BottomRight,
            TopLeft,
            TopRight,
        }

        public enum NotifyBoxSound
        {
            Asterisk,
            Warning,
            Notify,
            Question,
            None
        }

        [StructLayout(LayoutKind.Sequential)]
        public static class NotifyBoxStyle
        {
            private static double opacity = .95d;
            public static double Opacity
            {
                get { return opacity; }
                set { opacity = value < .2d ? .2d : value > 1d ? 1d : value; }
            }
            public static Color BackgroundColor = SystemColors.Menu;
            public static Color BorderColor = SystemColors.MenuHighlight;
            public static Color CaptionColor = SystemColors.MenuHighlight;
            public static Color MessageColor = SystemColors.MenuText;
        }

        private class NotifyForm : Form
        {
            private IContainer components = null;

            protected override void Dispose(bool disposing)
            {
                if (disposing && components != null)
                    components.Dispose();
                base.Dispose(disposing);
            }

            private BackgroundWorker AsyncWait = new BackgroundWorker();
            private Timer LoadingDots = new Timer();
            private Label TitleLabel, TextLabel;
            private int Duration = 0;

            public NotifyForm(string text, string title, NotifyBoxStartPosition position, int duration, bool borders)
            {
                SuspendLayout();

                AsyncWait.DoWork += new DoWorkEventHandler(AsyncWait_DoWork);
                AsyncWait.RunWorkerCompleted += new RunWorkerCompletedEventHandler(AsyncWait_RunWorkerCompleted);

                LoadingDots.Interval = 256;
                LoadingDots.Tick += new EventHandler(LoadingDots_Tick);

                TitleLabel = new Label()
                {
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Font = new Font("Tahoma", 11.25f, FontStyle.Bold),
                    ForeColor = NotifyBoxStyle.CaptionColor,
                    Location = new Point(3, 3),
                    Text = title
                };
                Controls.Add(TitleLabel);

                TextLabel = new Label()
                {
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Font = new Font("Tahoma", 8.25f, FontStyle.Regular),
                    ForeColor = NotifyBoxStyle.MessageColor,
                    Location = new Point(8, 24),
                    Text = text
                };
                Controls.Add(TextLabel);

                if (borders)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Controls.Add(new Label()
                        {
                            AutoSize = false,
                            BackColor = NotifyBoxStyle.BorderColor,
                            Dock = i == 0 ? DockStyle.Top : i == 1 ? DockStyle.Right : i == 2 ? DockStyle.Bottom : DockStyle.Left,
                            Location = new Point(0, 0),
                            Size = new Size(1, 1)
                        });
                    }
                }

                AutoScaleDimensions = new SizeF(6f, 13f);
                AutoScaleMode = AutoScaleMode.Font;
                BackColor = NotifyBoxStyle.BackgroundColor;
                ClientSize = new Size(48, 44);
                Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
                ForeColor = NotifyBoxStyle.MessageColor;
                FormBorderStyle = FormBorderStyle.None;
                Opacity = NotifyBoxStyle.Opacity;
                ShowIcon = false;
                ShowInTaskbar = false;
                Size = new Size((TitleLabel.Size.Width < TextLabel.Size.Width ? TextLabel.Size.Width : TitleLabel.Size.Width) + 12, TitleLabel.Size.Height + TextLabel.Size.Height + 12);

                StartPosition = position == NotifyBoxStartPosition.Center ? FormStartPosition.CenterScreen : FormStartPosition.Manual;
                if (StartPosition == FormStartPosition.Manual)
                {
                    WinAPI.TaskBar.Location TaskBarLocation = WinAPI.TaskBar.GetLocation();
                    int TaskBarSize = WinAPI.TaskBar.GetSize();
                    switch (position)
                    {
                        case NotifyBoxStartPosition.CenterLeft:
                        case NotifyBoxStartPosition.TopLeft:
                        case NotifyBoxStartPosition.BottomLeft:
                            Location = new Point(TaskBarLocation == WinAPI.TaskBar.Location.LEFT ? TaskBarSize + 3 : 3, Location.Y);
                            break;
                        case NotifyBoxStartPosition.CenterRight:
                        case NotifyBoxStartPosition.TopRight:
                        case NotifyBoxStartPosition.BottomRight:
                            Location = new Point(Screen.PrimaryScreen.Bounds.Width - Width - (TaskBarLocation == WinAPI.TaskBar.Location.RIGHT ? TaskBarSize + 3 : 3), Location.Y);
                            break;
                    }
                    switch (position)
                    {
                        case NotifyBoxStartPosition.CenterLeft:
                        case NotifyBoxStartPosition.CenterRight:
                            Location = new Point(Location.X, (Screen.PrimaryScreen.Bounds.Height / 2) - (Height / 2));
                            break;
                        case NotifyBoxStartPosition.BottomLeft:
                        case NotifyBoxStartPosition.BottomRight:
                            Location = new Point(Location.X, Screen.PrimaryScreen.Bounds.Height - Height - (TaskBarLocation == WinAPI.TaskBar.Location.BOTTOM ? TaskBarSize + 3 : 3));
                            break;
                        case NotifyBoxStartPosition.TopLeft:
                        case NotifyBoxStartPosition.TopRight:
                            Location = new Point(Location.X, TaskBarLocation == WinAPI.TaskBar.Location.TOP ? TaskBarSize + 3 : 3);
                            break;
                    }
                }

                Shown += new EventHandler(NotifyForm_Shown);
                ResumeLayout(false);
                PerformLayout();
                Duration = duration >= 0 ? duration : 0;
            }

            private void NotifyForm_Shown(object sender, EventArgs e)
            {
                if (TextLabel.Text.EndsWith(" . . ."))
                    LoadingDots.Enabled = true;
                if (Duration >= 100)
                    AsyncWait.RunWorkerAsync();
            }

            private void AsyncWait_DoWork(object sender, DoWorkEventArgs e) =>
                System.Threading.Thread.Sleep(Duration);

            private void AsyncWait_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) =>
                Close();

            private void LoadingDots_Tick(object sender, EventArgs e)
            {
                if (TextLabel.Text.EndsWith(" . . ."))
                {
                    TextLabel.Text = TextLabel.Text.Replace(" . . .", " .");
                    while (TextLabel.Text.EndsWith(" . ."))
                        TextLabel.Text = TextLabel.Text.Replace(" . .", " .");
                }
                else
                    TextLabel.Text = TextLabel.Text.EndsWith(" . .") ? TextLabel.Text.Replace(" . .", " . . .") : TextLabel.Text.Replace(" .", " . .");
            }
        }

        public static void Show(string text, string title, NotifyBoxStartPosition position, NotifyBoxSound sound, int duration, bool borders)
        {
            try
            {
                if (IsAlive)
                    throw new NotSupportedException("Multiple calls are not supported.");
                NotifyWindow = new NotifyForm(text, title, position, duration, borders);
                NotifyThread = new System.Threading.Thread(() => NotifyWindow.ShowDialog());
                NotifyThread.Start();
                switch (sound)
                {
                    case NotifyBoxSound.Asterisk:
                        SystemSounds.Asterisk.Play();
                        break;
                    case NotifyBoxSound.Warning:
                        SystemSounds.Hand.Play();
                        break;
                    case NotifyBoxSound.Question:
                        SystemSounds.Question.Play();
                        break;
                    case NotifyBoxSound.Notify:
                        string wavPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Media\\Windows Notify System Generic.wav");
                        if (File.Exists(wavPath))
                            new SoundPlayer(wavPath).Play();
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        public static void Show(string text, string title, NotifyBoxStartPosition position, NotifyBoxSound sound, bool borders) =>
           Show(text, title, position, sound, 0, borders);

        public static void Show(string text, string title, NotifyBoxStartPosition position, NotifyBoxSound sound) =>
           Show(text, title, position, sound, 0, true);

        public static void Show(string text, string title, NotifyBoxStartPosition position, int duration, bool borders) =>
           Show(text, title, position, NotifyBoxSound.None, duration, borders);

        public static void Show(string text, string title, NotifyBoxStartPosition position, int duration) =>
           Show(text, title, position, NotifyBoxSound.None, duration, true);

        public static void Show(string text, string title, NotifyBoxStartPosition position, bool borders) =>
           Show(text, title, position, NotifyBoxSound.None, 0, borders);

        public static void Show(string text, string title, NotifyBoxStartPosition position) =>
           Show(text, title, position, NotifyBoxSound.None, 0, true);

        public static void Show(string text, string title, NotifyBoxSound sound, int duration, bool borders) =>
           Show(text, title, NotifyBoxStartPosition.BottomRight, sound, duration, borders);

        public static void Show(string text, string title, NotifyBoxSound sound, int duration) =>
           Show(text, title, NotifyBoxStartPosition.BottomRight, sound, duration, true);

        public static void Show(string text, string title, NotifyBoxSound sound, bool borders) =>
           Show(text, title, NotifyBoxStartPosition.BottomRight, sound, 0, borders);

        public static void Show(string text, string title, NotifyBoxSound sound) =>
           Show(text, title, NotifyBoxStartPosition.BottomRight, sound, 0, true);

        public static void Show(string text, string title, int duration, bool borders) =>
           Show(text, title, NotifyBoxStartPosition.BottomRight, NotifyBoxSound.None, duration, borders);

        public static void Show(string text, string title, int duration) =>
           Show(text, title, NotifyBoxStartPosition.BottomRight, NotifyBoxSound.None, duration, true);

        public static void Show(string text, string title, bool borders) =>
           Show(text, title, NotifyBoxStartPosition.BottomRight, NotifyBoxSound.None, 0, borders);

        public static void Show(string text, string title) =>
           Show(text, title, NotifyBoxStartPosition.BottomRight, NotifyBoxSound.None, 0, true);

        public static bool IsAlive
        {
            get
            {
                try
                {
                    return NotifyThread.IsAlive;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static void Close()
        {
            try
            {
                if (NotifyWindow != null)
                    NotifyWindow.Close();
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        public static void Abort()
        {
            Close();
            try
            {
                if (IsAlive)
                    NotifyThread.Abort();
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }
    }
}

#endregion
