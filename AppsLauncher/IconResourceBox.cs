using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AppsLauncher
{
    public partial class IconResourceBox : UserControl
    {
        private static IntPtr[] icons;
        private static string file;

        public static void Init()
        {
            icons = new IntPtr[short.MaxValue];
            SilDev.WinAPI.SafeNativeMethods.ExtractIconEx(file, 0, icons, new IntPtr[short.MaxValue], short.MaxValue);
        }

        public static Icon GetIcon(int _num)
        {
            if (icons == null)
                Init();
            if (_num > icons.Length - 1)
                return null;
            return Icon.FromHandle(icons[_num]);
        }

        public IconResourceBox(string _file, int _num)
        {
            InitializeComponent();
            if (file != null && file != _file)
                icons = null;
            file = _file;
            Set(_num);
        }

        public void Set(int _num)
        {
            Icon myIcon = GetIcon(_num);
            iconSelectBtn.Image = new Bitmap(myIcon.ToBitmap(), myIcon.Width, myIcon.Height);
            iconSelectBtn.Text = _num.ToString();
        }

        private void iconSelectBtn_Click(object sender, EventArgs e)
        {
            ParentForm.Text = File.Exists(file) ? string.Format("{0},{1}", file, iconSelectBtn.Text) : string.Empty;
            ParentForm.Close();
        }
    }
}
