
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Linq;
using System.IO;
using System.Threading;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.Convert"/>.cs</para>
    /// <para><see cref="SilDev.Crypt"/>.cs</para>
    /// <para><see cref="SilDev.Log"/>.cs</para>
    /// <para><see cref="SilDev.WinAPI"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class Resource
    {
        public static void ExtractConvert(byte[] resData, string destPath, bool reverseBytes = true)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(resData))
                {
                    byte[] data = ms.ToArray();
                    if (reverseBytes)
                        data = data.Reverse().ToArray();
                    using (FileStream fs = new FileStream(destPath, FileMode.CreateNew, FileAccess.Write))
                        fs.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        public static void Extract(byte[] resData, string destPath) =>
            ExtractConvert(resData, destPath, false);

        public static void PlayWave(Stream resData)
        {
            try
            {
                using (Stream audio = resData)
                {
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(audio);
                    player.Play();
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        public static void PlayWaveAsync(Stream resData) =>
            new Thread(() => PlayWave(resData)).Start();

    }
}

#endregion
