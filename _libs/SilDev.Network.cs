
#region SILENT DEVELOPMENTS generated code

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

namespace SilDev
{
    public static class Network
    {
        #region DOWNLOAD

        private static WebClient client;
        private static Stopwatch watch = new Stopwatch();

        private static Dictionary<string, ASYNCDOWNLOADINFODATA> asyncDownloadInfo = new Dictionary<string, ASYNCDOWNLOADINFODATA>();
        public static Dictionary<string, ASYNCDOWNLOADINFODATA> AsyncDownloadInfo
        {
            get
            {
                return asyncDownloadInfo;
            }
            private set
            {
                asyncDownloadInfo = value;
            }
        }

        private static string LatestAsyncDownloadInfoKey = string.Empty;
        public static ASYNCDOWNLOADINFODATA LatestAsyncDownloadInfo
        {
            get
            {
                try
                {
                    return AsyncDownloadInfo[LatestAsyncDownloadInfoKey];
                }
                catch
                {
                    return new ASYNCDOWNLOADINFODATA();
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class ASYNCDOWNLOADINFODATA
        {
            public Uri FileUrl;
            public string FilePath;
            public string DataReceived = string.Empty;
            public long CurrentSize = 0;
            public long TotalSize = 0;
            public int ProgressPercentage = 0;
            public string TransferSpeed = string.Empty;
            public TimeSpan TimeElapsed = TimeSpan.MinValue;
            public int StatusCode = 0;
            public string StatusMessage = string.Empty;
        }

        public static void DownloadFileAsync(string _infoKey, string _input, string _output, string _user, string _password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_infoKey))
                    throw new Exception("Info key is empty.");
                if (AsyncDownloadIsBusy())
                    throw new Exception("Async file download is already busy, multiple calls are not allowed.");
                if (File.Exists(_output))
                    File.Delete(_output);
                using (client = new WebClient())
                {
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                    if (!string.IsNullOrWhiteSpace(_user) && !string.IsNullOrWhiteSpace(_password))
                        client.Credentials = new NetworkCredential(_user, _password);
                    ASYNCDOWNLOADINFODATA state = new ASYNCDOWNLOADINFODATA();
                    state.FileUrl = FilterUrl(_input);
                    state.FilePath = _output;
                    bool exists = OnlineFileExists(state.FileUrl);
                    if (!exists)
                    {
                        state.StatusCode = 3;
                        state.StatusMessage = "Download failed!";
                    }
                    if (!AsyncDownloadInfo.ContainsKey(_infoKey))
                        AsyncDownloadInfo.Add(_infoKey, state);
                    else
                        AsyncDownloadInfo[_infoKey] = state;
                    LatestAsyncDownloadInfoKey = _infoKey;
                    if (exists)
                    {
                        client.DownloadFileAsync(state.FileUrl, state.FilePath);
                        watch.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                watch.Reset();
                Log.Debug(ex);
            }
        }

        public static void DownloadFileAsync(string _input, string _output, string _user, string _password)
        {
            DownloadFileAsync(AsyncDownloadInfo.Keys.Count.ToString(), _input, _output, _user, _password);
        }

        public static void DownloadFileAsync(string _infoKey, string _input, string _output)
        {
            DownloadFileAsync(_infoKey, _input, _output, null, null);
        }

        public static void DownloadFileAsync(string _input, string _output)
        {
            DownloadFileAsync(AsyncDownloadInfo.Keys.Count.ToString(), _input, _output, null, null);
        }

        private static void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ASYNCDOWNLOADINFODATA state = AsyncDownloadInfo[LatestAsyncDownloadInfoKey];
            try
            {
                state.CurrentSize = e.BytesReceived;
                state.TotalSize = e.TotalBytesToReceive;
                state.TimeElapsed = watch.Elapsed;
                string received = string.Format("{0} MB / {1} MB", (state.CurrentSize / 1024f / 1024f).ToString("0.00"), (state.TotalSize / 1024f / 1024f).ToString("0.00"));
                string speed = (e.BytesReceived / 1024 / state.TimeElapsed.TotalSeconds).ToString("0");
                if (state.ProgressPercentage != e.ProgressPercentage)
                {
                    state.DataReceived = received;
                    state.ProgressPercentage = e.ProgressPercentage;
                    state.TransferSpeed = string.Format("{0} kb/s", speed);
                }
            }
            catch (Exception ex)
            {
                state.TimeElapsed = watch.Elapsed;
                state.StatusCode = 3;
                state.StatusMessage = string.Format("Error after {0}s.{1}{2}", state.TimeElapsed, Environment.NewLine, ex.Message);
                watch.Reset();
                Log.Debug(ex);
            }
            AsyncDownloadInfo[LatestAsyncDownloadInfoKey] = state;
        }

        private static void Completed(object sender, AsyncCompletedEventArgs e)
        {
            watch.Reset();
            ASYNCDOWNLOADINFODATA state = AsyncDownloadInfo[LatestAsyncDownloadInfoKey];
            if (e.Cancelled)
            {
                client.Dispose();
                try
                {
                    if (File.Exists(state.FilePath))
                        File.Delete(state.FilePath);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                }
                state.StatusCode = 2;
                state.StatusMessage = "Download canceled!";
            }
            else
            {
                if (File.Exists(state.FilePath))
                    state.CurrentSize = new FileInfo(state.FilePath).Length;
                if (File.Exists(state.FilePath) && state.CurrentSize == state.TotalSize)
                {
                    state.StatusCode = 1;
                    state.StatusMessage = "Download completed!";
                }
                else
                {
                    state.StatusCode = 3;
                    state.StatusMessage = "Download failed!";
                }
            }
            AsyncDownloadInfo[LatestAsyncDownloadInfoKey] = state;
        }

        public static bool AsyncDownloadIsBusy()
        {
            try
            {
                return client.IsBusy;
            }
            catch
            {
                return false;
            }
        }

        public static void CancelAsyncDownload()
        {
            if (AsyncDownloadIsBusy())
                client.CancelAsync();
        }

        public static bool DownloadFile(string _input, string _output, string _user, string _password)
        {
            try
            {
                if (File.Exists(_output))
                    File.Delete(_output);
                using (WebClient tmp = new WebClient())
                {
                    if (!string.IsNullOrWhiteSpace(_user) && !string.IsNullOrWhiteSpace(_password))
                        tmp.Credentials = new NetworkCredential(_user, _password);
                    tmp.DownloadFile(FilterUrl(_input), _output);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return false;
            }
        }

        public static bool DownloadFile(string _input, string _output)
        {
            return DownloadFile(_input, _output, string.Empty, string.Empty);
        }

        public static string DownloadString(string _url, string _user, string _password)
        {
            try
            {
                using (WebClient tmp = new WebClient())
                {
                    if (!string.IsNullOrWhiteSpace(_user) && !string.IsNullOrWhiteSpace(_password))
                        tmp.Credentials = new NetworkCredential(_user, _password);
                    return tmp.DownloadString(FilterUrl(_url));
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return string.Empty;
            }
        }

        public static string DownloadString(string _url)
        {
            return DownloadString(_url, string.Empty, string.Empty);
        }

        #endregion

        #region MISC

        public static bool OnlineFileExists(Uri _url)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            request = (HttpWebRequest)WebRequest.Create(_url);
            request.Timeout = 3000;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                return response.ContentLength > 0;
            }
            catch
            {
                return false;
            }
        }

        public static bool OnlineFileExists(string _url)
        {
            return OnlineFileExists(FilterUrl(_url));
        }

        public static DateTime GetOnlineFileDate(string _url)
        {
            try
            {
                HttpWebRequest file = (HttpWebRequest)WebRequest.Create(FilterUrl(_url));
                HttpWebResponse response = (HttpWebResponse)file.GetResponse();
                return response.LastModified;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return DateTime.Now;
            }
        }

        public static string GetOnlineFileName(string _url)
        {
            string name = string.Empty;
            try
            {
                using (WebClient client = new WebClient())
                {
                    using (Stream stream = client.OpenRead(_url))
                    {
                        string cd = client.ResponseHeaders["content-disposition"];
                        if (!string.IsNullOrWhiteSpace(cd))
                        {
                            int i = cd.IndexOf("filename=", StringComparison.CurrentCultureIgnoreCase);
                            if (i >= 0)
                                name = cd.Substring(i + 10);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return name;
        }

        public static bool UrlIsValid(string _url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_url);
                request.Timeout = 3000;
                request.Method = "HEAD";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                int statusCode = (int)response.StatusCode;
                if (statusCode >= 100 && statusCode < 400)
                    return true;
                else if (statusCode >= 500 && statusCode <= 510)
                {
                    Log.Debug(string.Format("The remote server has thrown an internal error. Url is not valid: {0}", _url));
                    return false;
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                    return false;
                else
                    Log.Debug(string.Format("Unhandled status [{0}] returned for url: {1}", ex.Status, _url), ex.StackTrace);
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return false;
        }

        private static Uri FilterUrl(string _url)
        {
            string url = _url;
            if (_url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                url = url.Substring(8);
            if (!_url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                url = string.Format("http://{0}", url);
            return new Uri(url);
        }

        public static System.Net.NetworkInformation.PingReply PingReply { get; set; }

        public static long Ping(string _url)
        {
            try
            {
                using (System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping())
                {
                    PingReply = ping.Send(FilterUrl(_url).Host, 3000);
                    if (PingReply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        Log.Debug(string.Format("Reply from '{0}': bytes={1} time<1ms TTL='{2}'", PingReply.Address, 32, PingReply.RoundtripTime));
                        return PingReply.RoundtripTime;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return long.MaxValue;
        }

        private enum PublicDnsProvider
        {
            censurfridns_dk,
            Comodo_Secure_DNS,
            DNS_Advantage,
            DNS_WATCH,
            Dyn,
            FreeDNS,
            Google,
            GreenTeamDNS,
            Hurricane_Electric,
            Level3,
            Norton_ConnectSafe,
            OpenDNS_Home,
            OpenNIC,
            puntCAT,
            SafeDNS,
            SmartViper
        }

        private static Dictionary<PublicDnsProvider, List<string>> PublicDnsList
        {
            get
            {
                Dictionary<PublicDnsProvider, List<string>> DNSList = new Dictionary<PublicDnsProvider, List<string>>();
                DNSList.Add(PublicDnsProvider.censurfridns_dk, new List<string> { "89.233.43.71", "91.239.100.100" });
                DNSList.Add(PublicDnsProvider.Comodo_Secure_DNS, new List<string> { "8.26.56.26", "8.20.247.20" });
                DNSList.Add(PublicDnsProvider.DNS_Advantage, new List<string> { "156.154.70.1", "156.154.71.1" });
                DNSList.Add(PublicDnsProvider.DNS_WATCH, new List<string> { "84.200.69.80", "84.200.70.40" });
                DNSList.Add(PublicDnsProvider.Dyn, new List<string> { "216.146.35.35", "216.146.36.36" });
                DNSList.Add(PublicDnsProvider.FreeDNS, new List<string> { "37.235.1.174", "37.235.1.177" });
                DNSList.Add(PublicDnsProvider.Google, new List<string> { "8.8.8.8", "8.8.4.4" });
                DNSList.Add(PublicDnsProvider.GreenTeamDNS, new List<string> { "81.218.119.11", "209.88.198.133" });
                DNSList.Add(PublicDnsProvider.Hurricane_Electric, new List<string> { "74.82.42.42" });
                DNSList.Add(PublicDnsProvider.Level3, new List<string> { "209.244.0.3", "209.244.0.4" });
                DNSList.Add(PublicDnsProvider.Norton_ConnectSafe, new List<string> { "199.85.126.10", "199.85.127.10" });
                DNSList.Add(PublicDnsProvider.OpenDNS_Home, new List<string> { "208.67.222.222", "208.67.220.220" });
                DNSList.Add(PublicDnsProvider.OpenNIC, new List<string> { "107.150.40.234", "50.116.23.211" });
                DNSList.Add(PublicDnsProvider.puntCAT, new List<string> { "109.69.8.51" });
                DNSList.Add(PublicDnsProvider.SafeDNS, new List<string> { "195.46.39.39", "195.46.39.40" });
                DNSList.Add(PublicDnsProvider.SmartViper, new List<string> { "208.76.50.50", "208.76.51.51" });
                return DNSList;
            }
        }

        public static bool InternetIsAvailable(object _publicDnsProvider)
        {
            bool IsAvailable = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces().Any(x => x.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up);
            if (!IsAvailable)
                return IsAvailable;
            IsAvailable = false;
            if (_publicDnsProvider is bool && (bool)_publicDnsProvider)
            {
                foreach (var entry in PublicDnsList)
                {
                    foreach (string ip in entry.Value)
                    {
                        IsAvailable = Ping(ip) < 999;
                        if (IsAvailable)
                            return IsAvailable;
                    }
                }
            }
            else
            {
                foreach (var ip in PublicDnsList[_publicDnsProvider is PublicDnsProvider ? (PublicDnsProvider)_publicDnsProvider : PublicDnsProvider.Google])
                {
                    IsAvailable = Ping(ip) < 999;
                    if (IsAvailable)
                        return IsAvailable;
                }
            }
            return IsAvailable;
        }

        public static bool InternetIsAvailable()
        {
            return InternetIsAvailable(PublicDnsProvider.Google);
        }

        public static string GetTheBestServer(string _iniUrl, bool _internetIsAvailable)
        {
            try
            {
                if (!_internetIsAvailable)
                    throw new Exception("Currently is no connection available.");
                string OnlineIniFileContent = string.Empty;
                for (int i = 0; i < 30; i++)
                {
                    OnlineIniFileContent = DownloadString(_iniUrl);
                    Log.Debug(string.Format("Get server infos from '{0}'", _iniUrl));
                    if (!string.IsNullOrWhiteSpace(OnlineIniFileContent))
                        break;
                    Thread.Sleep(100);
                }
                Dictionary<string, long> connections = new Dictionary<string, long>();
                foreach (string ent in Initialization.ReadValue("root", "sections", OnlineIniFileContent).Split(','))
                {
                    string address = Initialization.ReadValue(ent, "address", OnlineIniFileContent);
                    Ping(address);
                    connections.Add(address, PingReply.RoundtripTime);
                }
                var sortedConnections = from entry in connections orderby entry.Value ascending select entry;
                connections = sortedConnections.ToDictionary(pair => pair.Key, pair => pair.Value);
                foreach (var ent in connections)
                {
                    if (ent.Value < long.MaxValue)
                    {
                        Log.Debug(string.Format("The best connection has been selected: '{0}'", Dns.GetHostAddresses(ent.Key)[0]));
                        return ent.Key;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return null;
        }

        public static string GetTheBestServer(string _iniUrl)
        {
            return GetTheBestServer(_iniUrl, InternetIsAvailable());
        }

        #endregion
    }
}

#endregion
