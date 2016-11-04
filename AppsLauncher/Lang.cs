using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using System.Xml;
using SilDev;

internal static class Lang
{
    internal static readonly string SystemUi = CultureInfo.InstalledUICulture.Name;
    internal static string CurrentLang = SystemUi;
    private static readonly XmlDocument XmlData = new XmlDocument();
    private static string _xmlLang, _xmlKey;
    internal static string ResourcesNamespace { get; set; }

    private static string XmlKey
    {
        get
        {
            if (!string.IsNullOrEmpty(ResourcesNamespace) && string.IsNullOrEmpty(_xmlKey))
                _xmlKey = $"/root/{ResourcesNamespace}/";
            return _xmlKey;
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
                Log.Write(ex);
            }
            SetControlLang(child);
        }
    }

    internal static string GetText(string lang, Control obj)
    {
        try
        {
            string text;
            switch (lang.ToLower())
            {
                case "de":
                case "de-de":
                case "en":
                case "en-us":
                    var resManager = new ResourceManager($"{ResourcesNamespace}.LangResources.{lang}", Assembly.Load(Assembly.GetEntryAssembly().GetName().Name));
                    text = resManager.GetString(obj.Name);
                    break;
                default:
                    try
                    {
                        if (_xmlLang != lang)
                        {
                            _xmlLang = lang;
                            XmlData.Load(PathEx.Combine(PathEx.LocalDir, ResourcesNamespace == "AppsLauncher" ? $"Langs\\{lang}.xml" : $"..\\Langs\\{lang}.xml"));
                        }
                        text = XmlData?.DocumentElement?.SelectSingleNode(XmlKey + obj.Name)?.InnerText;
                        text = text?.RemoveText("\\r").Replace("\\n", Environment.NewLine); // Allow '\n' as string for line breaks
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
            Log.Write(ex);
        }
        return obj.Text;
    }

    internal static string GetText(string lang, string objName)
    {
        string s;
        using (var c = new Control { Name = objName })
            s = GetText(lang, c);
        return s;
    }

    internal static string GetText(Control obj)
    {
        var lang = Ini.ReadString("Settings", "Lang", SystemUi);
        if (!string.IsNullOrWhiteSpace(lang) && lang != CurrentLang)
            CurrentLang = lang;
        return GetText(CurrentLang, obj);
    }

    internal static string GetText(string objName)
    {
        string s;
        using (var c = new Control { Name = objName })
            s = GetText(c);
        return s;
    }
}
