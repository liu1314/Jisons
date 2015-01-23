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
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Threading;
using TVM.WPF.Library.Animation;
using UserMessageBox;
using ICESetting.Utils;
using ICESetting.Stage;
namespace TVMWPFLab.Control
{
    /// <summary>
    /// ProgressBar.xaml 的交互逻辑
    /// </summary>
    public partial class CoolProgressBar : UserControl
    {
        #region 构造
        public CoolProgressBar(string[] mediaArray)
        {
            InitializeComponent();
            DataContext = this;
            MediaNum = 0;
            ImportValue = 0;
            bar.Maximum = mediaArray.Length;
            GetMediaSize(mediaArray);
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
            Loaded += CoolProgressBar_Loaded;
        }
        public CoolProgressBar(int Max)
        {
            InitializeComponent();
            DataContext = this;
            MediaNum = 0;
            ImportValue = 0;
            bar.Maximum = Max;
            MaxMaximum = Max;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
            Loaded += CoolProgressBar_Loaded;
        }
        void CoolProgressBar_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= CoolProgressBar_Loaded;
            BarCome();
        }
        /// <summary>
        /// 进度条的文字标题
        /// </summary>
        public string ProgressBarContent
        {
            get { return (string)GetValue(ProgressBarContentProperty); }
            set { SetValue(ProgressBarContentProperty, value); }
        }
        // Using a DependencyProperty as the backing store for ProgressBarContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressBarContentProperty =
            DependencyProperty.Register("ProgressBarContent", typeof(string), typeof(CoolProgressBar), new UIPropertyMetadata("正在努力为您获取资源，请稍后"));
        #endregion
        #region 事件
        public event EventHandler LoadFinished;
        #endregion

        #region 变量
        public int MaxMaximum = 0;
        int MediaNum = 0;
        public List<Size> SizeList = new List<Size>();
        #endregion

