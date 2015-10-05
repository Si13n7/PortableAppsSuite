using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace AppsDownloader
{
    public static class Lang
    {
        public readonly static string SystemUI = CultureInfo.InstalledUICulture.Name;
        public static string CurrentLang = CultureInfo.InstalledUICulture.Name;

        public static void SetControlLang(Control _obj)
        {
            try
            {
                foreach (Control child in _obj.Controls)
                {
                    if (!string.IsNullOrWhiteSpace(child.Text))
                        child.Text = GetText(child);
                    SetControlLang(child);
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }

        public static string GetText(string _lang, Control _obj)
        {
            ResourceManager ResManager;
            string text = null;
            switch (_lang)
            {
                case "de-DE":
                    ResManager = new ResourceManager(string.Format("AppsDownloader.LangResources.{0}", _lang), Assembly.Load(Assembly.GetEntryAssembly().GetName().Name));
                    text = ResManager.GetString(_obj.Name);
                    break;
                default:
                    ResManager = new ResourceManager("AppsDownloader.LangResources.en-US", Assembly.Load(Assembly.GetEntryAssembly().GetName().Name));
                    text = ResManager.GetString(_obj.Name);
                    break;
            }
            if (!string.IsNullOrEmpty(text))
                return text;
            return _obj.Text;
        }

        public static string GetText(string _lang, string _objName)
        {
            return GetText(_lang, new Control() { Name = _objName });
        }

        public static string GetText(Control _obj)
        {
            string lang = SilDev.Initialization.ReadValue("Settings", "Lang");
            if (!string.IsNullOrWhiteSpace(lang) && lang != CurrentLang)
                CurrentLang = lang;
            return GetText(CurrentLang, _obj);
        }

        public static string GetText(string _objName)
        {
            return GetText(new Control() { Name = _objName });
        }
    }
}