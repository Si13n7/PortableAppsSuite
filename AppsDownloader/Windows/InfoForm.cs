namespace AppsDownloader.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using Libraries;
    using SilDev.Drawing;
    using SilDev.Forms;

    public partial class InfoForm : Form
    {
        private readonly string _infoText;

        public InfoForm(AppData appData)
        {
            InitializeComponent();
            if (appData == default(AppData))
                return;
            if (CacheData.AppImages.TryGetValue(appData.Key, out var image))
                Icon = image.ToIcon();
            Text = appData.Name;
            _infoText = appData.ToString(true);
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private void InfoForm_Load(object sender, EventArgs e)
        {
            FormEx.Dockable(this);

            if (_infoText == default(string))
                return;

            infoBox.Text = Environment.NewLine;
            infoBox.Text += _infoText;
            infoBox.Text += Environment.NewLine;
            infoBox.Font = new Font("Consolas", 8.25f);

            var colorMap = new Dictionary<Color, string[]>
            {
                {
                    Color.DeepSkyBlue, new[]
                    {
                        "Key:",
                        "Name:",
                        "Description:",
                        "Category:",
                        "Website:",
                        "DisplayVersion:",
                        "PackageVersion:",
                        "DefaultLanguage:",
                        "Languages:",
                        "DownloadCollection:",
                        "DownloadSize:",
                        "InstallSize:",
                        "InstallSize:",
                        "InstallDir:",
                        "Advanced:",
                        "ServerKey:",
                        "Item1:",
                        "Item2:",
                        "0:",
                        "1:",
                        "2:",
                        "3:",
                        "4:",
                        "5:",
                        "6:",
                        "7:",
                        "8:",
                        "9:",
                        "10:",
                        "11:",
                        "12:",
                        "13:",
                        "14:",
                        "15:",
                        "16:",
                        "17:",
                        "18:",
                        "19:",
                        "20:",
                        "21:",
                        "22:",
                        "23:",
                        "24:",
                        "25:",
                        "26:",
                        "27:",
                        "28:",
                        "29:",
                        "30:",
                        "31:",
                        "32:",
                        "33:",
                        "34:",
                        "35:",
                        "36:",
                        "37:",
                        "38:",
                        "39:",
                        "40:",
                        "41:",
                        "42:",
                        "43:",
                        "44:",
                        "45:",
                        "46:",
                        "47:",
                        "48:",
                        "49:"
                    }
                },
                {
                    Color.IndianRed, new[]
                    {
                        "{", "}",
                        ": ",
                        ":\r",
                        ":\n",
                        " '",
                        "'",
                        ","
                    }
                }
            };

            foreach (var color in colorMap)
                foreach (var s in color.Value)
                    infoBox.MarkText(s, color.Key);
        }
    }
}
