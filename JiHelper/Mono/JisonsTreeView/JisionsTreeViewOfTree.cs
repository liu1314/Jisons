
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gdk;
using Gtk;
using Mono.TextEditor;

namespace Jisons
{

    public partial class JisionsTreeView<R, T>
        where R : class, ITreeViewData<T>
        where T : class
    {

        public Gtk.SelectionMode SelectionMode
        {
            get { return this.TreeView.Selection.Mode; }
            set { this.TreeView.Selection.Mode = value; }
        }

        public void AllDrag()
        {
            this.TreeView.AllDrag();
        }

        /// <summary> 设置 可做为拖拽接受者 </summary>
        public void AllDrop()
        {
            this.TreeView.AllDrop();
        }

        public TreeStore TreeStore
        {
            get;
            private set;
        }

        public R RootViewData { get; set; }

        private List<CellRenderer> renders;
        public List<CellRenderer> Renders
        {
            get { return renders; }
            private set { renders = value; }
        }

        public event EventHandler SelectionChanged;


        private void InitJisionsTreeViewOfTree(R rootdata, Gtk.SelectionMode selectionMode = Gtk.SelectionMode.Multiple)
        {
            this.RootViewData = rootdata;
            this.TreeStore = new TreeStore(this.RootViewData.GetData().Types);
            this.TreeView = new JisonsTree<R, T>(this, this.RootViewData, this.TreeStore);
            this.SelectionMode = selectionMode;

            #region 设置树的显示属性

            this.TreeView.HeadersVisible = false;
            this.TreeView.EnableSearch = false;

            #endregion

            this.Renders = rootdata.AddColumns(this.treeView);

            if (this.Renders != null)
            {
                foreach (var item in this.Renders)
                {
                    var toggle = item as CellRendererImageToggle;
                    if (toggle != null)
                    {
                        toggle.Toggled += toggle_Toggled;
                    }
                }
            }
        }

        void toggle_Toggled(object o, ToggledArgs args)
        {
            var cellrender = o as CellRenderer;
            int index = this.Renders.IndexOf(cellrender);
            Gtk.TreeIter iter;
            if (this.TreeStore.GetIterFromString(out iter, args.Path))
            {
                var jti = this.TreeView.GetJisonsTreeIter(iter);
                if (jti != null)
                {
                    bool val = (bool)this.TreeStore.GetValue(iter, index);
                    this.RootViewData.SetValue(jti.Data, index, !val);
                    this.TreeStore.SetValue(iter, index, !val);
                }
            }
        }

    }
}
