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
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Media;
using System.IO;
using TVM.WPF.Library.Animation;
using ICESetting;
using ICESetting.Utils;
using ICESetting.Control;
using ICESetting.Stage;
namespace UserMessageBox
{
    /// <summary>
    /// Interaction logic for MessageWin.xaml
    /// </summary>
    public partial class MessageWin : UserControl
    {
        List<FrameworkElement> ListObject = new List<FrameworkElement>();
        public int SelectedIndex = 0;
        SelectedBorder mySelectedBorder;
        public MessageWin(string warningText)
        {
            InitializeComponent();

            WarningText = warningText;
            Loaded += MessageWin_Loaded;
            ListObject.Add(okBtn);
            ListObject.Add(cancleBtn);
        }
        public MessageWin()
        {
            InitializeComponent();
            Loaded += MessageWin_TextBox;

            ListObject.Add(okBtn);
            ListObject.Add(cancleBtn);
        }

        public delegate void ClickTextBoxYesBtnEventHandler(string text);
        public event ClickTextBoxYesBtnEventHandler ClickTextBoxYesBtn;

        public event EventHandler ClickYesBtn;
        public void Show()
        {
            WinAppear();
        }
        public void Show(bool _isOkOnly)
        {
            if (_isOkOnly)
            {
                cancleBtn.Visibility = Visibility.Collapsed;
                WinAppear();
            }
        }
        /// <summary>
        /// 提示语
        /// </summary>
        public string WarningText
        {
            get
            {
                return text.Text;
            }
            set
            {
                text.Text = value;
            }
        }

