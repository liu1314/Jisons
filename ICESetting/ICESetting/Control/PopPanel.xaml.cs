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
using TVM.WPF.Library.Animation;

namespace ICESetting.Control
{
    /// <summary>
    /// PopPanel.xaml 的交互逻辑
    /// </summary>
    public partial class PopPanel : UserControl
    {
        #region 变量
        public const double OffsetScreenWidth = 200d;//露出屏幕的位置
        bool isAnimating = false;
        #endregion
        #region 构造

        public PopPanel()
        {
            InitializeComponent();

            Loaded += PopPanel_Loaded;
        }

        void PopPanel_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= PopPanel_Loaded;

  
     
        }
        #endregion

        #region 方法
        Stack<PopCell> StackRight = new Stack<PopCell>();//右边资源发出队列
        Stack<PopCell> StackLeft = new Stack<PopCell>();//左边资源接收队列
        PopCell[] CenterPopArray = new PopCell[3];
        List<PopCell> PopCellList = new List<PopCell>();
        public void InitializeStack(List<string> listAddress)
        {
            //this.Width = SystemParameters.PrimaryScreenWidth;
            //this.Height = SystemParameters.PrimaryScreenHeight;
            List<ItemInfo> Listitems = new List<ItemInfo>();
            foreach (var item in listAddress)
            {
                ItemInfo info = new ItemInfo();
                info.Source = item;
                Listitems.Add(info);
            }
            List<ItemInfo> ListItems = Listitems;
            Stack<PopCell> PopCellQueue = new Stack<PopCell>();
            #region 资源分类
            for (int i = 0; i < ListItems.Count; i++)
            {

                TypeEnum_BigScreen_图片(PopCellQueue, ListItems[i]);
            }
            #endregion
            int n = PopCellQueue.Count;
            for (int i = 0; i < n; i++)
            {
                StackRight.Push(PopCellQueue.Pop());
            }
            InitializeCanvas();
        }

