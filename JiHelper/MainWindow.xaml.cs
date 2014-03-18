using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public MainWindow()
        {
            InitializeComponent();

            int a = (new Random()).Next(0, 2);
            int b5 = (new Random()).Next(1, 3);
            string b = "1";

            dynamic xiaolong = new { Eat = (Func<bool>)(() => (new Random()).Next(0, 2) != 0) }, fanli = new { Eat = (Func<bool>)(() => (new Random()).Next(1, 3) != 1) }, count = 0;
            var wenzhi = xiaolong.Eat() || fanli.Eat() ? ((Func<bool>)(() => count.Equals(count++))).Invoke() : ((Func<bool>)(() => (count++.Equals(count--)))).Invoke();
        }
    }
}
