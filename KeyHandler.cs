using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KeyClone
{
    public class KeyHandler
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private const uint WM_KEYDOWN = 0x100;
        private const uint WM_KEYUP = 0x101;

        private List<Process> _wowProcesses;
        private uint _foregroundWindowProcessId;        

        public KeyHandler()
        {
            
        }

        internal void KeyDown(int vkCode)
        {
            _wowProcesses = new List<Process>();
            _wowProcesses.AddRange(Process.GetProcessesByName("Wow"));
            _wowProcesses.AddRange(Process.GetProcessesByName("Wow-64"));            

            ProcessKey(vkCode, WM_KEYDOWN);
        }

        internal void KeyUp(int vkCode)
        {
            ProcessKey(vkCode, WM_KEYUP);
        }

        private void ProcessKey(int vkCode, uint WM_KEYTYPE)
        {
            if (isCurrentWindowWow())
            {
                foreach (Process proc in _wowProcesses)
                {
                    GetWindowThreadProcessId(GetForegroundWindow(), out _foregroundWindowProcessId);
                    if (_foregroundWindowProcessId != proc.Id)
                    {
                        IntPtr hWnd = FindWindowEx(proc.MainWindowHandle, IntPtr.Zero, null, null);

                        ushort wScanCode = 0;
                        bool bExtended = false;
                        bool bAltDown = false;
                        bool bPrevKeyState = WM_KEYTYPE == WM_KEYUP ? true : false; // true == down 
                        bool bUp = WM_KEYTYPE == WM_KEYUP ? true : false;

                        int lParam = 1; // repeat count

                        lParam |= (wScanCode << 16);
                        lParam |= (bExtended ? (1 << 24) : 0); // extended
                        lParam |= (bAltDown ? (1 << 29) : 0); // context code
                        lParam |= (bPrevKeyState ? (1 << 30) : 0); // previous key state; our keyboard hook handler inserted this bit
                        lParam |= (bUp ? (1 << 31) : 0); // transition state
                        PostMessage(proc.MainWindowHandle, WM_KEYTYPE, (IntPtr)vkCode, lParam);
                    }
                }
            }
        }
     
        private bool isCurrentWindowWow()
        {
            GetWindowThreadProcessId(GetForegroundWindow(), out _foregroundWindowProcessId);

            foreach (Process proc in _wowProcesses)
            {
                if (_foregroundWindowProcessId == proc.Id)
                    return true;
            }

            return false;
        }
    }
}
