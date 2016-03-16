
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region Si13n7 Dev. ® created code

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace SilDev
{
    public class MsgBox
    {
        private static IWin32Window _owner;

        private static WinAPI.SafeNativeMethods.HookProc _hookProc;

        private static WinAPI.SafeNativeMethods.EnumChildProc _enumProc;

        [ThreadStatic]
        private static IntPtr _hHook;

        public static bool MoveCursorToMsgBoxAtOwner = false;

        [ThreadStatic]
        private static int nButton;

        [StructLayout(LayoutKind.Sequential)]
        public static class ButtonText
        {
            public static bool OverrideEnabled = false;
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
            _owner = owner;
            Initialize();
            return MessageBox.Show(owner, text, caption, buttons, icon, defButton, options);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton)
        {
            _owner = owner;
            Initialize();
            return MessageBox.Show(owner, text, caption, buttons, icon, defButton);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            _owner = owner;
            Initialize();
            return MessageBox.Show(owner, text, caption, buttons, icon);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons)
        {
            _owner = owner;
            Initialize();
            return MessageBox.Show(owner, text, caption, buttons);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption)
        {
            _owner = owner;
            Initialize();
            return MessageBox.Show(owner, text, caption);
        }

        public static DialogResult Show(IWin32Window owner, string text)
        {
            _owner = owner;
            Initialize();
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
            _hookProc = new WinAPI.SafeNativeMethods.HookProc(MessageBoxHookProc);
            _enumProc = new WinAPI.SafeNativeMethods.EnumChildProc(MessageBoxEnumProc);
            _hHook = IntPtr.Zero;
        }

        private static void Initialize()
        {
            try
            {
                if (_hHook != IntPtr.Zero)
                    throw new NotSupportedException("Multiple calls are not supported.");
                if (_owner != null)
                {
                    if (_owner.Handle != IntPtr.Zero)
                    {
                        WinAPI.WINDOWPLACEMENT placement = new WinAPI.WINDOWPLACEMENT();
                        WinAPI.SafeNativeMethods.GetWindowPlacement(_owner.Handle, ref placement);
                        if (placement.showCmd == 2)
                            return;
                    }
                }
                if (_owner != null || ButtonText.OverrideEnabled)
                    _hHook = WinAPI.SafeNativeMethods.SetWindowsHookEx((int)WinAPI.Win32HookAction.WH_CALLWNDPROCRET, _hookProc, IntPtr.Zero, (int)WinAPI.SafeNativeMethods.GetCurrentThreadId());
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        private static IntPtr MessageBoxHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
                return WinAPI.SafeNativeMethods.CallNextHookEx(_hHook, nCode, wParam, lParam);
            WinAPI.CWPRETSTRUCT msg = (WinAPI.CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(WinAPI.CWPRETSTRUCT));
            IntPtr hook = _hHook;
            switch (msg.message)
            {
                case (int)WinAPI.Win32HookAction.HCBT_ACTIVATE:
                    try
                    {
                        if (_owner != null)
                        {
                            Rectangle recChild = new Rectangle(0, 0, 0, 0);
                            bool success = WinAPI.SafeNativeMethods.GetWindowRect(msg.hwnd, ref recChild);

                            int width = recChild.Width - recChild.X;
                            int height = recChild.Height - recChild.Y;

                            Rectangle recParent = new Rectangle(0, 0, 0, 0);
                            success = WinAPI.SafeNativeMethods.GetWindowRect(_owner.Handle, ref recParent);

                            Point ptCenter = new Point(0, 0);
                            ptCenter.X = recParent.X + ((recParent.Width - recParent.X) / 2);
                            ptCenter.Y = recParent.Y + ((recParent.Height - recParent.Y) / 2) - 10;

                            Point ptStart = new Point(0, 0);
                            ptStart.X = (ptCenter.X - (width / 2));
                            ptStart.Y = (ptCenter.Y - (height / 2));

                            ptStart.X = (ptStart.X < 0) ? 0 : ptStart.X;
                            ptStart.Y = (ptStart.Y < 0) ? 0 : ptStart.Y;

                            WinAPI.SafeNativeMethods.MoveWindow(msg.hwnd, ptStart.X, ptStart.Y, width, height, false);
                            if (MoveCursorToMsgBoxAtOwner)
                                WinAPI.SetCursorPos(msg.hwnd, new Point(width / 2, (height / 2) + 24));
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex);
                    }
                    if (!ButtonText.OverrideEnabled)
                        return MessageBoxUnhookProc();
                    break;
                case (int)WinAPI.Win32HookAction.WM_INITDIALOG:
                    if (ButtonText.OverrideEnabled)
                    {
                        try
                        {
                            int nLength = WinAPI.SafeNativeMethods.GetWindowTextLength(msg.hwnd);
                            StringBuilder className = new StringBuilder(10);
                            WinAPI.SafeNativeMethods.GetClassName(msg.hwnd, className, className.Capacity);
                            if (className.ToString() == "#32770")
                            {
                                nButton = 0;
                                WinAPI.SafeNativeMethods.EnumChildWindows(msg.hwnd, _enumProc, IntPtr.Zero);
                                if (nButton == 1)
                                {
                                    IntPtr hButton = WinAPI.SafeNativeMethods.GetDlgItem(msg.hwnd, 2);
                                    if (hButton != IntPtr.Zero)
                                        WinAPI.SafeNativeMethods.SetWindowText(hButton, ButtonText.OK);
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
            return WinAPI.SafeNativeMethods.CallNextHookEx(hook, nCode, wParam, lParam);
        }

        private static IntPtr MessageBoxUnhookProc()
        {
            WinAPI.SafeNativeMethods.UnhookWindowsHookEx(_hHook);
            _hHook = IntPtr.Zero;
            _owner = null;
            if (ButtonText.OverrideEnabled)
                ButtonText.OverrideEnabled = false;
            return _hHook;
        }

        private static bool MessageBoxEnumProc(IntPtr hWnd, IntPtr lParam)
        {
            StringBuilder className = new StringBuilder(10);
            WinAPI.SafeNativeMethods.GetClassName(hWnd, className, className.Capacity);
            if (className.ToString() == "Button")
            {
                int ctlId = WinAPI.SafeNativeMethods.GetDlgCtrlID(hWnd);
                switch (ctlId)
                {
                    case 1:
                        WinAPI.SafeNativeMethods.SetWindowText(hWnd, ButtonText.OK);
                        break;
                    case 2:
                        WinAPI.SafeNativeMethods.SetWindowText(hWnd, ButtonText.Cancel);
                        break;
                    case 3:
                        WinAPI.SafeNativeMethods.SetWindowText(hWnd, ButtonText.Abort);
                        break;
                    case 4:
                        WinAPI.SafeNativeMethods.SetWindowText(hWnd, ButtonText.Retry);
                        break;
                    case 5:
                        WinAPI.SafeNativeMethods.SetWindowText(hWnd, ButtonText.Ignore);
                        break;
                    case 6:
                        WinAPI.SafeNativeMethods.SetWindowText(hWnd, ButtonText.Yes);
                        break;
                    case 7:
                        WinAPI.SafeNativeMethods.SetWindowText(hWnd, ButtonText.No);
                        break;
                }
                nButton++;
            }
            return true;
        }
    }
}

#endregion
          