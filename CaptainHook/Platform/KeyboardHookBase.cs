using System;
using System.Collections.Generic;
using System.Text;

namespace CaptainHook.Platform
{
    public abstract class KeyboardHookBase : IKeyboardHook
    {
        public event EventHandler<KeyboardKeyEventArgs> KeyDown = delegate { };
        public event EventHandler<KeyboardKeyEventArgs> KeyUp = delegate { };

        public abstract void SendKeyDown(Key key, KeyModifiers modifiers);
        public abstract void SendKeyUp(Key key, KeyModifiers modifiers);
        public abstract void SendKeys(List<Key> keys, KeyModifiers modifiers);

        public abstract void Start();

        protected readonly KeyboardKeyEventArgs KeyDownArgs = new KeyboardKeyEventArgs();
        protected readonly KeyboardKeyEventArgs KeyUpArgs = new KeyboardKeyEventArgs();

        protected void OnKeyDown(Key key)
        {           
            var e = KeyDownArgs;
            e.Key = key;
            e.Handled = false;
            KeyDown(this, e);
        }

        protected void OnKeyUp(Key key)
        {
            
            var e = KeyUpArgs;
            e.Key = key;
            e.Handled = false;

            KeyUp(this, e);
        }

        public abstract void Dispose();
    }
}
