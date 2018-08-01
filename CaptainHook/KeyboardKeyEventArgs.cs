using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CaptainHook
{
    //Note - This code is inspired by some openTK stuff, but i'm deliberately diverging because I'm not tracking current keystate and existing
    //modifiers. To do that correctly, you need to do more than just track it. You really should be polling for keys down and updating the model.
    //I will likely do that in another project, but it's out of scope for what I want to do here.
    public class KeyboardKeyEventArgs : HandledEventArgs
    {
        public KeyboardKeyEventArgs() { }
        public KeyboardKeyEventArgs(KeyboardKeyEventArgs args)
        {
            Key = args.Key;
        }
        public Key Key { get;  set; }

    }
}
