
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.CONVERT"/>.cs</para>
    /// <para><see cref="SilDev.LOG"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class TASKBAR
    {
        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr LocalFree(IntPtr p);

            [DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern UIntPtr SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct APPBARDATA : IDisposable
        {
            internal uint cbSize;
            internal IntPtr hWnd;
            internal uint uCallbackMessage;
            internal uint uEdge;
            internal Rectangle rc;
            internal int lParam;

            public void Dispose()
            {
                if (hWnd != IntPtr.Zero)
                {
                    SafeNativeMethods.LocalFree(hWnd);
                    hWnd = IntPtr.Zero;
                }
            }
        }

        public enum Messages : int
        {
            New = 0x0,
            Remove = 0x1,
            QueryPos = 0x2,
            SetPos = 0x3,
            GetState = 0x4,
            GetTaskBarPos = 0x5,
            Activate = 0x6,
            GetAutoHideBar = 0x7,
            SetAutoHideBar = 0x8,
            WindowPosChanged = 0x9,
            SetState = 0xA
        }

        public enum Location
        {
            HIDDEN,
            TOP,
            BOTTOM,
            LEFT,
            RIGHT
        }

        public enum State
        {
            AutoHide = 0x1,
            AlwaysOnTop = 0x2
        }

        public static State GetState()
        {
            APPBARDATA msgData = new APPBARDATA();
            msgData.cbSize = (uint)Marshal.SizeOf(msgData);
            msgData.hWnd = SafeNativeMethods.FindWindow("System_TrayWnd", null);
            return (State)SafeNativeMethods.SHAppBarMessage((uint)Messages.GetState, ref msgData);
        }

        public static void SetState(State state)
        {
            APPBARDATA msgData = new APPBARDATA();
            msgData.cbSize = (uint)Marshal.SizeOf(msgData);
            msgData.hWnd = SafeNativeMethods.FindWindow("System_TrayWnd", null);
            msgData.lParam = (int)(state);
            SafeNativeMethods.SHAppBarMessage((uint)Messages.SetState, ref msgData);
        }

        public static Location GetLocation(IntPtr? windowHandle = null)
        {
            Screen screen = windowHandle == null ? Screen.PrimaryScreen : Screen.FromHandle((IntPtr)windowHandle);
            if (!(screen.WorkingArea == screen.Bounds))
            {
                if (!(screen.WorkingArea.Width == screen.Bounds.Width))
                    return (screen.WorkingArea.Left > 0) ? Location.LEFT : Location.RIGHT;
                return (screen.WorkingArea.Top > 0) ? Location.TOP : Location.BOTTOM;
            }
            return Location.HIDDEN;
        }

        public static int GetSize(IntPtr? windowHandle = null)
        {
            Screen screen = windowHandle == null ? Screen.PrimaryScreen : Screen.FromHandle((IntPtr)windowHandle);
            switch (GetLocation())
            {
                case Location.TOP:
                    return screen.WorkingArea.Top;
                case Location.BOTTOM:
                    return screen.Bounds.Bottom - screen.WorkingArea.Bottom;
                case Location.RIGHT:
                    return screen.Bounds.Right - screen.WorkingArea.Right;
                case Location.LEFT:
                    return screen.WorkingArea.Left;
                default:
                    return 0;
            }
        }

        public static class PROGRESS
        {
            public enum States
            {
                NoProgress = 0x0,
                Indeterminate = 0x1,
                Normal = 0x2,
                Error = 0x4,
                Paused = 0x8
            }

            [ComImport()]
            [Guid("EA1AFB91-9E28-4B86-90E9-9E9F8A5EEFAF")]
            [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            private interface ITaskBarList3
            {
                [PreserveSig]
                void HrInit();

                [PreserveSig]
                void AddTab(IntPtr hwnd);

                [PreserveSig]
                void DeleteTab(IntPtr hwnd);

                [PreserveSig]
                void ActivateTab(IntPtr hwnd);

                [PreserveSig]
                void SetActiveAlt(IntPtr hwnd);

                [PreserveSig]
                void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

                [PreserveSig]
                void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);

                [PreserveSig]
                void SetProgressState(IntPtr hwnd, States state);
            }

            [Guid("56FDF344-FD6D-11D0-958A-006097C9A090")]
            [ClassInterface(ClassInterfaceType.None)]
            [ComImport()]
            private class TaskbarInstance { }

            private static ITaskBarList3 taskbarInstance = (ITaskBarList3)new TaskbarInstance();
            private static bool taskbarSupported = Environment.OSVersion.Version >= new Version(6, 1);

            public static void SetState(IntPtr windowHandle, States taskbarState)
            {
                if (taskbarSupported)
                    taskbarInstance.SetProgressState(windowHandle, taskbarState);
            }

            public static void SetValue(IntPtr windowHandle, double progressValue, double progressMax)
            {
                if (taskbarSupported)
                    taskbarInstance.SetProgressValue(windowHandle, (ulong)progressValue, (ulong)progressMax);
            }
        }
    }
}

#endregion
