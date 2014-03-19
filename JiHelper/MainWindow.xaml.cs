using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Jisons
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public A aa
        {
            get { return (A)GetValue(aaProperty); }
            set { SetValue(aaProperty, value); }
        }
        public static readonly DependencyProperty aaProperty =
            DependencyProperty.Register("aa", typeof(A), typeof(MainWindow), new PropertyMetadata(new A()));


        public MainWindow()
        {
            InitializeComponent();

            int a = (new Random()).Next(0, 2);
            int b5 = (new Random()).Next(1, 3);
            string b = "1";

            //dynamic xiaolong = new { Eat = (Func<bool>)(() => (new Random()).Next(0, 2) != 0) }, fanli = new { Eat = (Func<bool>)(() => (new Random()).Next(1, 3) != 1) }, count = 0;
            //var wenzhi = xiaolong.Eat() || fanli.Eat() ? ((Func<bool>)(() => count.Equals(count++))).Invoke() : ((Func<bool>)(() => (count++.Equals(count--)))).Invoke();

            this.Loaded += MainWindow_Loaded;
            //   add.Count
            //var fieald = aa.GetType().FindField("PropertyChanged1");

            //var fields = aa.GetType();
            //var fields1 = aa.GetType().GetField("_invocationList");


            //var aa2 = typeof(JisonsINotifyPropertyChanged).GetField("PropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);


            //var multicastDelegate = typeof(JisonsINotifyPropertyChanged).GetField("PropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);
            //var multicastDelegate1 = typeof(JisonsINotifyPropertyChanged).GetField("CollectionChanged", BindingFlags.Instance | BindingFlags.NonPublic);

        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            aa.PropertyChanged += aa_PropertyChanged;
            aa.Name = "67";
        }

        void aa_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {


        }


        public class A : INotifyPropertyChanged
        {
            private string name;
            public string Name
            {
                get { return name; }
                set
                {
                    name = value;

                   // this.RaisePropertyChanged("Name");

                    this.RaisePropertyChanged(() => this.Name);
                }
            }


            #region INotifyPropertyChanged 成员

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            #endregion
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            aa.Name = "67" + DateTime.Now.ToString();
        }
    }
}
