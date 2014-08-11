using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gtk;
using Mono.TextEditor;

namespace Jisons
{
    public partial class JisionsTreeView<R, T>
    {

        public void InitJisionsTreeViewOfMouseAction()
        {
            this.treeView.OnMouseButtonDown += treeView_OnMouseButtonDown;

            this.treeView.OnMouseDoubleClick += treeView_OnMouseDoubleClick;
        }

        void treeView_OnMouseButtonDown(object sender, WidgetEventArgs e)
        {

        }

        void treeView_OnMouseDoubleClick(object sender, WidgetEventArgs e)
        {
            var selectednodes = this.treeView.GetAllSelectedTreePaths();
            if (selectednodes != null)
            {
                foreach (var node in selectednodes)
                {
                    var isexpanded = this.GetViewItemExpanded(node);
                    if (isexpanded)
                    {
                        this.CollapseSingleViewItem(node);
                    }
                    else
                    {
                        this.ExpandSingleViewItem(node);
                    }
                }
            }
        }

    }
}
