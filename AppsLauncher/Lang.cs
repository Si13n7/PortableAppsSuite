using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using SilDev;

internal static class Lang
{
    internal static string ResourcesNamespace { get; set; }
    internal static readonly string SystemUi = CultureInfo.InstalledUICulture.Name;
    internal static string CurrentLang = SystemUi;

    internal static void SetControlLang(Control control)
    {
        if (control == null)
            return;
        if (!string.IsNullOrWhiteSpace(control.Text))
            control.Text = GetText(control);
        foreach (Control child in control.Controls)
            SetControlLang(child);
    }

    private static string GetText(string lang, Control control)
    {
        try
        {
            string text;
            switch (lang)
            {
                case "de-DE":
                case "en-US":
                    var resManager = new ResourceManager(ResourcesNamespace + ".LangResources." + lang, Assembly.Load(Assembly.GetEntryAssembly().GetName().Name));
                    text = resManager.GetString(control.Name);
                    break;
                default:
                    text = GetText("en-US", control);
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

    internal static string GetText(string lang, string objName)
    {
        string s;
        using (var c = new Control { Name = objName })
            s = GetText(lang, c);
        return s;
    }

    internal static string GetText(Control control)
    {
        var lang = Ini.ReadString("Settings", "Lang", SystemUi);
        if (!string.IsNullOrWhiteSpace(lang) && !lang.EqualsEx(CurrentLang))
            CurrentLang = lang;
        return GetText(CurrentLang, control);
    }

    internal static string GetText(string objName)
    {
        string s;
        using (var c = new Control { Name = objName })
            s = GetText(c);
        return s;
    }
}
