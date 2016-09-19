
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class NET
    {
        public static bool InternetIsAvailable()
        {
            bool IsAvailable = NetworkInterface.GetAllNetworkInterfaces().Any(x => x.OperationalStatus == OperationalStatus.Up);
            if (!IsAvailable)
                return IsAvailable;
            IsAvailable = false;
            foreach (string s in new string[] { "8.8.8.8", "8.8.4.4" })
            {
                IsAvailable = Ping(s) < 3000;
                if (IsAvailable)
                    break;
            }
            return IsAvailable;
        }

        public static PingReply LastPingReply { get; private set; }

        public static long Ping(Uri uri)
        {
            long RoundtripTime = 3000;
            try
            {
                using (Ping ping = new Ping())
                {
                    LastPingReply = ping.Send(uri.Host, 3000);
                    if (LastPingReply.Status == IPStatus.Success)
                        RoundtripTime = LastPingReply.RoundtripTime;
                }
            }
            catch (Exception ex)
            {
                LOG.Debug($"{ex.Message} (URI: '{uri.ToString().Replace(" ", "%20")}')", ex.StackTrace);
            }
            return RoundtripTime;
        }

        public static long Ping(string uri) =>
            Ping(uri.ToUri());

        public static Uri ToUri(this string uri)
        {
            try
            {
                string s = uri;
                if (!s.StartsWith("https://", StringComparison.OrdinalIgnoreCase) && !s.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                    s = $"http://{uri}";
                return new Uri(s);
            }
            catch
            {
                return null;
            }
        }

        public static bool IsValid(this Uri uri)
        {
            int StatusCode = 500;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Timeout = 3000;
                request.Method = "HEAD";
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    StatusCode = (int)response.StatusCode;
                if (StatusCode >= 500 && StatusCode <= 510)
                    throw new HttpListenerException();
            }
            catch (Exception ex)
            {
                LOG.Debug($"{ex.Message} (URI: '{uri.ToString().Replace(" ", "%20")}')", ex.StackTrace);
            }
            return StatusCode >= 100 && StatusCode < 400;
        }

        public static bool FileIsAvailable(this Uri srcUri, string userName = null, string password = null)
        {
            long ContentLength = 0;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(srcUri);
                request.Timeout = 3000;
                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                    request.Credentials = new NetworkCredential(userName, password);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    ContentLength = response.ContentLength;
            }
            catch (Exception ex)
            {
                LOG.Debug($"{ex.Message} (URI: '{srcUri.ToString().Replace(" ", "%20")}')", ex.StackTrace);
            }
            return ContentLength > 0;
        }

        public static bool FileIsAvailable(string srcUri, string userName = null, string password = null) =>
            srcUri.ToUri().FileIsAvailable(userName, password);

        public static DateTime GetFileDate(this Uri srcUri, string userName = null, string password = null)
        {
            DateTime LastModified = DateTime.Now;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(srcUri);
                request.Timeout = 3000;
                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                    request.Credentials = new NetworkCredential(userName, password);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    LastModified = response.LastModified;
            }
            catch (Exception ex)
            {
                LOG.Debug($"{ex.Message} (URI: '{srcUri.ToString().Replace(" ", "%20")}')", ex.StackTrace);
            }
            return LastModified;
        }

        public static DateTime GetFileDate(string srcUri, string userName = null, string password = null) =>
            srcUri.ToUri().GetFileDate(userName, password);

        public static string GetFileName(this Uri srcUri)
        {
            string name = string.Empty;
            try
            {
                using (WebClient client = new WebClient())
                {
                    using (Stream stream = client.OpenRead(srcUri))
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
                LOG.Debug($"{ex.Message} (URI: '{srcUri.ToString().Replace(" ", "%20")}')", ex.StackTrace);
            }
            return name;
        }

        public static string GetFileName(string srcUri) =>
            srcUri.ToUri().GetFileName();

        public sealed class ASYNCTRANSFER
        {
            private WebClient webClient;
            private Stopwatch stopwatch = new Stopwatch();

            public Uri Address { get; private set; }

            public string FilePath { get; private set; }

            public long BytesReceived { get; private set; } = 0;

            public long TotalBytesToReceive { get; private set; } = 0;

            public string DataReceived
            {
                get
                {
                    string current = (BytesReceived / 1024f / 1024f).ToString("0.00");
                    string total = (TotalBytesToReceive / 1024f / 1024f).ToString("0.00");
                    return $"{current} MB / {total} MB"; ;
                }
            }

            public int ProgressPercentage { get; private set; } = 0;

            public double TransferSpeed { get; private set; } = 0d;

            public TimeSpan TimeElapsed { get; private set; } = TimeSpan.MinValue;

            public bool HasCanceled { get; set; } = true;

            public void DownloadFile(Uri srcUri, string destPath, string userName = null, string password = null)
            {
                try
                {
                    if (IsBusy)
                        throw new NotSupportedException("Async file download is already busy, multiple calls are not allowed.");
                    if (File.Exists(destPath))
                        File.Delete(destPath);
                    using (webClient = new WebClient())
                    {
                        webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFile_Completed);
                        webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadFile_ProgressChanged);

                        Address = srcUri;
                        FilePath = destPath;

                        bool exists = FileIsAvailable(Address, userName, password);
                        if (!exists)
                            throw new FileNotFoundException();

                        if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                            webClient.Credentials = new NetworkCredential(userName, password);
                        webClient.DownloadFileAsync(Address, FilePath);
                        stopwatch.Start();
                    }
                }
                catch (Exception ex)
                {
                    LOG.Debug($"{ex.Message} (URI: '{srcUri.ToString().Replace(" ", "%20")}')", ex.StackTrace);
                    HasCanceled = true;
                    stopwatch.Reset();
                }
            }

            public void DownloadFile(string srcUri, string destPath, string userName = null, string password = null) =>
                DownloadFile(srcUri.ToUri(), destPath, userName, password);

            private void DownloadFile_ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
            {
                try
                {
                    BytesReceived = e.BytesReceived;
                    TotalBytesToReceive = e.TotalBytesToReceive;
                    ProgressPercentage = e.ProgressPercentage;
                    TimeElapsed = stopwatch.Elapsed;
                    TransferSpeed = e.BytesReceived / 1024 / TimeElapsed.TotalSeconds;
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                    TimeElapsed = stopwatch.Elapsed;
                    HasCanceled = true;
                    stopwatch.Reset();
                }
            }

            private void DownloadFile_Completed(object sender, AsyncCompletedEventArgs e)
            {
                stopwatch.Reset();
                if (e.Cancelled)
                {
                    HasCanceled = true;
                    webClient.Dispose();
                    try
                    {
                        if (File.Exists(FilePath))
                            File.Delete(FilePath);
                    }
                    catch (Exception ex)
                    {
                        LOG.Debug(ex);
                    }
                }
                else
                {
                    if (File.Exists(FilePath))
                        BytesReceived = new FileInfo(FilePath).Length;
                    HasCanceled = !(File.Exists(FilePath) && BytesReceived == TotalBytesToReceive);
                }
            }

            public bool IsBusy
            {
                get
                {
                    try
                    {
                        return webClient.IsBusy;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            public void CancelAsync()
            {
                if (IsBusy)
                    webClient.CancelAsync();
            }
        }

        public sealed class TRANSFER
        {
            public bool DownloadFile(Uri srcUri, string destPath, string userName = null, string password = null)
            {
                try
                {
                    if (File.Exists(destPath))
                        File.Delete(destPath);
                    if (!FileIsAvailable(srcUri, userName, password))
                        throw new FileNotFoundException();
                    using (WebClient wc = new WebClient())
                    {
                        if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                            wc.Credentials = new NetworkCredential(userName, password);
                        wc.DownloadFile(srcUri, destPath);
                    }
                    return File.Exists(destPath);
                }
                catch (Exception ex)
                {
                    LOG.Debug($"{ex.Message} (URI: '{srcUri.ToString().Replace(" ", "%20")}')", ex.StackTrace);
                    return false;
                }
            }

            public bool DownloadFile(string srcUri, string destPath, string userName = null, string password = null) =>
                DownloadFile(srcUri.ToUri(), destPath, userName, password);

            public byte[] DownloadData(Uri srcUri, string userName = null, string password = null)
            {
                try
                {
                    byte[] ba = null;
                    using (WebClient wc = new WebClient())
                    {
                        if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                            wc.Credentials = new NetworkCredential(userName, password);
                        ba = wc.DownloadData(srcUri);
                    }
                    if (ba == null)
                        throw new Exception("No downloadable data found.");
                    return ba;
                }
                catch (Exception ex)
                {
                    LOG.Debug($"{ex.Message} (URI: '{srcUri.ToString().Replace(" ", "%20")}')", ex.StackTrace);
                    return null;
                }
            }

            public byte[] DownloadData(string srcUri, string userName = null, string password = null) =>
                DownloadData(srcUri.ToUri(), userName, password);

            public string DownloadString(Uri srcUri, string userName = null, string password = null)
            {
                try
                {
                    string s;
                    using (WebClient wc = new WebClient())
                    {
                        if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                            wc.Credentials = new NetworkCredential(userName, password);
                        s = wc.DownloadString(srcUri);
                    }
                    if (string.IsNullOrEmpty(s))
                        throw new Exception("No downloadable string found.");
                    return s;
                }
                catch (Exception ex)
                {
                    LOG.Debug($"{ex.Message} (URI: '{srcUri.ToString().Replace(" ", "%20")}')", ex.StackTrace);
                    return string.Empty;
                }
            }

            public string DownloadString(string srcUri, string userName = null, string password = null) =>
                DownloadString(srcUri.ToUri(), userName, password);
        }

        public static bool DownloadFile(this Uri srcUri, string destPath, string userName = null, string password = null) =>
            new TRANSFER().DownloadFile(srcUri, destPath, userName, password);

        public static bool DownloadFile(string srcUri, string destPath, string userName = null, string password = null) =>
            new TRANSFER().DownloadFile(srcUri.ToUri(), destPath, userName, password);

        public static byte[] DownloadData(this Uri srcUri, string userName = null, string password = null) =>
            new TRANSFER().DownloadData(srcUri, userName, password);

        public static byte[] DownloadData(string srcUri, string userName = null, string password = null) =>
            new TRANSFER().DownloadData(srcUri.ToUri(), userName, password);

        public static string DownloadString(this Uri srcUri, string userName = null, string password = null) =>
            new TRANSFER().DownloadString(srcUri, userName, password);

        public static string DownloadString(string srcUri, string userName = null, string password = null) =>
            new TRANSFER().DownloadString(srcUri.ToUri(), userName, password);
    }
}

#endregion
