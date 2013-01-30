using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace KeyClone
{
    public class Interop
    {
        //Constants
        private const int WH_KEYBOARD_LL = 13;
        private const uint WM_KEYDOWN = 0x100;
        private const uint WM_KEYUP = 0x101;

        private KeyHandler _keyHandler;
        private IntPtr _hookID = IntPtr.Zero;
        private Interop.LowLevelKeyboardProc _proc;   
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
               
        public Interop(KeyHandler keyHandler)
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
            _keyHandler = keyHandler;
        }

        private  IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        private  IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                _keyHandler.KeyDown(vkCode);
            }
            else if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                _keyHandler.KeyUp(vkCode);
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}
