using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CaptainHook.Platform.Windows.Native;

namespace CaptainHook.Platform.Windows
{

    //todo: investigate the message regarding debugging LL hooks from the windows docs. We need to use hooks in order to eat mouse events and keystrokes, 
    //https://msdn.microsoft.com/en-us/library/windows/desktop/ms644985(v=vs.85).aspx

    public class WindowsKeyboardHook : KeyboardHookBase, IDisposable
    {
        public const int WH_KEYBOARD_LL = 13;
         

        private IntPtr _user32LibraryHandle;        
        private IntPtr _windowsKeyboardHookHandle;
        private NativeMethods.HookProc _keyboardHookProc;
          

        public override void Start()
        {
            NativeMethods.SetProcessDPIAware();

            _user32LibraryHandle = IntPtr.Zero;           
            _windowsKeyboardHookHandle = IntPtr.Zero;
            _keyboardHookProc = LowLevelKeyboardProc; // we must keep alive _hookProc, because GC is not aware about SetWindowsHookEx behaviour.

            _user32LibraryHandle = NativeMethods.LoadLibrary("User32");
            if (_user32LibraryHandle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to load library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
            }

            _windowsKeyboardHookHandle = NativeMethods.SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardHookProc, _user32LibraryHandle, 0);
            if (_windowsKeyboardHookHandle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to adjust keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
            }

        }
         

        public override void SendKeyDown(Key key, KeyModifiers modifiers)
        {
            int tscancode;
            VirtualKeys tvk;
            int tflags;
            var keyup = false;
            var altDown = (modifiers & KeyModifiers.Alt) != 0;
            bool extended;
            WinKeyMap.ReverseTranslateKey(key, keyup, altDown, out tscancode, out tvk, out tflags, out extended);

            bool sysKey = (!altDown && key == Key.AltLeft) || (!altDown && key == Key.AltRight) || ((key != Key.AltLeft && key != Key.AltRight && altDown));

            var dwFlags = 0x0008;
            if (extended)
                dwFlags = dwFlags | 0x0001;


            var altdown = ((tflags) & ((int)KeyFlags.KF_ALTDOWN >> 8)) > 0;
            var dlgmode = ((tflags) & ((int)KeyFlags.KF_DLGMODE >> 8)) > 0;
            var menumode = ((tflags) & ((int)KeyFlags.KF_MENUMODE >> 8)) > 0;
            var repeat = ((tflags) & ((int)KeyFlags.KF_REPEAT >> 8)) > 0;
            var up = ((tflags) & ((int)KeyFlags.KF_UP >> 8)) > 0;


            NativeMethods.INPUT[] inputs;
            if (extended)
            {
                inputs = new[]
                {
                    new NativeMethods.INPUT
                    {
                        type = NativeMethods.INPUT_KEYBOARD,

                        u = new NativeMethods.InputUnion
                        {
                            ki = new NativeMethods.KEYBDINPUT()
                            {
                                wScan = (ushort) 0xe0,
                                wVk = (ushort) 0,
                                dwFlags = (ushort) 0,
                                dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                            }
                        }
                    },
                    new NativeMethods.INPUT
                    {
                        type = NativeMethods.INPUT_KEYBOARD,

                        u = new NativeMethods.InputUnion
                        {
                            ki = new NativeMethods.KEYBDINPUT()
                            {
                                wScan = (ushort) tscancode,
                                wVk = (ushort) tvk,
                                dwFlags = (ushort) dwFlags,
                                dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                            }
                        }
                    }
                };
            }
            else
            {
                inputs = new[]
                {
                    new NativeMethods.INPUT
                    {
                        type = NativeMethods.INPUT_KEYBOARD,

                        u = new NativeMethods.InputUnion
                        {
                            ki = new NativeMethods.KEYBDINPUT()
                            {
                                wScan = (ushort) tscancode,
                                wVk = (ushort) tvk,
                                dwFlags = (ushort) dwFlags,
                                dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                            }
                        }
                    }
                };
            }

            NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }
        public override void SendKeyUp(Key key, KeyModifiers modifiers)
        {
            int tscancode;
            VirtualKeys tvk;
            int tflags;
            var keyup = true;
            var altDown = (modifiers & KeyModifiers.Alt) != 0;
            bool extended;
            WinKeyMap.ReverseTranslateKey(key, keyup, altDown, out tscancode, out tvk, out tflags, out extended);

            bool sysKey = (!altDown && key == Key.AltLeft) || (!altDown && key == Key.AltRight) || ((key != Key.AltLeft && key != Key.AltRight && altDown));

            var dwFlags = 0x0008 | 0x0002;

            if (extended)
                dwFlags = dwFlags | 0x0001;
            

            var altdown = ((tflags) & ((int)KeyFlags.KF_ALTDOWN >> 8)) > 0;
            var dlgmode = ((tflags) & ((int)KeyFlags.KF_DLGMODE >> 8)) > 0;
            var menumode = ((tflags) & ((int)KeyFlags.KF_MENUMODE >> 8)) > 0;
            var repeat = ((tflags) & ((int)KeyFlags.KF_REPEAT >> 8)) > 0;
            var up = ((tflags) & ((int)KeyFlags.KF_UP >> 8)) > 0;



            NativeMethods.INPUT[] inputs;
            if (extended)
            {
                inputs = new[]
                {
                    new NativeMethods.INPUT
                    {
                        type = NativeMethods.INPUT_KEYBOARD,

                        u = new NativeMethods.InputUnion
                        {
                            ki = new NativeMethods.KEYBDINPUT()
                            {
                                wScan = (ushort) 0xe0,
                                wVk = (ushort) 0,
                                dwFlags = (ushort)  0,
                                dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                            }
                        }
                    },
                    new NativeMethods.INPUT
                    {
                        type = NativeMethods.INPUT_KEYBOARD,

                        u = new NativeMethods.InputUnion
                        {
                            ki = new NativeMethods.KEYBDINPUT()
                            {
                                wScan = (ushort) tscancode,
                                wVk = (ushort) tvk,
                                dwFlags = (ushort) dwFlags,
                                dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                            }
                        }
                    }
                };
            }
            else
            {
                inputs = new[]
                {
                    new NativeMethods.INPUT
                    {
                        type = NativeMethods.INPUT_KEYBOARD,

                        u = new NativeMethods.InputUnion
                        {
                            ki = new NativeMethods.KEYBDINPUT()
                            {
                                wScan = (ushort) tscancode,
                                wVk = (ushort) tvk,
                                dwFlags = (ushort) dwFlags,
                                dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                            }
                        }
                    }
                };
            }

            NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }

      
         

