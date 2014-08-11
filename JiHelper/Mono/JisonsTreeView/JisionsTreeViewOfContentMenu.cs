using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gtk;
using Mono.TextEditor;

namespace Jisons
{
    public partial class JisionsTreeView<R, T>
    {

        private void InitJisionsTreeViewOfContentMenu()
        {
            //this.DoPopupMenu += VisualObjectTreeView_ShowPopup;
        }

        #region 右键处理

        //public Action<Gdk.EventButton> DoPopupMenu { get; set; }

        //protected override bool OnButtonPressEvent(Gdk.EventButton evnt)
        //{
        //    if (!evnt.TriggersContextMenu())
        //    {
        //        return base.OnButtonPressEvent(evnt);
        //    }

        //    //pass click to base it it can update the selection
        //    //unless the node is already selected, in which case we don't want to change the selection
        //    bool res = false;
        //    if (!IsClickedNodeSelected((int)evnt.X, (int)evnt.Y))
        //    {
        //        res = base.OnButtonPressEvent(evnt);
        //    }

        //    if (DoPopupMenu != null)
        //    {
        //        DoPopupMenu(evnt);
        //        return true;
        //    }

        //    return res;
        //}

        //protected override bool OnButtonReleaseEvent(Gdk.EventButton evnt)
        //{
        //    bool res = base.OnButtonReleaseEvent(evnt);

        //    if (DoPopupMenu != null && evnt.IsContextMenuButton())
        //    {
        //        return true;
        //    }

        //    return res;
        //}

        #endregion

    }
}
