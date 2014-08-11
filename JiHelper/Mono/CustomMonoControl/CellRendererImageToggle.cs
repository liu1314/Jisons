/* 
 * 
 * FileName:   CellRendererImageToggle.cs
 * Version:    1.0
 * Date:       2014.05.21
 * Author:     Ji
 * 
 *========================================
 * @namespace  Gtk 
 * @class      CellRendererImageToggle
 * @extends    CellRendererToggle
 * 
 *             在列表控件中实现换图功能的Toggle
 * 
 *========================================
 * 
 * Copyright © Chukong Aipu 2014
 * 
 */

using Gdk;
using GLib;
using System;
using System.Runtime.InteropServices;

namespace Gtk
{
    public class CellRendererImageToggle : CellRendererToggle
    {

        public Pixbuf CheckPixbuf { get; set; }
        public Pixbuf UnCheckPixbuf { get; set; }

        public int DrawSpan = 4;
        public int DrawWidth = 10;

        protected override void Render(Drawable window, Widget widget, Rectangle background_area, Rectangle cell_area, Rectangle expose_area, CellRendererState flags)
        {
            if (this.CheckPixbuf != null || this.UnCheckPixbuf != null)
            {
                var gc = widget.Style.ForegroundGC(StateType.Normal);
                if (this.Active)
                {
                    window.DrawPixbuf(gc, CheckPixbuf, 0, 0, background_area.X, background_area.Y + DrawSpan, -1, -1, Gdk.RgbDither.None, 0, 0);
                }
                else
                {
                    window.DrawPixbuf(gc, UnCheckPixbuf, 0, 0, background_area.X, background_area.Y + DrawSpan, -1, -1, Gdk.RgbDither.None, 0, 0);
                }
            }
            else
            {
                base.Render(window, widget, background_area, cell_area, expose_area, flags);
            }
        }

    }
}
