
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
    public static class WinAPI
    {
        #region WINDOWS API

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
            /// <summary>Posted to a window when the cursor moves. If the mouse is not captured,
            /// the message is posted to the window that contains the cursor. Otherwise, the 
            /// message is posted to the window that has captured the mouse.</summary>
            /// <remarks>See WM_MOUSEMOVE</remarks>
            WM_MOUSEMOVE = 0x00000200,
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

        [SuppressUnmanagedCodeSecurity]
        internal static class SafeNativeMethods
        {
            internal delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
            internal delegate void TimerProc(IntPtr hWnd, uint uMsg, UIntPtr nIDEvent, uint dwTime);

            #region SERVICE CORE FUNCTIONS

            [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerA", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr OpenSCManager(string lpMachineName, string lpDatabaseName, ServiceTools.ServiceManagerRights dwDesiredAccess);

            [DllImport("advapi32.dll", EntryPoint = "OpenServiceA", SetLastError = true, CharSet = CharSet.Ansi)]
            internal static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, ServiceTools.ServiceRights dwDesiredAccess);

            [DllImport("advapi32.dll", EntryPoint = "CreateServiceA", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr CreateService(IntPtr hSCManager, string lpServiceName, string lpDisplayName, ServiceTools.ServiceRights dwDesiredAccess, int dwServiceType, ServiceTools.ServiceBootFlag dwStartType, ServiceTools.ServiceError dwErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, IntPtr lpdwTagId, string lpDependencies, string lp, string lpPassword);

            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int CloseServiceHandle(IntPtr hSCObject);

            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int QueryServiceStatus(IntPtr hService, ServiceTools.SERVICE_STATUS lpServiceStatus);

            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int DeleteService(IntPtr hService);

            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int ControlService(IntPtr hService, ServiceTools.ServiceControl dwControl, ServiceTools.SERVICE_STATUS lpServiceStatus);

            [DllImport("advapi32.dll", EntryPoint = "StartServiceA", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int StartService(IntPtr hService, int dwNumServiceArgs, int lpServiceArgVectors);

            #endregion

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

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            internal static extern bool CloseHandle(IntPtr handle);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            internal static extern int GetLastError();

            [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int GetPrivateProfileString(string _section, string _key, string _def, StringBuilder _retVal, int _size, string _file);

            [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            internal static extern IntPtr GetStdHandle(int nStdHandle);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr LoadLibrary(string lpFileName);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr LocalAlloc(int flag, int size);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr LocalFree(IntPtr p);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            internal static extern IntPtr OpenProcess(uint access, bool inheritHandle, uint procID);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr otherAddress, IntPtr localAddress, int size, ref uint bytesRead);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr otherAddress, StringBuilder localAddress, int size, ref uint bytesRead);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            internal static extern IntPtr VirtualAllocEx(IntPtr hProcess, int address, int size, uint allocationType, uint protection);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            internal static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr address, int size, uint freeType);

            [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int WritePrivateProfileString(string _section, string _key, string _val, string _file);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            internal static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr otherAddress, IntPtr localAddress, int size, ref uint bytesWritten);

            #endregion

            #region PROCESS STATUS FUNCTIONS

            [DllImport("psapi.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            internal static extern bool GetProcessImageFileName(IntPtr hProcess, StringBuilder fileName, int fileNameSize);

            #endregion

            #region SHELL32 FUNCTIONS

            [DllImport("shell32.dll")]
            internal extern static int ExtractIconEx(string libName, int iconIndex, IntPtr[] largeIcon, IntPtr[] smallIcon, int nIcons);

            #endregion

            #region USER32 FUNCTIONS

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.Dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool ClientToScreen(IntPtr hWnd, ref Point point);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool DrawMenuBar(IntPtr hWnd);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int EndDialog(IntPtr hDlg, IntPtr nResult);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

            [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

            [DllImport("user32.dll", EntryPoint = "FindWindowEx", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern short GetAsyncKeyState(ushort virtualKeyCode);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            internal static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

            [DllImport("user32.dll")]
            internal static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
            internal static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr GetMenu(IntPtr hWnd);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int GetMenuItemCount(IntPtr hMenu);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int GetWindowLong(IntPtr hWnd, Win32HookAction nIndex);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

            internal static WINDOWPLACEMENT GetWindowPlacement(IntPtr hWnd)
            {
                WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                GetWindowPlacement(hWnd, ref placement);
                return placement;
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
            internal static extern bool ReleaseCapture();

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

            [DllImport("user32.Dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern long SetCursorPos(int x, int y);

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
            internal static extern int SendMessage(IntPtr hWnd, int uMsg, IntPtr wParam, ref CopyDataStruct lParam);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            private static extern IntPtr SendMessage(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam);

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
            internal static extern uint SHAppBarMessage(uint _dwMessage, ref TaskBar.APPBARDATA _pData);

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

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            internal int left;
            internal int top;
            internal int right;
            internal int bottom;
        }

        internal struct WINDOWPLACEMENT
        {
            internal int length;
            internal int flags;
            internal int showCmd;
            internal Point ptMinPosition;
            internal Point ptMaxPosition;
            internal Rectangle rcNormalPosition;
        }

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

        public static bool RefreshVisibleTrayArea()
        {
            IntPtr hWndTray = SafeNativeMethods.FindWindow("Shell_TrayWnd", null);
            if (hWndTray == IntPtr.Zero)
                return false;
            foreach (string className in new string[] { "TrayNotifyWnd", "SysPager", "ToolbarWindow32" })
            {
                hWndTray = SafeNativeMethods.FindWindowEx(hWndTray, IntPtr.Zero, className, null);
                if (hWndTray == IntPtr.Zero)
                    return false;
            }
            RECT rect;
            SafeNativeMethods.GetClientRect(hWndTray, out rect);
            for (int x = 0; x < rect.right; x += 5)
                for (int y = 0; y < rect.bottom; y += 5)
                    SafeNativeMethods.SendMessage(hWndTray, (int)Win32HookAction.WM_MOUSEMOVE, 0, (y << 16) + x);
            return true;
        }

        public static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = SafeNativeMethods.GetForegroundWindow();
            if (SafeNativeMethods.GetWindowText(handle, Buff, nChars) > 0)
                return Buff.ToString();
            return string.Empty;
        }

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

        public static void RemoveWindowFromTaskBar(IntPtr _wndHandle)
        {
            SafeNativeMethods.ShowWindow(_wndHandle, (int)Win32HookAction.SW_HIDE);
            SafeNativeMethods.SetWindowLong(_wndHandle, (int)Win32HookAction.GWL_EXSTYLE, SafeNativeMethods.GetWindowLong(_wndHandle, Win32HookAction.GWL_EXSTYLE) | (int)Win32HookAction.WS_EX_TOOLWINDOW);
            SafeNativeMethods.ShowWindow(_wndHandle, (int)Win32HookAction.SW_SHOW);
        }

        public static void SetWindowBorderlessFullscreen(IntPtr _wndHandle)
        {
            RemoveWindowBorders(_wndHandle);
            SetWindowFullscreen(_wndHandle);
        }

        public static void SetWindowFullscreen(IntPtr _wndHandle)
        {
            SetWindowSize(_wndHandle, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        }

        public static void SetWindowSize(IntPtr _wndHandle, int _width, int _height)
        {
            SafeNativeMethods.MoveWindow(_wndHandle, 0, 0, _width, _height, false);
        }

        public static void SetCursorPos(IntPtr hWnd, Point point)
        {
            SafeNativeMethods.ClientToScreen(hWnd, ref point);
            SafeNativeMethods.SetCursorPos(point.X, point.Y);
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

        public static Color GetSystemThemeColor()
        {
            SafeNativeMethods.DWM_COLORIZATION_PARAMS parameters;
            SafeNativeMethods.DwmGetColorizationParameters(out parameters);
            try
            {
                return Color.FromArgb(int.Parse(parameters.clrColor.ToString("X"), NumberStyles.HexNumber));
            }
            catch
            {
                return SystemColors.Highlight;
            }
        }

        #endregion

        #region TASKBAR

        public static class TaskBarProgress
        {
            public enum States
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
                void SetProgressState(IntPtr hwnd, States state);
            }

            [Guid("56FDF344-FD6D-11d0-958A-006097C9A090")]
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

        public static class TaskBar
        {
            public enum Messages : int
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
                AutoHide = 0x00000001,
                AlwaysOnTop = 0x00000002
            }

            public static State GetState()
            {
                APPBARDATA msgData = new APPBARDATA();
                msgData.cbSize = (uint)Marshal.SizeOf(msgData);
                msgData.hWnd = SafeNativeMethods.FindWindow("System_TrayWnd", null);
                return (State)SafeNativeMethods.SHAppBarMessage((uint)Messages.GetState, ref msgData);
            }

            public static void SetState(State _option)
            {
                APPBARDATA msgData = new APPBARDATA();
                msgData.cbSize = (uint)Marshal.SizeOf(msgData);
                msgData.hWnd = SafeNativeMethods.FindWindow("System_TrayWnd", null);
                msgData.lParam = (int)(_option);
                SafeNativeMethods.SHAppBarMessage((uint)Messages.SetState, ref msgData);
            }

            public static Location GetLocation()
            {
                if (!(Screen.PrimaryScreen.WorkingArea == Screen.PrimaryScreen.Bounds))
                {
                    if (!(Screen.PrimaryScreen.WorkingArea.Width == Screen.PrimaryScreen.Bounds.Width))
                        return (Screen.PrimaryScreen.WorkingArea.Left > 0) ? Location.LEFT : Location.RIGHT;
                    else
                        return (Screen.PrimaryScreen.WorkingArea.Top > 0) ? Location.TOP : Location.BOTTOM;
                }
                else
                    return Location.HIDDEN;
            }
        }

        #endregion

        #region KEY STATE

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

            public static ushort GetVirtualKeyCode(Key _key)
            {
                return (ushort)_key;
            }

            public static ushort GetVirtualKeyCode(string _key)
            {
                try
                {
                    return Convert.ToUInt16(Enum.Parse(typeof(Key), _key));
                }
                catch
                {
                    return 0;
                }
            }

            public static string GetVirtualKeyString(Key _key)
            {
                try
                {
                    return Enum.GetName(typeof(Key), _key);
                }
                catch
                {
                    return null;
                }
            }

            public static string GetVirtualKeyString(ushort _key)
            {
                return ((Key)_key).ToString();
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

            public static void SendState(IntPtr _hWnd, Key _key)
            {
                SafeNativeMethods.PostMessage(_hWnd, 0x0100, (int)_key, 0);
                SafeNativeMethods.PostMessage(_hWnd, 0x0101, (int)_key, 0);
            }
        }

        #endregion

        #region SERVICE TOOLS

        public static class ServiceTools
        {
            [Flags]
            public enum ServiceManagerRights
            {
                Connect = 0x0001,
                CreateService = 0x0002,
                EnumerateService = 0x0004,
                Lock = 0x0008,
                QueryLockStatus = 0x0010,
                ModifyBootConfig = 0x0020,
                StandardRightsRequired = 0xF0000,
                AllAccess = (StandardRightsRequired | Connect | CreateService | EnumerateService | Lock | QueryLockStatus | ModifyBootConfig)
            }

            [Flags]
            public enum ServiceRights
            {
                QueryConfig = 0x1,
                ChangeConfig = 0x2,
                QueryStatus = 0x4,
                EnumerateDependants = 0x8,
                Start = 0x10,
                Stop = 0x20,
                PauseContinue = 0x40,
                Interrogate = 0x80,
                UserDefinedControl = 0x100,
                Delete = 0x00010000,
                StandardRightsRequired = 0xF0000,
                AllAccess = (StandardRightsRequired | QueryConfig | ChangeConfig |
                QueryStatus | EnumerateDependants | Start | Stop | PauseContinue |
                Interrogate | UserDefinedControl)
            }

            public enum ServiceBootFlag
            {
                Start = 0x00000000,
                SystemStart = 0x00000001,
                AutoStart = 0x00000002,
                DemandStart = 0x00000003,
                Disabled = 0x00000004
            }

            public enum ServiceState
            {
                Unknown = -1,
                NotFound = 0,
                Stop = 1,
                Run = 2,
                Stopping = 3,
                Starting = 4,
            }

            public enum ServiceControl
            {
                Stop = 0x00000001,
                Pause = 0x00000002,
                Continue = 0x00000003,
                Interrogate = 0x00000004,
                Shutdown = 0x00000005,
                ParamChange = 0x00000006,
                NetBindAdd = 0x00000007,
                NetBindRemove = 0x00000008,
                NetBindEnable = 0x00000009,
                NetBindDisable = 0x0000000A
            }

            public enum ServiceError
            {
                Ignore = 0x00000000,
                Normal = 0x00000001,
                Severe = 0x00000002,
                Critical = 0x00000003
            }

            private const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
            private const int SERVICE_WIN32_OWN_PROCESS = 0x00000010;

            [StructLayout(LayoutKind.Sequential)]
            internal class SERVICE_STATUS
            {
                internal int dwServiceType = 0;
                internal ServiceState dwCurrentState = 0;
                internal int dwControlsAccepted = 0;
                internal int dwWin32ExitCode = 0;
                internal int dwServiceSpecificExitCode = 0;
                internal int dwCheckPoint = 0;
                internal int dwWaitHint = 0;
            }

            public static void InstallService(string _serviceName, string _displayName, string _path, string _args)
            {
                IntPtr scman = OpenSCManager(ServiceManagerRights.Connect | ServiceManagerRights.CreateService);
                try
                {
                    IntPtr service = SafeNativeMethods.OpenService(scman, _serviceName, ServiceRights.QueryStatus | ServiceRights.Start);
                    if (service == IntPtr.Zero)
                        service = SafeNativeMethods.CreateService(scman, _serviceName, _displayName, ServiceRights.QueryStatus | ServiceRights.Start, SERVICE_WIN32_OWN_PROCESS, ServiceBootFlag.AutoStart, ServiceError.Normal, string.Format("{0} {1}", _path, _args).TrimEnd(), null, IntPtr.Zero, null, null, null);
                    if (service == IntPtr.Zero)
                        throw new ApplicationException("Failed to install service.");
                }
                finally
                {
                    SafeNativeMethods.CloseServiceHandle(scman);
                }
            }

            public static void InstallService(string _serviceName, string _displayName, string _path)
            {
                InstallService(_serviceName, _displayName, _path, string.Empty);
            }

            public static void InstallService(string _name, string _path)
            {
                InstallService(_name, _name, _path, string.Empty);
            }

            public static void UninstallService(string _name)
            {
                IntPtr scman = OpenSCManager(ServiceManagerRights.Connect);
                try
                {
                    IntPtr service = SafeNativeMethods.OpenService(scman, _name, ServiceRights.StandardRightsRequired | ServiceRights.Stop | ServiceRights.QueryStatus);
                    if (service == IntPtr.Zero)
                        throw new ApplicationException("Service not installed.");
                    try
                    {
                        StopService(service);
                        int ret = SafeNativeMethods.DeleteService(service);
                        if (ret == 0)
                        {
                            int error = Marshal.GetLastWin32Error();
                            throw new ApplicationException("Could not delete service " + error);
                        }
                    }
                    finally
                    {
                        SafeNativeMethods.CloseServiceHandle(service);
                    }
                }
                finally
                {
                    SafeNativeMethods.CloseServiceHandle(scman);
                }
            }

            public static bool ServiceExists(string _name)
            {
                IntPtr scman = OpenSCManager(ServiceManagerRights.Connect);
                try
                {
                    IntPtr service = SafeNativeMethods.OpenService(scman, _name, ServiceRights.QueryStatus);
                    if (service == IntPtr.Zero)
                        return false;
                    SafeNativeMethods.CloseServiceHandle(service);
                    return true;
                }
                finally
                {
                    SafeNativeMethods.CloseServiceHandle(scman);
                }
            }

            public static void StartService(string _name)
            {
                IntPtr scman = OpenSCManager(ServiceManagerRights.Connect);
                try
                {
                    IntPtr hService = SafeNativeMethods.OpenService(scman, _name, ServiceRights.QueryStatus |
                    ServiceRights.Start);
                    if (hService == IntPtr.Zero)
                        throw new ApplicationException("Could not open service.");
                    try
                    {
                        StartService(hService);
                    }
                    finally
                    {
                        SafeNativeMethods.CloseServiceHandle(hService);
                    }
                }
                finally
                {
                    SafeNativeMethods.CloseServiceHandle(scman);
                }
            }

            public static void StopService(string _name)
            {
                IntPtr scman = OpenSCManager(ServiceManagerRights.Connect);
                try
                {
                    IntPtr hService = SafeNativeMethods.OpenService(scman, _name, ServiceRights.QueryStatus |
                    ServiceRights.Stop);
                    if (hService == IntPtr.Zero)
                        throw new ApplicationException("Could not open service.");
                    try
                    {
                        StopService(hService);
                    }
                    finally
                    {
                        SafeNativeMethods.CloseServiceHandle(hService);
                    }
                }
                finally
                {
                    SafeNativeMethods.CloseServiceHandle(scman);
                }
            }

            private static void StartService(IntPtr hService)
            {
                SERVICE_STATUS status = new SERVICE_STATUS();
                SafeNativeMethods.StartService(hService, 0, 0);
                WaitForServiceStatus(hService, ServiceState.Starting, ServiceState.Run);
            }

            private static void StopService(IntPtr hService)
            {
                SERVICE_STATUS status = new SERVICE_STATUS();
                SafeNativeMethods.ControlService(hService, ServiceControl.Stop, status);
                WaitForServiceStatus(hService, ServiceState.Stopping, ServiceState.Stop);
            }

            public static ServiceState GetServiceStatus(string _name)
            {
                IntPtr scman = OpenSCManager(ServiceManagerRights.Connect);
                try
                {
                    IntPtr hService = SafeNativeMethods.OpenService(scman, _name, ServiceRights.QueryStatus);
                    if (hService == IntPtr.Zero)
                        return ServiceState.NotFound;
                    try
                    {
                        return GetServiceStatus(hService);
                    }
                    finally
                    {
                        SafeNativeMethods.CloseServiceHandle(scman);
                    }
                }
                finally
                {
                    SafeNativeMethods.CloseServiceHandle(scman);
                }
            }

            private static ServiceState GetServiceStatus(IntPtr hService)
            {
                SERVICE_STATUS ssStatus = new SERVICE_STATUS();
                if (SafeNativeMethods.QueryServiceStatus(hService, ssStatus) == 0)
                    throw new ApplicationException("Failed to query service status.");
                return ssStatus.dwCurrentState;
            }

            private static bool WaitForServiceStatus(IntPtr hService, ServiceState WaitStatus, ServiceState DesiredStatus)
            {
                SERVICE_STATUS ssStatus = new SERVICE_STATUS();
                int dwOldCheckPoint;
                int dwStartTickCount;

                SafeNativeMethods.QueryServiceStatus(hService, ssStatus);
                if (ssStatus.dwCurrentState == DesiredStatus)
                    return true;
                dwStartTickCount = Environment.TickCount;
                dwOldCheckPoint = ssStatus.dwCheckPoint;

                while (ssStatus.dwCurrentState == WaitStatus)
                {
                    int dwWaitTime = ssStatus.dwWaitHint / 10;
                    dwWaitTime = dwWaitTime < 1000 ? 1000 : dwWaitTime > 10000 ? 10000 : dwWaitTime;
                    System.Threading.Thread.Sleep(dwWaitTime);
                    if (SafeNativeMethods.QueryServiceStatus(hService, ssStatus) == 0)
                        break;
                    if (ssStatus.dwCheckPoint > dwOldCheckPoint)
                    {
                        dwStartTickCount = Environment.TickCount;
                        dwOldCheckPoint = ssStatus.dwCheckPoint;
                    }
                    else
                    {
                        if (Environment.TickCount - dwStartTickCount > ssStatus.dwWaitHint)
                            break;
                    }
                }
                return ssStatus.dwCurrentState == DesiredStatus;
            }

            private static IntPtr OpenSCManager(ServiceManagerRights _rights)
            {
                IntPtr scman = SafeNativeMethods.OpenSCManager(null, null, _rights);
                if (scman == IntPtr.Zero)
                    throw new ApplicationException("Could not connect to service control manager.");
                return scman;
            }
        }

        #endregion
    }
}

#endregion