        private void TypeEnum_BigScreen_图片(Stack<PopCell> PopCellQueue, ItemInfo itemInfo)
        {
            PopCell cell = new PopCell();
            cell.TilePosition = ICESetting.Control.PopCell.TilePositionEnum.Other;
           
            cell.CellSource = itemInfo.Source;
            cell.X = (this.Width - cell.Width) / 2.0;
            cell.Y = (this.Height - cell.Height) / 2.0;
            cell.ID = itemInfo.ID;
            cell.UID = itemInfo.UID;
            
            PopCellQueue.Push(cell);

            PopCellList.Add(cell);
        }
        private void InitializeCanvas()
        {
            if (StackRight.Count == 0)
            {

            }
            else if (StackRight.Count == 1)
            {
                PopCell cellA = StackRight.Pop();
                cellA.TilePosition = ICESetting.Control.PopCell.TilePositionEnum.Center;
                PutToCenter(cellA, Equations.Linear);
                _canvas.Children.Add(cellA);
                cellA.Scale = 1;
                cellA.Opacity = 1;
                CenterPopArray[1] = cellA;
            }
            else
            {
                PopCell cellA = StackRight.Pop();
                cellA.TilePosition = ICESetting.Control.PopCell.TilePositionEnum.Center;
                PutToCenter(cellA, Equations.Linear);
                _canvas.Children.Add(cellA);
                PopCell cellB = StackRight.Pop();
                cellB.TilePosition = ICESetting.Control.PopCell.TilePositionEnum.Right;
                PutToRight(cellB, Equations.Linear);
                _canvas.Children.Add(cellB);
                CenterPopArray[0] = null;
                CenterPopArray[1] = cellA;
                CenterPopArray[2] = cellB;//中间列表数组
                cellA.Scale = 1;
                cellA.Opacity = 1;
            }
        }
        private void PutToCenter(PopCell cell, Equations equation)
        {
            MoveToX(cell, (this.Width - cell.Width) / 2.0, equation);
            MoveToScaleOpacity(cell, 1d, 1d);
            cell.Y = (this.Height - cell.Height) / 2.0;
            cell.IsHitTestVisible = true;
        }
        private void PutToRight(PopCell cell, Equations equation)
        {
            MoveToX(cell, this.Width - OffsetScreenWidth, equation);
            cell.Y = (this.Height - cell.Height) / 2.0;
            MoveToScaleOpacity(cell, 0.9d, 0.7d);
            cell.IsHitTestVisible = false;
        }
        private void PutToLeft(PopCell cell, Equations equation)
        {
            MoveToX(cell, -cell.Width + OffsetScreenWidth, equation);
            cell.Y = (this.Height - cell.Height) / 2.0;
            MoveToScaleOpacity(cell, 0.9d, 0.7d);
            cell.IsHitTestVisible = false; ;
        }
        /// <summary>
        /// 右边栈所有元素重置初始坐标
        /// </summary>
        private void ResetStackRight()
        {
            double x;
            PopCell cell = CenterPopArray[2];
            if (cell == null)
            {
                x = this.Width;
            }
            else
            {
                x = cell.X + cell.Width;
            }
            foreach (PopCell item in StackRight)
            {
                item.X = x;
                item.Scale = 0.9;
                item.Opacity = 0.7d;
            }
        }
        /// <summary>
        /// 左边栈所有元素重置初始坐标
        /// </summary>
        private void ResetStackLeft()
        {
            PopCell cell = CenterPopArray[0];
            if (cell == null)
            {
                foreach (PopCell item in StackLeft)
                {
                    item.X = -item.Width;
                    item.Scale = 0.9;
                    item.Opacity = 0.7d;
                }
            }
            else
            {
                foreach (PopCell item in StackLeft)
                {
                    item.X = -item.Width - cell.Width + OffsetScreenWidth;
                    item.Scale = 0.9;
                    item.Opacity = 0.7d;
                }
            }
        }
        private void MoveToX(PopCell Cell, double toX, Equations equation)
        {
            isAnimating = true;
            PennerDoubleAnimation daX = new PennerDoubleAnimation()
            {
                To = toX,
                Equation = equation,
                FillBehavior = FillBehavior.Stop,
                Duration = TimeSpan.FromSeconds(0.7)
            };
            if (equation == Equations.QuadEaseOut)
            {
                daX.Duration = TimeSpan.FromSeconds(1);
            }
            daX.Completed += delegate { Cell.X = toX; isAnimating = false; };
            Cell.BeginAnimation(Canvas.LeftProperty, daX);
        }
        private void MoveToScaleOpacity(PopCell cell, double toScale, double toOpacity)
        {

            DoubleAnimation daScale = new DoubleAnimation()
            {
                To = toScale,
                //Equation=Equations.QuartEaseOut,
                FillBehavior = FillBehavior.Stop,
                Duration = TimeSpan.FromSeconds(0.7)
            };
            DoubleAnimation daO = new DoubleAnimation()
            {
                To = toOpacity,
                FillBehavior = FillBehavior.Stop,
                Duration = TimeSpan.FromSeconds(0.7)
            };
            daO.Completed += delegate
            {
                cell.Opacity = toOpacity;
            };
            daScale.Completed += delegate
            {
                cell.Scale = toScale;

            };
            cell._scale.BeginAnimation(ScaleTransform.ScaleXProperty, daScale);
            cell._scale.BeginAnimation(ScaleTransform.ScaleYProperty, daScale);
            cell.BeginAnimation(OpacityProperty, daO);
        }
        #endregion


