
#region SILENT DEVELOPMENTS generated code

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SilDev
{
    /// <summary>
    /// To unlock irrKlang functions:
    /// Define 'irrKlang' for compiling and add the 'irrKlang.NET.dll' reference to your project.
    /// </summary>
    public static class Media
    {
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
            private static readonly string _alias = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", string.Empty);

            public static uint timeBeginPeriod(uint _uPeriod)
            {
                return WinAPI.SafeNativeMethods.timeBeginPeriod(_uPeriod);
            }

            public static uint timeEndPeriod(uint _period)
            {
                return WinAPI.SafeNativeMethods.timeEndPeriod(_period);
            }

            public static int GetSoundVolume()
            {
                uint CurrVol = 0;
                WinAPI.SafeNativeMethods.waveOutGetVolume(IntPtr.Zero, out CurrVol);
                ushort CalcVol = (ushort)(CurrVol & 0x0000ffff);
                return (CalcVol / (ushort.MaxValue / 10)) * 10;
            }

            public static void SetSoundVolume(int _value)
            {
                int newVolume = ((ushort.MaxValue / 10) * ((_value < 0 || _value > 100) ? 100 : _value / 10));
                uint newVolumeAllChannels = (((uint)newVolume & 0x0000ffff) | ((uint)newVolume << 16));
                WinAPI.SafeNativeMethods.waveOutSetVolume(IntPtr.Zero, newVolumeAllChannels);
            }

            private static string sndStatus()
            {
                StringBuilder Buffer = new StringBuilder(128);
                WinAPI.SafeNativeMethods.mciSendString("status " + _alias + " mode", Buffer, Buffer.Capacity, IntPtr.Zero);
                return Buffer.ToString();
            }

            private static void sndOpen(string _file)
            {
                if (sndStatus() != string.Empty)
                    sndClose();

                string Command = "open \"" + _file + "\" alias " + _alias;
                WinAPI.SafeNativeMethods.mciSendString(Command, null, 0, IntPtr.Zero);
            }

            private static void sndClose()
            {
                string Command = "close " + _alias;
                WinAPI.SafeNativeMethods.mciSendString(Command, null, 0, IntPtr.Zero);
            }

            private static void sndPlay(bool _loop)
            {
                string Command = "play " + _alias + (_loop ? " repeat" : string.Empty);
                WinAPI.SafeNativeMethods.mciSendString(Command, null, 0, IntPtr.Zero);
            }

            private static void sndPlay()
            {
                sndPlay(false);
            }

            public static void Play(string _file, bool _loop, int _vol)
            {
                if (File.Exists(_file))
                {
                    if (GetSoundVolume() != _vol)
                        SetSoundVolume(_vol);

                    sndOpen(_file);
                    sndPlay(_loop);
                }
            }

            public static void Play(string _file, bool _loop)
            {
                Play(_file, _loop, 100);
            }

            public static void Play(string _file, int _vol)
            {
                Play(_file, false, _vol);
            }

            public static void Stop()
            {
                sndClose();
            }
        }

        #endregion

        #region irrKlang Libary

        #if irrKlang

        public class IrrKlangLib
        {
            protected static IrrKlang.ISoundEngine irrKlangEngine = new IrrKlang.ISoundEngine();
            protected static IrrKlang.ISound currentlyPlayingSound;

            public static void Play(string _file, bool _loop, int _vol)
            {
                if (File.Exists(_file))
                {
                    if (WindowsLib.GetSoundVolume() != _vol)
                        WindowsLib.SetSoundVolume(_vol);

                    Stop();
                    currentlyPlayingSound = irrKlangEngine.Play2D(_file, _loop);
                    currentlyPlayingSound.Volume = 1F;
                }
            }

            public static void Play(string _file, bool _loop)
            {
                Play(_file, _loop, 100);
            }

            public static void Play(string _file, int _vol)
            {
                Play(_file, false, _vol);
            }

            public static void Play(string _file)
            {
                Play(_file, false, 100);
            }

            public static void Stop()
            {
                if (currentlyPlayingSound != null)
                    currentlyPlayingSound.Stop();
            }
        }

        #endif

        #endregion
    }
}

#endregion
