using System;

namespace CaptainHook.Platform.OSX.Native
{
    public static class Selector
    {
        public static IntPtr Get(string name)
        {
            IntPtr cfstrSelector = CF.CF.CFSTR(name);
            
            IntPtr selector = NS.NS.NSSelectorFromString(cfstrSelector);
            CF.CF.CFRelease(cfstrSelector);
            return selector;
        }
    }
}
