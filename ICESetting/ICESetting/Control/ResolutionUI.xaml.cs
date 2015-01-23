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
using System.Windows.Threading;
using TVM.WPF.Library.Animation;
using TVMWPFLab.Control;

namespace ICESetting.Control
{
    /// <summary>
    /// ResolutionUI.xaml 的交互逻辑
    /// </summary>
    public partial class ResolutionUI : UserControl
    {
        #region 变量
        Resolution r = new Resolution();
        Resolution.DEVMODE dm = new Resolution.DEVMODE();
        List<double> ListX = new List<double>();
        double traceWidth = 14;
        int SelectedIndex = 0;
        List<ResolutionStruct> ListResolution;
        #endregion
        #region 构造

        public ResolutionUI()
        {
            InitializeComponent();
            Loaded += ResolutionUI_Loaded;
        }
        void ResolutionUI_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ResolutionUI_Loaded;
            InitializeSlider();
        }
        #endregion

        public void InitializeSlider()
        {
            try
            {

                List<TVMWPFLab.Control.Resolution.DEVMODE> allMode = new List<TVMWPFLab.Control.Resolution.DEVMODE>();
                allMode = r.getAllResolution();
                List<string> ListSize = new List<string>();
                List<string> ListResolutionDesc0 = new List<string>();
                ListResolution = new List<ResolutionStruct>();

                foreach (Resolution.DEVMODE dm in allMode)
                {
                    ResolutionStruct rs = new ResolutionStruct();
                    //string sitem = dm.dmPelsWidth + "," + dm.dmPelsHeight + "," + dm.dmDisplayFrequency + "Hz," + dm.dmBitsPerPel + "位";
                    string sitem = dm.dmPelsWidth + "," + dm.dmPelsHeight;

                    if (!ListSize.Contains(sitem))
                    {
                        ListSize.Add(sitem);
                    }
                }
                for (int i = 0; i < ListSize.Count; i++)
                {
                    string size = ListSize[i];
                    List<string> ListSt = new List<string>();
                    foreach (Resolution.DEVMODE dm in allMode)
                    {
                        string sitem = dm.dmPelsWidth + "," + dm.dmPelsHeight + "," + dm.dmDisplayFrequency + "," + dm.dmBitsPerPel;
                        if (sitem.Contains(size))
                        {
                            ListSt.Add(sitem);
                        }
                    }
                    ListResolutionDesc0.Add(ListSt.Last());
                }
                List<string> ListResolutionDesc1 = new List<string>();

                for (int i = 0; i < ListResolutionDesc0.Count; i++)
                {

                    double width = double.Parse(ListResolutionDesc0[i].Split(',')[0]);
                    if (width >= 1024)
                    {
                        ListResolutionDesc1.Add(ListResolutionDesc0[i]);
                    }
                }
                if (ListResolutionDesc1.Count < 6)
                {
                    ListResolutionDesc0.RemoveRange(0, ListResolutionDesc0.Count - 6);
                    ListResolutionDesc1.Clear();
                    ListResolutionDesc1 = ListResolutionDesc0;
                }

                foreach (string item in ListResolutionDesc1)
                {
                    int _width = int.Parse(item.Split(',')[0]);
                    int _height = int.Parse(item.Split(',')[1]);
                    int _freq = int.Parse(item.Split(',')[2]);
                    int _mybit = int.Parse(item.Split(',')[3]);
                    ResolutionStruct rs = new ResolutionStruct() { width = _width, height = _height, frequence = _freq, bitNum = _mybit };
                    ListResolution.Add(rs);
                }

                double gap = (this.Width - ListResolution.Count * traceWidth) / (ListResolution.Count - 1);
                List<Image> ListImage = new List<Image>();
                for (int i = 0; i < ListResolution.Count; i++)
                {
                    Image img = new Image() { Width = 14, Height = 22 };
                    img.Source = Utility.GetReleativeImageSource(@"/Assets/刻度.png");
                    if ((i + 1) != ListResolution.Count)
                    {
                        img.Margin = new Thickness(0, 0, gap, 0);
                    }
                    else
                    {
                        img.Margin = new Thickness(0, 0, 0, 0);
                    }
                    ListImage.Add(img);
                    stack.Children.Add(img);
                }
                for (int i = 0; i < ListResolution.Count; i++)
                {
                    double sysWidth = SystemParameters.PrimaryScreenWidth;
                    double sysHeight = SystemParameters.PrimaryScreenHeight;
                    if (ListResolution[i].width == sysWidth && ListResolution[i].height == sysHeight)
                    {
                        SelectedIndex = i;
                        break;
                    }
                }
                DispatcherTimer dt = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.2) };
                dt.Tick += delegate
                {
                    dt.Stop();
                    for (int i = 0; i < ListImage.Count; i++)
                    {
                        Point pt = ListImage[i].TranslatePoint(new Point(0, 0), this);
                        ListX.Add(pt.X);
                    }
                    MoveTo();
                };
                dt.Start();
            }
            catch { }
        }

        public void GoNext()
        {
            if (SelectedIndex != ListX.Count - 1)
            {
                SelectedIndex++;
            }
            MoveTo();
        }

        public void GoPre()
        {
            if (SelectedIndex != 0)
            {
                SelectedIndex--;
            }
            MoveTo();
        }
        private void MoveTo()
        {
            double x = ListX[SelectedIndex];
            double tox = x - showR.Width / 2.0 + 7;
            Utility.PlaySound("clickMove");
            PennerDoubleAnimation daX = new PennerDoubleAnimation()
            {
                To = tox,
                Equation = Equations.QuartEaseOut,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            showR.BeginAnimation(Canvas.LeftProperty, daX);
            //Canvas.SetLeft(showR, x - showR.Width / 2.0+15);
            text.Text = ListResolution[SelectedIndex].width.ToString() + "*" + ListResolution[SelectedIndex].height.ToString();
            DoubleAnimation daO = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            text.BeginAnimation(OpacityProperty, daO);
        }
        public event EventHandler UpdateResolution;
        internal void SetResolution()
        {
            ResolutionStruct rs = ListResolution[SelectedIndex];
            Utility.PlaySound("clickDown");
            DoubleAnimation daO = new DoubleAnimation()
            {
                From = 1,
                To = 0.5,
                AutoReverse = true,
                Duration = TimeSpan.FromSeconds(0.3)
            };
            img.BeginAnimation(OpacityProperty, daO);
            text.BeginAnimation(OpacityProperty, daO);
            Utility.INIFILE.SetValue("MAIN", "Resolution", rs.width.ToString() + "*" + rs.height.ToString());
            r.setResolution(rs.width, rs.height, rs.frequence, rs.bitNum);
            if (UpdateResolution != null)
            {
                UpdateResolution(this, new EventArgs());
            }
        }
    }
}
