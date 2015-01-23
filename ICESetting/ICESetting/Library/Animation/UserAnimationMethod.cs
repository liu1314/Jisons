/* * * * * * * * * * * * * ** * * * * * ** * * * * * ** * * * * * ** * * * * * *
 * Copyright (C) 天脉聚源传媒科技有限公司
 * 版权所有。
 * 文件名：UserAnimationMethod.cs
 * 文件功能描述：常用静态动画方法
 * 创建标识：Jimmy.Bright 2011/09/30 , chujinming@tvmining.com
 * 开发须知：在进行较大逻辑改动之前请联系原作者
 * * * * * * * * * * * * * ** * * * * * ** * * * * * ** * * * * * ** * * * * * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Media;

namespace TVM.WPF.Library.Animation
{
    class UserAnimationMethod
    {
        #region 弹出框出现和消失动画
        /// <summary>
        /// 消失动画结束事件
        /// </summary>
        public static event EventHandler UIElementGoFinished;
        /// <summary>
        /// 控件窗口弹出动画
        /// </summary>
        public static void UIElementCome(UIElement uIElement, ScaleTransform _scale)
        {
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
                uIElement.Opacity = 1;
            };
            _scale.BeginAnimation(ScaleTransform.ScaleXProperty, da);
            _scale.BeginAnimation(ScaleTransform.ScaleYProperty, da);
            uIElement.BeginAnimation(UIElement.OpacityProperty, daO);
        }
        /// <summary>
        /// 控件窗口消失动画
        /// </summary>
        public static void UIElementGo(UIElement uIElement, ScaleTransform _scale)
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
                uIElement.Opacity = 0;
                if (UIElementGoFinished != null)
                {
                    UIElementGoFinished(uIElement, new EventArgs());
                }
            };
            _scale.BeginAnimation(ScaleTransform.ScaleXProperty, da);
            _scale.BeginAnimation(ScaleTransform.ScaleYProperty, da);
            uIElement.BeginAnimation(UIElement.OpacityProperty, daO);
        }
        #endregion
    }
}