        public IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            
            bool fEatKeyStroke = false;

            var wparamTyped = wParam.ToInt32();
            if (Enum.IsDefined(typeof(KeyboardState), wparamTyped))
            {
                object o = Marshal.PtrToStructure(lParam, typeof(LowLevelKeyboardInputEvent));
                LowLevelKeyboardInputEvent p = (LowLevelKeyboardInputEvent)o;

                var eventArguments = new KeyboardHookEventArgs(p, (KeyboardState)wparamTyped);

                var scancode = eventArguments.KeyboardData.HardwareScanCode;
                var vkey = (VirtualKeys)eventArguments.KeyboardData.VirtualCode;

                var flags = eventArguments.KeyboardData.Flags;
                var extended = ((flags) & ((int)KeyFlags.KF_EXTENDED >> 8)) > 0;

                var altdown = ((flags) & ((int)KeyFlags.KF_ALTDOWN >> 8)) > 0;
                var dlgmode = ((flags) & ((int)KeyFlags.KF_DLGMODE >> 8)) > 0;
                var menumode = ((flags) & ((int)KeyFlags.KF_MENUMODE >> 8)) > 0;
                var repeat = ((flags) & ((int)KeyFlags.KF_REPEAT >> 8)) > 0;
                var up = ((flags) & ((int)KeyFlags.KF_UP >> 8)) > 0;


                //TODO: why is this code ignoring virtual keys and mapping it custom?
                //TODO: figure out what extended 2 is supposed to do from the raw input opentk code....
                var is_valid = true;
                Key key = WinKeyMap.TranslateKey(scancode, vkey, extended, false, out is_valid);



                if (is_valid)
                {
                    
                    if (eventArguments.KeyboardState == KeyboardState.KeyDown || eventArguments.KeyboardState == KeyboardState.SysKeyDown)
                    {
                        OnKeyDown(key);
                        fEatKeyStroke = KeyDownArgs.Handled;                       
                    }
                    if (eventArguments.KeyboardState == KeyboardState.KeyUp || eventArguments.KeyboardState == KeyboardState.SysKeyUp)
                    {
                        OnKeyUp(key);
                        fEatKeyStroke = KeyUpArgs.Handled;
                    }
                }
              

              
            }
            
