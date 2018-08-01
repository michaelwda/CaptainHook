using System;
using System.Collections.Generic;

namespace CaptainHook
{
    public interface IKeyboardHook
    {
        event EventHandler<KeyboardKeyEventArgs> KeyDown;
        event EventHandler<KeyboardKeyEventArgs> KeyUp;


        void SendKeyDown(Key key, KeyModifiers modifiers);
        void SendKeyUp(Key key, KeyModifiers modifiers);
        void SendKeys(List<Key> keys, KeyModifiers modifiers);

        void Start();
    }
}