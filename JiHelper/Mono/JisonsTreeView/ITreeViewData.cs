using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Gtk;

namespace Jisons
{
    public interface ITreeViewData<T> where T : class
    {
        string GetName(T data);
        void SetName(T data, string name);

        bool GetIsSelected(T data);
        void SetIsSelected(T data, bool value);

        //bool GetCanEdit(T data);
        //void SetCanEdit(T data, bool value);

        //bool GetVisible(T data);
        //void SetVisible(T data, bool value);

        //object GetValue(T data, int colum);
        void SetValue(T data, int colum, object value);

        T GetParentPart(T data);

        ObservableCollection<T> FirstDepthChildren { get; }

        ObservableCollection<T> GetTreeViewChildren(T data = null);

        ITreeViewItem GetData(T data = null);

        void AddPropertyChangedEventHandler(T data, PropertyChangedEventHandler handle);

        void RemovePropertyChangedEventHandler(T data, PropertyChangedEventHandler handle);

        List<CellRenderer> AddColumns(TreeView treeview);

        bool CanDrop(T data, object node, DropPosition mode, bool copy);

        void Drop(T data, List<object> node, DropPosition mode, bool copy);

    }

    public class ITreeViewItem
    {
        public Type[] Types { get; set; }

        public object[] Values { get; set; }
    }
}
