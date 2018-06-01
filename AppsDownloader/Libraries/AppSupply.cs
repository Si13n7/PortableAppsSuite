namespace AppsDownloader.Libraries
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using SilDev;

    internal static class AppSupply
    {
        private static Dictionary<Suppliers, List<string>> _mirrors;

        internal static List<string> FindAppInstaller()
        {
            var appInstaller = new List<string>();
            var searchPattern = new[]
            {
                "*.7z",
                "*.rar",
                "*.zip",
                "*.paf.exe"
            };
            appInstaller.AddRange(Settings.CorePaths.AppDirs.SelectMany(x => searchPattern.SelectMany(y => DirectoryEx.EnumerateFiles(x, y))));
            return appInstaller;
        }

        internal static List<string> FindInstalledApps()
        {
            var appDirs = Settings.CorePaths.AppDirs;
            var appNames = new List<string>();
            for (var i = 0; i < appDirs.Length - 1; i++)
            {
                var dirs = DirectoryEx.GetDirectories(appDirs[i]);
                if (i == 0)
                    dirs = dirs.Where(x => !x.StartsWith(".")).ToArray();
                appNames.AddRange(dirs);
            }

            if (Shareware.Enabled)
            {
                var dirs = DirectoryEx.GetDirectories(appDirs.Last());
                if (dirs.Any())
                    appNames.AddRange(dirs);
            }

            appNames = appNames.Where(x => DirectoryEx.EnumerateFiles(x, "*.exe").Any() ||
                                           DirectoryEx.EnumerateFiles(x, $"{Path.GetFileNameWithoutExtension(x)}.ini").Any() &&
                                           DirectoryEx.EnumerateFiles(x, "*.exe", SearchOption.AllDirectories).Any()).ToList();

            if (appNames.Any())
                appNames = appNames.Select(x => x.StartsWithEx(appDirs.Last()) ? $"{Path.GetFileName(x)}###" : Path.GetFileName(x)).ToList();
            foreach (var item in new[]
            {
                "Java",
                "Java64"
            })
            {
                var jrePath = Path.Combine(appDirs.First(), "CommonFiles", item, "bin", "java.exe");
                if (!appNames.ContainsEx(item) && File.Exists(jrePath))
                    appNames.Add(item);
            }

            if (appNames.Any())
                appNames = appNames.Distinct().Where(x => Settings.CacheData.AppInfo.Any(y => y.Key.EqualsEx(x))).ToList();
            return appNames;
        }

        internal static List<string> FindOutdatedApps()
        {
            var outdatedApps = new List<string>();
            foreach (var key in FindInstalledApps())
            {
                var appData = Settings.CacheData.AppInfo.FirstOrDefault(x => x.Key.EqualsEx(key));
                if (appData == default(AppData))
                    continue;

                if (appData.Settings.NoUpdates)
                {
                    if (appData.Settings.NoUpdatesTime == default(DateTime) || Math.Abs((DateTime.Now - appData.Settings.NoUpdatesTime).TotalDays) <= 7d)
                        continue;
                    appData.Settings.NoUpdates = false;
                    appData.Settings.NoUpdatesTime = default(DateTime);
                }

                if (appData.VersionData.Any())
                {
                    if (appData.VersionData
                               .Select(data => new
                               {
                                   data,
                                   path = Path.Combine(appData.InstallDir, data.Item1)
                               })
                               .Where(x => File.Exists(x.path) && !x.data.Item2.EqualsEx(Crypto.EncryptFileToSha256(x.path)))
                               .Select(x => x.data).Any())
                        outdatedApps.Add(appData.Key);
                    continue;
                }

                var appIniDir = Path.Combine(appData.InstallDir, "App", "AppInfo");
                var appIniPath = Path.Combine(appIniDir, "appinfo.ini");
                if (!File.Exists(appIniPath))
                {
                    appIniPath = Path.Combine(appIniDir, "plugininstaller.ini");
                    if (!File.Exists(appIniPath))
                        continue;
                }

                var packageVersion = Ini.Read("Version", nameof(appData.PackageVersion), Version.Parse("0.0.0.0"), appIniPath);
                if (packageVersion >= appData.PackageVersion)
                    continue;

                if (outdatedApps.ContainsEx(appData.Key))
                    continue;
                if (Log.DebugMode > 0)
                    Log.Write($"Update: Outdated app has been found (Key: '{appData.Key}'; LocalVersion: '{packageVersion}'; ServerVersion: {appData.PackageVersion}).");
                outdatedApps.Add(appData.Key);
            }
            if (Log.DebugMode > 0)
                Log.Write($"Update: {outdatedApps.Count} outdated apps have been found (Keys: '{outdatedApps.Join("'; '")}').");
            return outdatedApps;
        }

        internal static string GetHost(Suppliers supplier)
        {
            switch (supplier)
            {
                case Suppliers.PortableApps:
                    return SupplierHosts.PortableApps;
                case Suppliers.SourceForge:
                    return SupplierHosts.SourceForge;
                default:
                    return SupplierHosts.Internal;
            }
        }

        internal static List<string> GetMirrors(Suppliers supplier)
        {
            if (_mirrors == default(Dictionary<Suppliers, List<string>>))
                _mirrors = new Dictionary<Suppliers, List<string>>();

            if (!_mirrors.ContainsKey(supplier))
                _mirrors.Add(supplier, new List<string>());

            if (_mirrors[supplier].Any())
                return _mirrors[supplier];

            switch (supplier)
            {
                // PortableApps.com
                case Suppliers.PortableApps:
                {
                    var mirrors = new[]
                    {
                        "http://downloads.portableapps.com",
                        "http://downloads2.portableapps.com"
                    };
                    _mirrors[supplier].AddRange(mirrors);
                    break;
                }

                // SourceForge.net
                case Suppliers.SourceForge:
                {
                    var mirrors = new[]
                    {
                        // IPv4 + IPv6 (however, a download via IPv6 doesn't work)
                        "https://netcologne.dl.sourceforge.net",
                        "https://freefr.dl.sourceforge.net",
                        "https://heanet.dl.sourceforge.net",

                        // IPv4
                        "https://kent.dl.sourceforge.net",
                        "https://netix.dl.sourceforge.net",
                        "https://vorboss.dl.sourceforge.net",
                        "https://downloads.sourceforge.net"
                    };
                    if (Network.IPv4IsAvalaible)
                    {
                        var sortHelper = new Dictionary<string, long>();
                        if (Log.DebugMode > 0)
                            Log.Write($"{nameof(Suppliers.SourceForge)}: Try to find the best server . . .");
                        foreach (var mirror in mirrors)
                        {
                            if (sortHelper.Keys.ContainsEx(mirror))
                                continue;
                            var time = NetEx.Ping(mirror);
                            if (Log.DebugMode > 0)
                                Log.Write($"{nameof(Suppliers.SourceForge)}: Reply from '{mirror}'; time={time}ms.");
                            sortHelper.Add(mirror, time);
                        }
                        mirrors = sortHelper.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.ToArray();
                        if (Log.DebugMode > 0)
                            Log.Write($"{nameof(Suppliers.SourceForge)}: New sort order: '{mirrors.Join("'; '")}'.");
                    }
                    _mirrors[supplier].AddRange(mirrors);
                    break;
                }

                // Internal ('si13n7.com')
                default:
                {
                    var servers = new[]
                    {
                        "https://www.si13n7.com/dns/info.ini",
                        "http://ns.0.si13n7.com",
                        "http://ns.1.si13n7.com",
                        "http://ns.2.si13n7.com",
                        "http://ns.3.si13n7.com",
                        "http://ns.4.si13n7.com",
                        "http://ns.5.si13n7.com"
                    };
                    if (!Network.IPv4IsAvalaible && Network.IPv6IsAvalaible)
                        servers = servers.Take(2).ToArray();

                    var info = default(string);
                    for (var i = 0; i < servers.Length; i++)
                    {
                        try
                        {
                            var server = servers[i];
                            if (!NetEx.FileIsAvailable(server, 20000))
                                throw new PathNotFoundException(server);
                            var data = NetEx.Transfer.DownloadString(server);
                            if (string.IsNullOrWhiteSpace(data))
                                throw new ArgumentNullException(nameof(data));
                            info = data;
                            if (Log.DebugMode > 0)
                                Log.Write($"{nameof(Suppliers.Internal)}: Domain names have been set successfully from '{server}'.");
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                        if (string.IsNullOrWhiteSpace(info) && i < servers.Length - 1)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }
                        break;
                    }

                    if (!string.IsNullOrWhiteSpace(info))
                    {
                        foreach (var section in Ini.GetSections(info))
                        {
                            var addr = string.Empty;
                            if (Network.IPv4IsAvalaible)
                                addr = Ini.Read(section, "addr", info);
                            if (string.IsNullOrEmpty(addr) && Network.IPv6IsAvalaible)
                                addr = Ini.Read(section, "ipv6", info);
                            if (string.IsNullOrEmpty(addr))
                                continue;
                            var domain = Ini.Read(section, "domain", info);
                            if (string.IsNullOrEmpty(domain))
                                continue;
                            var ssl = Ini.Read(section, "ssl", false, info);
                            if (Log.DebugMode > 0)
                                Log.Write($"{nameof(Suppliers.Internal)}: Section '{section}'; Address: '{addr}'; '{domain}'; SSL: '{ssl}';");
                            domain = PathEx.AltCombine(ssl ? "https:" : "http:", domain);
                            if (_mirrors[supplier].ContainsEx(domain))
                                continue;
                            _mirrors[supplier].Add(domain);
                            if (Log.DebugMode > 0)
                                Log.Write($"{nameof(Suppliers.Internal)}: Domain '{domain}' added.");
                        }
                        Ini.Detach(info);
                    }
                    break;
                }
            }
            return _mirrors[supplier];
        }

        internal enum Suppliers
        {
            Internal,
            PortableApps,
            SourceForge
        }

        internal static class SupplierHosts
        {
            internal const string Internal = "si13n7.com";
            internal const string PortableApps = "portableapps.com";
            internal const string SourceForge = "sourceforge.net";
        }
    }
}