        #region 遥控移动
        /// <summary>
        /// 左移动
        /// </summary>
        private void MoveLeft()
        {
            if (!isAnimating)
            {
                ResetStackRight();
                PopCell cellA = CenterPopArray[0];
                PopCell cellB = CenterPopArray[1];
                PopCell cellC = CenterPopArray[2];
                if (cellC != null)
                {
        
                    if (cellA != null)
                    {
                        DoubleAnimation daX = new DoubleAnimation()
                        {
                            To = -cellA.Width,
                            FillBehavior = FillBehavior.Stop,
                            Duration = TimeSpan.FromSeconds(0.2)
                        };
                        daX.Completed += delegate
                        {
                            _canvas.Children.Remove(cellA);
                        };
                        cellA.BeginAnimation(Canvas.LeftProperty, daX);

                        //_canvas.Children.Remove(cellA);
                        cellA.TilePosition = ICESetting.Control.PopCell.TilePositionEnum.Other;
                        StackLeft.Push(cellA);
                    }
                    CenterPopArray[0] = cellB;
                    CenterPopArray[0].TilePosition = ICESetting.Control.PopCell.TilePositionEnum.Left;
                    PutToLeft(CenterPopArray[0], Equations.QuadEaseOut);

                    CenterPopArray[1] = cellC;
                    CenterPopArray[1].TilePosition = ICESetting.Control.PopCell.TilePositionEnum.Center;
                    PutToCenter(CenterPopArray[1], Equations.SineEaseOut);

                    if (StackRight.Count != 0)
                    {
                        CenterPopArray[2] = StackRight.Pop();
                        CenterPopArray[2].TilePosition = ICESetting.Control.PopCell.TilePositionEnum.Right;
                        PutToRight(CenterPopArray[2], Equations.QuadEaseOut);
                        _canvas.Children.Add(CenterPopArray[2]);
                    }
                    else
                    {
                        CenterPopArray[2] = null;
                    }
                }
            }
        }
        /// <summary>
        /// 右移动
        /// </summary>
        private void MoveRight()
        {
            if (!isAnimating)
            {
                ResetStackLeft();
                PopCell cellA = CenterPopArray[0];
                PopCell cellB = CenterPopArray[1];
                PopCell cellC = CenterPopArray[2];
                if (cellA != null)
                {
         
                    if (StackLeft.Count != 0)
                    {
                        CenterPopArray[0] = StackLeft.Pop();
                        CenterPopArray[0].TilePosition =ICESetting.Control.PopCell.TilePositionEnum.Left;
                        PutToLeft(CenterPopArray[0], Equations.QuadEaseOut);
                        _canvas.Children.Add(CenterPopArray[0]);
                    }
                    else
                    {
                        CenterPopArray[0] = null;
                    }
                    CenterPopArray[1] = cellA;
                    CenterPopArray[1].TilePosition = ICESetting.Control.PopCell.TilePositionEnum.Center;
                    PutToCenter(CenterPopArray[1], Equations.SineEaseOut);

                    CenterPopArray[2] = cellB;
                    CenterPopArray[2].TilePosition = ICESetting.Control.PopCell.TilePositionEnum.Right;
                    PutToRight(CenterPopArray[2], Equations.QuadEaseOut);

                    if (cellC != null)
                    {

                        DoubleAnimation daX = new DoubleAnimation() { To = this.Width, FillBehavior = FillBehavior.Stop, Duration = TimeSpan.FromSeconds(0.2) };
                        daX.Completed += delegate
                        {
                            _canvas.Children.Remove(cellC);
                        };

                        cellC.BeginAnimation(Canvas.LeftProperty, daX);
                        cellC.TilePosition =ICESetting.Control.PopCell.TilePositionEnum.Other;
                        StackRight.Push(cellC);
                    }
                }
                
            }
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
        }
        public event EventHandler ClosePopPanel;
        internal void DealCmd(Msg msg)
        {
            switch (msg)
            {
       
   
                case Msg.LEFT:
                    MoveLeft();
                    break;
                case Msg.RIGHT:
                    MoveRight();
                    break;
                case Msg.CANCEL:
                    if (ClosePopPanel!=null)
                    {
                        ClosePopPanel(this, new EventArgs());
                    }
                    break;
                default:
                    break;
            }
        }



    }
}
