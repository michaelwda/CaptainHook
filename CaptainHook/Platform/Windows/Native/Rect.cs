using System.Runtime.InteropServices;

namespace CaptainHook.Platform.Windows.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}