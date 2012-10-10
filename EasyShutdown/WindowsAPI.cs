using EasyShutdown.View;
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace EasyShutdown
{
    static class WindowsAPI
    {
        private const int TIMEOUT = 10;

        private const int SE_PRIVILEGE_ENABLED = 0x00000002;
        private const int TOKEN_QUERY = 0x00000008;
        private const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        private const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

        public static bool IsFullScreenMode()
        {
            IntPtr hDesktopWnd = GetDesktopWindow();
            IntPtr hShellWnd = GetShellWindow();
            IntPtr hWnd = GetForegroundWindow();

            if (hWnd == null || hWnd.Equals(IntPtr.Zero))
            {
                return false;
            }

            // Ignore the Desktop and the Shell windows.
            if (hWnd.Equals(hDesktopWnd) || hWnd.Equals(hShellWnd))
            {
                return false;
            }

            RECT wndBounds;
            GetWindowRect(hWnd, out wndBounds);
            int activeWinHeight = wndBounds.Bottom - wndBounds.Top;
            int activeWndWidth = wndBounds.Right - wndBounds.Left;

            return activeWinHeight == SystemParameters.PrimaryScreenHeight &&
                        activeWndWidth == SystemParameters.PrimaryScreenWidth;
        }

        public static void LogOut(bool askToConfirm)
        {
            if (!CanExecute(askToConfirm, "Do you want to log off?", "Log off"))
            {
                return;
            }

            ExitWindowsEx(WindowsAPI.ExitWindows.LogOff, 
                          WindowsAPI.ShutdownReason.MajorOther | WindowsAPI.ShutdownReason.MinorOther);
        }

        public static void Restart(bool askToConfirm)
        {
            if (!CanExecute(askToConfirm, "Do you want to restart your computer?", "Restart"))
            {
                return;
            }

            GetShutdownPrivileges();
            ExitWindowsEx(WindowsAPI.ExitWindows.Reboot, 
                          WindowsAPI.ShutdownReason.MajorOther | WindowsAPI.ShutdownReason.MinorOther);
        }

        public static void Shutdown(bool askToConfirm)
        {
            if (!CanExecute(askToConfirm, "Do you want to shut down your computer?", "Shut down"))
            {
                return;
            }

            GetShutdownPrivileges();
            ExitWindowsEx(WindowsAPI.ExitWindows.ShutDown, 
                          WindowsAPI.ShutdownReason.MajorOther | WindowsAPI.ShutdownReason.MinorOther);
        }

        private static bool CanExecute(bool askToConfirm, string confirmaiton, string action)
        {
            if (askToConfirm)
            {
                MessageBoxResult answer = AutoConfirmDialog.Show(confirmaiton, "EasyShutdown", action, TIMEOUT);
                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }
            }

            return true;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ExitWindowsEx(ExitWindows uFlags, 
                                                  ShutdownReason dwReason);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(IntPtr htok, 
                                                          bool disall, 
                                                          ref TokPriv1Luid newst, 
                                                          int len, 
                                                          IntPtr prev, 
                                                          IntPtr relen);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr h, 
                                                     int acc, 
                                                     ref IntPtr phtok);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LookupPrivilegeValue(string host, 
                                                         string name,
                                                         ref long pluid);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowRect(IntPtr hwnd, out RECT rc);

        private static void GetShutdownPrivileges()
        {
            TokPriv1Luid tp;
            IntPtr hproc = GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;
            OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;
            LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tp.Luid);
            AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
        }

        [Flags]
        private enum ExitWindows : uint
        {
            // ONE of the following five:
            LogOff = 0x00,
            ShutDown = 0x01,
            Reboot = 0x02,
            PowerOff = 0x08,
            RestartApps = 0x40,
            // plus AT MOST ONE of the following two:
            Force = 0x04,
            ForceIfHung = 0x10,
        }

        [Flags]
        private enum ShutdownReason : uint
        {
            MajorApplication = 0x00040000,
            MajorHardware = 0x00010000,
            MajorLegacyApi = 0x00070000,
            MajorOperatingSystem = 0x00020000,
            MajorOther = 0x00000000,
            MajorPower = 0x00060000,
            MajorSoftware = 0x00030000,
            MajorSystem = 0x00050000,

            MinorBlueScreen = 0x0000000F,
            MinorCordUnplugged = 0x0000000b,
            MinorDisk = 0x00000007,
            MinorEnvironment = 0x0000000c,
            MinorHardwareDriver = 0x0000000d,
            MinorHotfix = 0x00000011,
            MinorHung = 0x00000005,
            MinorInstallation = 0x00000002,
            MinorMaintenance = 0x00000001,
            MinorMMC = 0x00000019,
            MinorNetworkConnectivity = 0x00000014,
            MinorNetworkCard = 0x00000009,
            MinorOther = 0x00000000,
            MinorOtherDriver = 0x0000000e,
            MinorPowerSupply = 0x0000000a,
            MinorProcessor = 0x00000008,
            MinorReconfig = 0x00000004,
            MinorSecurity = 0x00000013,
            MinorSecurityFix = 0x00000012,
            MinorSecurityFixUninstall = 0x00000018,
            MinorServicePack = 0x00000010,
            MinorServicePackUninstall = 0x00000016,
            MinorTermSrv = 0x00000020,
            MinorUnstable = 0x00000006,
            MinorUpgrade = 0x00000003,
            MinorWMI = 0x00000015,

            FlagUserDefined = 0x40000000,
            FlagPlanned = 0x80000000
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
