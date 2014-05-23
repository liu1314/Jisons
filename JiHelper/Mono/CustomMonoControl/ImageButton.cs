/* 
 * 
 * FileName:   ImageButton.cs
 * Version:    1.0
 * Date:       2014.05.21
 * Author:     Ji
 * 
 *========================================
 * @namespace  Gtk 
 * @class      ImageButton
 * @extends    Gtk.Button
 * 
 *             实现渲染图片的 Button 
 * 
 *========================================
 * 
 * Copyright © Chukong Aipu 2014
 * 
 */

using Gdk;
using Gtk;

namespace Gtk
{
    public class ImageButton : Gtk.Button
    {

        public Gdk.Pixbuf Background { get; set; }

        public ImageButton()
        { }

        public ImageButton(Gdk.Pixbuf background)
        {
            this.Background = background;
        }

        protected override bool OnExposeEvent(Gdk.EventExpose evnt)
        {
            bool r = base.OnExposeEvent(evnt);

            if (Background != null && Background.Width <= this.Allocation.Width && Background.Height <= this.Allocation.Height)
            {
                var x = (this.Allocation.Width - Background.Width) / 2 + this.Allocation.X;
                var y = (this.Allocation.Height - Background.Height) / 2 + this.Allocation.Y;

                GdkWindow.DrawPixbuf(this.Style.BackgroundGC(StateType.Normal), Background, 0, 0, x, y, Background.Width, Background.Height, RgbDither.Normal, 0, 0);
            }

            return r;
        }

    }
}
