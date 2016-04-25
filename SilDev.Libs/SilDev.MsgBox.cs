
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows.Forms;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.Convert"/>.cs</para>
    /// <para><see cref="SilDev.Crypt"/>.cs</para>
    /// <para><see cref="SilDev.Log"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class MsgBox
    {
        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods
        {
            internal delegate bool EnumChildProc(IntPtr hWnd, IntPtr lParam);
            internal delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool ClientToScreen(IntPtr hWnd, ref Point point);

            [DllImport("user32.dll", SetLastError = true)]
            internal static extern bool EnumChildWindows(IntPtr hWndParent, EnumChildProc lpEnumFunc, IntPtr lParam);

            [DllImport("user32.dll", EntryPoint = "GetClassNameW", BestFitMapping = false, SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Ansi)]
            internal static extern int GetClassName(IntPtr hWnd, [MarshalAs(UnmanagedType.LPTStr)]StringBuilder lpClassName, int nMaxCount);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            internal static extern uint GetCurrentThreadId();

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            internal static extern int GetDlgCtrlID(IntPtr hwndCtl);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            internal static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

            [DllImport("user32.dll", EntryPoint = "GetWindowTextLengthW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int GetWindowTextLength(IntPtr hWnd);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern uint SetCursorPos(uint x, uint y);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

            [DllImport("user32.dll", EntryPoint = "SetWindowTextW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool SetWindowText(IntPtr hWnd, string lpString);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int UnhookWindowsHookEx(IntPtr idHook);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CWPRETSTRUCT
        {
            internal IntPtr lResult;
            internal IntPtr lParam;
            internal IntPtr wParam;
            internal uint message;
            internal IntPtr hwnd;
        };

        internal struct WINDOWPLACEMENT
        {
            internal int length;
            internal int flags;
            internal int showCmd;
            internal Point ptMinPosition;
            internal Point ptMaxPosition;
            internal Rectangle rcNormalPosition;
        }

        private enum Win32HookAction : int
        {
            HCBT_ACTIVATE = 0x5,
            WH_CALLWNDPROCRET = 0xC,
            WM_INITDIALOG = 0x110,
        }

        public static void SetCursorPos(IntPtr hWnd, Point point)
        {
            SafeNativeMethods.ClientToScreen(hWnd, ref point);
            SafeNativeMethods.SetCursorPos((uint)point.X, (uint)point.Y);
        }

        private static IWin32Window _owner;

        private static SafeNativeMethods.HookProc _hookProc;

        private static SafeNativeMethods.EnumChildProc _enumProc;

        [ThreadStatic]
        private static IntPtr _hHook;

        public static bool MoveCursorToMsgBoxAtOwner = false;

        [ThreadStatic]
        private static int nButton;

        [StructLayout(LayoutKind.Sequential)]
        public static class ButtonText
        {
            public static bool OverrideEnabled { get; set; } = false;
            public static string OK = "&OK";
            public static string Cancel = "&Cancel";
            public static string Abort = "&Abort";
            public static string Retry = "&Retry";
            public static string Ignore = "&Ignore";
            public static string Yes = "&Yes";
            public static string No = "&No";
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton, MessageBoxOptions options)
        {
            Initialize(owner);
            return MessageBox.Show(owner, text, caption, buttons, icon, defButton, options);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton)
        {
            Initialize(owner);
            return MessageBox.Show(owner, text, caption, buttons, icon, defButton);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            Initialize(owner);
            return MessageBox.Show(owner, text, caption, buttons, icon);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons)
        {
            Initialize(owner);
            return MessageBox.Show(owner, text, caption, buttons);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption)
        {
            Initialize(owner);
            return MessageBox.Show(owner, text, caption);
        }

        public static DialogResult Show(IWin32Window owner, string text)
        {
            Initialize(owner);
            return MessageBox.Show(owner, text);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton, MessageBoxOptions options)
        {
            Initialize();
            return MessageBox.Show(text, caption, buttons, icon, defButton, options);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton)
        {
            Initialize();
            return MessageBox.Show(text, caption, buttons, icon, defButton);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            Initialize();
            return MessageBox.Show(text, caption, buttons, icon);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
        {
            Initialize();
            return MessageBox.Show(text, caption, buttons);
        }

        public static DialogResult Show(string text, string caption)
        {
            Initialize();
            return MessageBox.Show(text, caption);
        }

        public static DialogResult Show(string text)
        {
            Initialize();
            return MessageBox.Show(text);
        }

        static MsgBox()
        {
            _hookProc = new SafeNativeMethods.HookProc(MessageBoxHookProc);
            _enumProc = new SafeNativeMethods.EnumChildProc(MessageBoxEnumProc);
            _hHook = IntPtr.Zero;
        }

        private static void Initialize(IWin32Window owner = null)
        {
            try
            {
                if (_hHook != IntPtr.Zero)
                    throw new NotSupportedException("Multiple calls are not supported.");
                if (owner != null)
                {
                    _owner = owner;
                    if (_owner.Handle != IntPtr.Zero)
                    {
                        WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                        SafeNativeMethods.GetWindowPlacement(_owner.Handle, ref placement);
                        if (placement.showCmd == 2)
                            return;
                    }
                }
                if (_owner != null || ButtonText.OverrideEnabled)
                    _hHook = SafeNativeMethods.SetWindowsHookEx((int)Win32HookAction.WH_CALLWNDPROCRET, _hookProc, IntPtr.Zero, (int)SafeNativeMethods.GetCurrentThreadId());
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        private static IntPtr MessageBoxHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
                return SafeNativeMethods.CallNextHookEx(_hHook, nCode, wParam, lParam);
            CWPRETSTRUCT msg = (CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPRETSTRUCT));
            IntPtr hook = _hHook;
            switch (msg.message)
            {
                case (int)Win32HookAction.HCBT_ACTIVATE:
                    try
                    {
                        if (_owner != null)
                        {
                            Rectangle cRect = new Rectangle(0, 0, 0, 0);
                            if (SafeNativeMethods.GetWindowRect(msg.hwnd, ref cRect))
                            {
                                int width = cRect.Width - cRect.X;
                                int height = cRect.Height - cRect.Y;
                                Rectangle pRect = new Rectangle(0, 0, 0, 0);
                                if (SafeNativeMethods.GetWindowRect(_owner.Handle, ref pRect))
                                {
                                    Point ptCenter = new Point(pRect.X, pRect.Y);
                                    ptCenter.X += (pRect.Width - pRect.X) / 2;
                                    ptCenter.Y += ((pRect.Height - pRect.Y) / 2) - 10;

                                    Point ptStart = new Point(ptCenter.X, ptCenter.Y);
                                    ptStart.X -= width / 2;
                                    if (ptStart.X < 0)
                                        ptStart.X = 0;
                                    ptStart.Y -= height / 2;
                                    if (ptStart.Y < 0)
                                        ptStart.Y = 0;

                                    SafeNativeMethods.MoveWindow(msg.hwnd, ptStart.X, ptStart.Y, width, height, false);
                                    if (MoveCursorToMsgBoxAtOwner)
                                        SetCursorPos(msg.hwnd, new Point(width / 2, (height / 2) + 24));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex);
                    }
                    if (!ButtonText.OverrideEnabled)
                        return MessageBoxUnhookProc();
                    break;
                case (int)Win32HookAction.WM_INITDIALOG:
                    if (ButtonText.OverrideEnabled)
                    {
                        try
                        {
                            int nLength = SafeNativeMethods.GetWindowTextLength(msg.hwnd);
                            StringBuilder className = new StringBuilder(10);
                            SafeNativeMethods.GetClassName(msg.hwnd, className, className.Capacity);
                            if (className.ToString() == "#32770")
                            {
                                nButton = 0;
                                SafeNativeMethods.EnumChildWindows(msg.hwnd, _enumProc, IntPtr.Zero);
                                if (nButton == 1)
                                {
                                    IntPtr hButton = SafeNativeMethods.GetDlgItem(msg.hwnd, 2);
                                    if (hButton != IntPtr.Zero)
                                        SafeNativeMethods.SetWindowText(hButton, ButtonText.OK);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Debug(ex);
                        }
                    }
                    return MessageBoxUnhookProc();
            }
            return SafeNativeMethods.CallNextHookEx(hook, nCode, wParam, lParam);
        }

        private static IntPtr MessageBoxUnhookProc()
        {
            SafeNativeMethods.UnhookWindowsHookEx(_hHook);
            _hHook = IntPtr.Zero;
            _owner = null;
            if (ButtonText.OverrideEnabled)
                ButtonText.OverrideEnabled = false;
            return _hHook;
        }

        private static bool MessageBoxEnumProc(IntPtr hWnd, IntPtr lParam)
        {
            StringBuilder className = new StringBuilder(10);
            SafeNativeMethods.GetClassName(hWnd, className, className.Capacity);
            if (className.ToString() == "Button")
            {
                switch (SafeNativeMethods.GetDlgCtrlID(hWnd))
                {
                    case 1:
                        SafeNativeMethods.SetWindowText(hWnd, ButtonText.OK);
                        break;
                    case 2:
                        SafeNativeMethods.SetWindowText(hWnd, ButtonText.Cancel);
                        break;
                    case 3:
                        SafeNativeMethods.SetWindowText(hWnd, ButtonText.Abort);
                        break;
                    case 4:
                        SafeNativeMethods.SetWindowText(hWnd, ButtonText.Retry);
                        break;
                    case 5:
                        SafeNativeMethods.SetWindowText(hWnd, ButtonText.Ignore);
                        break;
                    case 6:
                        SafeNativeMethods.SetWindowText(hWnd, ButtonText.Yes);
                        break;
                    case 7:
                        SafeNativeMethods.SetWindowText(hWnd, ButtonText.No);
                        break;
                }
                nButton++;
            }
            return true;
        }
    }
}

#endregion
