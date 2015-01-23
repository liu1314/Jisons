/* * * * * * * * * * * * * ** * * * * * ** * * * * * ** * * * * * ** * * * * * *
 * Copyright (C) 天脉聚源传媒科技有限公司
 * 版权所有。
 * 文件名：WarningPlate.cs
 * 文件功能描述：程序操作友好提示牌逻辑
 * 创建标识：Jimmy.Bright 2011/09/25 , chujinming@tvmining.com
 * 开发须知：在进行较大逻辑改动之前请联系原作者
 * * * * * * * * * * * * * ** * * * * * ** * * * * * ** * * * * * ** * * * * * */
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
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Media;
using System.IO;
using TVM.WPF.Library.Animation;
namespace TVMWPFLab.Control
{
    /// <summary>
    /// Interaction logic for WarningPlate.xaml
    /// </summary>
    public partial class WarningPlate : UserControl
    {
        public WarningPlate()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(WarningPlate_Loaded);
        }
        void WarningPlate_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= WarningPlate_Loaded;
            CompositionTarget.Rendering -= new EventHandler(CompositionTarget_Rendering);
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }
        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            Width = warningText.ActualWidth + 20;
        }
        private static DispatcherTimer dt0 = new DispatcherTimer();
        /// <summary>
        /// 显示文字提示
        /// </summary>
        private void PlateShow()
        {
            dt0.Stop();
            dt0.Interval = TimeSpan.FromSeconds(2);
            gradien1.Offset = 1;
            this.Visibility = Visibility.Visible;
            PennerDoubleAnimation daY = new PennerDoubleAnimation()
            {
                From = -20,
                To = 0,
                Equation = Equations.BackEaseOut,
                FillBehavior = FillBehavior.Stop,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            DoubleAnimation daO = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                FillBehavior = FillBehavior.Stop,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            daY.Completed += delegate
            {
                translate.Y = 0;
                Opacity = 1;
                gradient.Offset = 1;
                dt0.Tick += delegate
                {
                    dt0.Stop();
                    #region 后续隐藏提示文字
                    daY = new PennerDoubleAnimation()
                    {
                        From = 0,
                        To = -20,
                        Equation = Equations.BackEaseIn,
                        FillBehavior = FillBehavior.Stop,
                        Duration = TimeSpan.FromSeconds(0.5)
                    };
                    daO = new DoubleAnimation()
                    {
                        From = 1,
                        To = 0,
                        FillBehavior = FillBehavior.Stop,
                        Duration = TimeSpan.FromSeconds(0.5)
                    };
                    daO.Completed += delegate
                    {
                        translate.Y = -20;
                        Opacity = 0;
                        //gradien1.Offset = 0;
                        this.Visibility = Visibility.Collapsed;
                    };
                    translate.BeginAnimation(TranslateTransform.YProperty, daY);
                    BeginAnimation(OpacityProperty, daO);
                    #endregion
                };
                dt0.Start();
            };
            translate.BeginAnimation(TranslateTransform.YProperty, daY);
            BeginAnimation(OpacityProperty, daO);
            gradient.BeginAnimation(GradientStop.OffsetProperty, daO);
        }

        /// <summary>
        /// 友好提示
        /// </summary>
        public string WarningText
        {
            get
            {
                return warningText.Text;
            }
            set
            {
                warningText.Text = value;

                PlateShow();
            }
        }
    }
}
