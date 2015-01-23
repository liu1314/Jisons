using ICESetting.Utils;
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
    /// DescCell.xaml 的交互逻辑
    /// </summary>
    public partial class PopCell : UserControl
    {
        public PopCell()
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(PopCell_Loaded);
        }
        void PopCell_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= PopCell_Loaded;
            //if (Utility.PlayMediaAuto)//如果要求自动播放，则去掉播放暂停按钮
            //{
            //    btn.Opacity = 0;
            //    btn.IsHitTestVisible = false;
            //}
            Appear();
        }
        private void Appear()
        {
            DoubleAnimation daO = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                FillBehavior = FillBehavior.Stop,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            daO.Completed += delegate
            {
                Opacity = 1;
            };
            BeginAnimation(OpacityProperty, daO);
        }
        public enum TilePositionEnum { Left, Center, Right, Other, }
        public TilePositionEnum TilePosition
        {
            get;
            set;
        }
        private string m_CellSource;
        public string CellSource
        {
            get
            {
                return m_CellSource;
            }
            set
            {
                m_CellSource = value;
                string fileName = System.IO.Path.GetFileName(value);
                try
                {
                    BitmapSource bitmap = null;

                    bitmap = new BitmapImage(new Uri(value, UriKind.RelativeOrAbsolute));
                    //this.Width = bitmap.PixelWidth;
                    this.Height = SystemParameters.PrimaryScreenHeight;
                    this.Width = bitmap.PixelWidth * (this.Height / bitmap.PixelHeight);
                    _img.Source = bitmap;
                    CellSize = new Size(this.Width,this.Height);
                }
                catch (Exception e)
                {
     
                }
            }
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
        public double Scale
        {
            get
            {
                return _scale.ScaleX;

            }
            set
            {
                _scale.ScaleX = _scale.ScaleY = value;
            }
        }
        public string ID
        {
            get;
            set;
        }
        public string UID
        {
            get;
            set;
        }
        /// <summary>
        /// 资源实际尺寸
        /// </summary>
        public Size CellSize
        {
            set
            {
                this.Height = Utility.PopCellHeightMax;
                Width = this.Height * value.Width / value.Height;
                //if (Width > SystemParameters.PrimaryScreenWidth - 2 * 150)
                //{
                //    Width = SystemParameters.PrimaryScreenWidth - 2 * 150;
                //    this.Height = Width * value.Height / value.Width;
                //}
            }
        }
    }
}
