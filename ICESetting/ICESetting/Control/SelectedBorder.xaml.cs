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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace ICESetting.Control
{
    /// <summary>
    /// SelectedBorder.xaml 的交互逻辑
    /// </summary>
    public partial class SelectedBorder : UserControl
    {
        public SelectedBorder()
        {
            InitializeComponent();
        }
        public double X
        {
            get
            {
                return Canvas.GetLeft(this);
            }
            set
            {
                Canvas.SetLeft(this, value);
            }
        }
        public double Y
        {
            get
            {
                return Canvas.GetTop(this);
            }
            set
            {
                Canvas.SetTop(this, value);
            }
        }
        //public void SetSize(Size size)
        //{
        //    this.Width = size.Width + 20;
        //    this.Height = size.Width + 20;
        //}
    }
}
