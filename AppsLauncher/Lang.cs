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

    internal static void SetControlLang(Control control)
    {
        if (control == null)
            return;
        if (!string.IsNullOrWhiteSpace(control.Text))
            control.Text = GetText(control);
        foreach (Control child in control.Controls)
            SetControlLang(child);
    }

    internal static string GetText(string lang, Control control)
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
                    var resManager = new ResourceManager(ResourcesNamespace + ".LangResources." + lang, Assembly.Load(Assembly.GetEntryAssembly().GetName().Name));
                    text = resManager.GetString(control.Name);
                    break;
                default:
                    try
                    {
                        if (_xmlLang != lang)
                        {
                            _xmlLang = lang;
                            XmlData.Load(PathEx.Combine(PathEx.LocalDir, ResourcesNamespace == "AppsLauncher" ? $"Langs\\{lang}.xml" : $"..\\Langs\\{lang}.xml"));
                        }
                        text = XmlData?.DocumentElement?.SelectSingleNode(XmlKey + control.Name)?.InnerText;
                        text = text?.RemoveText("\\r").Replace("\\n", Environment.NewLine);
                    }
                    catch
                    {
                        text = GetText("en-US", control);
                    }
                    break;
            }
            if (!string.IsNullOrWhiteSpace(text))
                return text;
        }
        catch (Exception ex)
        {
            Log.Write(ex);
        }
        return control.Text;
    }

    internal static string GetText(Control control)
    {
        var lang = Ini.ReadString("Settings", "Lang", SystemUi);
        if (!string.IsNullOrWhiteSpace(lang) && !lang.EqualsEx(CurrentLang))
            CurrentLang = lang;
        return GetText(CurrentLang, control);
    }

    internal static string GetText(string lang, string objName)
    {
        string s;
        using (var c = new Control { Name = objName })
            s = GetText(lang, c);
        return s;
    }

    internal static string GetText(string objName)
    {
        string s;
        using (var c = new Control { Name = objName })
            s = GetText(c);
        return s;
    }
}
