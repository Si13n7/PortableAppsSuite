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

    internal static void SetControlLang(Control obj)
    {
        try
        {
            foreach (Control child in obj.Controls)
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

    internal static string GetText(string lang, Control obj)
    {
        try
        {
            ResourceManager ResManager;
            string text = null;
            switch (lang)
            {
                case "de-DE":
                case "en-US":
                    ResManager = new ResourceManager($"{ResourcesNamespace}.LangResources.{lang}", Assembly.Load(Assembly.GetEntryAssembly().GetName().Name));
                    text = ResManager.GetString(obj.Name);
                    break;
                default:
                    try
                    {
                        if (XmlLang != lang)
                        {
                            XmlLang = lang;
                            XmlData.Load(Path.GetFullPath(Path.Combine(Application.StartupPath, ResourcesNamespace == "AppsLauncher" ? $"Langs\\{lang}.xml" : $"..\\Langs\\{lang}.xml")));
                        }
                        text = XmlData.DocumentElement.SelectSingleNode($"{XmlKey}{obj.Name}").InnerText;
                        text = text.Replace("\\r", string.Empty).Replace("\\n", Environment.NewLine); // Allow '\n' as string for line breaks
                    }
                    catch
                    {
                        text = GetText("en-US", obj);
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
        return obj.Text;
    }

    internal static string GetText(string lang, string objName) =>
        GetText(lang, new Control() { Name = objName });

    internal static string GetText(Control obj)
    {
        string lang = SilDev.Ini.ReadString("Settings", "Lang", SystemUI);
        if (!string.IsNullOrWhiteSpace(lang) && lang != CurrentLang)
            CurrentLang = lang;
        return GetText(CurrentLang, obj);
    }

    internal static string GetText(string objName) =>
        GetText(new Control() { Name = objName });
}