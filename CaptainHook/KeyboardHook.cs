using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using CaptainHook.Platform.OSX;
using CaptainHook.Platform.Windows;

namespace CaptainHook
{
    public class KeyboardHook : IDisposable
    {
        public readonly IKeyboardHook Hook;
        public KeyboardHook()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Hook = new WindowsKeyboardHook();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Hook = new OsxKeyboardHook();
            }
            else
            {
                throw new PlatformNotSupportedException("Platform not yet supported. Maybe you should do a PR.");
            }
        }

        
        public void Start()
        {
            Hook.Start();
        }


        public void Dispose()
        {
        }
    }
}
