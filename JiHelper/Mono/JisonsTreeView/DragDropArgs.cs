using System;
using Gdk;

namespace Jisons
{
    public class DragDropJudedArgs : EventArgs
    {
        private bool handle = false;
        public bool Handle
        {
            get { return handle; }
            set { handle = value; }
        }

        public DragContext DragContext { get; set; }
    }
}
