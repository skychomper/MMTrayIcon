namespace gunit {
    using System;
    using System.Text;
    using System.Runtime.InteropServices;

    public enum GetWindow_Cmd : uint {
        GW_HWNDFIRST = 0,
        GW_HWNDLAST = 1,
        GW_HWNDNEXT = 2,
        GW_HWNDPREV = 3,
        GW_OWNER = 4,
        GW_CHILD = 5,
        GW_ENABLEDPOPUP = 6
    }

    [Flags]
    public enum FlashWindowFlags : uint {
        /// <summary>Stop flashing. The system restores the window to its original state.</summary>
        Stop = 0,
        /// <summary>Flash the window caption.</summary>
        Caption = 1,
        /// <summary>Flash the taskbar button.</summary>
        Tray = 2,
        /// <summary>
        /// Flash both the window caption and taskbar button.
        /// This is equivalent to setting the FlashWindowFlags.Caption | FlashWindowFlags.Tray flags.
        /// </summary>
        All = 3,
        /// <summary>Flash continuously, until the FlashWindowFlags.Stop flag is set.</summary>
        Timer = 4,
        /// <summary>Flash continuously until the window comes to the foreground.</summary>
        TimerNoForeground = 12
    }

    public partial class gUtils {
        public const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO {
            /// <summary>The size of the structure in bytes.</summary>
            public uint cbSize;
            /// <summary>A Handle to the Window to be Flashed. The window can be either opened or minimized.</summary>
            public IntPtr hwnd;
            /// <summary>The Flash Status.</summary>
            public uint dwFlags;
            /// <summary>The number of times to Flash the window.</summary>
            public uint uCount;
            /// <summary>The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.</summary>
            public uint dwTimeout;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetWindow(IntPtr hWnd, GetWindow_Cmd uCmd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAdress, int size, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int nSize, IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int nSize, IntPtr lpNumberOfBytesWritten);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32")]
        static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint dwFreeType);

        [DllImport("kernel32")]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("dwmapi.dll")]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FlashWindowEx(ref FLASHWINFO flashInfo);

        public static bool UseImmersiveDarkMode(IntPtr handle, bool enabled) {
            var darkMode = enabled ? (int)1 : 0;

            return
              (DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref darkMode, sizeof(int)) == 0) ||
              (DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int)) == 0);
        }

        public static string GetWindowText(IntPtr hWnd) {
            int len = GetWindowTextLength(hWnd) + 1;
            StringBuilder sb = new StringBuilder(len);
            len = GetWindowText(hWnd, sb, len);
            return sb.ToString(0, len);
        }

        public static string GetWindowClass(IntPtr hWnd) {
            int len = 256;
            StringBuilder sb = new StringBuilder(len);
            len = GetClassName(hWnd, sb, len);
            return sb.ToString(0, len);
        }

        public static IntPtr FindWindowG(string ClassName, string WindowName) {
            var res = IntPtr.Zero;

            ClassName = ClassName.ToLower();
            WindowName = WindowName.ToLower();

            EnumWindows((hwnd, lParam) => {
                var classname = GetWindowClass(hwnd).ToLower();
                var windowname = GetWindowText(hwnd).ToLower();

                if (((ClassName == "") && (windowname.Contains(WindowName))) ||
                    ((WindowName == "") && (classname.Contains(ClassName))) ||
                    ((classname.Contains(ClassName)) && (windowname.Contains(WindowName)))) {
                    res = hwnd;
                    return false;
                }

                return true;
            }, IntPtr.Zero);

            return res;
        }

        public static IntPtr FindWindowChildG(IntPtr hwnd, string ClassName, string WindowName, bool recursive) {
            var res = IntPtr.Zero;

            hwnd = gUtils.GetWindow(hwnd, GetWindow_Cmd.GW_CHILD);
            if (hwnd != IntPtr.Zero) {
                ClassName = ClassName.ToLower();
                WindowName = WindowName.ToLower();

                while (hwnd != IntPtr.Zero) {
                    var classname = GetWindowClass(hwnd).ToLower();
                    var windowname = GetWindowText(hwnd).ToLower();

                    if (((ClassName == "") && (windowname.Contains(WindowName))) ||
                        ((WindowName == "") && (classname.Contains(ClassName))) ||
                        ((classname.Contains(ClassName)) && (windowname.Contains(WindowName)))) {
                        res = hwnd;
                        break;
                    }

                    if (recursive) {
                        var hwnd2 = FindWindowChildG(hwnd, ClassName, WindowName, recursive);
                        if (hwnd2 != IntPtr.Zero)
                            return hwnd2;
                    }

                    hwnd = gUtils.GetWindow(hwnd, GetWindow_Cmd.GW_HWNDNEXT);
                }
            }

            return res;
        }

        public static bool FlashWindow(IntPtr handle, uint flags) {
            var fi = new gUtils.FLASHWINFO() {
                hwnd = handle,
                dwFlags = flags,
                uCount = uint.MaxValue,
                dwTimeout = 0,
            };

            fi.cbSize = Convert.ToUInt32(Marshal.SizeOf(fi));

            return
              FlashWindowEx(ref fi);
        }
    }

    public static class Security {
        public const int LOGON32_LOGON_INTERACTIVE = 2;
        public const int LOGON32_PROVIDER_DEFAULT = 0;

        //static WindowsImpersonationContext impersonationContext;

        [DllImport("advapi32.dll")]
        public static extern int LogonUserA(String lpszUserName, String lpszDomain, String lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int DuplicateToken(IntPtr hToken, int impersonationLevel, ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);

        /*public static bool ImpersonateValidUser(String userName, String domain, String password) {
          WindowsIdentity tempWindowsIdentity;
          IntPtr token = IntPtr.Zero;
          IntPtr tokenDuplicate = IntPtr.Zero;

          if (RevertToSelf()) {
            if (LogonUserA(userName, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref token) != 0) {
              if (DuplicateToken(token, 2, ref tokenDuplicate) != 0) {
                tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                impersonationContext = tempWindowsIdentity.Impersonate();
                if (impersonationContext != null) {
                  CloseHandle(token);
                  CloseHandle(tokenDuplicate);
                  return true;
                }
              }
            }
          }
          if (token != IntPtr.Zero)
            CloseHandle(token);
          if (tokenDuplicate != IntPtr.Zero)
            CloseHandle(tokenDuplicate);
          return false;
        }

        public static void UndoImpersonation() {
          impersonationContext.Undo();
        } */
    }
}
