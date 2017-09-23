using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace GlobalHotkeys
{
    public class ActiveProcess
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        internal static Process GetActiveProcess()
        {
            IntPtr hwnd = GetForegroundWindow();
            return hwnd != null ? GetProcessByHandle(hwnd) : null;
        }
        
        private static Process GetProcessByHandle(IntPtr hwnd)
        {
            try
            {
                uint processId;
                GetWindowThreadProcessId(hwnd, out processId);
                return Process.GetProcessById((int)processId);
            }
            catch { return null; }
        }
    }
}
