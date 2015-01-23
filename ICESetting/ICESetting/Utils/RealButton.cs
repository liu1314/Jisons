/* * * * * * * * * * * * * ** * * * * * ** * * * * * ** * * * * * ** * * * * * *
 * Copyright (C) 天脉聚源传媒科技有限公司
 * 版权所有。
 * 文件名：RealButton.cs
 * 文件功能描述：按钮
 * 创建标识：Jimmy.Bright 2011/09/27 , chujinming@tvmining.com
 * 开发须知：在进行较大逻辑改动之前请联系原作者
 * * * * * * * * * * * * * ** * * * * * ** * * * * * ** * * * * * ** * * * * * */
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.IO;
using System.Windows.Input;
using System.Windows.Threading;
using ICESetting.Utils;
namespace TVMWPFLab.Button
{
    public class RealButton : Image
    {
        public RealButton()
        {
            Loaded += new RoutedEventHandler(RealButton_Loaded);
        }
        void RealButton_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= RealButton_Loaded;
            Source = ImageUpSource;
            MouseLeftButtonDown += RealButton_MouseLeftButtonDown;
            MouseLeftButtonUp += RealButton_MouseLeftButtonUp;
            MouseEnter += RealButton_MouseEnter;
            MouseLeave += RealButton_MouseLeave;
            StylusUp += new StylusEventHandler(RealButton_StylusUp);
        }


        public event EventHandler Click;
        public static readonly DependencyProperty ImageUpSourceProperty = DependencyProperty.Register("ImageUpSource", typeof(ImageSource), typeof(RealButton), new UIPropertyMetadata(null, new PropertyChangedCallback(ImageUpSourceChanged)));
        public static readonly DependencyProperty ImageDownSourceProperty = DependencyProperty.Register("ImageDownSource", typeof(ImageSource), typeof(RealButton), new UIPropertyMetadata(null));
        public static readonly DependencyProperty ImageHoverSourceProperty = DependencyProperty.Register("ImageHoverSource", typeof(ImageSource), typeof(RealButton), new UIPropertyMetadata(null));
        private static void ImageUpSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as RealButton).Source = e.NewValue as ImageSource;
        }
        /// <summary>
        /// 遥控点击状态变化
        /// </summary>
        public void RemoteClick()
        {
            Utility.PlaySound("clickDown");
            Source = ImageDownSource;
            DoubleAnimation daO = new DoubleAnimation()
            {
                From = 0.5,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            daO.Completed += delegate
            {
                Source = ImageUpSource;
            };
            this.BeginAnimation(OpacityProperty, daO);
        }
        #region 鼠标事件
        void RealButton_StylusUp(object sender, StylusEventArgs e)
        {
            //Utility.PlaySound("clickDown");
            dt.Interval = TimeSpan.FromSeconds(0.2);
            dt.Tick += delegate
            {
                dt.Stop();
                Source = ImageUpSource;
            };
            dt.Start();
            if (Click != null)
            {
                Click(this, new EventArgs());
            }
            isDown = false;
        }
        bool isDown = false;
        DispatcherTimer dt = new DispatcherTimer();
        void RealButton_MouseLeave(object sender, MouseEventArgs e)
        {
            Source = ImageUpSource;
            isDown = false;
        }
        void RealButton_MouseEnter(object sender, MouseEventArgs e)
        {
            //Utility.PlaySound("clickMove");
            if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
            {
                Source = ImageDownSource;
            }
            else
            {
                Source = ImageHoverSource;
                //DoubleAnimation daO = new DoubleAnimation()
                //{
                //    From = 0.4,
                //    To = 1,
                //    FillBehavior = FillBehavior.Stop,
                //    Duration = TimeSpan.FromSeconds(0.7)
                //};
                //daO.Completed += delegate
                //{
                //    Opacity = 1;
                //};
                //BeginAnimation(OpacityProperty, daO);
            }
        }

        void RealButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Utility.PlaySound("clickDown");

            Source = ImageDownSource;
            isDown = true;
        }

        void RealButton_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isDown == true)
            {
                dt.Interval = TimeSpan.FromSeconds(0.2);
                dt.Tick += delegate
                {
                    dt.Stop();
                    Source = ImageUpSource;
                };
                dt.Start();
                if (Click != null)
                {
                    Click(this, new EventArgs());
                }
                isDown = false;
            }
        }
        #endregion
        #region 图片来源
        //鼠标抬起切换图片
        public ImageSource ImageUpSource
        {
            get { return (ImageSource)GetValue(ImageUpSourceProperty); }
            set { SetValue(ImageUpSourceProperty, value); }
        }
        //鼠标按下切换图片
        public ImageSource ImageDownSource
        {
            get { return (ImageSource)GetValue(ImageDownSourceProperty); }
            set { SetValue(ImageDownSourceProperty, value); }
        }
        //鼠标滑过切换图片
        public ImageSource ImageHoverSource
        {
            get { return (ImageSource)GetValue(ImageHoverSourceProperty); }
            set { SetValue(ImageHoverSourceProperty, value); }
        }
        #endregion
    }
}
