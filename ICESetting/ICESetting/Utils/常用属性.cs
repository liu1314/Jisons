using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
namespace Jimmy_Global.Utils
{
    class 常用属性:UserControl
    {
        #region 属性
        /// <summary>
        /// 资源块序号
        /// 
        /// </summary>
        private string _itemID;
        public string ItemID
        {
            get
            {
                return _itemID;
            }
            set
            {
                _itemID = value;
            }
        }
        /// <summary>
        /// 图片路径
        /// </summary>
        private string _thumbSource;
        public string thumbSource
        {
            get
            {
                return _thumbSource;
            }
            set
            {
                _thumbSource = value;
            }
        }
        /// <summary>
        /// 视频路径
        /// </summary>
        private string _mediaSource;
        public string MediaSource
        {
            get
            {
                return _mediaSource;
            }
            set
            {
                _mediaSource = value;
            }
        }
        /// <summary>
        /// 标题
        /// </summary>
        private string _titleText;
        public string TitleText
        {
            get
            {
                return _titleText;
            }
            set
            {
                _titleText = value;
            }
        }
        private string _desc;
        public string Desc
        {
            get
            {
                return _desc;
            }
            set
            {
                _desc = value;
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

        #endregion
    }
}
