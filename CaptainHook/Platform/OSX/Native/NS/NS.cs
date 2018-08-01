using System;
using System.Runtime.InteropServices;

namespace CaptainHook.Platform.OSX.Native.NS
{
    public static class NS
    {

        [DllImport(Frameworks.CocoaFramework)]
        public static extern IntPtr NSSelectorFromString(IntPtr cfstr);
    }
}
