using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using SilDev;

internal static class Lang
{
    internal static string ResourcesNamespace { get; set; }

    internal static string SystemUi => CultureInfo.InstalledUICulture.Name;

    internal static string CurrentLang { get; set; } = SystemUi;

    private static string _configLang;

    internal static string ConfigLang
    {
        get => _configLang ?? (_configLang = Ini.Read<string>("Settings", "Lang", SystemUi));
        set => _configLang = value;
    }

    internal static void SetControlLang(Control control)
    {
        if (control == null)
            return;
        if (!string.IsNullOrWhiteSpace(control.Text))
        {
            var s = GetText(control);
            if (!s.EqualsEx(control.Name))
                control.Text = s;
        }
        foreach (Control child in control.Controls)
            SetControlLang(child);
    }

    internal static string GetText(string lang, string key)
    {
        try
        {
            string s;
            switch (lang)
            {
                case "de-DE":
                case "en-US":
                    var rm = new ResourceManager(ResourcesNamespace + ".LangResources." + lang, Assembly.Load(Assembly.GetEntryAssembly().GetName().Name));
                    s = rm.GetString(key);
                    break;
                default:
                    s = GetText("en-US", key);
                    break;
            }
            if (!string.IsNullOrWhiteSpace(s))
                return s;
        }
        catch (Exception ex)
        {
            Log.Write(ex);
        }
        return key;
    }

    internal static string GetText(string lang, Control control) =>
        GetText(lang, control.Name);

    internal static string GetText(string key)
    {
        if (!CurrentLang.Equals(ConfigLang))
            CurrentLang = ConfigLang;
        return GetText(CurrentLang, key);
    }

    internal static string GetText(Control control)
    {
        if (!CurrentLang.Equals(ConfigLang))
            CurrentLang = ConfigLang;
        return GetText(CurrentLang, control.Name);
    }
}
