using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace AppsDownloader
{
    public partial class TipForm : Form
    {
        string title = string.Empty;
        string text = string.Empty;
        int time = 0;
        FormStartPosition startPos = FormStartPosition.Manual;

        public TipForm(params object[] _args)
        {
            InitializeComponent();
            if (_args.Length >= 2)
            {
                if (_args[0] is string)
                    title = (string)_args[0];
                if (_args[1] is string)
                    text = (string)_args[1];
                if (_args.Length >= 3 && _args[2] is int)
                    time = (int)_args[2];
                if (_args.Length >= 4 && _args[3] is FormStartPosition)
                    startPos = (FormStartPosition)_args[3];
                if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(text))
                    return;
            }
            Close();
        }

        private void TipForm_Load(object sender, EventArgs e)
        {
            Title.Text = title;
            InfoMsg.Text = text;
            Size = new Size((Title.Size.Width <= InfoMsg.Size.Width ? InfoMsg.Size.Width : Title.Size.Width) + 12, (Title.Size.Height + InfoMsg.Size.Height) + 12);
            if (startPos == FormStartPosition.CenterParent || startPos == FormStartPosition.CenterScreen)
                Location = new Point((Screen.PrimaryScreen.Bounds.Width / 2) - (Width / 2), (Screen.PrimaryScreen.Bounds.Height / 2) - (Height / 2));
            else
                Location = new Point(Screen.PrimaryScreen.Bounds.Width - Width - 3, Screen.PrimaryScreen.Bounds.Height - Height - 35);
        }

        private void TipForm_Shown(object sender, EventArgs e)
        {
            if (time >= 100)
                bw.RunWorkerAsync();
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(time);
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Close();
        }
    }
}