            return fEatKeyStroke ? (IntPtr)1 : NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private NativeMethods.INPUT[] GenerateKeyDown(Key key, KeyModifiers modifiers)
        {
            int tscancode;
            VirtualKeys tvk;
            int tflags;
            var keyup = false;
            var altDown = modifiers == KeyModifiers.Alt;
            bool extended;
            WinKeyMap.ReverseTranslateKey(key, keyup, altDown, out tscancode, out tvk, out tflags, out extended);


            var dwFlags = 0x0008;
            if (extended)
                dwFlags = dwFlags | 0x0001;


            NativeMethods.INPUT[] inputs;
            if (extended)
            {
                inputs = new[]
                {
                    new NativeMethods.INPUT
                    {
                        type = NativeMethods.INPUT_KEYBOARD,

                        u = new NativeMethods.InputUnion
                        {
                            ki = new NativeMethods.KEYBDINPUT()
                            {
                                wScan = (ushort) 0xe0,
                                wVk = (ushort) 0,
                                dwFlags = (ushort) dwFlags,
                                dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                            }
                        }
                    },
                    new NativeMethods.INPUT
                    {
                        type = NativeMethods.INPUT_KEYBOARD,

                        u = new NativeMethods.InputUnion
                        {
                            ki = new NativeMethods.KEYBDINPUT()
                            {
                                wScan = (ushort) tscancode,
                                wVk = (ushort) tvk,
                                dwFlags = (ushort) dwFlags,
                                dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                            }
                        }
                    }
                };
            }
            else
            {
                inputs = new[]
                {
                    new NativeMethods.INPUT
                    {
                        type = NativeMethods.INPUT_KEYBOARD,

                        u = new NativeMethods.InputUnion
                        {
                            ki = new NativeMethods.KEYBDINPUT()
                            {
                                wScan = (ushort) tscancode,
                                wVk = (ushort) tvk,
                                dwFlags = (ushort) dwFlags,
                                dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                            }
                        }
                    }
                };
            }

            return inputs;


        }
        public NativeMethods.INPUT[] GenerateKeyUp(Key key, KeyModifiers modifiers)
        {
            int tscancode;
            VirtualKeys tvk;
            int tflags;
            var keyup = true;
            var altDown = modifiers == KeyModifiers.Alt;
            bool extended;
            WinKeyMap.ReverseTranslateKey(key, keyup, altDown, out tscancode, out tvk, out tflags, out extended);

            bool sysKey = (!altDown && key == Key.AltLeft) || (!altDown && key == Key.AltRight) || ((key != Key.AltLeft && key != Key.AltRight && altDown));

            var dwFlags = 0x0008 | 0x0002;
            if (extended)
                dwFlags = dwFlags | 0x0001;


            NativeMethods.INPUT[] inputs;
            if (extended)
            {
                inputs = new[]
                {
                    new NativeMethods.INPUT
                    {
                        type = NativeMethods.INPUT_KEYBOARD,

                        u = new NativeMethods.InputUnion
                        {
                            ki = new NativeMethods.KEYBDINPUT()
                            {
                                wScan = (ushort) 0xe0,
                                wVk = (ushort) 0,
                                dwFlags = (ushort) dwFlags,
                                dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                            }
                        }
                    },
                    new NativeMethods.INPUT
                    {
                        type = NativeMethods.INPUT_KEYBOARD,

                        u = new NativeMethods.InputUnion
                        {
                            ki = new NativeMethods.KEYBDINPUT()
                            {
                                wScan = (ushort) tscancode,
                                wVk = (ushort) tvk,
                                dwFlags = (ushort) dwFlags,
                                dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                            }
                        }
                    }
                };
            }
            else
            {
                inputs = new[]
                {
                    new NativeMethods.INPUT
                    {
                        type = NativeMethods.INPUT_KEYBOARD,

                        u = new NativeMethods.InputUnion
                        {
                            ki = new NativeMethods.KEYBDINPUT()
                            {
                                wScan = (ushort) tscancode,
                                wVk = (ushort) tvk,
                                dwFlags = (ushort) dwFlags,
                                dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                            }
                        }
                    }
                };
            }

            return inputs;
        }

        //I should change this to allow modifiers per key. Actually we'll need to track keystate....ugh
        public override void SendKeys(List<Key> keys, KeyModifiers modifiers)
        {
            List<NativeMethods.INPUT> inputs = new List<NativeMethods.INPUT>();
            foreach (var key in keys)
            {
                inputs.AddRange(GenerateKeyDown(key, modifiers));
                inputs.AddRange(GenerateKeyUp(key, modifiers));
            }
            NativeMethods.SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                
                // because we can unhook only in the same thread, not in garbage collector thread
                if (_windowsKeyboardHookHandle != IntPtr.Zero)
                {
                    if (!NativeMethods.UnhookWindowsHookEx(_windowsKeyboardHookHandle))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        throw new Win32Exception(errorCode, $"Failed to remove keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                    }
                    _windowsKeyboardHookHandle = IntPtr.Zero;

                    // ReSharper disable once DelegateSubtraction
                    _keyboardHookProc -= LowLevelKeyboardProc;
                }
            }

            if (_user32LibraryHandle != IntPtr.Zero)
            {
                if (!NativeMethods.FreeLibrary(_user32LibraryHandle)) // reduces reference to library by 1.
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode, $"Failed to unload library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                }
                _user32LibraryHandle = IntPtr.Zero;
            }
        }

        ~WindowsKeyboardHook()
        {
            Dispose(false);
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
