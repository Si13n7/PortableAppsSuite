
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace SilDev
{
    /// <summary>To unlock <see cref="IrrKlang.ISoundEngine"/> functions:
    /// <para>Define 'irrKlang' for compiling and add the 'irrKlang.NET4.dll' reference to your project.</para>
    /// <para>---</para>
    /// <para>Requirements:</para>
    /// <para><see cref="SilDev.Convert"/>.cs</para>
    /// <para><see cref="SilDev.Log"/>.cs</para>
    /// <para><seealso cref="SilDev"/></para></summary>
    public static class MEDIA
    {
        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods
        {
            [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);

            [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

            [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

            [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern uint timeBeginPeriod(uint uPeriod);

            [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern uint timeEndPeriod(uint period);
        }

        #region Device Manager

        public static class DeviceManager
        {
            public static float? GetApplicationVolume(string name)
            {
                ISimpleAudioVolume volume = GetVolumeObject(name);
                if (volume == null)
                    return null;
                float level;
                volume.GetMasterVolume(out level);
                return level * 100;
            }

            public static bool? GetApplicationMute(string name)
            {
                ISimpleAudioVolume volume = GetVolumeObject(name);
                if (volume == null)
                    return null;
                bool mute;
                volume.GetMute(out mute);
                return mute;
            }

            public static void SetApplicationVolume(string name, float level)
            {
                ISimpleAudioVolume volume = GetVolumeObject(name);
                if (volume == null)
                    return;
                Guid guid = Guid.Empty;
                volume.SetMasterVolume(level / 100, ref guid);
            }

            public static void SetApplicationMute(string name, bool mute)
            {
                ISimpleAudioVolume volume = GetVolumeObject(name);
                if (volume == null)
                    return;
                Guid guid = Guid.Empty;
                volume.SetMute(mute, ref guid);
            }

            public static IEnumerable<string> EnumerateApplications()
            {
                IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
                IMMDevice speakers;
                deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);

                Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
                object o;
                speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
                IAudioSessionManager2 mgr = (IAudioSessionManager2)o;

                IAudioSessionEnumerator sessionEnumerator;
                mgr.GetSessionEnumerator(out sessionEnumerator);
                int count;
                sessionEnumerator.GetCount(out count);

                for (int i = 0; i < count; i++)
                {
                    IAudioSessionControl ctl;
                    sessionEnumerator.GetSession(i, out ctl);
                    string dn;
                    ctl.GetDisplayName(out dn);
                    yield return dn;
                    Marshal.ReleaseComObject(ctl);
                }

                Marshal.ReleaseComObject(sessionEnumerator);
                Marshal.ReleaseComObject(mgr);
                Marshal.ReleaseComObject(speakers);
                Marshal.ReleaseComObject(deviceEnumerator);
            }

            private static ISimpleAudioVolume GetVolumeObject(string name)
            {
                IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
                IMMDevice speakers;
                deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);

                Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
                object o;
                speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
                IAudioSessionManager2 mgr = (IAudioSessionManager2)o;

                IAudioSessionEnumerator sessionEnumerator;
                mgr.GetSessionEnumerator(out sessionEnumerator);
                int count;
                sessionEnumerator.GetCount(out count);

                ISimpleAudioVolume volumeControl = null;
                for (int i = 0; i < count; i++)
                {
                    IAudioSessionControl ctl;
                    sessionEnumerator.GetSession(i, out ctl);
                    string dn;
                    ctl.GetDisplayName(out dn);
                    if (string.Compare(name, dn, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        volumeControl = ctl as ISimpleAudioVolume;
                        break;
                    }
                    Marshal.ReleaseComObject(ctl);
                }

                Marshal.ReleaseComObject(sessionEnumerator);
                Marshal.ReleaseComObject(mgr);
                Marshal.ReleaseComObject(speakers);
                Marshal.ReleaseComObject(deviceEnumerator);

                return volumeControl;
            }
        }

        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        internal class MMDeviceEnumerator { }

        internal enum EDataFlow
        {
            eRender,
            eCapture,
            eAll,
            EDataFlow_enum_count
        }

        internal enum ERole
        {
            eConsole,
            eMultimedia,
            eCommunications,
            ERole_enum_count
        }

        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMDeviceEnumerator
        {
            int NotImpl1();

            [PreserveSig]
            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);
        }

        [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMDevice
        {
            [PreserveSig]
            int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
        }

        [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionManager2
        {
            int NotImpl1();
            int NotImpl2();

            [PreserveSig]
            int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);
        }

        [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionEnumerator
        {
            [PreserveSig]
            int GetCount(out int SessionCount);

            [PreserveSig]
            int GetSession(int SessionCount, out IAudioSessionControl Session);
        }

        [Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionControl
        {
            int NotImpl1();

            [PreserveSig]
            int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        }

        [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ISimpleAudioVolume
        {
            [PreserveSig]
            int SetMasterVolume(float fLevel, ref Guid EventContext);

            [PreserveSig]
            int GetMasterVolume(out float pfLevel);

            [PreserveSig]
            int SetMute(bool bMute, ref Guid EventContext);

            [PreserveSig]
            int GetMute(out bool pbMute);
        }

        #endregion

        #region Windows Media Libary

        public static class WindowsLib
        {
            private static readonly string alias = Assembly.GetEntryAssembly().GetName().Name.RemoveChar(' ');

            public static uint timeBeginPeriod(uint uPeriod) =>
                SafeNativeMethods.timeBeginPeriod(uPeriod);

            public static uint timeEndPeriod(uint uPeriod) =>
                SafeNativeMethods.timeEndPeriod(uPeriod);

            public static int GetSoundVolume()
            {
                uint CurrVol = 0;
                SafeNativeMethods.waveOutGetVolume(IntPtr.Zero, out CurrVol);
                ushort CalcVol = (ushort)(CurrVol & 0x0000ffff);
                return (CalcVol / (ushort.MaxValue / 10)) * 10;
            }

            public static void SetSoundVolume(int value)
            {
                int newVolume = ((ushort.MaxValue / 10) * (value < 0 || value > 100 ? 100 : value / 10));
                uint newVolumeAllChannels = (((uint)newVolume & 0x0000ffff) | ((uint)newVolume << 16));
                SafeNativeMethods.waveOutSetVolume(IntPtr.Zero, newVolumeAllChannels);
            }

            private static string sndStatus()
            {
                StringBuilder sb = new StringBuilder(128);
                SafeNativeMethods.mciSendString($"status {alias} mode", sb, sb.Capacity, IntPtr.Zero);
                return sb.ToString();
            }

            private static void sndOpen(string path)
            {
                if (!string.IsNullOrEmpty(sndStatus()))
                    sndClose();
                string arg = $"open \"{path}\" alias {alias}";
                SafeNativeMethods.mciSendString(arg, null, 0, IntPtr.Zero);
            }

            private static void sndClose()
            {
                string arg = $"close {alias}";
                SafeNativeMethods.mciSendString(arg, null, 0, IntPtr.Zero);
            }

            private static void sndPlay(bool loop = false)
            {
                string arg = $"play {alias}{(loop ? " repeat" : string.Empty)}";
                SafeNativeMethods.mciSendString(arg, null, 0, IntPtr.Zero);
            }

            public static void Play(string path, bool loop = false, int volume = 100)
            {
                if (File.Exists(path))
                {
                    if (GetSoundVolume() != volume)
                        SetSoundVolume(volume);
                    sndOpen(path);
                    sndPlay(loop);
                }
            }

            public static void Play(string path, int volume) =>
                Play(path, false, volume);

            public static void Stop() =>
                sndClose();
        }

        #endregion

        #region irrKlang Libary

        #if irrKlang

        public static class IrrKlangLib
        {
            protected static IrrKlang.ISoundEngine irrKlangEngine = new IrrKlang.ISoundEngine();
            protected static IrrKlang.ISound irrKlangPlayer;

            public static void Play(string path, bool loop = false, int volume = 100)
            {
                if (File.Exists(path))
                {
                    if (WindowsLib.GetSoundVolume() != volume)
                        WindowsLib.SetSoundVolume(volume);
                    Stop();
                    irrKlangPlayer = irrKlangEngine.Play2D(path, loop);
                    irrKlangPlayer.Volume = 1F;
                }
            }

            public static void Play(string path, int volume) =>
                Play(path, false, volume);

            public static void Stop()
            {
                if (irrKlangPlayer != null)
                    irrKlangPlayer.Stop();
            }
        }

        #endif

        #endregion
    }
}

#endregion
