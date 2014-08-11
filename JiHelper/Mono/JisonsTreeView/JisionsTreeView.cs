/* 
 * 
 * FileName:   VisualObjectTreeView.cs
 * Version:    1.0
 * Date:       2014.05.08
 * Author:     Ji
 * 
 *========================================
 * @namespace  Jisons 
 * @class      VisualObjectTreeView 
 * @extends    TreeView
 * 
 *             在此实现 UI 编辑器中的结构树 。。。
 *             在 Gtk 中，TreeView 果然是万能的 。。。
 *             需要扩展 Drag ， Selections 事件即可 。。。
 *             Ps 。 此项默认支持多选 设置 SelectionMode 。。。
 * 
 *========================================
 * 
 * Copyright © Chukong Aipu 2014
 * 
 *                              迹I柳燕
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gtk;
using Mono.TextEditor;

namespace Jisons
{

    public partial class JisionsTreeView<R, T> : JisonsScrolledWindow
    {
        private JisonsTree<R, T> treeView;
        public JisonsTree<R, T> TreeView
        {
            get { return treeView; }
            private set { treeView = value; }
        }

        public string DisplayName
        {
            get
            {
                if (this.treeView != null)
                {
                    return this.treeView.DisplayName;
                }
                return null;
            }
            set
            {
                if (this.treeView != null)
                {
                    this.treeView.DisplayName = value;
                }
            }
        }

        protected JisionsTreeView()
        { }

        /// <summary> 构造函数，默认为多选树 </summary>
        /// <param name="selectionMode"></param>
        public JisionsTreeView(R rootdata)
        {
            InitJisionsTreeViewOfTree(rootdata, Gtk.SelectionMode.Multiple);
            InitJisionsTreeViewOfContentMenu();
            InitVisualObjectTreeViewOfExpanad();
            InitJisionsTreeViewOfData();
            InitJisionsTreeViewOfMouseAction();
            InitJisionsTreeViewOfDragDrop();

            #region 树的事件注册

            treeView.RowActivated += VisualObjectTreeView_RowActivated;

            #endregion

            this.treeView.Show();

            this.Add(this.TreeView);

            this.ShowAll();
        }

        #region 树的事件处理

        private void VisualObjectTreeView_RowActivated(object o, RowActivatedArgs args)
        {

        }

        private void VisualObjectTreeView_ShowPopup(Gdk.EventButton evt)
        {
            //var menu = CreateContextMenu();
            //if (menu != null)
            //    IdeApp.CommandService.ShowContextMenu(this, evt, menu, this);

            Console.WriteLine("右键菜单？？？");
        }

        #endregion

        private bool IsClickedNodeSelected(int x, int y)
        {
            if (treeView != null)
            {
                Gtk.TreePath path;
                if (treeView.GetPathAtPos(x, y, out path))
                    return treeView.Selection.PathIsSelected(path);
            }
            return false;
        }

        public bool MultipleNodesSelected()
        {
            return treeView != null && treeView.Selection.GetSelectedRows().Length > 1;
        }

    }
}
