
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <para><see cref="SilDev.TASKBAR"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public class NOTIFYBOX
    {
        public NOTIFYBOX() { }

        private double opacity = .95d;
        public double Opacity
        {
            get { return opacity; }
            set { opacity = value < .2d ? .2d : value > 1d ? 1d : value; }
        }

        public Color BackColor { get; set; } = SystemColors.Menu;

        public Color BorderColor { get; set; } = SystemColors.MenuHighlight;

        public Color CaptionColor { get; set; } = SystemColors.MenuHighlight;

        public Color TextColor { get; set; } = SystemColors.MenuText;

        private NotifyForm NotifyWindow { get; set; }

        private System.Threading.Thread NotifyThread { get; set; }

        public enum NotifyBoxStartPosition
        {
            Center,
            CenterLeft,
            CenterRight,
            BottomLeft,
            BottomRight,
            TopLeft,
            TopRight
        }

        public enum NotifyBoxSound
        {
            Asterisk,
            Warning,
            Notify,
            Question,
            None
        }

        private sealed class NotifyForm : Form
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

            public NotifyForm(string text, string title, NotifyBoxStartPosition position, ushort duration, bool borders, double opacity, Color backColor, Color borderColor, Color captionColor, Color textColor)
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
                    ForeColor = captionColor,
                    Location = new Point(3, 3),
                    Text = title
                };
                Controls.Add(TitleLabel);

                TextLabel = new Label()
                {
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Font = new Font("Tahoma", 8.25f, FontStyle.Regular),
                    ForeColor = textColor,
                    Location = new Point(8, 24),
                    Text = text
                };
                Controls.Add(TextLabel);

                if (borders)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Controls.Add(new Panel()
                        {
                            AutoSize = false,
                            BackColor = borderColor,
                            Dock = i == 0 ? DockStyle.Top : i == 1 ? DockStyle.Right : i == 2 ? DockStyle.Bottom : DockStyle.Left,
                            Location = new Point(0, 0),
                            Size = new Size(1, 1)
                        });
                    }
                }

                AutoScaleDimensions = new SizeF(6f, 13f);
                AutoScaleMode = AutoScaleMode.Font;
                BackColor = backColor;
                ClientSize = new Size(48, 44);
                Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
                ForeColor = textColor;
                FormBorderStyle = FormBorderStyle.None;
                Opacity = opacity;
                ShowIcon = false;
                ShowInTaskbar = false;
                Size = new Size((TitleLabel.Size.Width < TextLabel.Size.Width ? TextLabel.Size.Width : TitleLabel.Size.Width) + 12, TitleLabel.Size.Height + TextLabel.Size.Height + 12);

                StartPosition = position == NotifyBoxStartPosition.Center ? FormStartPosition.CenterScreen : FormStartPosition.Manual;
                if (StartPosition == FormStartPosition.Manual)
                {
                    TASKBAR.Location TaskBarLocation = TASKBAR.GetLocation();
                    int TaskBarSize = TASKBAR.GetSize();
                    switch (position)
                    {
                        case NotifyBoxStartPosition.CenterLeft:
                        case NotifyBoxStartPosition.TopLeft:
                        case NotifyBoxStartPosition.BottomLeft:
                            Location = new Point(TaskBarLocation == TASKBAR.Location.LEFT ? TaskBarSize + 3 : 3, Location.Y);
                            break;
                        case NotifyBoxStartPosition.CenterRight:
                        case NotifyBoxStartPosition.TopRight:
                        case NotifyBoxStartPosition.BottomRight:
                            Location = new Point(Screen.PrimaryScreen.Bounds.Width - Width - (TaskBarLocation == TASKBAR.Location.RIGHT ? TaskBarSize + 3 : 3), Location.Y);
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
                            Location = new Point(Location.X, Screen.PrimaryScreen.Bounds.Height - Height - (TaskBarLocation == TASKBAR.Location.BOTTOM ? TaskBarSize + 3 : 3));
                            break;
                        case NotifyBoxStartPosition.TopLeft:
                        case NotifyBoxStartPosition.TopRight:
                            Location = new Point(Location.X, TaskBarLocation == TASKBAR.Location.TOP ? TaskBarSize + 3 : 3);
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

        public void Show(string text, string caption, NotifyBoxStartPosition position = NotifyBoxStartPosition.BottomRight, NotifyBoxSound sound = NotifyBoxSound.None, ushort duration = 0, bool borders = true)
        {
            try
            {
                if (IsAlive)
                    throw new NotSupportedException("Multiple calls are not supported.");
                NotifyWindow = new NotifyForm(text, caption, position, duration, borders, opacity, BackColor, BorderColor, CaptionColor, TextColor);
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
                LOG.Debug(ex);
            }
        }

        public void Show(string text, string caption, NotifyBoxStartPosition position, NotifyBoxSound sound, bool borders) =>
           Show(text, caption, position, sound, 0, borders);

        public void Show(string text, string caption, NotifyBoxStartPosition position, ushort duration, bool borders = true) =>
           Show(text, caption, position, NotifyBoxSound.None, duration, borders);

        public void Show(string text, string caption, NotifyBoxStartPosition position, bool borders) =>
           Show(text, caption, position, NotifyBoxSound.None, 0, borders);

        public void Show(string text, string caption, NotifyBoxSound sound, ushort duration = 0, bool borders = true) =>
           Show(text, caption, NotifyBoxStartPosition.BottomRight, sound, duration, borders);

        public void Show(string text, string caption, NotifyBoxSound sound, bool borders) =>
           Show(text, caption, NotifyBoxStartPosition.BottomRight, sound, 0, borders);

        public void Show(string text, string caption, ushort duration, bool borders = true) =>
           Show(text, caption, NotifyBoxStartPosition.BottomRight, NotifyBoxSound.None, duration, borders);

        public bool IsAlive
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

        public void Close()
        {
            try
            {
                if (NotifyWindow != null)
                    NotifyWindow.Close();
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }

        public void Abort()
        {
            Close();
            try
            {
                if (IsAlive)
                    NotifyThread.Abort();
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }
    }
}

#endregion
