using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using System.Xml;

namespace AppsDownloader
{
    public static class Lang
    {
        public readonly static string SystemUI = CultureInfo.InstalledUICulture.Name;
        public static string CurrentLang = CultureInfo.InstalledUICulture.Name;

        private static XmlDocument XmlData = new XmlDocument();
        private static string XmlLang = null;
        private static string xmlKey = null;
        private static string XmlKey
        {
            get
            {
                if (string.IsNullOrEmpty(xmlKey))
                    xmlKey = $"/root/{Process.GetCurrentProcess().ProcessName.Replace("64", string.Empty)}/";
                return xmlKey;
            }
        }

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
            try
            {
                ResourceManager ResManager;
                string text = null;
                switch (_lang)
                {
                    case "de-DE":
                    case "en-US":
                        ResManager = new ResourceManager($"AppsDownloader.LangResources.{_lang}", Assembly.Load(Assembly.GetEntryAssembly().GetName().Name));
                        text = ResManager.GetString(_obj.Name);
                        break;
                    default:
                        try
                        {
                            if (XmlLang != _lang)
                            {
                                XmlLang = _lang;
                                XmlData.Load(Path.GetFullPath(Path.Combine(Application.StartupPath, $"..\\Langs\\{_lang}.xml")));
                            }
                            text = XmlData.DocumentElement.SelectSingleNode($"{XmlKey}{_obj.Name}").InnerText.Replace("\\r", string.Empty).Replace("\\n", Environment.NewLine);
                        }
                        catch
                        {
                            text = GetText("en-US", _obj);
                        }
                        break;
                }
                if (!string.IsNullOrEmpty(text))
                    return text;
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            return _obj.Text;
        }

        public static string GetText(string _lang, string _objName) =>
            GetText(_lang, new Control() { Name = _objName });

        public static string GetText(Control _obj)
        {
            string lang = SilDev.Initialization.ReadValue("Settings", "Lang");
            if (!string.IsNullOrWhiteSpace(lang) && lang != CurrentLang)
                CurrentLang = lang;
            return GetText(CurrentLang, _obj);
        }

        public static string GetText(string _objName) =>
            GetText(new Control() { Name = _objName });
    }
}