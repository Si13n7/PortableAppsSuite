namespace AppsDownloader
{
    using System;
    using System.Threading;
    using System.Windows.Forms;
    using Windows;
    using Libraries;
    using SilDev;
    using SilDev.Forms;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Settings.Initialize();

            var instanceKey = PathEx.LocalPath.GetHashCode().ToString();
            using (new Mutex(true, instanceKey, out var newInstance))
            {
                var allowInstance = newInstance;
                if (!allowInstance)
                {
                    var instances = ProcessEx.GetInstances(PathEx.LocalPath);
                    var count = 0;
                    foreach (var instance in instances)
                    {
                        if (instance?.GetCommandLine()?.ContainsEx(ActionGuid.UpdateInstance) == true)
                            count++;
                        instance?.Dispose();
                    }
                    allowInstance = count == 1;
                }
                if (!allowInstance)
                    return;

                Language.ResourcesNamespace = typeof(Program).Namespace;
                MessageBoxEx.TopMost = true;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm().Plus());
            }
        }
    }
}