        #region 属性
        /// <summary>
        /// 当前导入值,文件序号
        /// </summary>
        public double ImportValue
        {
            set
            {
                bar.Value = value;
                if (value == bar.Maximum)
                {
                    if (LoadFinished != null)
                    {
                        LoadFinished(this, new EventArgs());
                    }
                }
                //string st = ((int)Math.Round((value * 100d) / bar.Maximum)).ToString() + "%";
                //perCent.Text = st;
            }
        }
        #endregion
        #region 方法
        /// <summary>
        /// 获取视频和图片的原始尺寸
        /// </summary>
        /// <param name="mediaArray"></param>
        private void GetMediaSize(string[] mediaArray)
        {

            MediaPlayer player = new MediaPlayer();
            player.IsMuted = true;
            player.Open(new Uri(mediaArray[MediaNum]));//添加计时，失败则重新加载
            bool isOpened = false;
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(5);
            dt.Start();
            dt.Tick += delegate
            {
                if (!isOpened)//5秒内未能成功打开，重新加载资源
                {
                    player.Close();
                    GetMediaSize(mediaArray);
                }
                dt.Stop();
            };
            player.MediaOpened += (s, e) =>
            {
                ImportValue = MediaNum + 1;
                MediaPlayer myPlayer = s as MediaPlayer;
                Console.WriteLine("Num={0}：({1},{2})", MediaNum, myPlayer.NaturalVideoWidth, myPlayer.NaturalVideoHeight);
                SizeList.Add(new Size(myPlayer.NaturalVideoWidth, myPlayer.NaturalVideoHeight));
                myPlayer.Close();
                MediaNum++;
                isOpened = true;
                dt.Stop();
                if (MediaNum < mediaArray.Length)
                {
                    GetMediaSize(mediaArray);
                }
                else
                {

                    BarGo();
                }
            };
            player.MediaFailed += (s1, e1) =>
            {
                dt.Stop();
                MessageBox.Show("资源格式不支持(只支持图片与视频)或网络资源不存在，导入失败。");
            };
        }
        /// <summary>
        /// 显示一行的文字
        /// </summary>
        public void OneRowTitle()
        {
            _loadText1.Visibility = Visibility.Visible;
            detail.Visibility = Visibility.Collapsed;
            perCent1.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// 显示两行的文字
        /// </summary>
        public void TwoRowTitle()
        {
            _loadText1.Visibility = Visibility.Collapsed;
            perCent1.Visibility = Visibility.Collapsed;
            detail.Visibility = Visibility.Visible;
        }
        public void ReversePercentSign()
        {
            //titleBlock.Visibility = Visibility.Visible;
            //_loadText1.Visibility = Visibility.Visible;
            //perCent1.Visibility = Visibility.Visible;
            //perCent.Visibility = Visibility.Collapsed;
        }
        #endregion
        public event EventHandler BarReadyEvent;
        #region 动画
        private void BarCome()
        {
       

            PennerDoubleAnimation daScaleY = new PennerDoubleAnimation()
            {
                From = 0,
                To = 1,
                FillBehavior = FillBehavior.Stop,
                Equation = Equations.BackEaseOut,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            daScaleY.Completed += delegate
            {
                scaleGrid.ScaleY = 1;
    

                if (BarReadyEvent != null)
                {
                    BarReadyEvent(this, new EventArgs());
                }
            };
            scaleGrid.BeginAnimation(ScaleTransform.ScaleYProperty, daScaleY);
        }
        public void BarGo()
        {
            SettingStage.CurrentStage = StageEnum.MainFace;
            PennerDoubleAnimation daScaleY = new PennerDoubleAnimation()
            {
                To = 0,
                Equation = Equations.BackEaseIn,
                FillBehavior = FillBehavior.Stop,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            daScaleY.Completed += delegate
            {
                scaleGrid.ScaleY = 0;
                SettingStage.PopGrid.Visibility = Visibility.Collapsed;
                //SettingStage.CurrentStage = StageEnum.MainFace;
                Utility.RemoveUIElement(this);
                //DispatcherTimer dtX = new DispatcherTimer();
                //dtX.Interval = TimeSpan.FromSeconds(0.1);
                //dtX.Tick += delegate
                //{
                //    dtX.Stop();
                //    Utility.RemoveUIElement(this);
                //    if (!isFailed)
                //    {
                //        if (LoadFinished != null)
                //        {
                //            LoadFinished(this, new EventArgs());
                //        }
                //    }
                //};
                //dtX.Start();
            };
            daScaleY.BeginTime = TimeSpan.FromSeconds(0.5);
            scaleGrid.BeginAnimation(ScaleTransform.ScaleYProperty, daScaleY);


        }
        #endregion

        private void image0_Loaded(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            img.Loaded -= image0_Loaded;
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(0.3);
            int count = 1;
            dt.Tick += delegate
            {
                img.Source = new BitmapImage(new Uri("pack://application:,,/Library/LoadMediaSize/Assets/" + count.ToString() + ".png"));
                if (count == 3)
                {
                    count = 1;
                }
                else
                {
                    count++;
                }
            };
            dt.Start();
        }
        bool isFailed = false;
        private void RealButton_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("导入失败");
            isFailed = true;
            BarGo();
            MessageWin wm = new MessageWin("已手动停止资源获取，本次导入资源失败。");
            wm.Show(true);
        }
        internal void DealCmd(Msg msg)
        {
            if (Utility.isDownLoading)
            {
                switch (msg)
                {
                    case Msg.CANCEL://取消下载

                        if (label.Visibility==Visibility.Visible)
                        {
                            SettingStage.OrdersIntervial(1);
                            isFailed = true;
                            BarGo();
                            Utility.StopDownLoadData();
                            SettingStage.Warning.WarningText = "您已经手动取消下载更新包，系统更新失败。";
                            Utility.isManuStopLoad = true;
                            SettingStage.Stage.StopZipTimers();
                            SettingStage.Stage.StopThread();
                        }

                        break;
                    default:
                        break;
                }
            }
            else
            {
                label.Visibility = Visibility.Collapsed;
                //SettingStage.Warning.WarningText = "正在解压更新包，期间不响应任何用户操作。";
            }
        }
    }
}
