using SilDev;
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
    internal static string CurrentLang = SystemUI;

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
        foreach (Control child in obj.Controls)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(child.Text))
                    child.Text = GetText(child);
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
            SetControlLang(child);
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
                            XmlData.Load(PATH.Combine("%CurDir%", ResourcesNamespace == "AppsLauncher" ? $"Langs\\{lang}.xml" : $"..\\Langs\\{lang}.xml"));
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
            LOG.Debug(ex);
        }
        return obj.Text;
    }

    internal static string GetText(string lang, string objName)
    {
        string s;
        using (Control c = new Control() { Name = objName })
            s = GetText(lang, c);
        return s;
    }

    internal static string GetText(Control obj)
    {
        string lang = INI.ReadString("Settings", "Lang", SystemUI);
        if (!string.IsNullOrWhiteSpace(lang) && lang != CurrentLang)
            CurrentLang = lang;
        return GetText(CurrentLang, obj);
    }

    internal static string GetText(string objName)
    {
        string s;
        using (Control c = new Control() { Name = objName })
            s = GetText(c);
        return s;
    }
}