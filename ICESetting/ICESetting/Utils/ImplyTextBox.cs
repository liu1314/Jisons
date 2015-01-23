/* * * * * * * * * * * * * ** * * * * * ** * * * * * ** * * * * * ** * * * * * *
 * Copyright (C) 天脉聚源传媒科技有限公司
 * 版权所有。
 * 文件名：ImplyTextBox.cs
 * 文件功能描述：带有灰色提示信息的TextBlock
 * 创建标识：Jimmy.Bright 2011/09/27 , chujinming@tvmining.com
 * 开发须知：在进行较大逻辑改动之前请联系原作者
 * * * * * * * * * * * * * ** * * * * * ** * * * * * ** * * * * * ** * * * * * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace TVMWPFLab.Control
{
    class ImplyTextBox : TextBox
    {
        public ImplyTextBox()
        {
            Loaded += new RoutedEventHandler(ImplyTextBox_Loaded);
        }
        void ImplyTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ImplyTextBox_Loaded;
            Text = ImplyWord;
            Foreground = Brushes.White;
            FontStyle = FontStyles.Italic;
            FontSize = 17;
            Padding = new Thickness(0, 4, 0, 0);
            this.GotFocus += ImplyTextBox_GotFocus;
        }
        void ImplyTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Text = "";
            this.Foreground = Brushes.White;
            this.FontStyle = FontStyles.Normal;
            this.FontSize = 17;
        }
        /// <summary>
        /// 默认提示语
        /// </summary>
        public string ImplyWord
        {
            get { return (string)GetValue(ImplyWordProperty); }
            set
            {
                SetValue(ImplyWordProperty, value);
                Text = value;
            }
        }
        // Using a DependencyProperty as the backing store for ImplyWord.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImplyWordProperty =
            DependencyProperty.Register("ImplyWord", typeof(string), typeof(ImplyTextBox), new UIPropertyMetadata("默认提示!"));

    }
}
