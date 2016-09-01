using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AppsLauncher
{
    public partial class IconResourceBox : UserControl
    {
        private static IntPtr[] _icons;
        private static string _file;

        public IconResourceBox(string file, int index)
        {
            InitializeComponent();

            BackColor = Main.Colors.Control;
            ForeColor = Main.Colors.ControlText;

            iconSelectBtn.BackColor = BackColor;
            iconSelectBtn.ForeColor = ForeColor;

            if (_file != null && _file != file)
                _icons = null;
            _file = file;

            Set(index);
        }
        private void Set(int index)
        {
            Icon myIcon = GetIcon(index);
            iconSelectBtn.Image = new Bitmap(myIcon.ToBitmap(), myIcon.Width, myIcon.Height);
            iconSelectBtn.Text = index.ToString();
        }

        private static Icon GetIcon(int index)
        {
            if (_icons == null)
                Init();
            if (index > _icons.Length - 1)
                return null;
            return Icon.FromHandle(_icons[index]);
        }

        private static void Init()
        {
            _icons = new IntPtr[short.MaxValue];
            SilDev.Resource.SafeNativeMethods.ExtractIconEx(_file, 0, _icons, new IntPtr[short.MaxValue], short.MaxValue);
        }

        private void iconSelectBtn_Click(object sender, EventArgs e)
        {
            ParentForm.Text = File.Exists(_file) ? string.Format("{0},{1}", _file, iconSelectBtn.Text) : string.Empty;
            ParentForm.Close();
        }
    }
}
