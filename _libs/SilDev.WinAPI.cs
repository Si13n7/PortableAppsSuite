
#region SILENT DEVELOPMENTS generated code

using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows.Forms;

namespace SilDev
{
    [ComVisible(false), SuppressUnmanagedCodeSecurity]
    internal static class WinAPI
    {
        #region WINDOWS API

        #region SAFE NATIVE METHODS

        [SuppressUnmanagedCodeSecurity]
        internal static class SafeNativeMethods
        {
            internal delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
            internal delegate void TimerProc(IntPtr hWnd, uint uMsg, UIntPtr nIDEvent, uint dwTime);

            #region DESKTOP WINDOW MANAGER FUNCTIONS

            [DllImport("dwmapi.dll", EntryPoint = "#127", PreserveSig = false, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern void DwmGetColorizationParameters(out DWM_COLORIZATION_PARAMS parameters);

            [DllImport("dwmapi.dll", EntryPoint = "#131", PreserveSig = false, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern void DwmSetColorizationParameters(ref DWM_COLORIZATION_PARAMS parameters, bool unknown);

            internal struct DWM_COLORIZATION_PARAMS
            {
                internal uint clrColor;
                internal uint clrAfterGlow;
                internal uint nIntensity;
                internal uint clrAfterGlowBalance;
                internal uint clrBlurBalance;
                internal uint clrGlassReflectionIntensity;
                internal bool fOpaque;
            }

            #endregion

            #region KERNEL32 FUNCTIONS

            [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            internal static extern int AllocConsole();

            [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            internal static extern IntPtr GetStdHandle(int nStdHandle);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr LoadLibrary(string lpFileName);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr LocalAlloc(int flag, int size);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr LocalFree(IntPtr p);

            #endregion

            #region USER32 FUNCTIONS

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool DrawMenuBar(IntPtr hWnd);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int EndDialog(IntPtr hDlg, IntPtr nResult);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

            [DllImport("user32.dll", EntryPoint = "FindWindowEx", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

            [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

            internal static IntPtr FindWindowByCaption(string lpWindowName)
            {
                return FindWindowByCaption(IntPtr.Zero, lpWindowName);
            }

            internal static string GetActiveWindowTitle()
            {
                const int nChars = 256;
                StringBuilder Buff = new StringBuilder(nChars);
                IntPtr handle = GetForegroundWindow();
                if (GetWindowText(handle, Buff, nChars) > 0)
                    return Buff.ToString();
                return string.Empty;
            }

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern short GetAsyncKeyState(ushort virtualKeyCode);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
            internal static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool ReleaseCapture();

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr GetMenu(IntPtr hWnd);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int GetMenuItemCount(IntPtr hMenu);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int GetWindowLong(IntPtr hWnd, Win32HookAction nIndex);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            internal static WINDOWPLACEMENT GetWindowPlacement(IntPtr hWnd)
            {
                WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                GetWindowPlacement(hWnd, ref placement);
                return placement;
            }

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

            internal struct WINDOWPLACEMENT
            {
                internal int length;
                internal int flags;
                internal int showCmd;
                internal Point ptMinPosition;
                internal Point ptMaxPosition;
                internal Rectangle rcNormalPosition;
            }

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxLength);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int GetWindowTextLength(IntPtr hWnd);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool SetForegroundWindow(IntPtr hWnd);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern UIntPtr SetTimer(IntPtr hWnd, UIntPtr nIDEvent, uint uElapse, TimerProc lpTimerFunc);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int SetWindowLong(IntPtr hWnd, Win32HookAction nIndex, int dwNewLong);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref CopyDataStruct lParam);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            private static extern IntPtr SendMessage(IntPtr hWnd, ushort Msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, int lParam);

            internal static int SendMessage(IntPtr hWnd, Win32HookAction wParam, int lParam)
            {
                return SendMessage(hWnd, (int)Win32HookAction.WM_SYSCOMMAND, (int)wParam, lParam);
            }

            internal static int SendMessage(IntPtr hWnd, Win32HookAction wParam)
            {
                return SendMessage(hWnd, wParam, 0);
            }

            [DllImport("user32.dll", EntryPoint = "SendMessageTimeout", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern uint SendMessageTimeoutText(IntPtr hWnd, int Msg, int countOfChars, StringBuilder wndTitle, uint flags, uint uTImeoutj, uint result);

            [DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern uint SHAppBarMessage(uint _dwMessage, ref APPBARDATA _pData);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool ShowWindow(IntPtr hWnd, Win32HookAction nCmdShow);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool ShowWindowAsync(IntPtr hWnd, Win32HookAction nCmdShow);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int UnhookWindowsHookEx(IntPtr idHook);

            #endregion

            #region WINDOWS MEDIA FUNCTIONS

            [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern long mciSendString(string _strCommand, StringBuilder _strReturn, int _iReturnLength, IntPtr _hwndCallback);

            [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern long PlaySound(byte[] _data, IntPtr _hMod, uint _dwFlags);

            [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int waveOutGetVolume(IntPtr _hwo, out uint _dwVolume);

            [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int waveOutSetVolume(IntPtr _hwo, uint _dwVolume);

            [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern uint timeBeginPeriod(uint _uPeriod);

            [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern uint timeEndPeriod(uint _period);

            #endregion
        }

        #endregion

        internal struct CopyDataStruct : IDisposable
        {
            internal IntPtr dwData;
            internal int cbData;
            internal IntPtr lpData;
            public void Dispose()
            {
                if (lpData != IntPtr.Zero)
                {
                    SafeNativeMethods.LocalFree(lpData);
                    lpData = IntPtr.Zero;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Message
        {
            internal IntPtr hWnd;
            internal uint msg;
            internal IntPtr wParam;
            internal IntPtr lParam;
            internal uint time;
            internal Point p;
        }

        internal static bool SendArgs(IntPtr targetHWnd, string args)
        {
            CopyDataStruct cds = new CopyDataStruct();
            try
            {
                cds.cbData = (args.Length + 1) * 2;
                cds.lpData = SafeNativeMethods.LocalAlloc(0x40, cds.cbData);
                Marshal.Copy(args.ToCharArray(), 0, cds.lpData, args.Length);
                cds.dwData = (IntPtr)1;
                SafeNativeMethods.SendMessage(targetHWnd, (int)Win32HookAction.WM_COPYDATA, IntPtr.Zero, ref cds);
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            finally
            {
                cds.Dispose();
            }
            return true;
        }

        public enum Win32HookAction : int
        {
            /// <summary>Retrieves the address of the dialog box procedure, or a handle
            /// representing the address of the dialog box procedure. You must use the
            /// CallWindowProc function to call the dialog box procedure.</summary>
            /// <remarks>See GWL_EXSTYLE</remarks>
            DWL_DLGPROC = 0x00000004,
            /// <summary>Retrieves the return value of a message processed in the dialog
            /// box procedure.</summary>
            /// <remarks>See DWL_MSGRESULT</remarks>
            DWL_MSGRESULT = 0x00000000,
            /// <summary>Retrieves extra information private to the application, such
            /// as handles or pointers.</summary>
            /// <remarks>See DWL_USER</remarks>
            DWL_USER = 0x00000008,
            /// <summary>Sets a new extended window style.</summary>
            /// <remarks>See GWL_EXSTYLE</remarks>
            GWL_EXSTYLE = -20,
            /// <summary>Sets a new application instance handle.</summary>
            /// <remarks>See GWL_HINSTANCE</remarks>
            GWL_HINSTANCE = -6,
            /// <summary>Sets a new identifier of the child window. The window cannot
            /// be a top-level window.</summary>
            /// <remarks>See GWL_ID</remarks>
            GWL_ID = -12,
            /// <summary>Sets a new window style.</summary>
            /// <remarks>See GWL_STYLE</remarks>
            GWL_STYLE = -16,
            /// <summary>Sets the user data associated with the window. This data is
            /// intended for use by the application that created the window. Its value
            /// is initially zero.</summary>
            /// <remarks>See GWL_USERDATA</remarks>
            GWL_USERDATA = -21,
            /// <summary>Sets a new address for the window procedure.
            /// You cannot change this attribute if the window does not belong to the
            /// same process as the calling thread.</summary>
            /// <remarks>See GWL_WNDPROC</remarks>
            GWL_WNDPROC = -4,
            /// <summary>The system is about to activate a window.</summary>
            /// <remarks>See HCBT_ACTIVATE</remarks>
            HCBT_ACTIVATE = 0x00000005,
            /// <summary>The system has removed a mouse message from the system
            /// message queue. Upon receiving this hook code, a CBT application
            /// must install a WH_JOURNALPLAYBACK hook procedure in response to
            /// the mouse message.</summary>
            /// <remarks>See HCBT_CLICKSKIPPED</remarks>
            HCBT_CLICKSKIPPED = 0x00000006,
            /// <summary>A window is about to be created. The system calls the hook
            /// procedure before sending the WM_CREATE or WM_NCCREATE message to the
            /// window. If the hook procedure returns a nonzero value, the system
            /// destroys the window; the CreateWindow function returns NULL, but the
            /// WM_DESTROY message is not sent to the window. If the hook procedure
            /// returns zero, the window is created normally.
            /// At the time of the HCBT_CREATEWND notification, the window has been
            /// created, but its final size and position may not have been determined
            /// and its parent window may not have been established. It is possible
            /// to send messages to the newly created window, although it has not yet
            /// received WM_NCCREATE or WM_CREATE messages. It is also possible to
            /// change the position in the z-order of the newly created window by
            /// modifying the hwndInsertAfter member of the CBT_CREATEWND
            /// structure.</summary>
            /// <remarks>See HCBT_CREATEWND</remarks>
            HCBT_CREATEWND = 0x00000003,
            /// <summary>A window is about to be destroyed.</summary>
            /// <remarks>See HCBT_DESTROYWND</remarks>
            HCBT_DESTROYWND = 0x00000004,
            /// <summary>The system has removed a keyboard message from the system
            /// message queue. Upon receiving this hook code, a CBT application must
            /// install a WH_JOURNALPLAYBACK hook procedure in response to the
            /// keyboard message.</summary>
            /// <remarks>See HCBT_KEYSKIPPED</remarks>
            HCBT_KEYSKIPPED = 0x00000007,
            /// <summary>A window is about to be minimized or maximized.</summary>
            /// <remarks>See HCBT_MINMAX</remarks>
            HCBT_MINMAX = 0x00000001,
            /// <summary>A window is about to be moved or sized.</summary>
            /// <remarks>See HCBT_MOVESIZE</remarks>
            HCBT_MOVESIZE = 0x00000000,
            /// <summary>The system has retrieved a WM_QUEUESYNC message from the 
            /// system message queue.</summary>
            /// <remarks>See HCBT_QS</remarks>
            HCBT_QS = 0x00000002,
            /// <summary>A window is about to receive the keyboard focus.</summary>
            /// <remarks>See HCBT_SETFOCUS</remarks>
            HCBT_SETFOCUS = 0x00000009,
            /// <summary>A system command is about to be carried out. This allows a CBT
            /// application to prevent task switching by means of hot keys.</summary>
            /// <remarks>See HCBT_SYSCOMMAND</remarks>
            HCBT_SYSCOMMAND = 0x00000008,
            /// <summary>Indicates that the uPosition parameter gives the identifier
            /// of the menu item. The MF_BYCOMMAND flag is the default if neither the
            /// MF_BYCOMMAND nor MF_BYPOSITION flag is specified.</summary>
            /// <remarks>See MF_BYCOMMAND</remarks>
            MF_BYCOMMAND = 0x00000000,
            /// <summary>Indicates that the uPosition parameter gives the zero-based
            /// relative position of the menu item.</summary>
            /// <remarks>See MF_BYPOSITION</remarks>
            MF_BYPOSITION = 0x00000400,
            /// <summary>Uses a bitmap as the menu item. The lpNewItem parameter
            /// contains a handle to the bitmap.</summary>
            /// <remarks>See MF_BITMAP</remarks>
            MF_BITMAP = 0x00000004,
            /// <summary>Places a check mark next to the item. If your application provides
            /// check-mark bitmaps (see the SetMenuItemBitmaps function), this flag displays
            /// a selected bitmap next to the menu item.</summary>
            /// <remarks>See MF_CHECKED</remarks>
            MF_CHECKED = 0x00000008,
            /// <summary>Disables the menu item so that it cannot be selected, but
            /// this flag does not gray it.</summary>
            /// <remarks>See MF_DISABLED</remarks>
            MF_DISABLED = 0x00000002,
            /// <summary>Enables the menu item so that it can be selected and restores
            /// it from its grayed state.</summary>
            /// <remarks>See MF_ENABLED</remarks>
            MF_ENABLED = 0x00000000,
            /// <summary>Disables the menu item and grays it so that it cannot be selected.</summary>
            /// <remarks>See MF_GRAYED</remarks>
            MF_GRAYED = 0x00000001,
            /// <summary>Functions the same as the MF_MENUBREAK flag for a menu bar. For
            /// a drop-down menu, submenu, or shortcut menu, the new column is separated
            /// from the old column by a vertical line.</summary>
            /// <remarks>See MF_MENUBARBREAK</remarks>
            MF_MENUBARBREAK = 0x00000020,
            /// <summary>Places the item on a new line (for menu bars) or in a new column (for
            /// a drop-down menu, submenu, or shortcut menu) without separating columns.</summary>
            /// <remarks>See MF_MENUBREAK</remarks>
            MF_MENUBREAK = 0x00000040,
            /// <summary>Specifies that the item is an owner-drawn item. Before the menu
            /// is displayed for the first time, the window that owns the menu receives
            /// a WM_MEASUREITEM message to retrieve the width and height of the menu item.
            /// The WM_DRAWITEM message is then sent to the window procedure of the owner
            /// window whenever the appearance of the menu item must be updated.</summary>
            /// <remarks>See MF_OWNERDRAW</remarks>
            MF_OWNERDRAW = 0x00000100,
            /// <summary>Specifies that the menu item opens a drop-down menu or submenu. The
            /// uIDNewItem parameter specifies a handle to the drop-down menu or submenu.
            /// This flag is used to add a menu name to a menu bar or a menu item that opens
            /// a submenu to a drop-down menu, submenu, or shortcut menu.</summary>
            /// <remarks>See MF_POPUP</remarks>
            MF_POPUP = 0x00000010,
            /// <summary>Draws a horizontal dividing line. This flag is used only in a
            /// drop-down menu, submenu, or shortcut menu. The line cannot be grayed,
            /// disabled, or highlighted. The lpNewItem and uIDNewItem parameters are
            /// ignored.</summary>
            /// <remarks>See MF_SEPARATOR</remarks>
            MF_SEPARATOR = 0x00000800,
            /// <summary>Specifies that the menu item is a text string; the lpNewItem
            /// parameter is a pointer to the string.</summary>
            /// <remarks>See MF_STRING</remarks>
            MF_STRING = 0x00000000,
            /// <summary>Does not place a check mark next to the item (the default). If
            /// your application supplies check-mark bitmaps (see the SetMenuItemBitmaps
            /// function), this flag displays a clear bitmap next to the menu item.</summary>
            /// <remarks>See MF_UNCHECKED</remarks>
            MF_UNCHECKED = 0x00000000,
            /// <summary>Remove uPosition parameters.</summary>
            /// <remarks>See MF_REMOVE</remarks>
            MF_REMOVE = 0x00001000,
            /// <summary>The input event occurred in a message box or dialog box.</summary>
            /// <remarks>See MSGF_DIALOGBOX</remarks>
            MSGF_DIALOGBOX = 0x00000000,
            /// <summary>The input event occurred in a menu.</summary>
            /// <remarks>See MSGF_MENU</remarks>
            MSGF_MENU = 0x00000002,
            /// <summary>The input event occurred in a scroll bar.</summary>
            /// <remarks>See MSGF_SCROLLBAR</remarks>
            MSGF_SCROLLBAR = 0x00000005,
            /// <summary>Closes the window.</summary>
            /// <remarks>See SC_CLOSE</remarks>
            SC_CLOSE = 0x0000F060,
            /// <summary>Changes the cursor to a question mark with a pointer. If the
            /// user then clicks a control in the dialog box, the control receives a
            /// WM_HELP message.</summary>
            /// <remarks>See SC_CONTEXTHELP</remarks>
            SC_CONTEXTHELP = 0x0000F180,
            /// <summary>Selects the default item; the user double-clicked the window
            /// menu.</summary>
            /// <remarks>See SC_DEFAULT</remarks>
            SC_DEFAULT = 0x0000F160,
            /// <summary>Activates the window associated with the application-specified
            /// hot key. The lParam parameter identifies the window to activate.</summary>
            /// <remarks>See SC_HOTKEY</remarks>
            SC_HOTKEY = 0x0000F150,
            /// <summary>Scrolls horizontally.</summary>
            /// <remarks>See SC_HSCROLL</remarks>
            SC_HSCROLL = 0x0000F080,
            /// <summary>Retrieves the window menu as a result of a keystroke. For more
            /// information, see the Remarks section.</summary>
            /// <remarks>See SC_KEYMENU</remarks>
            SC_KEYMENU = 0x0000F100,
            /// <summary>Maximizes the window.</summary>
            /// <remarks>See SC_MAXIMIZE</remarks>
            SC_MAXIMIZE = 0x0000F030,
            /// <summary>Minimizes the window.</summary>
            /// <remarks>See SC_MINIMIZE</remarks>
            SC_MINIMIZE = 0x0000F020,
            /// <summary>Sets the state of the display. This command supports devices
            /// that have power-saving features, such as a battery-powered personal
            /// computer. - The lParam parameter can have the following values: -1
            /// (the display is powering on), 1 (the display is going to low power),
            /// 2 (the display is being shut off)</summary>
            /// <remarks>See SC_MONITORPOWER</remarks>
            SC_MONITORPOWER = 0x0000F170,
            /// <summary>Retrieves the window menu as a result of a mouse click.</summary>
            /// <remarks>See SC_MOUSEMENU</remarks>
            SC_MOUSEMENU = 0x0000F090,
            /// <summary>Moves the window.</summary>
            /// <remarks>See SC_MOVE</remarks>
            SC_MOVE = 0x0000F010,
            /// <summary>Moves to the next window.</summary>
            /// <remarks>See SC_NEXTWINDOW</remarks>
            SC_NEXTWINDOW = 0x0000F040,
            /// <summary>Moves to the previous window.</summary>
            /// <remarks>See SC_PREVWINDOW</remarks>
            SC_PREVWINDOW = 0x0000F050,
            /// <summary>Restores the window to its normal position and size.</summary>
            /// <remarks>See SC_RESTORE</remarks>
            SC_RESTORE = 0x0000F120,
            /// <summary>Executes the screen saver application specified in the
            /// [boot] section of the System.ini file.</summary>
            /// <remarks>See SC_SCREENSAVE</remarks>
            SC_SCREENSAVE = 0x0000F140,
            /// <summary>Sizes the window.</summary>
            /// <remarks>See SC_SIZE</remarks>
            SC_SIZE = 0x0000F000,
            /// <summary>Activates the Start menu.</summary>
            /// <remarks>See SC_TASKLIST</remarks>
            SC_TASKLIST = 0x0000F130,
            /// <summary>Scrolls vertically.</summary>
            /// <remarks>See SC_VSCROLL</remarks>
            SC_VSCROLL = 0x0000F070,
            /// <summary>Indicates whether the screen saver is secure.</summary>
            /// <remarks>See SCF_ISSECURE</remarks>
            SCF_ISSECURE = 0x00000001,
            /// <summary>Minimizes a window, even if the thread that owns the window
            /// is not responding. This flag should only be used when minimizing
            /// windows from a different thread.</summary>
            /// <remarks>See SW_FORCEMINIMIZE</remarks>
            SW_FORCEMINIMIZE = 0x0000000B,
            /// <summary>Hides the window and activates another window.</summary>
            /// <remarks>See SW_HIDE</remarks>
            SW_HIDE = 0x00000000,
            /// <summary>Maximizes the specified window.</summary>
            /// <remarks>See SW_MAXIMIZE</remarks>
            SW_MAXIMIZE = 0x00000003,
            /// <summary>Minimizes the specified window and activates the next
            /// top-level window in the Z order.</summary>
            /// <remarks>See SW_MINIMIZE</remarks>
            SW_MINIMIZE = 0x00000006,
            /// <summary>Activates and displays the window. If the window is minimized or
            /// maximized, the system restores it to its original size and position. An
            /// application should specify this flag when restoring a minimized window.</summary>
            /// <remarks>See SW_RESTORE</remarks>
            SW_RESTORE = 0x00000009,
            /// <summary>Activates the window and displays it in its current size and position.</summary>
            /// <remarks>See SW_SHOW</remarks>
            SW_SHOW = 0x00000005,
            /// <summary>Sets the show state based on the SW_ value specified in the
            /// STARTUPINFO structure passed to the CreateProcess function by the
            /// program that started the application.</summary>
            /// <remarks>See SW_SHOWDEFAULT</remarks>
            SW_SHOWDEFAULT = 0x0000000A,
            /// <summary>Activates the window and displays it as a maximized window.</summary>
            /// <remarks>See SW_SHOWMAXIMIZED</remarks>
            SW_SHOWMAXIMIZED = 0x00000003,
            /// <summary>Activates the window and displays it as a minimized window.</summary>
            /// <remarks>See SW_SHOWMINIMIZED</remarks>
            SW_SHOWMINIMIZED = 0x00000002,
            /// <summary>Displays the window as a minimized window. This value is similar
            /// to SW_SHOWMINIMIZED, except the window is not activated.</summary>
            /// <remarks>See SW_SHOWMINNOACTIVE</remarks>
            SW_SHOWMINNOACTIVE = 0x00000007,
            /// <summary>Displays the window in its current size and position. This value
            /// is similar to SW_SHOW, except that the window is not activated.</summary>
            /// <remarks>See SW_SHOWNA</remarks>
            SW_SHOWNA = 0x00000008,
            /// <summary>Displays a window in its most recent size and position. This value is
            /// similar to SW_SHOWNORMAL, except that the window is not activated.</summary>
            /// <remarks>See SW_SHOWNOACTIVATE</remarks>
            SW_SHOWNOACTIVATE = 0x00000004,
            /// <summary>Activates and displays a window. If the window is minimized or
            /// maximized, the system restores it to its original size and position. An
            /// application should specify this flag when displaying the window for the
            /// first time.</summary>
            /// <remarks>See SW_SHOWNORMAL</remarks>
            SW_SHOWNORMAL = 0x00000001,
            /// <summary>The WH_CALLWNDPROC and WH_CALLWNDPROCRET hooks enable you to
            /// monitor messages sent to window procedures. The system calls a
            /// WH_CALLWNDPROC hook procedure before passing the message to the receiving
            /// window procedure, and calls the WH_CALLWNDPROCRET hook procedure after
            /// the window procedure has processed the message.
            /// The WH_CALLWNDPROCRET hook passes a pointer to a CWPRETSTRUCT structure
            /// to the hook procedure. The structure contains the return value from the
            /// window procedure that processed the message, as well as the message
            /// parameters associated with the message. Subclassing the window does not
            /// work for messages set between processes.</summary>
            /// <remarks>See WH_CALLWNDPROC</remarks>
            WH_CALLWNDPROC = 0x00000004,
            /// <summary>The WH_CALLWNDPROC and WH_CALLWNDPROCRET hooks enable you to
            /// monitor messages sent to window procedures. The system calls a
            /// WH_CALLWNDPROC hook procedure before passing the message to the receiving
            /// window procedure, and calls the WH_CALLWNDPROCRET hook procedure after
            /// the window procedure has processed the message.
            /// The WH_CALLWNDPROCRET hook passes a pointer to a CWPRETSTRUCT structure
            /// to the hook procedure. The structure contains the return value from the
            /// window procedure that processed the message, as well as the message
            /// parameters associated with the message. Subclassing the window does not
            /// work for messages set between processes.</summary>
            /// <remarks>See WH_CALLWNDPROCRET</remarks>
            WH_CALLWNDPROCRET = 0x0000000C,
            /// <summary>The system calls a WH_CBT hook procedure before activating,
            /// creating, destroying, minimizing, maximizing, moving, or sizing a
            /// window; before completing a system command; before removing a mouse
            /// or keyboard event from the system message queue; before setting the
            /// input focus; or before synchronizing with the system message queue.
            /// The value the hook procedure returns determines whether the system
            /// allows or prevents one of these operations. The WH_CBT hook is intended
            /// primarily for computer-based training (CBT) applications.</summary>
            /// <remarks>See WH_CBT</remarks>
            WH_CBT = 0x00000005,
            /// <summary>The system calls a WH_DEBUG hook procedure before calling hook
            /// procedures associated with any other hook in the system. You can use
            /// this hook to determine whether to allow the system to call hook procedures
            /// associated with other types of hooks.</summary>
            /// <remarks>See WH_DEBUG</remarks>
            WH_DEBUG = 0x00000009,
            /// <summary>The WH_FOREGROUNDIDLE hook enables you to perform low priority
            /// tasks during times when its foreground thread is idle. The system calls
            /// a WH_FOREGROUNDIDLE hook procedure when the application's foreground
            /// thread is about to become idle.</summary>
            /// <remarks>See WH_FOREGROUNDIDLE</remarks>
            WH_FOREGROUNDIDLE = 0x0000000B,
            /// <summary>The WH_GETMESSAGE hook enables an application to monitor messages
            /// about to be returned by the GetMessage or PeekMessage function. You can use
            /// the WH_GETMESSAGE hook to monitor mouse and keyboard input and other messages
            /// posted to the message queue.</summary>
            /// <remarks>See WH_GETMESSAGE</remarks>
            WH_GETMESSAGE = 0x00000003,
            /// <summary></summary>
            /// <remarks>See WH_HARDWARE</remarks>
            WH_HARDWARE = 0x00000008,
            /// <summary>The WH_JOURNALPLAYBACK hook enables an application to insert
            /// messages into the system message queue. You can use this hook to play
            /// back a series of mouse and keyboard events recorded earlier by using
            /// WH_JOURNALRECORD. Regular mouse and keyboard input is disabled as long
            /// as a WH_JOURNALPLAYBACK hook is installed. A WH_JOURNALPLAYBACK hook
            /// is a global hook—it cannot be used as a thread-specific hook.
            /// The WH_JOURNALPLAYBACK hook returns a time-out value. This value tells
            /// the system how many milliseconds to wait before processing the current
            /// message from the playback hook. This enables the hook to control the
            /// timing of the events it plays back.</summary>
            /// <remarks>See WH_JOURNALPLAYBACK</remarks>
            WH_JOURNALPLAYBACK = 0x00000001,
            /// <summary>The WH_JOURNALRECORD hook enables you to monitor and record
            /// input events. Typically, you use this hook to record a sequence of
            /// mouse and keyboard events to play back later by using WH_JOURNALPLAYBACK.
            /// The WH_JOURNALRECORD hook is a global hook—it cannot be used as a
            /// thread-specific hook.</summary>
            /// <remarks>See WH_JOURNALRECORD</remarks>
            WH_JOURNALRECORD = 0x00000000,
            /// <summary>The WH_KEYBOARD hook enables an application to monitor message
            /// traffic for WM_KEYDOWN and WM_KEYUP messages about to be returned by the
            /// GetMessage or PeekMessage function. You can use the WH_KEYBOARD hook to
            /// monitor keyboard input posted to a message queue.</summary>
            /// <remarks>See WH_KEYBOARD</remarks>
            WH_KEYBOARD = 0x00000002,
            /// <summary>The WH_KEYBOARD_LL hook enables you to monitor keyboard
            /// input events about to be posted in a thread input queue.</summary>
            /// <remarks>See WH_KEYBOARD_LL</remarks>
            WH_KEYBOARD_LL = 0x0000000D,
            /// <summary>The WH_MOUSE hook enables you to monitor mouse messages about
            /// to be returned by the GetMessage or PeekMessage function. You can use
            /// the WH_MOUSE hook to monitor mouse input posted to a message queue.</summary>
            /// <remarks>See WH_MOUSE</remarks>
            WH_MOUSE = 0x00000007,
            /// <summary>The WH_MOUSE_LL hook enables you to monitor mouse input events
            /// about to be posted in a thread input queue.</summary>
            /// <remarks>See WH_MOUSE_LL</remarks>
            WH_MOUSE_LL = 0x0000000E,
            /// <summary>The WH_MSGFILTER and WH_SYSMSGFILTER hooks enable you to
            /// monitor messages about to be processed by a menu, scroll bar,
            /// message box, or dialog box, and to detect when a different window
            /// is about to be activated as a result of the user's pressing the
            /// ALT+TAB or ALT+ESC key combination. The WH_MSGFILTER hook can only
            /// monitor messages passed to a menu, scroll bar, message box, or
            /// dialog box created by the application that installed the hook
            /// procedure. The WH_SYSMSGFILTER hook monitors such messages for
            /// all applications.</summary>
            /// <remarks>See WH_MSGFILTER</remarks>
            WH_MSGFILTER = -1,
            /// <summary>A shell application can use the WH_SHELL hook to receive
            /// important notifications. The system calls a WH_SHELL hook procedure
            /// when the shell application is about to be activated and when a
            /// top-level window is created or destroyed.
            /// Note that custom shell applications do not receive WH_SHELL messages.
            /// Therefore, any application that registers itself as the default shell
            /// must call the SystemParametersInfo function before it (or any other
            /// application) can receive WH_SHELL messages. This function must be called
            /// with SPI_SETMINIMIZEDMETRICS and a MINIMIZEDMETRICS structure. Set the
            /// iArrange member of this structure to ARW_HIDE.</summary>
            /// <remarks>See WH_SHELL</remarks>
            WH_SHELL = 0x0000000A,
            /// <summary>The WH_MSGFILTER and WH_SYSMSGFILTER hooks enable you to
            /// monitor messages about to be processed by a menu, scroll bar,
            /// message box, or dialog box, and to detect when a different window
            /// is about to be activated as a result of the user's pressing the
            /// ALT+TAB or ALT+ESC key combination. The WH_MSGFILTER hook can only
            /// monitor messages passed to a menu, scroll bar, message box, or
            /// dialog box created by the application that installed the hook
            /// procedure. The WH_SYSMSGFILTER hook monitors such messages for
            /// all applications.
            /// The WH_MSGFILTER and WH_SYSMSGFILTER hooks enable you to perform
            /// message filtering during modal loops that is equivalent to the
            /// filtering done in the main message loop. For example, an application
            /// often examines a new message in the main loop between the time it
            /// retrieves the message from the queue and the time it dispatches the
            /// message, performing special processing as appropriate. However,
            /// during a modal loop, the system retrieves and dispatches messages
            /// without allowing an application the chance to filter the messages in
            /// its main message loop. If an application installs a WH_MSGFILTER or
            /// WH_SYSMSGFILTER hook procedure, the system calls the procedure during
            /// the modal loop.</summary>
            /// <remarks>See WH_SYSMSGFILTER</remarks>
            WH_SYSMSGFILTER = 0x00000006,
            /// <summary>If the receiving application processes this message, it should
            /// return TRUE; otherwise, it should return FALSE.
            /// The data being passed must not contain pointers or other references to
            /// objects not accessible to the application receiving the data.
            /// While this message is being sent, the referenced data must not be changed
            /// by another thread of the sending process.
            /// The receiving application should consider the data read-only. The lParam
            /// parameter is valid only during the processing of the message. The receiving
            /// application should not free the memory referenced by lParam. If the receiving
            /// application must access the data after SendMessage returns, it must copy the
            /// data into a local buffer.</summary>
            /// <remarks>See WM_COPYDATA</remarks>
            WM_COPYDATA = 0x0000004A,
            /// <summary>A window receives this message when the user chooses a command
            /// from the Window menu (formerly known as the system or control menu) or
            /// when the user chooses the maximize button, minimize button, restore
            /// button, or close button.</summary>
            /// <remarks>See WM_SYSCOMMAND</remarks>
            WM_SYSCOMMAND = 0x00000112,
            /// <summary>The window has a thin-line border.</summary>
            /// <remarks>See WS_BORDER</remarks>
            WS_BORDER = 0x00800000,
            /// <summary>The window has a title bar (includes the WS_BORDER style).</summary>
            /// <remarks>See WS_CAPTION</remarks>
            WS_CAPTION = 0x00C00000,
            /// <summary>The window is a child window. A window with this style cannot
            /// have a menu bar. This style cannot be used with the WS_POPUP style.</summary>
            /// <remarks>See WS_CHILD</remarks>
            WS_CHILD = 0x40000000,
            /// <summary>Same as the WS_CHILD style.</summary>
            /// <remarks>See WS_CHILDWINDOW</remarks>
            WS_CHILDWINDOW = WS_CHILD,
            /// <summary>Excludes the area occupied by child windows when drawing
            /// occurs within the parent window. This style is used when creating
            /// the parent window.</summary>
            /// <remarks>See WS_CLIPCHILDREN</remarks>
            WS_CLIPCHILDREN = 0x02000000,
            /// <summary>Clips child windows relative to each other; that is, when a
            /// particular child window receives a WM_PAINT message, the WS_CLIPSIBLINGS
            /// style clips all other overlapping child windows out of the region of the
            /// child window to be updated. If WS_CLIPSIBLINGS is not specified and child
            /// windows overlap, it is possible, when drawing within the client area of a
            /// child window, to draw within the client area of a neighboring child
            /// window.</summary>
            /// <remarks>See WS_CLIPSIBLINGS</remarks>
            WS_CLIPSIBLINGS = 0x04000000,
            /// <summary>The window is initially disabled. A disabled window cannot
            /// receive input from the user. To change this after a window has been
            /// created, use the EnableWindow function.</summary>
            /// <remarks>See WS_DISABLED</remarks>
            WS_DISABLED = 0x08000000,
            /// <summary>The window has a border of a style typically used with dialog
            /// boxes. A window with this style cannot have a title bar.</summary>
            /// <remarks>See WS_DLGFRAME</remarks>
            WS_DLGFRAME = 0x00400000,
            /// <summary>The window is the first control of a group of controls. The
            /// group consists of this first control and all controls defined after
            /// it, up to the next control with the WS_GROUP style. The first control
            /// in each group usually has the WS_TABSTOP style so that the user can
            /// move from group to group. The user can subsequently change the keyboard
            /// focus from one control in the group to the next control in the group by
            /// using the direction keys.</summary>
            /// <remarks>See WS_GROUP</remarks>
            WS_GROUP = 0x00020000,
            /// <summary>The window has a horizontal scroll bar.</summary>
            /// <remarks>See WS_HSCROLL</remarks>
            WS_HSCROLL = 0x00100000,
            /// <summary>The window is initially minimized. Same as the WS_MINIMIZE style.</summary>
            /// <remarks>See WS_ICONIC</remarks>
            WS_ICONIC = WS_MINIMIZE,
            /// <summary>The window is initially maximized.</summary>
            /// <remarks>See WS_MAXIMIZE</remarks>
            WS_MAXIMIZE = 0x01000000,
            /// <summary>The window has a maximize button. Cannot be combined with the
            /// WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary>
            /// <remarks>See WS_MAXIMIZEBOX</remarks>
            WS_MAXIMIZEBOX = 0x00010000,
            /// <summary>The window is initially minimized. Same as the WS_ICONIC style.</summary>
            /// <remarks>See WS_MINIMIZE</remarks>
            WS_MINIMIZE = 0x20000000,
            /// <summary>The window has a minimize button. Cannot be combined with the
            /// WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary>
            /// <remarks>See WS_MINIMIZEBOX</remarks>
            WS_MINIMIZEBOX = 0x00020000,
            /// <summary>The window is an overlapped window. An overlapped window has a
            /// title bar and a border. Same as the WS_TILED style.</summary>
            /// <remarks>See WS_OVERLAPPED</remarks>
            WS_OVERLAPPED = 0x00000000,
            /// <summary>The window is an overlapped window. Same as the WS_TILEDWINDOW style.</summary>
            /// <remarks>See WS_OVERLAPPEDWINDOW</remarks>
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            /*
            64-bit integer doesn't work for 32-bit applications
            /// <summary>The windows is a pop-up window. This style cannot be used with
            /// the WS_CHILD style.</summary>
            /// <remarks>See WS_POPUP</remarks>
            WS_POPUP = 0x80000000L,
            /// <summary>The window is a pop-up window. The WS_CAPTION and WS_POPUPWINDOW
            /// styles must be combined to make the window menu visible.</summary>
            /// <remarks>See WS_POPUPWINDOW</remarks>
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            */
            /// <summary>The window has a sizing border. Same as the WS_THICKFRAME style.</summary>
            /// <remarks>See WS_SIZEBOX</remarks>
            WS_SIZEBOX = 0x00040000,
            /// <summary>The window has a window menu on its title bar. The WS_CAPTION
            /// style must also be specified.</summary>
            /// <remarks>See WS_SYSMENU</remarks>
            WS_SYSMENU = 0x00080000,
            /// <summary>The window is a control that can receive the keyboard focus when
            /// the user presses the TAB key. Pressing the TAB key changes the keyboard
            /// focus to the next control with the WS_TABSTOP style.
            /// You can turn this style on and off to change dialog box navigation. To
            /// change this style after a window has been created, use the SetWindowLong
            /// function. For user-created windows and modeless dialogs to work with tab
            /// stops, alter the message loop to call the IsDialogMessage function.</summary>
            /// <remarks>See WS_TABSTOP</remarks>
            WS_TABSTOP = 0x00010000,
            /// <summary>The window has a sizing border. Same as the WS_SIZEBOX style.</summary>
            /// <remarks>See WS_THICKFRAME</remarks>
            WS_THICKFRAME = 0x00040000,
            /// <summary>The window is an overlapped window. An overlapped window has a
            /// title bar and a border. Same as the WS_OVERLAPPED style.</summary>
            /// <remarks>See WS_TILED</remarks>
            WS_TILED = 0x00000000,
            /// <summary>The window is an overlapped window. Same as the WS_OVERLAPPEDWINDOW style.</summary>
            /// <remarks>See WS_TILEDWINDOW</remarks>
            WS_TILEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            /// <summary>The window is initially visible.
            /// This style can be turned on and off by using the ShowWindow or SetWindowPos
            /// function.</summary>
            /// <remarks>See WS_VISIBLE</remarks>
            WS_VISIBLE = 0x10000000,
            /// <summary>The window has a vertical scroll bar.</summary>
            /// <remarks>See WS_VSCROLL</remarks>
            WS_VSCROLL = 0x00200000,
            /// <summary>The window accepts drag-drop files.</summary>
            /// <remarks>See WS_EX_ACCEPTFILES</remarks>
            WS_EX_ACCEPTFILES = 0x00000010,
            /// <summary>Forces a top-level window onto the taskbar when the window is visible.</summary>
            /// <remarks>See WS_EX_APPWINDOW</remarks>
            WS_EX_APPWINDOW = 0x00040000,
            /// <summary>The window has a border with a sunken edge.</summary>
            /// <remarks>See WS_EX_CLIENTEDGE</remarks>
            WS_EX_CLIENTEDGE = 0x00000200,
            /// <summary>Paints all descendants of a window in bottom-to-top painting order
            /// using double-buffering. For more information, see Remarks. This cannot be
            /// used if the window has a class style of either CS_OWNDC or CS_CLASSDC.</summary>
            /// <remarks>See WS_EX_COMPOSITED</remarks>
            WS_EX_COMPOSITED = 0x02000000,
            /// <summary>The title bar of the window includes a question mark. When the
            /// user clicks the question mark, the cursor changes to a question mark with
            /// a pointer. If the user then clicks a child window, the child receives a
            /// WM_HELP message. The child window should pass the message to the parent
            /// window procedure, which should call the WinHelp function using the
            /// HELP_WM_HELP command. The Help application displays a pop-up window that
            /// typically contains help for the child window.
            /// WS_EX_CONTEXTHELP cannot be used with the WS_MAXIMIZEBOX or
            /// WS_MINIMIZEBOX styles.</summary>
            /// <remarks>See WS_EX_CONTEXTHELP</remarks>
            WS_EX_CONTEXTHELP = 0x00000400,
            /// <summary>The window itself contains child windows that should take part
            /// in dialog box navigation. If this style is specified, the dialog manager
            /// recurses into children of this window when performing navigation
            /// operations such as handling the TAB key, an arrow key, or a keyboard
            /// mnemonic.</summary>
            /// <remarks>See WS_EX_CONTROLPARENT</remarks>
            WS_EX_CONTROLPARENT = 0x00010000,
            /// <summary>The window has a double border; the window can, optionally,
            /// be created with a title bar by specifying the WS_CAPTION style in the
            /// dwStyle parameter.</summary>
            /// <remarks>See WS_EX_DLGMODALFRAME</remarks>
            WS_EX_DLGMODALFRAME = 0x00000001,
            /// <summary>The window is a layered window. This style cannot be used
            /// if the window has a class style of either CS_OWNDC or CS_CLASSDC.</summary>
            /// <remarks>See WS_EX_LAYERED</remarks>
            WS_EX_LAYERED = 0x00080000,
            /// <summary>If the shell language is Hebrew, Arabic, or another language
            /// that supports reading order alignment, the horizontal origin of the
            /// window is on the right edge. Increasing horizontal values advance to
            /// the left.</summary>
            /// <remarks>See WS_EX_LAYOUTRTL</remarks>
            WS_EX_LAYOUTRTL = 0x00400000,
            /// <summary>The window has generic left-aligned properties. This is the default.</summary>
            /// <remarks>See WS_EX_LEFT</remarks>
            WS_EX_LEFT = 0x00000000,
            /// <summary>If the shell language is Hebrew, Arabic, or another language
            /// that supports reading order alignment, the vertical scroll bar (if
            /// present) is to the left of the client area. For other languages, the
            /// style is ignored.</summary>
            /// <remarks>See WS_EX_LEFTSCROLLBAR</remarks>
            WS_EX_LEFTSCROLLBAR = 0x00004000,
            /// <summary>The window text is displayed using left-to-right reading-order
            /// properties. This is the default.</summary>
            /// <remarks>See WS_EX_LTRREADING</remarks>
            WS_EX_LTRREADING = 0x00000000,
            /// <summary>The window is a MDI child window.</summary>
            /// <remarks>See WS_EX_MDICHILD</remarks>
            WS_EX_MDICHILD = 0x00000040,
            /// <summary>A top-level window created with this style does not become
            /// the foreground window when the user clicks it. The system does not
            /// bring this window to the foreground when the user minimizes or
            /// closes the foreground window.
            /// To activate the window, use the SetActiveWindow or SetForegroundWindow
            /// function.
            /// The window does not appear on the taskbar by default. To force the
            /// window to appear on the taskbar, use the WS_EX_APPWINDOW style.</summary>
            /// <remarks>See WS_EX_NOACTIVATE</remarks>
            WS_EX_NOACTIVATE = 0x08000000,
            /// <summary>The window does not pass its window layout to its child windows.</summary>
            /// <remarks>See WS_EX_NOINHERITLAYOUT</remarks>
            WS_EX_NOINHERITLAYOUT = 0x00100000,
            /// <summary>The child window created with this style does not send the
            /// WM_PARENTNOTIFY message to its parent window when it is created or
            /// destroyed.</summary>
            /// <remarks>See WS_EX_NOPARENTNOTIFY</remarks>
            WS_EX_NOPARENTNOTIFY = 0x00000004,
            /// <summary>The window does not render to a redirection surface. This
            /// is for windows that do not have visible content or that use mechanisms
            /// other than surfaces to provide their visual.</summary>
            /// <remarks>See WS_EX_NOREDIRECTIONBITMAP</remarks>
            WS_EX_NOREDIRECTIONBITMAP = 0x00200000,
            /// <summary>The window is an overlapped window.</summary>
            /// <remarks>See WS_EX_OVERLAPPEDWINDOW</remarks>
            WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
            /// <summary>The window is palette window, which is a modeless dialog
            /// box that presents an array of commands.</summary>
            /// <remarks>See WS_EX_PALETTEWINDOW</remarks>
            WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,
            /// <summary>The window has generic "right-aligned" properties. This
            /// depends on the window class. This style has an effect only if the
            /// shell language is Hebrew, Arabic, or another language that supports
            /// reading-order alignment; otherwise, the style is ignored.
            /// Using the WS_EX_RIGHT style for static or edit controls has the same
            /// effect as using the SS_RIGHT or ES_RIGHT style, respectively. Using
            /// this style with button controls has the same effect as using BS_RIGHT
            /// and BS_RIGHTBUTTON styles.</summary>
            /// <remarks>See WS_EX_RIGHT</remarks>
            WS_EX_RIGHT = 0x00001000,
            /// <summary>The vertical scroll bar (if present) is to the right of
            /// the client area. This is the default.</summary>
            /// <remarks>See WS_EX_RIGHTSCROLLBAR</remarks>
            WS_EX_RIGHTSCROLLBAR = 0x00000000,
            /// <summary>If the shell language is Hebrew, Arabic, or another language
            /// that supports reading-order alignment, the window text is displayed
            /// using right-to-left reading-order properties. For other languages,
            /// the style is ignored.</summary>
            /// <remarks>See WS_EX_RTLREADING</remarks>
            WS_EX_RTLREADING = 0x00002000,
            /// <summary>The window has a three-dimensional border style intended to
            /// be used for items that do not accept user input.</summary>
            /// <remarks>See WS_EX_STATICEDGE</remarks>
            WS_EX_STATICEDGE = 0x00020000,
            /// <summary>The window is intended to be used as a floating toolbar. A
            /// tool window has a title bar that is shorter than a normal title bar,
            /// and the window title is drawn using a smaller font. A tool window
            /// does not appear in the taskbar or in the dialog that appears when
            /// the user presses ALT+TAB. If a tool window has a system menu, its
            /// icon is not displayed on the title bar. However, you can display the
            /// system menu by right-clicking or by typing ALT+SPACE.</summary>
            /// <remarks>See WS_EX_TOOLWINDOW</remarks>
            WS_EX_TOOLWINDOW = 0x00000080,
            /// <summary>The window should be placed above all non-topmost windows
            /// and should stay above them, even when the window is deactivated.
            /// To add or remove this style, use the SetWindowPos function.</summary>
            /// <remarks>See WS_EX_TOPMOST</remarks>
            WS_EX_TOPMOST = 0x00000008,
            /// <summary>The window should not be painted until siblings beneath the
            /// window (that were created by the same thread) have been painted. The
            /// window appears transparent because the bits of underlying sibling
            /// windows have already been painted.
            /// To achieve transparency without these restrictions, use the
            /// SetWindowRgn function.</summary>
            /// <remarks>See WS_EX_TRANSPARENT</remarks>
            WS_EX_TRANSPARENT = 0x00000020,
            /// <summary>The window has a border with a raised edge.</summary>
            /// <remarks>See WS_EX_WINDOWEDGE</remarks>
            WS_EX_WINDOWEDGE = 0x00000100
        }

        #endregion

        #region WINDOW FUNCTIONS

        public static bool ExistsWindowByCaption(string lpWindowName)
        {
            return SafeNativeMethods.FindWindowByCaption(IntPtr.Zero, lpWindowName) != IntPtr.Zero;
        }

        public static void HideWindow(IntPtr _wndHandle)
        {
            SafeNativeMethods.ShowWindow(_wndHandle, Win32HookAction.SW_MINIMIZE);
            SafeNativeMethods.ShowWindow(_wndHandle, Win32HookAction.SW_HIDE);
        }

        public static void ShowWindow(IntPtr _wndHandle)
        {
            SafeNativeMethods.ShowWindow(_wndHandle, Win32HookAction.SW_RESTORE);
            SafeNativeMethods.ShowWindow(_wndHandle, Win32HookAction.SW_SHOW);
        }

        public static void RemoveFromTaskBar(IntPtr _wndHandle)
        {
            SafeNativeMethods.ShowWindow(_wndHandle, (int)Win32HookAction.SW_HIDE);
            SafeNativeMethods.SetWindowLong(_wndHandle, (int)Win32HookAction.GWL_EXSTYLE, SafeNativeMethods.GetWindowLong(_wndHandle, Win32HookAction.GWL_EXSTYLE) | (int)Win32HookAction.WS_EX_TOOLWINDOW);
            SafeNativeMethods.ShowWindow(_wndHandle, (int)Win32HookAction.SW_SHOW);
        }

        public static void RemoveWindowBorders(IntPtr _wndHandle)
        {
            try
            {
                IntPtr pFoundWindow = _wndHandle;
                int style = SafeNativeMethods.GetWindowLong(pFoundWindow, (int)Win32HookAction.GWL_STYLE);
                IntPtr HMENU = SafeNativeMethods.GetMenu(_wndHandle);
                int count = SafeNativeMethods.GetMenuItemCount(HMENU);
                for (int i = 0; i < count; i++)
                    SafeNativeMethods.RemoveMenu(HMENU, 0, ((uint)Win32HookAction.MF_BYPOSITION | (uint)Win32HookAction.MF_REMOVE));
                SafeNativeMethods.DrawMenuBar(_wndHandle);
                SafeNativeMethods.SetWindowLong(pFoundWindow, (int)Win32HookAction.GWL_STYLE, (style & ~(int)Win32HookAction.WS_SYSMENU));
                SafeNativeMethods.SetWindowLong(pFoundWindow, (int)Win32HookAction.GWL_STYLE, (style & ~(int)Win32HookAction.WS_CAPTION));
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        public static void SetWindowSize(IntPtr _wndHandle, int _width, int _height)
        {
            try
            {
                SafeNativeMethods.MoveWindow(_wndHandle, 0, 0, _width, _height, false);
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        public static void SetFullscreen(IntPtr _wndHandle)
        {
            SetWindowSize(_wndHandle, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        }

        public static void SetBorderlessFullscreen(IntPtr _wndHandle)
        {
            RemoveWindowBorders(_wndHandle);
            SetFullscreen(_wndHandle);
        }

        public static void MoveWindow_Mouse(IWin32Window owner, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left)
                {
                    SafeNativeMethods.ReleaseCapture();
                    SafeNativeMethods.SendMessage(owner.Handle, 0xA1, 0x2, 0);
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        #endregion

        #region TASKBAR FUNCTIONS

        public static class TaskBarProgress
        {
            public enum TaskBarStates
            {
                NoProgress = 0x00000000,
                Indeterminate = 0x00000001,
                Normal = 0x00000002,
                Error = 0x00000004,
                Paused = 0x00000008
            }

            [ComImport()]
            [Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
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
                void SetProgressState(IntPtr hwnd, TaskBarStates state);
            }

            [Guid("56FDF344-FD6D-11d0-958A-006097C9A090")]
            [ClassInterface(ClassInterfaceType.None)]
            [ComImport()]
            private class TaskbarInstance { }

            private static ITaskBarList3 taskbarInstance = (ITaskBarList3)new TaskbarInstance();
            private static bool taskbarSupported = Environment.OSVersion.Version >= new Version(6, 1);

            public static void SetState(IntPtr windowHandle, TaskBarStates taskbarState)
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

        public enum TaskBarMessages : int
        {
            New = 0x00000000,
            Remove = 0x00000001,
            QueryPos = 0x00000002,
            SetPos = 0x00000003,
            GetState = 0x00000004,
            GetTaskBarPos = 0x00000005,
            Activate = 0x00000006,
            GetAutoHideBar = 0x00000007,
            SetAutoHideBar = 0x00000008,
            WindowPosChanged = 0x00000009,
            SetState = 0x0000000A
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

        public enum TaskBarLocation
        {
            HIDDEN,
            TOP,
            BOTTOM,
            LEFT,
            RIGHT
        }

        public enum State
        {
            AutoHide = 0x00000001,
            AlwaysOnTop = 0x00000002
        }

        public static State GetTaskBarState()
        {
            APPBARDATA msgData = new APPBARDATA();
            msgData.cbSize = (uint)Marshal.SizeOf(msgData);
            msgData.hWnd = SafeNativeMethods.FindWindow("System_TrayWnd", null);
            return (State)SafeNativeMethods.SHAppBarMessage((uint)TaskBarMessages.GetState, ref msgData);
        }

        public static void SetTaskBarState(State _option)
        {
            APPBARDATA msgData = new APPBARDATA();
            msgData.cbSize = (uint)Marshal.SizeOf(msgData);
            msgData.hWnd = SafeNativeMethods.FindWindow("System_TrayWnd", null);
            msgData.lParam = (int)(_option);
            SafeNativeMethods.SHAppBarMessage((uint)TaskBarMessages.SetState, ref msgData);
        }

        public static TaskBarLocation GetTaskBarLocation()
        {
            if (!(Screen.PrimaryScreen.WorkingArea == Screen.PrimaryScreen.Bounds))
            {
                if (!(Screen.PrimaryScreen.WorkingArea.Width == Screen.PrimaryScreen.Bounds.Width))
                    return (Screen.PrimaryScreen.WorkingArea.Left > 0) ? TaskBarLocation.LEFT : TaskBarLocation.RIGHT;
                else
                    return (Screen.PrimaryScreen.WorkingArea.Top > 0) ? TaskBarLocation.TOP : TaskBarLocation.BOTTOM;
            }
            else
                return TaskBarLocation.HIDDEN;
        }

        #endregion

        #region WINDOWS THEME FUNCTIONS

        public static Color GetSystemThemeColor()
        {
            SafeNativeMethods.DWM_COLORIZATION_PARAMS parameters;
            SafeNativeMethods.DwmGetColorizationParameters(out parameters);
            return Color.FromArgb(int.Parse(parameters.clrColor.ToString("X"), NumberStyles.HexNumber));
        }

        #endregion

        #region KEY STATE FUNCTIONS

        public static class KeyState
        {
            public enum Key
            {
                VK_LBUTTON = 0x01,             // Left mouse button
                VK_RBUTTON = 0x02,             // Right mouse button
                VK_CANCEL = 0x03,              // Control-break processing
                VK_MBUTTON = 0x04,             // Middle mouse button (three-button mouse)
                VK_XBUTTON1 = 0x05,            // X1 mouse button
                VK_XBUTTON2 = 0x06,            // X2 mouse button
                VK_BACK = 0x08,                // BACKSPACE key
                VK_TAB = 0x09,                 // TAB key
                VK_CLEAR = 0x0C,               // CLEAR key
                VK_RETURN = 0x0D,              // ENTER key
                VK_SHIFT = 0x10,               // SHIFT key
                VK_CONTROL = 0x11,             // CTRL key
                VK_MENU = 0x12,                // ALT key
                VK_PAUSE = 0x13,               // PAUSE key
                VK_CAPITAL = 0x14,             // CAPS LOCK key
                VK_KANA_HANGUL = 0x15,         // IME Kana/Hangul mode
                VK_JUNJA = 0x17,               // IME Junja mode
                VK_FINAL = 0x18,               // IME final mode
                VK_HANJA_KANJI = 0x19,         // IME Hanja/Kanji mode
                VK_ESCAPE = 0x1B,              // ESC key
                VK_CONVERT = 0x1C,             // IME convert
                VK_NONCONVERT = 0x1D,          // IME nonconvert
                VK_ACCEPT = 0x1E,              // IME accept
                VK_MODECHANGE = 0x1F,          // IME mode change request
                VK_SPACE = 0x20,               // SPACEBAR
                VK_PRIOR = 0x21,               // PAGE UP key
                VK_NEXT = 0x22,                // PAGE DOWN key
                VK_END = 0x23,                 // END key
                VK_HOME = 0x24,                // HOME key
                VK_LEFT = 0x25,                // LEFT ARROW key
                VK_UP = 0x26,                  // UP ARROW key
                VK_RIGHT = 0x27,               // RIGHT ARROW key
                VK_DOWN = 0x28,                // DOWN ARROW key
                VK_SELECT = 0x29,              // SELECT key
                VK_PRINT = 0x2A,               // PRINT key
                VK_EXECUTE = 0x2B,             // EXECUTE key
                VK_SNAPSHOT = 0x2C,            // PRINT SCREEN key
                VK_INSERT = 0x2D,              // INS key
                VK_DELETE = 0x2E,              // DEL key
                VK_HELP = 0x2F,                // HELP key
                VK_0 = 0x30,                   // 0 key
                VK_1 = 0x31,                   // 1 key
                VK_2 = 0x32,                   // 2 key
                VK_3 = 0x33,                   // 3 key
                VK_4 = 0x34,                   // 4 key
                VK_5 = 0x35,                   // 5 key
                VK_6 = 0x36,                   // 6 key
                VK_7 = 0x37,                   // 7 key
                VK_8 = 0x38,                   // 8 key
                VK_9 = 0x39,                   // 9 key
                VK_A = 0x41,                   // A key
                VK_B = 0x42,                   // B key
                VK_C = 0x43,                   // C key
                VK_D = 0x44,                   // D key
                VK_E = 0x45,                   // E key
                VK_F = 0x46,                   // F key
                VK_G = 0x47,                   // G key
                VK_H = 0x48,                   // H key
                VK_I = 0x49,                   // I key
                VK_J = 0x4A,                   // J key
                VK_K = 0x4B,                   // K key
                VK_L = 0x4C,                   // L key
                VK_M = 0x4D,                   // M key
                VK_N = 0x4E,                   // N key
                VK_O = 0x4F,                   // O key
                VK_P = 0x50,                   // P key
                VK_Q = 0x51,                   // Q key
                VK_R = 0x52,                   // R key
                VK_S = 0x53,                   // S key
                VK_T = 0x54,                   // T key
                VK_U = 0x55,                   // U key
                VK_V = 0x56,                   // V key
                VK_W = 0x57,                   // W key
                VK_X = 0x58,                   // X key
                VK_Y = 0x59,                   // Y key
                VK_Z = 0x5A,                   // Z key
                VK_LWIN = 0x5B,                // Left Windows key (Natural keyboard) 
                VK_RWIN = 0x5C,                // Right Windows key (Natural keyboard)
                VK_APPS = 0x5D,                // Applications key (Natural keyboard)
                VK_SLEEP = 0x5F,               // Computer Sleep key
                VK_NUMPAD0 = 0x60,             // Numeric keypad 0 key
                VK_NUMPAD1 = 0x61,             // Numeric keypad 1 key
                VK_NUMPAD2 = 0x62,             // Numeric keypad 2 key
                VK_NUMPAD3 = 0x63,             // Numeric keypad 3 key
                VK_NUMPAD4 = 0x64,             // Numeric keypad 4 key
                VK_NUMPAD5 = 0x65,             // Numeric keypad 5 key
                VK_NUMPAD6 = 0x66,             // Numeric keypad 6 key
                VK_NUMPAD7 = 0x67,             // Numeric keypad 7 key
                VK_NUMPAD8 = 0x68,             // Numeric keypad 8 key
                VK_NUMPAD9 = 0x69,             // Numeric keypad 9 key
                VK_MULTIPLY = 0x6A,            // Multiply key
                VK_ADD = 0x6B,                 // Add key
                VK_SEPARATOR = 0x6C,           // Separator key
                VK_SUBTRACT = 0x6D,            // Subtract key
                VK_DECIMAL = 0x6E,             // Decimal key
                VK_DIVIDE = 0x6F,              // Divide key
                VK_F1 = 0x70,                  // F1 key
                VK_F2 = 0x71,                  // F2 key
                VK_F3 = 0x72,                  // F3 key
                VK_F4 = 0x73,                  // F4 key
                VK_F5 = 0x74,                  // F5 key
                VK_F6 = 0x75,                  // F6 key
                VK_F7 = 0x76,                  // F7 key
                VK_F8 = 0x77,                  // F8 key
                VK_F9 = 0x78,                  // F9 key
                VK_F10 = 0x79,                 // F10 key
                VK_F11 = 0x7A,                 // F11 key
                VK_F12 = 0x7B,                 // F12 key
                VK_F13 = 0x7C,                 // F13 key
                VK_F14 = 0x7D,                 // F14 key
                VK_F15 = 0x7E,                 // F15 key
                VK_F16 = 0x7F,                 // F16 key
                VK_F17 = 0x80,                 // F17 key
                VK_F18 = 0x81,                 // F18 key
                VK_F19 = 0x82,                 // F19 key
                VK_F20 = 0x83,                 // F20 key
                VK_F21 = 0x84,                 // F21 key
                VK_F22 = 0x85,                 // F22 key
                VK_F23 = 0x86,                 // F23 key
                VK_F24 = 0x87,                 // F24 key
                VK_NUMLOCK = 0x90,             // NUM LOCK key
                VK_SCROLL = 0x91,              // SCROLL LOCK key
                VK_LSHIFT = 0xA0,              // Left SHIFT key
                VK_RSHIFT = 0xA1,              // Right SHIFT key
                VK_LCONTROL = 0xA2,            // Left CONTROL key
                VK_RCONTROL = 0xA3,            // Right CONTROL key
                VK_LMENU = 0xA4,               // Left MENU key
                VK_RMENU = 0xA5,               // Right MENU key
                VK_BROWSER_BACK = 0xA6,        // Browser Back key
                VK_BROWSER_FORWARD = 0xA7,     // Browser Forward key
                VK_BROWSER_REFRESH = 0xA8,     // Browser Refresh key
                VK_BROWSER_STOP = 0xA9,        // Browser Stop key
                VK_BROWSER_SEARCH = 0xAA,      // Browser Search key 
                VK_BROWSER_FAVORITES = 0xAB,   // Browser Favorites key
                VK_BROWSER_HOME = 0xAC,        // Browser Start and Home key
                VK_VOLUME_MUTE = 0xAD,         // Volume Mute key
                VK_VOLUME_DOWN = 0xAE,         // Volume Down key
                VK_VOLUME_UP = 0xAF,           // Volume Up key
                VK_MEDIA_NEXT_TRACK = 0xB0,    // Next Track key
                VK_MEDIA_PREV_TRACK = 0xB1,    // Previous Track key
                VK_MEDIA_STOP = 0xB2,          // Stop Media key
                VK_MEDIA_PLAY_PAUSE = 0xB3,    // Play/Pause Media key
                VK_LAUNCH_MAIL = 0xB4,         // Start Mail key
                VK_LAUNCH_MEDIA_SELECT = 0xB5, // Select Media key
                VK_LAUNCH_APP1 = 0xB6,         // Select Media key
                VK_LAUNCH_APP2 = 0xB7,         // Start Application 2 key
                VK_OEM_1 = 0xBA,               // For the US standard keyboard, the ',:' key 
                VK_OEM_PLUS = 0xBB,            // For any country/region, the '+' key
                VK_OEM_COMMA = 0xBC,           // For any country/region, the ',' key
                VK_OEM_MINUS = 0xBD,           // For any country/region, the '-' key
                VK_OEM_PERIOD = 0xBE,          // For any country/region, the '.' key
                VK_OEM_2 = 0xBF,               // For the US standard keyboard, the '/?' key 
                VK_OEM_3 = 0xC0,               // For the US standard keyboard, the '`~' key 
                VK_OEM_4 = 0xDB,               // For the US standard keyboard, the '[{' key
                VK_OEM_5 = 0xDC,               // For the US standard keyboard, the '\|' key
                VK_OEM_6 = 0xDD,               // For the US standard keyboard, the ']}' key
                VK_OEM_7 = 0xDF,               // For the US standard keyboard, the 'single-quote/double-quote' key
                VK_OEM_8 = 0xE1,               // Used for miscellaneous characters, it can vary by keyboard.
                VK_OEM_102 = 0xE2,             // Either the angle bracket key or the backslash key on the RT 102-key keyboard
                VK_PROCESSKEY = 0xE5,          // IME PROCESS key
                VK_PACKET = 0xE7,              // Used to pass Unicode characters as if they were keystrokes. The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods.
                VK_ATTN = 0xF6,                // Attn key
                VK_CRSEL = 0xF7,               // CrSel key
                VK_EXSEL = 0xF8,               // ExSel key
                VK_EREOF = 0xF9,               // Erase EOF key
                VK_PLAY = 0xFA,                // Play key
                VK_ZOOM = 0xFB,                // Zoom key
                VK_PA1 = 0xFD,                 // PA1 key
                VK_OEM_CLEAR = 0xFE            // Clear key
            }

            public static string GetVirtualKeyString(Key _key)
            {
                return Enum.GetName(typeof(Key), _key);
            }

            public static string GetVirtualKeyString(ushort _key)
            {
                switch (_key)
                {
                    case 0x01: return "VK_LBUTTON";             // Left mouse button
                    case 0x02: return "VK_RBUTTON";             // Right mouse button
                    case 0x03: return "VK_CANCEL";              // Control-break processing
                    case 0x04: return "VK_MBUTTON";             // Middle mouse button (three-button mouse)
                    case 0x05: return "VK_XBUTTON1";            // X1 mouse button
                    case 0x06: return "VK_XBUTTON2";            // X2 mouse button
                    case 0x08: return "VK_BACK";                // BACKSPACE key
                    case 0x09: return "VK_TAB";                 // TAB key
                    case 0x0C: return "VK_CLEAR";               // CLEAR key
                    case 0x0D: return "VK_RETURN";              // ENTER key
                    case 0x10: return "VK_SHIFT";               // SHIFT key
                    case 0x11: return "VK_CONTROL";             // CTRL key
                    case 0x12: return "VK_MENU";                // ALT key
                    case 0x13: return "VK_PAUSE";               // PAUSE key
                    case 0x14: return "VK_CAPITAL";             // CAPS LOCK key
                    case 0x15: return "VK_KANA_HANGUL";         // IME Kana/Hangul mode
                    case 0x17: return "VK_JUNJA";               // IME Junja mode
                    case 0x18: return "VK_FINAL";               // IME final mode
                    case 0x19: return "VK_HANJA_KANJI";         // IME Hanja/Kanji mode
                    case 0x1B: return "VK_ESCAPE";              // ESC key
                    case 0x1C: return "VK_CONVERT";             // IME convert
                    case 0x1D: return "VK_NONCONVERT";          // IME nonconvert
                    case 0x1E: return "VK_ACCEPT";              // IME accept
                    case 0x1F: return "VK_MODECHANGE";          // IME mode change request
                    case 0x20: return "VK_SPACE";               // SPACEBAR
                    case 0x21: return "VK_PRIOR";               // PAGE UP key
                    case 0x22: return "VK_NEXT";                // PAGE DOWN key
                    case 0x23: return "VK_END";                 // END key
                    case 0x24: return "VK_HOME";                // HOME key
                    case 0x25: return "VK_LEFT";                // LEFT ARROW key
                    case 0x26: return "VK_UP";                  // UP ARROW key
                    case 0x27: return "VK_RIGHT";               // RIGHT ARROW key
                    case 0x28: return "VK_DOWN";                // DOWN ARROW key
                    case 0x29: return "VK_SELECT";              // SELECT key
                    case 0x2A: return "VK_PRINT";               // PRINT key
                    case 0x2B: return "VK_EXECUTE";             // EXECUTE key
                    case 0x2C: return "VK_SNAPSHOT";            // PRINT SCREEN key
                    case 0x2D: return "VK_INSERT";              // INS key
                    case 0x2E: return "VK_DELETE";              // DEL key
                    case 0x2F: return "VK_HELP";                // HELP key
                    case 0x30: return "VK_0";                   // 0 key
                    case 0x31: return "VK_1";                   // 1 key
                    case 0x32: return "VK_2";                   // 2 key
                    case 0x33: return "VK_3";                   // 3 key
                    case 0x34: return "VK_4";                   // 4 key
                    case 0x35: return "VK_5";                   // 5 key
                    case 0x36: return "VK_6";                   // 6 key
                    case 0x37: return "VK_7";                   // 7 key
                    case 0x38: return "VK_8";                   // 8 key
                    case 0x39: return "VK_9";                   // 9 key
                    case 0x41: return "VK_A";                   // A key
                    case 0x42: return "VK_B";                   // B key
                    case 0x43: return "VK_C";                   // C key
                    case 0x44: return "VK_D";                   // D key
                    case 0x45: return "VK_E";                   // E key
                    case 0x46: return "VK_F";                   // F key
                    case 0x47: return "VK_G";                   // G key
                    case 0x48: return "VK_H";                   // H key
                    case 0x49: return "VK_I";                   // I key
                    case 0x4A: return "VK_J";                   // J key
                    case 0x4B: return "VK_K";                   // K key
                    case 0x4C: return "VK_L";                   // L key
                    case 0x4D: return "VK_M";                   // M key
                    case 0x4E: return "VK_N";                   // N key
                    case 0x4F: return "VK_O";                   // O key
                    case 0x50: return "VK_P";                   // P key
                    case 0x51: return "VK_Q";                   // Q key
                    case 0x52: return "VK_R";                   // R key
                    case 0x53: return "VK_S";                   // S key
                    case 0x54: return "VK_T";                   // T key
                    case 0x55: return "VK_U";                   // U key
                    case 0x56: return "VK_V";                   // V key
                    case 0x57: return "VK_W";                   // W key
                    case 0x58: return "VK_X";                   // X key
                    case 0x59: return "VK_Y";                   // Y key
                    case 0x5A: return "VK_Z";                   // Z key
                    case 0x5B: return "VK_LWIN";                // Left Windows key (Natural keyboard) 
                    case 0x5C: return "VK_RWIN";                // Right Windows key (Natural keyboard)
                    case 0x5D: return "VK_APPS";                // Applications key (Natural keyboard)
                    case 0x5F: return "VK_SLEEP";               // Computer Sleep key
                    case 0x60: return "VK_NUMPAD0";             // Numeric keypad 0 key
                    case 0x61: return "VK_NUMPAD1";             // Numeric keypad 1 key
                    case 0x62: return "VK_NUMPAD2";             // Numeric keypad 2 key
                    case 0x63: return "VK_NUMPAD3";             // Numeric keypad 3 key
                    case 0x64: return "VK_NUMPAD4";             // Numeric keypad 4 key
                    case 0x65: return "VK_NUMPAD5";             // Numeric keypad 5 key
                    case 0x66: return "VK_NUMPAD6";             // Numeric keypad 6 key
                    case 0x67: return "VK_NUMPAD7";             // Numeric keypad 7 key
                    case 0x68: return "VK_NUMPAD8";             // Numeric keypad 8 key
                    case 0x69: return "VK_NUMPAD9";             // Numeric keypad 9 key
                    case 0x6A: return "VK_MULTIPLY";            // Multiply key
                    case 0x6B: return "VK_ADD";                 // Add key
                    case 0x6C: return "VK_SEPARATOR";           // Separator key
                    case 0x6D: return "VK_SUBTRACT";            // Subtract key
                    case 0x6E: return "VK_DECIMAL";             // Decimal key
                    case 0x6F: return "VK_DIVIDE";              // Divide key
                    case 0x70: return "VK_F1";                  // F1 key
                    case 0x71: return "VK_F2";                  // F2 key
                    case 0x72: return "VK_F3";                  // F3 key
                    case 0x73: return "VK_F4";                  // F4 key
                    case 0x74: return "VK_F5";                  // F5 key
                    case 0x75: return "VK_F6";                  // F6 key
                    case 0x76: return "VK_F7";                  // F7 key
                    case 0x77: return "VK_F8";                  // F8 key
                    case 0x78: return "VK_F9";                  // F9 key
                    case 0x79: return "VK_F10";                 // F10 key
                    case 0x7A: return "VK_F11";                 // F11 key
                    case 0x7B: return "VK_F12";                 // F12 key
                    case 0x7C: return "VK_F13";                 // F13 key
                    case 0x7D: return "VK_F14";                 // F14 key
                    case 0x7E: return "VK_F15";                 // F15 key
                    case 0x7F: return "VK_F16";                 // F16 key
                    case 0x80: return "VK_F17";                 // F17 key
                    case 0x81: return "VK_F18";                 // F18 key
                    case 0x82: return "VK_F19";                 // F19 key
                    case 0x83: return "VK_F20";                 // F20 key
                    case 0x84: return "VK_F21";                 // F21 key
                    case 0x85: return "VK_F22";                 // F22 key
                    case 0x86: return "VK_F23";                 // F23 key
                    case 0x87: return "VK_F24";                 // F24 key
                    case 0x90: return "VK_NUMLOCK";             // NUM LOCK key
                    case 0x91: return "VK_SCROLL";              // SCROLL LOCK key
                    case 0xA0: return "VK_LSHIFT";              // Left SHIFT key
                    case 0xA1: return "VK_RSHIFT";              // Right SHIFT key
                    case 0xA2: return "VK_LCONTROL";            // Left CONTROL key
                    case 0xA3: return "VK_RCONTROL";            // Right CONTROL key
                    case 0xA4: return "VK_LMENU";               // Left MENU key
                    case 0xA5: return "VK_RMENU";               // Right MENU key
                    case 0xA6: return "VK_BROWSER_BACK";        // Browser Back key
                    case 0xA7: return "VK_BROWSER_FORWARD";     // Browser Forward key
                    case 0xA8: return "VK_BROWSER_REFRESH";     // Browser Refresh key
                    case 0xA9: return "VK_BROWSER_STOP";        // Browser Stop key
                    case 0xAA: return "VK_BROWSER_SEARCH";      // Browser Search key 
                    case 0xAB: return "VK_BROWSER_FAVORITES";   // Browser Favorites key
                    case 0xAC: return "VK_BROWSER_HOME";        // Browser Start and Home key
                    case 0xAD: return "VK_VOLUME_MUTE";         // Volume Mute key
                    case 0xAE: return "VK_VOLUME_DOWN";         // Volume Down key
                    case 0xAF: return "VK_VOLUME_UP";           // Volume Up key
                    case 0xB0: return "VK_MEDIA_NEXT_TRACK";    // Next Track key
                    case 0xB1: return "VK_MEDIA_PREV_TRACK";    // Previous Track key
                    case 0xB2: return "VK_MEDIA_STOP";          // Stop Media key
                    case 0xB3: return "VK_MEDIA_PLAY_PAUSE";    // Play/Pause Media key
                    case 0xB4: return "VK_LAUNCH_MAIL";         // Start Mail key
                    case 0xB5: return "VK_LAUNCH_MEDIA_SELECT"; // Select Media key
                    case 0xB6: return "VK_LAUNCH_APP1";         // Select Media key
                    case 0xB7: return "VK_LAUNCH_APP2";         // Start Application 2 key
                    case 0xBA: return "VK_OEM_1";               // For the US standard keyboard, the ';:' key 
                    case 0xBB: return "VK_OEM_PLUS";            // For any country/region, the '+' key
                    case 0xBC: return "VK_OEM_COMMA";           // For any country/region, the ',' key
                    case 0xBD: return "VK_OEM_MINUS";           // For any country/region, the '-' key
                    case 0xBE: return "VK_OEM_PERIOD";          // For any country/region, the '.' key
                    case 0xBF: return "VK_OEM_2";               // For the US standard keyboard, the '/?' key 
                    case 0xC0: return "VK_OEM_3";               // For the US standard keyboard, the '`~' key 
                    case 0xDB: return "VK_OEM_4";               // For the US standard keyboard, the '[{' key
                    case 0xDC: return "VK_OEM_5";               // For the US standard keyboard, the '\|' key
                    case 0xDD: return "VK_OEM_6";               // For the US standard keyboard, the ']}' key
                    case 0xDF: return "VK_OEM_7";               // For the US standard keyboard, the 'single-quote/double-quote' key
                    case 0xE1: return "VK_OEM_8";               // Used for miscellaneous characters; it can vary by keyboard.
                    case 0xE2: return "VK_OEM_102";             // Either the angle bracket key or the backslash key on the RT 102-key keyboard
                    case 0xE5: return "VK_PROCESSKEY";          // IME PROCESS key
                    case 0xE7: return "VK_PACKET";              // Used to pass Unicode characters as if they were keystrokes. The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods.
                    case 0xF6: return "VK_ATTN";                // Attn key
                    case 0xF7: return "VK_CRSEL";               // CrSel key
                    case 0xF8: return "VK_EXSEL";               // ExSel key
                    case 0xF9: return "VK_EREOF";               // Erase EOF key
                    case 0xFA: return "VK_PLAY";                // Play key
                    case 0xFB: return "VK_ZOOM";                // Zoom key
                    case 0xFD: return "VK_PA1";                 // PA1 key
                    case 0xFE: return "VK_OEM_CLEAR";           // Clear key
                    default: return _key.ToString();
                }
            }

            public static ushort GetVirtualKeyCode(Key _key)
            {
                return (ushort)_key;
            }

            public static ushort GetVirtualKeyCode(string _key)
            {
                switch (_key)
                {
                    case "VK_LBUTTON": return 0x01;             // Left mouse button
                    case "VK_RBUTTON": return 0x02;             // Right mouse button
                    case "VK_CANCEL": return 0x03;              // Control-break processing
                    case "VK_MBUTTON": return 0x04;             // Middle mouse button (three-button mouse)
                    case "VK_XBUTTON1": return 0x05;            // X1 mouse button
                    case "VK_XBUTTON2": return 0x06;            // X2 mouse button
                    case "VK_BACK": return 0x08;                // BACKSPACE key
                    case "VK_TAB": return 0x09;                 // TAB key
                    case "VK_CLEAR": return 0x0C;               // CLEAR key
                    case "VK_RETURN": return 0x0D;              // ENTER key
                    case "VK_SHIFT": return 0x10;               // SHIFT key
                    case "VK_CONTROL": return 0x11;             // CTRL key
                    case "VK_MENU": return 0x12;                // ALT key
                    case "VK_PAUSE": return 0x13;               // PAUSE key
                    case "VK_CAPITAL": return 0x14;             // CAPS LOCK key
                    case "VK_KANA_HANGUL": return 0x15;         // IME Kana/Hangul mode
                    case "VK_JUNJA": return 0x17;               // IME Junja mode
                    case "VK_FINAL": return 0x18;               // IME final mode
                    case "VK_HANJA_KANJI": return 0x19;         // IME Hanja/Kanji mode
                    case "VK_ESCAPE": return 0x1B;              // ESC key
                    case "VK_CONVERT": return 0x1C;             // IME convert
                    case "VK_NONCONVERT": return 0x1D;          // IME nonconvert
                    case "VK_ACCEPT": return 0x1E;              // IME accept
                    case "VK_MODECHANGE": return 0x1F;          // IME mode change request
                    case "VK_SPACE": return 0x20;               // SPACEBAR
                    case "VK_PRIOR": return 0x21;               // PAGE UP key
                    case "VK_NEXT": return 0x22;                // PAGE DOWN key
                    case "VK_END": return 0x23;                 // END key
                    case "VK_HOME": return 0x24;                // HOME key
                    case "VK_LEFT": return 0x25;                // LEFT ARROW key
                    case "VK_UP": return 0x26;                  // UP ARROW key
                    case "VK_RIGHT": return 0x27;               // RIGHT ARROW key
                    case "VK_DOWN": return 0x28;                // DOWN ARROW key
                    case "VK_SELECT": return 0x29;              // SELECT key
                    case "VK_PRINT": return 0x2A;               // PRINT key
                    case "VK_EXECUTE": return 0x2B;             // EXECUTE key
                    case "VK_SNAPSHOT": return 0x2C;            // PRINT SCREEN key
                    case "VK_INSERT": return 0x2D;              // INS key
                    case "VK_DELETE": return 0x2E;              // DEL key
                    case "VK_HELP": return 0x2F;                // HELP key
                    case "VK_0": return 0x30;                   // 0 key
                    case "VK_1": return 0x31;                   // 1 key
                    case "VK_2": return 0x32;                   // 2 key
                    case "VK_3": return 0x33;                   // 3 key
                    case "VK_4": return 0x34;                   // 4 key
                    case "VK_5": return 0x35;                   // 5 key
                    case "VK_6": return 0x36;                   // 6 key
                    case "VK_7": return 0x37;                   // 7 key
                    case "VK_8": return 0x38;                   // 8 key
                    case "VK_9": return 0x39;                   // 9 key
                    case "VK_A": return 0x41;                   // A key
                    case "VK_B": return 0x42;                   // B key
                    case "VK_C": return 0x43;                   // C key
                    case "VK_D": return 0x44;                   // D key
                    case "VK_E": return 0x45;                   // E key
                    case "VK_F": return 0x46;                   // F key
                    case "VK_G": return 0x47;                   // G key
                    case "VK_H": return 0x48;                   // H key
                    case "VK_I": return 0x49;                   // I key
                    case "VK_J": return 0x4A;                   // J key
                    case "VK_K": return 0x4B;                   // K key
                    case "VK_L": return 0x4C;                   // L key
                    case "VK_M": return 0x4D;                   // M key
                    case "VK_N": return 0x4E;                   // N key
                    case "VK_O": return 0x4F;                   // O key
                    case "VK_P": return 0x50;                   // P key
                    case "VK_Q": return 0x51;                   // Q key
                    case "VK_R": return 0x52;                   // R key
                    case "VK_S": return 0x53;                   // S key
                    case "VK_T": return 0x54;                   // T key
                    case "VK_U": return 0x55;                   // U key
                    case "VK_V": return 0x56;                   // V key
                    case "VK_W": return 0x57;                   // W key
                    case "VK_X": return 0x58;                   // X key
                    case "VK_Y": return 0x59;                   // Y key
                    case "VK_Z": return 0x5A;                   // Z key
                    case "VK_LWIN": return 0x5B;                // Left Windows key (Natural keyboard) 
                    case "VK_RWIN": return 0x5C;                // Right Windows key (Natural keyboard)
                    case "VK_APPS": return 0x5D;                // Applications key (Natural keyboard)
                    case "VK_SLEEP": return 0x5F;               // Computer Sleep key
                    case "VK_NUMPAD0": return 0x60;             // Numeric keypad 0 key
                    case "VK_NUMPAD1": return 0x61;             // Numeric keypad 1 key
                    case "VK_NUMPAD2": return 0x62;             // Numeric keypad 2 key
                    case "VK_NUMPAD3": return 0x63;             // Numeric keypad 3 key
                    case "VK_NUMPAD4": return 0x64;             // Numeric keypad 4 key
                    case "VK_NUMPAD5": return 0x65;             // Numeric keypad 5 key
                    case "VK_NUMPAD6": return 0x66;             // Numeric keypad 6 key
                    case "VK_NUMPAD7": return 0x67;             // Numeric keypad 7 key
                    case "VK_NUMPAD8": return 0x68;             // Numeric keypad 8 key
                    case "VK_NUMPAD9": return 0x69;             // Numeric keypad 9 key
                    case "VK_MULTIPLY": return 0x6A;            // Multiply key
                    case "VK_ADD": return 0x6B;                 // Add key
                    case "VK_SEPARATOR": return 0x6C;           // Separator key
                    case "VK_SUBTRACT": return 0x6D;            // Subtract key
                    case "VK_DECIMAL": return 0x6E;             // Decimal key
                    case "VK_DIVIDE": return 0x6F;              // Divide key
                    case "VK_F1": return 0x70;                  // F1 key
                    case "VK_F2": return 0x71;                  // F2 key
                    case "VK_F3": return 0x72;                  // F3 key
                    case "VK_F4": return 0x73;                  // F4 key
                    case "VK_F5": return 0x74;                  // F5 key
                    case "VK_F6": return 0x75;                  // F6 key
                    case "VK_F7": return 0x76;                  // F7 key
                    case "VK_F8": return 0x77;                  // F8 key
                    case "VK_F9": return 0x78;                  // F9 key
                    case "VK_F10": return 0x79;                 // F10 key
                    case "VK_F11": return 0x7A;                 // F11 key
                    case "VK_F12": return 0x7B;                 // F12 key
                    case "VK_F13": return 0x7C;                 // F13 key
                    case "VK_F14": return 0x7D;                 // F14 key
                    case "VK_F15": return 0x7E;                 // F15 key
                    case "VK_F16": return 0x7F;                 // F16 key
                    case "VK_F17": return 0x80;                 // F17 key
                    case "VK_F18": return 0x81;                 // F18 key
                    case "VK_F19": return 0x82;                 // F19 key
                    case "VK_F20": return 0x83;                 // F20 key
                    case "VK_F21": return 0x84;                 // F21 key
                    case "VK_F22": return 0x85;                 // F22 key
                    case "VK_F23": return 0x86;                 // F23 key
                    case "VK_F24": return 0x87;                 // F24 key
                    case "VK_NUMLOCK": return 0x90;             // NUM LOCK key
                    case "VK_SCROLL": return 0x91;              // SCROLL LOCK key
                    case "VK_LSHIFT": return 0xA0;              // Left SHIFT key
                    case "VK_RSHIFT": return 0xA1;              // Right SHIFT key
                    case "VK_LCONTROL": return 0xA2;            // Left CONTROL key
                    case "VK_RCONTROL": return 0xA3;            // Right CONTROL key
                    case "VK_LMENU": return 0xA4;               // Left MENU key
                    case "VK_RMENU": return 0xA5;               // Right MENU key
                    case "VK_BROWSER_BACK": return 0xA6;        // Browser Back key
                    case "VK_BROWSER_FORWARD": return 0xA7;     // Browser Forward key
                    case "VK_BROWSER_REFRESH": return 0xA8;     // Browser Refresh key
                    case "VK_BROWSER_STOP": return 0xA9;        // Browser Stop key
                    case "VK_BROWSER_SEARCH": return 0xAA;      // Browser Search key 
                    case "VK_BROWSER_FAVORITES": return 0xAB;   // Browser Favorites key
                    case "VK_BROWSER_HOME": return 0xAC;        // Browser Start and Home key
                    case "VK_VOLUME_MUTE": return 0xAD;         // Volume Mute key
                    case "VK_VOLUME_DOWN": return 0xAE;         // Volume Down key
                    case "VK_VOLUME_UP": return 0xAF;           // Volume Up key
                    case "VK_MEDIA_NEXT_TRACK": return 0xB0;    // Next Track key
                    case "VK_MEDIA_PREV_TRACK": return 0xB1;    // Previous Track key
                    case "VK_MEDIA_STOP": return 0xB2;          // Stop Media key
                    case "VK_MEDIA_PLAY_PAUSE": return 0xB3;    // Play/Pause Media key
                    case "VK_LAUNCH_MAIL": return 0xB4;         // Start Mail key
                    case "VK_LAUNCH_MEDIA_SELECT": return 0xB5; // Select Media key
                    case "VK_LAUNCH_APP1": return 0xB6;         // Select Media key
                    case "VK_LAUNCH_APP2": return 0xB7;         // Start Application 2 key
                    case "VK_OEM_1": return 0xBA;               // For the US standard keyboard, the ';:' key 
                    case "VK_OEM_PLUS": return 0xBB;            // For any country/region, the '+' key
                    case "VK_OEM_COMMA": return 0xBC;           // For any country/region, the ',' key
                    case "VK_OEM_MINUS": return 0xBD;           // For any country/region, the '-' key
                    case "VK_OEM_PERIOD": return 0xBE;          // For any country/region, the '.' key
                    case "VK_OEM_2": return 0xBF;               // For the US standard keyboard, the '/?' key 
                    case "VK_OEM_3": return 0xC0;               // For the US standard keyboard, the '`~' key 
                    case "VK_OEM_4": return 0xDB;               // For the US standard keyboard, the '[{' key
                    case "VK_OEM_5": return 0xDC;               // For the US standard keyboard, the '\|' key
                    case "VK_OEM_6": return 0xDD;               // For the US standard keyboard, the ']}' key
                    case "VK_OEM_7": return 0xDF;               // For the US standard keyboard, the 'single-quote/double-quote' key
                    case "VK_OEM_8": return 0xE1;               // Used for miscellaneous characters; it can vary by keyboard.
                    case "VK_OEM_102": return 0xE2;             // Either the angle bracket key or the backslash key on the RT 102-key keyboard
                    case "VK_PROCESSKEY": return 0xE5;          // IME PROCESS key
                    case "VK_PACKET": return 0xE7;              // Used to pass Unicode characters as if they were keystrokes. The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods.
                    case "VK_ATTN": return 0xF6;                // Attn key
                    case "VK_CRSEL": return 0xF7;               // CrSel key
                    case "VK_EXSEL": return 0xF8;               // ExSel key
                    case "VK_EREOF": return 0xF9;               // Erase EOF key
                    case "VK_PLAY": return 0xFA;                // Play key
                    case "VK_ZOOM": return 0xFB;                // Zoom key
                    case "VK_PA1": return 0xFD;                 // PA1 key
                    case "VK_OEM_CLEAR": return 0xFE;           // Clear key
                    default: return 0;
                }
            }

            public static bool GetState(Key _key)
            {
                return SafeNativeMethods.GetAsyncKeyState((ushort)_key) < 0;
            }

            public static bool GetState(ushort _key)
            {
                return SafeNativeMethods.GetAsyncKeyState(_key) < 0;
            }

            public static bool GetState(string _key)
            {
                return SafeNativeMethods.GetAsyncKeyState(GetVirtualKeyCode(_key)) < 0;
            }

            public static string DetectState()
            {
                foreach (object k in Enum.GetValues(typeof(Key)))
                    if (SafeNativeMethods.GetAsyncKeyState(Convert.ToUInt16(k)) < 0)
                        return GetVirtualKeyString(Convert.ToUInt16(k));
                return string.Empty;
            }
        }

        #endregion
    }
}

#endregion
