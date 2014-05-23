using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Gtk;
using Mono.TextEditor;

namespace Jisons
{

    public partial class JisionsTreeView<R, T>
    {

        /// <summary> 在此屏蔽内部的添加删除触发，只处理对外的数据更改事件 </summary>
        private bool IsDoSelf = false;

        private void InitJisionsTreeViewOfData()
        {
            this.TreeView.SelectionChanged += TreeView_SelectionChanged;
        }

        #region 界面数据操作

        public JisonsTreeIter<T> AddTreeViewItem(T parent, T data)
        {
            return this.AddTreeViewItem(this.TreeView.GetJisonsTreeIter(parent), data);
        }

        public JisonsTreeIter<T> AddTreeViewItem(JisonsTreeIter<T> parent, T data)
        {
            this.IsDoSelf = true;
            return this.TreeView.AddViewItem(parent.TreeIter, data);
            this.IsDoSelf = false;
        }

        public void DeleteTreeViewItem(T data)
        {
            this.IsDoSelf = true;
            this.TreeView.DeleteJisonsTreeIter(this.TreeView.GetJisonsTreeIter(data));
            this.IsDoSelf = false;
        }

        public void DeleteTreeViewItem(JisonsTreeIter<T> jti)
        {
            this.IsDoSelf = true;
            this.TreeView.DeleteViewItem(jti.TreeIter);
            this.IsDoSelf = false;
        }

        public void DeleteAllItems()
        {
            this.IsDoSelf = true;
            this.treeView.DeleteAllItems();
            this.IsDoSelf = false;
        }

        public void RefreshTreeView(R rootview = null)
        {
            if (rootview != null)
            {
                this.RootViewData = rootview;
            }

            this.treeView.ReSetRootViewData(this.RootViewData);
        }

        #endregion

        #region 选中事件处理

        /// <summary> 屏蔽内部自己的选中事件标识 </summary>
        private bool IsSelfSelcted = false;

        /// <summary> 通知外部有选中项更改 </summary>
        protected void OnSelectionChangedHandle()
        {
            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(this, EventArgs.Empty);
            }
        }

        /// <summary> 在此屏蔽内部自己的选中事件 </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TreeView_SelectionChanged(object sender, EventArgs e)
        {
            if (!IsSelfSelcted)
            {
                OnSelectionChangedHandle();
            }
        }

        public void SelectTreeViewItem(T data)
        {
            this.IsSelfSelcted = true;

            var jti = this.TreeView.GetJisonsTreeIter(data);
            if (jti != null)
            {
                this.TreeView.SelectTreeIter(jti.TreeIter);
            }

            this.IsSelfSelcted = false;
        }

        public void SelectAll()
        {
            this.IsSelfSelcted = true;
            this.TreeView.SelectAll();
            this.IsSelfSelcted = false;
        }

        public void UnSelectTreeViewItem(T data)
        {
            this.IsSelfSelcted = true;
            this.TreeView.UnSelectTreeIter(this.TreeView.GetJisonsTreeIter(data).TreeIter);
            this.IsSelfSelcted = false;
        }

        public void UnselectAll()
        {
            this.IsSelfSelcted = true;
            this.TreeView.UnselectAll();
            this.IsSelfSelcted = false;
        }

        public List<T> GetSelectViewData()
        {
            return this.treeView.GetSelectViewData();
        }

        public List<T> GetAllSelectViewData()
        {
            return this.treeView.GetAllSelectViewData();
        }

        #endregion

        #region 数据获取

        public JisonsTreeIter<T> GetJisonsTreeIter(T data)
        {
            return this.TreeView.GetJisonsTreeIter(data);
        }

        #endregion

    }
}
