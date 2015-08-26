
#region SILENT DEVELOPMENTS generated code

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace SilDev
{
    public static class Network
    {
        #region DOWNLOAD

        private static string FilePath { get; set; }
        private static string DataReceived { get; set; }
        private static string StatusMessage { get; set; }
        private static int ProgressPercentage { get; set; }
        private static string TransferSpeed { get; set; }

        public static class DownloadInfo
        {
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

            public static string GetFilePath
            {
                get
                {
                    return !string.IsNullOrEmpty(FilePath) ? FilePath : string.Empty;
                }
            }

            public static string GetDataReceived
            {
                get
                {
                    return !string.IsNullOrEmpty(DataReceived) ? DataReceived : string.Empty;
                }
            }

            public static string GetStatusMessage
            {
                get
                {
                    return !string.IsNullOrEmpty(StatusMessage) ? StatusMessage : string.Empty;
                }
            }

            public static int GetProgressPercentage
            {
                get
                {
                    return !string.IsNullOrEmpty(ProgressPercentage.ToString()) ? ProgressPercentage : 0;
                }
            }

            public static string GetTransferSpeed
            {
                get
                {
                    return !string.IsNullOrEmpty(TransferSpeed) ? TransferSpeed : string.Empty;
                }
            }
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

        public static void DownloadFile(string _input, string _output, string _user, string _password)
        {
            if (File.Exists(_output))
                File.Delete(_output);
            try
            {
                using (WebClient tmp = new WebClient())
                {

                    if (!string.IsNullOrWhiteSpace(_user) && !string.IsNullOrWhiteSpace(_password))
                        tmp.Credentials = new NetworkCredential(_user, _password);
                    FilePath = _output;
                    tmp.DownloadFile(FilterUrl(_input), _output);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                Log.Debug(ex);
            }
        }

        public static void DownloadFile(string _input, string _output)
        {
            DownloadFile(_input, _output, string.Empty, string.Empty);
        }

        private static WebClient client;
        private static Stopwatch watch = new Stopwatch();

        public static void DownloadFileAsync(string _input, string _output, string _user, string _password)
        {
            if (File.Exists(_output))
                File.Delete(_output);
            try
            {
                using (client = new WebClient())
                {
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                    if (!string.IsNullOrWhiteSpace(_user) && !string.IsNullOrWhiteSpace(_password))
                        client.Credentials = new NetworkCredential(_user, _password);
                    watch.Start();
                    FilePath = _output;
                    client.DownloadFileAsync(FilterUrl(_input), _output);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format("Error after {0}s.{1}{2}", watch.Elapsed, Environment.NewLine, ex.Message);
                watch.Reset();
                Log.Debug(ex);
            }
        }

        public static void DownloadFileAsync(string _input, string _output)
        {
            DownloadFileAsync(_input, _output, string.Empty, string.Empty);
        }

        public static void CancelAsyncDownload()
        {
            client.CancelAsync();
        }

        private static void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                string received = string.Format("{0} MB / {1} MB", (Convert.ToDouble(e.BytesReceived) / 1024 / 1024).ToString("0.00"), (Convert.ToDouble(e.TotalBytesToReceive) / 1024 / 1024).ToString("0.00"));
                string speed = (Convert.ToDouble(e.BytesReceived) / 1024 / watch.Elapsed.TotalSeconds).ToString("0");
                if (DownloadInfo.GetProgressPercentage != e.ProgressPercentage)
                {
                    DataReceived = received;
                    ProgressPercentage = e.ProgressPercentage;
                    TransferSpeed = string.Format("{0} kb/s", speed);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format("Error after {0}s.{1}{2}", watch.Elapsed, Environment.NewLine, ex.Message);
                Log.Debug(ex);
            }
        }

        private static void Completed(object sender, AsyncCompletedEventArgs e)
        {
            watch.Reset();
            if (e.Cancelled)
            {
                client.Dispose();
                if (File.Exists(DownloadInfo.GetFilePath))
                    File.Delete(DownloadInfo.GetFilePath);
                StatusMessage = "Download canceled.";
            }
            else
                StatusMessage = "Download completed!";
        }

        public static bool AsyncIsBusy()
        {
            return client.IsBusy;
        }

        #endregion

        #region MISC

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

        private enum PublicDnsProvider : uint
        {
            censurfridns_dk = 0,
            Comodo_Secure_DNS = 10,
            DNS_Advantage = 20,
            DNS_WATCH = 30,
            Dyn = 40,
            FreeDNS = 50,
            Google = 60,
            GreenTeamDNS = 70,
            Hurricane_Electric = 80,
            Level3 = 90,
            Norton_ConnectSafe = 100,
            OpenDNS_Home = 110,
            OpenNIC = 120,
            puntCAT = 130,
            SafeDNS = 140,
            SmartViper = 150
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
