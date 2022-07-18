using System;
using System.Runtime.InteropServices;

namespace FartPoop
{
    public static class MessageBox
    {
        [DllImport("user32.dll")]
        private static extern int MessageBoxA(IntPtr hWnd, string lpText, string lpCaption, uint uType);

        public static void Show(string text)
        {
            MessageBoxA(IntPtr.Zero, text, "\0", 0x00000040);
        }

        public static void Show(string text, string caption)
        {
            MessageBoxA(IntPtr.Zero, text, caption, 0x00000040);
        }
    }
}