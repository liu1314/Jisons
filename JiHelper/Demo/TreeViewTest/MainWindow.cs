using System;
using Gtk;
using MonoDevelop.Ide.Gui.Components;
using Mono.TextEditor;
using MonoDevelop.Components;
using TreeViewTest;
using Jisons;
using System.Collections.Generic;
using System.Linq;
using Gdk;

public partial class MainWindow : Gtk.Window
{

    JisionsTreeView<MyTreeItem> b;
    JisionsTreeView<MyTreeItem> b1;

    MyTreeItem RootViewItemData1 = new MyTreeItem("RootView1", toplevel1);

    MyTreeItem RootViewItemData2 = new MyTreeItem("RootView2", toplevel11);

    public MainWindow()
        : base(Gtk.WindowType.Toplevel)
    {
        Build();

        // create tree store
        //store = new TreeStore(typeof(string), typeof(MyTreeItem[]));

        //store1 = new TreeStore(typeof(string), typeof(MyTreeItem[]));

        b = new JisionsTreeView<MyTreeItem>(RootViewItemData1) { DisplayName = "1 " };
        b.BorderWidth = 20;

        b1 = new JisionsTreeView<MyTreeItem>(RootViewItemData2) { DisplayName = "2 " };
        b1.BorderWidth = 20;

        HBox hbox = new HBox();
        hbox.Add(b);
        hbox.Add(b1);
        this.Add(hbox);

        //CreateModel();

        this.ShowAll();

        this.ButtonPressEvent += MainWindow_ButtonPressEvent;
        this.KeyPressEvent += MainWindow_KeyPressEvent;

        this.b.KeyUp += b_KeyUp;
    }

    void b_KeyUp(object sender, WidgetEventArgs e)
    {
        if (((EventKey)e.Event).Key.Equals(Gdk.Key.Delete))
        {
            this.b.GetAllSelectViewData().ForEach(i => this.b.DeleteTreeViewItem(i));
        }
    }

    void MainWindow_KeyPressEvent(object o, KeyPressEventArgs args)
    {


    }

    void MainWindow_ButtonPressEvent(object o, ButtonPressEventArgs args)
    {

    }

    void b_KeyPressEvent(object o, KeyPressEventArgs args)
    {

    }

    void b_DragBegin(object o, DragBeginArgs args)
    {

    }

    void b_ButtonPressEvent(object o, ButtonPressEventArgs args)
    {

    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }
    private static List<MyTreeItem> t4 = new List<MyTreeItem>()
		{
			new MyTreeItem ( "41",   null )
            //new MyTreeItem ( "42",   null )
		};
    private static List<MyTreeItem> t44 = new List<MyTreeItem>()
		{
			new MyTreeItem ( "441",   null )
            //,
            //new MyTreeItem ( "442",   null )
		};
    private static List<MyTreeItem> t3 = new List<MyTreeItem>()
		{
			new MyTreeItem ( "31",   t4 )
            //,
            //new MyTreeItem ( "32", null ),
            //new MyTreeItem ( "33",   null )
		};
    private static List<MyTreeItem> t33 = new List<MyTreeItem>()
		{
			new MyTreeItem ( "331",   t44 )
            //,
            //new MyTreeItem ( "332", null ),
            //new MyTreeItem ( "333",   null )
		};
    private static List<MyTreeItem> t2 = new List<MyTreeItem>()
		{
			new MyTreeItem ( "21", t3 )
            //,
            //new MyTreeItem ( "22",   null )
		};
    private static List<MyTreeItem> t22 = new List<MyTreeItem>()
		{
			new MyTreeItem ( "221", t33 )
            //,
            //new MyTreeItem ( "222",   null )
		};
    private static List<MyTreeItem> t1 = new List<MyTreeItem>()
		{
			new MyTreeItem ( "11",t2 )
            //,
            //new MyTreeItem ( "12",  null ),
            //new MyTreeItem ( "13",   null ),
            //new MyTreeItem ( "14", null ),
            //new MyTreeItem ( "15",   null )
		};
    private static List<MyTreeItem> t11 = new List<MyTreeItem>()
		{
			new MyTreeItem ( "111",t22 )
            //,
            //new MyTreeItem ( "112",  null ),
            //new MyTreeItem ( "113",   null ),
            //new MyTreeItem ( "114", null ),
            //new MyTreeItem ( "115",   null )
		};

    private static List<MyTreeItem> toplevel1 = new List<MyTreeItem>()
		{
			new MyTreeItem ("Test1", t1)
        };

    private static List<MyTreeItem> toplevel11 = new List<MyTreeItem>()
		{
			new MyTreeItem ("Test2", t11)
		};

    // TreeItem structure
    public class MyTreeItem : ITreeViewData<MyTreeItem>
    {
        public string Label;
        public IList<MyTreeItem> Children;

        public MyTreeItem(string label, IList<MyTreeItem> children)
        {
            Label = label;
            Children = children;
            if (Children == null)
            {
                Children = new List<MyTreeItem>();
            }
        }

        #region ITreeViewData 成员

        public string TreeViewName
        {
            get
            {
                return Label;
            }
            set
            {
                Label = value;
            }
        }

        public MyTreeItem TreeViewParent
        {
            get;
            set;
        }


        public object[] TreeViewValues
        {
            get
            {
                return new object[] { TreeViewName, TreeViewChildren };
            }
        }

        public Type[] TreeStoreTypes
        {
            get
            {
                return new Type[] { typeof(string), typeof(MyTreeItem[]) };
            }
        }

        #endregion


        #region ITreeViewData 成员


        public void Add(MyTreeItem viewdata)
        {
            this.Children.Add(viewdata);
        }

        public void Remove(MyTreeItem viewdata)
        {
            this.Children.Remove(viewdata);
        }

        public void Clear()
        {
            this.Children.Clear();
        }

        #endregion

        #region ITreeViewData<MyTreeItem> 成员

        public IList<MyTreeItem> TreeViewChildren
        {
            get { return Children; }
        }

        #endregion

        #region ITreeViewData<MyTreeItem> 成员

        public string Name
        {
            get
            {
                return Label;
            }
            set
            {
                Label = value;
            }
        }

        public bool CanEdit
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Visible
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IsSelected
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }

}