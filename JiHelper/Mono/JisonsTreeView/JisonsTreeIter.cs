using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gtk;

namespace Jisons
{
    public class JisonsTreeIter<T> where T : class
    {

        public TreeIter TreeIter { get; private set; }

        public T Data { get; set; }

        public JisonsTreeIter(T data, TreeIter treeiter)
        {
            this.TreeIter = treeiter;
            this.Data = data;
        }
    }

    public static class JisonsTreeIterHelper
    {
        public static JisonsTreeIter<R> CreatDefault<R, T>(this R itvd)
            where R : class, ITreeViewData<T>
            where T : class
        {
            return new JisonsTreeIter<R>(itvd, TreeIter.Zero);
        }
    }
}
