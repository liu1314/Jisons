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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TVM.WPF.Library.Animation;

namespace ICESetting.Control
{
    /// <summary>
    /// LawText.xaml 的交互逻辑
    /// </summary>
    public partial class LawText : UserControl
    {
        public LawText()
        {
            InitializeComponent();
            Loaded += LawText_Loaded;
        }

        void LawText_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= LawText_Loaded;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate()
            {
                ReadAllLaw();
            }));
        }
        private void ReadAllLaw()
        {
            this.BeginAnimation(OpacityProperty, new DoubleAnimation() { From = 0, To = 1, Duration = TimeSpan.FromSeconds(1) });
            PennerDoubleAnimation daScale = new PennerDoubleAnimation()
            {
                From = 0,
                To = 1,
                Equation = Equations.BackEaseOut,
                Duration = TimeSpan.FromSeconds(1)
            };
            _scale.BeginAnimation(ScaleTransform.ScaleYProperty, daScale);

            var lawpath = System.IO.Path.Combine(Directory.GetCurrentDirectory() + "lawText.txt");
            if (File.Exists(lawpath))
            {
                String st = File.ReadAllText(Directory.GetCurrentDirectory() + "\\lawText.txt", UnicodeEncoding.GetEncoding("GB2312"));
                _text.Text = st;
            }
            else
            {
                Console.WriteLine(lawpath + " 不存在...");
            }
        }

        public void GoUp()
        {
            scroll.PageDown();
            //for (int i = 0; i < 10; i++)
            //{
            //    scroll.LineUp();

            //}
        }
        public void GoDown()
        {
            scroll.PageUp();
            //for (int i = 0; i < 10; i++)
            //{
            //    scroll.LineDown();

            //}
        }
    }
}
