using System;
using System.Collections.Generic;
using System.Diagnostics;
using CaptainHook.Platform.OSX.Native;
using CaptainHook.Platform.OSX.Native.CF;
using CaptainHook.Platform.OSX.Native.CG;

namespace CaptainHook.Platform.OSX
{
    public class OsxKeyboardHook : KeyboardHookBase, IDisposable
    {
        
        private CG.EventTapCallBack _eventHookProc;
        private IntPtr _eventTap;
        private IntPtr _runLoop;


        public override void SendKeys(List<Key> keys, KeyModifiers modifiers)
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            _eventHookProc = EventProc;

            _runLoop = CF.CFRunLoopGetMain();
            if (_runLoop == IntPtr.Zero)
                _runLoop = CF.CFRunLoopGetCurrent();
            if (_runLoop == IntPtr.Zero)
            {
                Debug.Print("[Error] No CFRunLoop found for {0}", GetType().FullName);
                throw new InvalidOperationException();
            }
            CF.CFRetain(_runLoop);
            
            _eventTap = CG.EventTapCreate(CGEventTapLocation.HIDEventTap, CGEventTapPlacement.HeadInsert, CGEventTapOptions.Default, CGEventMask.All, _eventHookProc, IntPtr.Zero);
            CG.EventTapEnable(_eventTap, true);
            var runLoopSource = CF.MachPortCreateRunLoopSource(IntPtr.Zero, _eventTap, IntPtr.Zero);
            
            CF.RunLoopAddSource(_runLoop, runLoopSource, CF.RunLoopModeDefault);
        }
        
        
        private IntPtr EventProc(IntPtr proxy, CGEventType type, IntPtr @event, IntPtr refcon)
        {
             
            if (type == CGEventType.KeyDown)
            {
                var code = CG.EventGetIntegerValueField(@event, CGEventField.KeyboardEventKeycode);
                if (!Enum.IsDefined(typeof(MacOSKeyCode), code))
                {
                    Console.WriteLine("THIS KEY IS NOT DEFINED!!!");
                    return @event;
                }
                
                var key = MacOSKeyMap.GetKey((MacOSKeyCode)code);
                OnKeyDown(key);
                return KeyDownArgs.Handled ? IntPtr.Zero : @event;
            }
            if (type == CGEventType.KeyUp)
            {
                var code = CG.EventGetIntegerValueField(@event, CGEventField.KeyboardEventKeycode);
                if (!Enum.IsDefined(typeof(MacOSKeyCode), code))
                {
                    Console.WriteLine("THIS KEY IS NOT DEFINED!!!");
                    return @event;
                }
                
                var key = MacOSKeyMap.GetKey((MacOSKeyCode)code);
                OnKeyUp(key);
                return KeyUpArgs.Handled ? IntPtr.Zero : @event;
            }
            if (type == CGEventType.FlagsChanged) //this may be important for persistent effects. like a caps lock key press will return a keydown, and not immediately a keyup. This is likely relevant so we can track caps state. 
            //TODO: caps lock sync and led
            {
                var code = CG.EventGetIntegerValueField(@event, CGEventField.KeyboardEventKeycode);
                if (!Enum.IsDefined(typeof(MacOSKeyCode), code))
                {
                    Console.WriteLine("THIS KEY IS NOT DEFINED!!!");
                    return @event;
                }
               // Console.WriteLine(code);
            }
            
             
            
            return @event;
        }

       

        public override void SendKeyDown(Key key, KeyModifiers modifiers)
        {
         
            var osxkey = MacOSKeyMap.GetKey(key);

            //just testing if I can do a key conversion here
            if (key == Key.WinRight)
                osxkey = MacOSKeyCode.Fn;
            
            var e = CG.CGEventCreateKeyboardEvent(IntPtr.Zero, (ushort)osxkey, true);
            CGEventFlags flags = BuildFlags(e, modifiers);
            CG.CGEventSetFlags(e,flags);
            
            CG.CGEventPost(CGEventTapLocation.HIDEventTap, e);
            CF.CFRelease(e);
        }

        public override void SendKeyUp(Key key, KeyModifiers modifiers)
        {
         
            var osxkey = MacOSKeyMap.GetKey(key);
            
            //just testing if I can do a key conversion here
            if (key == Key.WinRight)
                osxkey = MacOSKeyCode.Fn;
            
            var e = CG.CGEventCreateKeyboardEvent(IntPtr.Zero, (ushort)osxkey, false);
            CGEventFlags flags = BuildFlags(e, modifiers);
            CG.CGEventSetFlags(e,flags);
            CG.CGEventPost(CGEventTapLocation.HIDEventTap, e);
            CF.CFRelease(e);
        }

        private CGEventFlags BuildFlags(IntPtr e, KeyModifiers modifiers)
        {
            var flags = CG.CGEventGetFlags(e);
            if ((modifiers & KeyModifiers.Shift) != 0)
            {                
                flags = flags | CGEventFlags.Shift;
            }

            if ((modifiers & KeyModifiers.Caps) != KeyModifiers.Caps) //not sure about this, i likely need to poll for caps state and then toggle.          
            {
                flags = flags | CGEventFlags.CapsLock;
            }
            if (modifiers == KeyModifiers.Alt) //not sure about this, i likely need to poll for caps state and then toggle.          
            {
                flags = flags | CGEventFlags.Alt;
            }
            if ((modifiers & KeyModifiers.Control) != 0) //not sure about this, i likely need to poll for caps state and then toggle.          
            {
                flags = flags | CGEventFlags.Control;
            }
            
            if ((modifiers & KeyModifiers.Command) != 0) //not sure about this, i likely need to poll for caps state and then toggle.          
            {
                flags = flags | CGEventFlags.Command;
            }
            if ((modifiers & KeyModifiers.Win) != 0) //not sure about this, i likely need to poll for caps state and then toggle.          
            {
                flags = flags | CGEventFlags.Command;
            }

            //treat winright as a function key on osx??? Do i really need to set this flag?
            if ((modifiers & KeyModifiers.WinRight) != KeyModifiers.WinRight) //not sure about this, i likely need to poll for caps state and then toggle.          
            {
                flags = flags | CGEventFlags.SecondaryFn;
            }
            
            if ((modifiers & KeyModifiers.NumPad) != KeyModifiers.NumPad)           
            {
                flags = flags | CGEventFlags.NumericPad;
            }

            return flags;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // because we can unhook only in the same thread, not in garbage collector thread
                if (_eventTap != IntPtr.Zero)
                {
                    CG.EventTapEnable(_eventTap, false);
                    CF.CFRelease(_eventTap);
                   
                    
                    _eventTap = IntPtr.Zero;

                    // ReSharper disable once DelegateSubtraction
                    _eventHookProc -= EventProc;
                }

                if (_runLoop != IntPtr.Zero)
                {
                    CF.CFRelease(_runLoop);
                    
                }
           
            }
        }

        ~OsxKeyboardHook()
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
