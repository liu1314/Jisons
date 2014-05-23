/* 
 * 
 * FileName:   JisionsTreeViewOfExpanad.cs
 * Version:    1.0
 * Date:       2014.05.08
 * Author:     Ji
 * 
 *========================================
 * @namespace  Jisons 
 * @class      JisionsTreeView 
 * @extends    JisonsScrolledWindow
 * 
 *             在此实现多选结构树 。。。
 *             在 Gtk 中，TreeView 果然是万能的 。。。
 *             
 *             JisionsTreeViewOfExpanad 实现树的展开与隐藏操作
 * 
 *========================================
 * 
 * Copyright © Chukong Aipu 2014
 *                              迹I柳燕
 */

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

        /// <summary> 加载展开控制 </summary>
        public void InitVisualObjectTreeViewOfExpanad()
        {
            treeView.TestExpandRow += treeView_TestExpandRow;
        }

        /// <summary> 展开树节点时的通知 </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
        void treeView_TestExpandRow(object o, TestExpandRowArgs args)
        {

        }

        /// <summary> 展开所有项 </summary>
        public void ExpanadAllViewItem()
        {
            this.treeView.ExpandAll();
        }

        /// <summary> 只展开当前项 </summary>
        /// <param name="treepath"></param>
        public void ExpandSingleViewItem(TreePath treepath)
        {
            this.treeView.ExpandToPath(treepath);
        }

        public void ExpandSingleViewItem(T itemdata)
        {
            DoActionOfJisonsTreeIter(itemdata, (jti) => this.treeView.ExpandToPath(this.treeView.GetTreePath(jti.TreeIter)));
        }

        /// <summary> 展开当前和其所有的子项 </summary>
        /// <param name="treepath"></param>
        /// <returns></returns>
        public bool ExpandSingleAndChildrenViewItem(TreePath treepath)
        {
            return this.treeView.ExpandRow(treepath, true);
        }

        /// <summary> 关闭所有的展开项 </summary>
        public void CollapseAllViewItem()
        {
            this.treeView.CollapseAll();
        }

        /// <summary> 否决的，暂时未正真实现，其操作现在同于 CollapseSingleAndChildrenViewItem </summary>
        /// <param name="treepath"></param>
        public void CollapseSingleViewItem(TreePath treepath)
        {
            CollapseSingleAndChildrenViewItem(treepath);
        }


        /// <summary> 关闭当前和其所有的子项的展开 </summary>
        /// <param name="treepath"></param>
        public void CollapseSingleAndChildrenViewItem(TreePath treepath)
        {
            this.treeView.CollapseRow(treepath);
        }

        /// <summary> 关闭当前和其所有的子项的展开 </summary>
        /// <param name="treepath"></param>
        public void CollapseSingleAndChildrenViewItem(T itemdata)
        {
            DoActionOfJisonsTreeIter(itemdata, (jti) => this.treeView.CollapseRow(this.treeView.GetTreePath(jti.TreeIter)));
        }

        private void DoActionOfJisonsTreeIter(T itemdata, System.Action<JisonsTreeIter<T>> action)
        {
            var jti = this.treeView.GetJisonsTreeIter(itemdata);
            if (jti != null)
            {
                action.Invoke(jti);
            }
        }

        /// <summary> 获取指定项是否为展开状态 </summary>
        /// <param name="treepath"></param>
        /// <returns></returns>
        public bool GetViewItemExpanded(TreePath treepath)
        {
            return this.treeView.GetRowExpanded(treepath);
        }

        public void SelectRow()
        {
            //this.treeView.Selection.SelectIter(((IterPos)pos).Iter);
        }

        public void UnselectRow()
        {
            //Widget.Selection.UnselectIter(((IterPos)pos).Iter);
        }

        public void ScrollToRow()
        {
            //this.treeView.ScrollToCell(Widget.Model.GetPath(((IterPos)pos).Iter), Widget.Columns[0], false, 0, 0);
        }
    }
}
