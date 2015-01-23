using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ICESetting.Control
{
    /// <summary>
    /// MyTime.xaml 的交互逻辑
    /// </summary>
    public partial class MyTime : UserControl
    {
        public MyTime()
        {
            InitializeComponent();
            this.Loaded += MyTime_Loaded;
        }

        void MyTime_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MyTime_Loaded;
            BeginAnimation(OpacityProperty, new DoubleAnimation() { From=0,To=1,Duration=TimeSpan.FromSeconds(0.5)});
            _time.Text = DefaultValue + " " + Unit;

        }

        private string _unit = "";
        public string Unit
        {
            get { return _unit; }
            set { _unit = value; }
        }

        private List<string> _listData = new List<string>();
        public List<string> ListData
        {
            get { return _listData; }
            set { _listData = value; }
        }

        private string _defaultValue = "";
        public string DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }

        /// <summary>
        /// 上
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int index = ListData.IndexOf(DefaultValue);
            if (index > 0 && index < ListData.Count)
            {
                string t = ListData[index - 1];
                DefaultValue = t;
                _time.Text = t + " " + Unit;
            }
        }

        /// <summary>
        /// 下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            int index = ListData.IndexOf(DefaultValue);
            if (index + 1 > 0 && index + 1 < ListData.Count)
            {
                string t = ListData[index + 1];
                DefaultValue = t;
                _time.Text = t + " " + Unit;
            }
        }
    }
}