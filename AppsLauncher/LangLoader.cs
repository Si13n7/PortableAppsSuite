using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using System.Xml;

internal static class Lang
{
    internal static string ResourcesNamespace { get; set; }

    internal readonly static string SystemUI = CultureInfo.InstalledUICulture.Name;
    internal static string CurrentLang = CultureInfo.InstalledUICulture.Name;

    private static XmlDocument XmlData = new XmlDocument();
    private static string XmlLang = null;
    private static string xmlKey = null;
    private static string XmlKey
    {
        get
        {
            if (!string.IsNullOrEmpty(ResourcesNamespace) && string.IsNullOrEmpty(xmlKey))
                xmlKey = $"/root/{ResourcesNamespace}/";
            return xmlKey;
        }
    }

    internal static void SetControlLang(Control _obj)
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

    internal static string GetText(string _lang, Control _obj)
    {
        try
        {
            ResourceManager ResManager;
            string text = null;
            switch (_lang)
            {
                case "de-DE":
                case "en-US":
                    ResManager = new ResourceManager($"{ResourcesNamespace}.LangResources.{_lang}", Assembly.Load(Assembly.GetEntryAssembly().GetName().Name));
                    text = ResManager.GetString(_obj.Name);
                    break;
                default:
                    try
                    {
                        if (XmlLang != _lang)
                        {
                            XmlLang = _lang;
                            XmlData.Load(Path.GetFullPath(Path.Combine(Application.StartupPath, ResourcesNamespace == "AppsLauncher" ? $"Langs\\{_lang}.xml" : $"..\\Langs\\{_lang}.xml")));
                        }
                        text = XmlData.DocumentElement.SelectSingleNode($"{XmlKey}{_obj.Name}").InnerText;
                        text = text.Replace("\\r", string.Empty).Replace("\\n", Environment.NewLine); // Allow '\n' as string for line breaks
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

    internal static string GetText(string _lang, string _objName) =>
        GetText(_lang, new Control() { Name = _objName });

    internal static string GetText(Control _obj)
    {
        string lang = SilDev.Ini.ReadString("Settings", "Lang", SystemUI);
        if (!string.IsNullOrWhiteSpace(lang) && lang != CurrentLang)
            CurrentLang = lang;
        return GetText(CurrentLang, _obj);
    }

    internal static string GetText(string _objName) =>
        GetText(new Control() { Name = _objName });
}