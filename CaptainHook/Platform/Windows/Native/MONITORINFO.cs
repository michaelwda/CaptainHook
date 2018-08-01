using System.Runtime.InteropServices;

namespace CaptainHook.Platform.Windows.Native
{


    /// <summary>
    /// Monitor information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MONITORINFO
    {
        public uint size;
        public Rect monitor;
        public Rect work;
        public uint flags;
    }
}
