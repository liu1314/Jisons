using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gdk;
using Gtk;

namespace Jisons
{
    public class JisonsScrolledWindow : Gtk.ScrolledWindow
    {
        public event EventHandler<WidgetEventArgs> KeyDown;

        public event EventHandler<WidgetEventArgs> KeyUp;

        public JisonsScrolledWindow()
        {
            this.WidgetEvent += JisonsScrolledWindow_WidgetEvent;
        }

        void JisonsScrolledWindow_WidgetEvent(object o, Gtk.WidgetEventArgs args)
        {
            switch (args.Event.Type)
            {
                case EventType.KeyPress:
                    {
                        KeyDownHandle(o, args);
                        break;
                    }

                case EventType.KeyRelease:
                    {
                        KeyUpHandle(o, args);
                        break;
                    }

                default: break;
            }
        }

        public void KeyDownHandle(object o, WidgetEventArgs args)
        {
            OnKeyDown((EventKey)args.Event);
            if (!args.Event.SendEvent)
            {
                OnKeyDownHandle(o, args);
            }
        }

        public void KeyUpHandle(object o, WidgetEventArgs args)
        {
            OnKeyUp((EventKey)args.Event);
            if (!args.Event.SendEvent)
            {
                OnKeyUpHandle(o, args);
            }
        }

        public virtual void OnKeyDown(EventKey args)
        { }

        public virtual void OnKeyUp(EventKey args)
        { }

        protected void OnKeyDownHandle(object o, WidgetEventArgs args)
        {
            if (this.KeyDown != null)
            {
                this.KeyDown(o, args);
            }
        }

        protected void OnKeyUpHandle(object o, WidgetEventArgs args)
        {
            if (this.KeyUp != null)
            {
                this.KeyUp(o, args);
            }
        }

    }
}
