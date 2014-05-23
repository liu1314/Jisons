using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gdk;
using Gtk;
using Mono.TextEditor;

namespace Jisons
{

    public partial class JisionsTreeView<R, T>
    {


        /// <summary>   </summary>
        public event EventHandler<DragDropJudedArgs> OnDragBeginJudged;

        /// <summary>   </summary>
        public event EventHandler<DragDropArgs> OnDragDropJudged;

        /// <summary>   </summary>
        public event EventHandler<DragDropJudedArgs> OnDragMotionJudged;

        /// <summary>   </summary>
        public event EventHandler<DragDropJudedArgs> OnDragDataReceivedJudged;

        public void OnlyDragDropOut()
        {
            this.TreeView.OnlyDragDropOut();
        }

        public void InitJisionsTreeViewOfDragDrop()
        {
            this.TreeView.OnDragBeginJudged += TreeView_OnDragBeginJudged;
            this.TreeView.OnDragDropJudged += TreeView_OnDragDropJudged;
            this.TreeView.OnDragDataReceivedJudged += TreeView_OnDragDataReceivedJudged;
            this.TreeView.OnDragMotionJudged += TreeView_OnDragMotionJudged;
        }

        protected void TreeView_OnDragBeginJudged(object sender, DragDropJudedArgs e)
        {
            if (this.OnDragBeginJudged != null)
            {
                this.OnDragBeginJudged(sender, e);
            }
        }

        protected void TreeView_OnDragDropJudged(object sender, DragDropArgs e)
        {
            if (this.OnDragDropJudged != null)
            {
                this.OnDragDropJudged(sender, e);
            }
        }

        protected void TreeView_OnDragDataReceivedJudged(object sender, DragDropJudedArgs e)
        {
            if (this.OnDragDataReceivedJudged != null)
            {
                this.OnDragDataReceivedJudged(sender, e);
            }
        }

        void TreeView_OnDragMotionJudged(object sender, DragDropJudedArgs e)
        {
            if (this.OnDragMotionJudged != null)
            {
                this.OnDragMotionJudged(this, e);
            }
        }

    }
}