        private IconKind warningIcon;
        /// <summary>
        /// 图标
        /// </summary>
        public IconKind WarningIcon
        {
            get
            {
                return warningIcon;
            }
            set
            {
                warningIcon = value;
            }
        }
        /// <summary>
        /// 图标类型
        /// </summary>
        public enum IconKind
        {
            ERROE,
            QUESTION
        }
        void MessageWin_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MessageWin_Loaded;
            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;
            InitializeSelectedBorder();
        }
        private void MessageWin_TextBox(object sender, RoutedEventArgs e)
        {
            stack.Visibility = Visibility.Collapsed;
            titleBox.Visibility = Visibility.Visible;

            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;
        }
        public void WinDisAppear()
        {
          
            PennerDoubleAnimation da = new PennerDoubleAnimation()
            {
                From = 1,
                To = 0.9,
                Equation = Equations.BackEaseIn,
                FillBehavior = FillBehavior.Stop,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            da.Completed += delegate
            {
                _scale.ScaleX = 1;
                _scale.ScaleY = 1;
            };
            DoubleAnimation daO = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                FillBehavior = FillBehavior.Stop,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            daO.Completed += delegate
            {
                Opacity = 0;
                //Close();
                MainWindow.ShowMessageWin.Children.Clear();

                MainWindow.ShowMessageWin.Visibility = Visibility.Collapsed;
            };
            _scale.BeginAnimation(ScaleTransform.ScaleXProperty, da);
            _scale.BeginAnimation(ScaleTransform.ScaleYProperty, da);
            BeginAnimation(OpacityProperty, daO);
        }
        public void WinAppear()
        {


            MainWindow.ShowMessageWin.Children.Add(this);
            MainWindow.ShowMessageWin.Visibility = Visibility.Visible;
            PennerDoubleAnimation da = new PennerDoubleAnimation()
            {
                From = 0.8,
                To = 1,
                Equation = Equations.BackEaseOut,
                FillBehavior = FillBehavior.Stop,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            da.Completed += delegate
            {
                _scale.ScaleX = 1;
                _scale.ScaleY = 1;
            };
            DoubleAnimation daO = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                FillBehavior = FillBehavior.Stop,
                Duration = TimeSpan.FromSeconds(0.7)
            };
            daO.Completed += delegate
            {
                Opacity = 1;


            };
            _scale.BeginAnimation(ScaleTransform.ScaleXProperty, da);
            _scale.BeginAnimation(ScaleTransform.ScaleYProperty, da);
            BeginAnimation(OpacityProperty, daO);
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //DragMove();
        }
        private void close_Click(object sender, EventArgs e)
        {
            IsHitTestVisible = false;
            WinDisAppear();
        }
        private void PressCancleBtn()
        {
            SettingStage.OrdersIntervial(1);

            SettingStage.CurrentStage = StageEnum.MainFace;
            cancleBtn.RemoteClick();
            IsHitTestVisible = false;
            isValid = false;
            WinDisAppear();

        }
        private void RealButton_Click(object sender, EventArgs e)
        {
            PressOkBtn();
        }
        void PressOkBtn()
        {
            SettingStage.OrdersIntervial(1);
            okBtn.RemoteClick();
            isValid = false;
            IsHitTestVisible = false;
            if (ClickYesBtn != null)
            {
                ClickYesBtn(this, new EventArgs());
            }
            if (titleBox.Visibility == Visibility.Visible)
            {
                //MessageBox.Show(titleBox.Text);
                if (ClickTextBoxYesBtn != null)
                {
                    ClickTextBoxYesBtn(titleBox.Text);
                }
            }
            WinDisAppear();
        }
        #region 指令处理
        bool isValid = true;
        private void InitializeSelectedBorder()
        {
            mySelectedBorder = new SelectedBorder();
            selectedCanvas.Children.Add(mySelectedBorder);
            FrameworkElement element = ListObject[SelectedIndex];
            Point pt = element.TranslatePoint(new Point(0, 0), selectedCanvas);
            mySelectedBorder.Width = element.Width;
            mySelectedBorder.Height = element.Height;
            mySelectedBorder.X = pt.X;
            mySelectedBorder.Y = pt.Y;
        }
        public void DealCmd(Msg msg)
        {
            if (isValid)
            {
                switch (SelectedIndex)
                {
                    case 0:
                        SelectedOkBtn(msg);
                        break;
                    case 1:
                        SelectedCancelBtn(msg);
                        break;
                    default:
                        break;
                }
            }
        }
        private void SelectedOkBtn(Msg msg)
        {
            switch (msg)
            {
                case Msg.LEFT:
                    GoBackward();
                    break;
                case Msg.RIGHT:
                    GoBackward();
                    break;
                case Msg.OK:
          
                    PressOkBtn();
                    break;
                case Msg.CANCEL:
                    //PressCancleBtn();
                    break;

                default:
                    break;
            }
        }
        private void SelectedCancelBtn(Msg msg)
        {
            switch (msg)
            {

                case Msg.LEFT:
                    GoBackward();
                    break;
                case Msg.RIGHT:
                    GoBackward();
                    break;
                case Msg.OK:
                    SettingStage.CurrentStage = StageEnum.MainFace;
                    PressCancleBtn();
                    break;
                case Msg.CANCEL:
                    //PressCancleBtn();
                    break;

                default:
                    break;
            }
        }

        private void GoBackward()
        {
            if (SelectedIndex == 0)
            {
                SelectedIndex = 1;
            }
            else
            {
                SelectedIndex = 0;
            }
            MoveBoderTo();
        }
        private void MoveBoderTo()
        {
            FrameworkElement element = ListObject[SelectedIndex];
            Point pt = element.TranslatePoint(new Point(0, 0), selectedCanvas);
            //mySelectedBorder.Width = element.Width;
            //mySelectedBorder.Height = element.Height;
            //mySelectedBorder.X = pt.X;
            //mySelectedBorder.Y = pt.Y;
            MoveXY(mySelectedBorder, pt.X, pt.Y, element.Width, element.Height);
        }
        private void MoveXY(SelectedBorder element, double toX, double toY, double width, double height)
        {
            PennerDoubleAnimation daX = new PennerDoubleAnimation()
            {
                To = toX,
                Equation = Equations.QuartEaseOut,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            PennerDoubleAnimation daY = new PennerDoubleAnimation()
            {
                To = toY,
                Equation = Equations.QuartEaseOut,

                Duration = TimeSpan.FromSeconds(0.5)
            };
            PennerDoubleAnimation daW = new PennerDoubleAnimation()
            {
                To = width,
                Equation = Equations.QuintEaseOut,
                Duration = TimeSpan.FromSeconds(0.3)
            };
            PennerDoubleAnimation daH = new PennerDoubleAnimation()
            {
                To = height,
                Equation = Equations.QuintEaseOut,

                Duration = TimeSpan.FromSeconds(0.3)
            };
            element.BeginAnimation(Canvas.LeftProperty, daX);
            element.BeginAnimation(Canvas.TopProperty, daY);
            element.BeginAnimation(WidthProperty, daW);
            element.BeginAnimation(HeightProperty, daH);
        }
        #endregion

    }
}
