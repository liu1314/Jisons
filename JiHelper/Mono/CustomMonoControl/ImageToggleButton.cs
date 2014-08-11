/* 
 * 
 * FileName:   ToggleButton.cs
 * Version:    1.0
 * Date:       2014.05.21
 * Author:     Ji
 * 
 *========================================
 * @namespace  Gtk 
 * @class      ToggleButton
 * @extends    Gtk.ToggleButton
 * 
 *             实现渲染图片的 ToggleButton 
 * 
 *========================================
 * 
 * Copyright © Chukong Aipu 2014
 * 
 */

using System;
using System.Runtime.InteropServices;
using Gdk;
using GLib;
using Gtk;

namespace Gtk
{
    public class ImageToggleButton : Gtk.ToggleButton
    {

        public Gdk.Pixbuf CheckPixbuf { get; set; }
        public Gdk.Pixbuf UnCheckPixbuf { get; set; }

        public string CheckString { get; set; }
        public string UnCheckString { get; set; }

        Label lable = new Gtk.Label();

        public ImageToggleButton()
        {
            lable.ModifyFg(StateType.Normal, new Gdk.Color(255, 0, 0));
        }

        public ImageToggleButton(Gdk.Pixbuf checkimage, Gdk.Pixbuf uncheckimage)
            : base()
        {
            this.CheckPixbuf = checkimage;
            this.UnCheckPixbuf = uncheckimage;
        }

        protected override bool OnExposeEvent(Gdk.EventExpose evnt)
        {
            bool r = base.OnExposeEvent(evnt);

            if (this.CheckPixbuf != null && this.UnCheckPixbuf != null)
            {
                if (this.Active)
                {
                    if (this.CheckPixbuf.Width <= this.Allocation.Width && this.CheckPixbuf.Height <= this.Allocation.Height)
                    {
                        var ix = this.Allocation.X + 4;
                        var iy = (this.Allocation.Height - this.CheckPixbuf.Height) / 2 + this.Allocation.Y;
                        this.GdkWindow.DrawPixbuf(this.Style.BackgroundGC(StateType.Normal), this.CheckPixbuf, 0, 0, ix, iy, this.CheckPixbuf.Width, this.CheckPixbuf.Height, RgbDither.Normal, 0, 0);

                        var sx = ix + this.CheckPixbuf.Width + 2;
                        var sy = iy + 3;

                        lable.Text = CheckString;
                        this.GdkWindow.DrawLayout(this.Style.ForegroundGC(StateType.Normal), sx, sy, lable.Layout);
                    }
                }
                else
                {
                    if (this.UnCheckPixbuf.Width <= this.Allocation.Width && this.UnCheckPixbuf.Height <= this.Allocation.Height)
                    {
                        var ix = this.Allocation.X + 4;
                        var iy = (this.Allocation.Height - this.UnCheckPixbuf.Height) / 2 + this.Allocation.Y;
                        this.GdkWindow.DrawPixbuf(this.Style.BackgroundGC(StateType.Normal), this.UnCheckPixbuf, 0, 0, ix, iy, this.UnCheckPixbuf.Width, this.UnCheckPixbuf.Height, RgbDither.Normal, 0, 0);

                        var sx = ix + this.CheckPixbuf.Width + 2;
                        var sy = iy + 3;

                        lable.Text = UnCheckString;
                        this.GdkWindow.DrawLayout(this.Style.ForegroundGC(StateType.Normal), sx, sy, lable.Layout);
                    }
                }
            }

            return r;
        }
    }
}
