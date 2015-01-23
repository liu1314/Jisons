//*-------------------------------------------------------------------
// Copyright (C) 天脉聚源传媒科技有限公司
// 版权所有。
//
// 文件名：ImageRenderingHelper.cs
// 文件功能描述：图片帧序列渲染帮助类
// 创建标识：Allen.H 2011/03 , hulunfu@tvmining.com
//--------------------------------------------------------------------- * /

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows;

namespace TVMWPFLab.Utils
{
    /// <summary>
    /// 图片帧序列渲染帮助类
    /// </summary>
    public static class ImageRenderingHelper
    {
        #region << 变量
        public static DispatcherTimer Timer = new DispatcherTimer(); //异步计时器
        public static float RENDERING_TIME = 1f; //时间
        public static HashSet<System.Windows.Controls.Image> IMAGE_LIST = new HashSet<System.Windows.Controls.Image>(); //Image控件List
        public static Dictionary<string, ImageCollection> IMAGE_FRAMES_MAP = new Dictionary<string, ImageCollection>(); //帧序列源字典
        #endregion

        #region << 方法

        /// <summary>
        /// 执行渲染
        /// </summary>
        public static void Run()
        {
            Timer.Interval = TimeSpan.FromMilliseconds(RENDERING_TIME);
            Timer.Tick += delegate
            {
                System.Windows.Controls.Image img;
                for (int i = 0; i < IMAGE_LIST.Count; ++i)
                {
                    //Dispatcher.CurrentDispatcher.Invoke(new Action(delegate()
                    //{
                        img = IMAGE_LIST.ElementAt(i) as System.Windows.Controls.Image;
                        ImageCollection col = IMAGE_FRAMES_MAP[img.Name];
                        img.Source = col.Next();
                    //}), System.Windows.Threading.DispatcherPriority.Normal);
                }
            };
            Timer.Start();
        }

        /// <summary>
        /// 停止渲染
        /// </summary>
        public static void Stop() 
        {
            Timer.Stop();
        }

        /// <summary>
        /// 添加Image
        /// </summary>
        /// <param name="img">执行帧序列的Image控件</param>
        /// <param name="source">帧序列源</param>
        public static void AddImageBase(System.Windows.Controls.Image img, ImageCollection source)
        {
            IMAGE_FRAMES_MAP.Add(img.Name, source);
            //source.Name = img.Name;
            IMAGE_LIST.Add(img);
        }

        /// <summary>
        /// 移除Image
        /// </summary>
        /// <param name="img">执行帧序列的Image控件</param>
        public static void RemoveImageBase(System.Windows.Controls.Image img)
        {
            IMAGE_LIST.Remove(img);
            IMAGE_FRAMES_MAP.Remove(img.Name);
        }

        #endregion
    }

    /// <summary>
    /// 帧序列类
    /// </summary>
    public class ImageCollection : HashSet<ImageSource>
    {
        #region << 构造函数

        public ImageCollection() { }

        public ImageCollection(ImageSource[] iArr)
        {
            for (int i = 0; i < iArr.Length; ++i) 
            {
                Add(iArr[i]);
            }
        }

        #endregion 

        #region << 属性

        /// <summary>
        /// 名字字段
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 指针
        /// </summary>
        private int _pointer = 0;
        public int Pointer
        {
            get
            {
                return _pointer;
            }
            set
            {
                _pointer = value;
            }

        }

        #endregion 

        #region << 方法

        /// <summary>
        /// 克隆对象
        /// </summary>
        /// <returns>返回类型</returns>
        public ImageCollection Clone()
        {
            ImageCollection hs = new ImageCollection();

            //ImageSource[] iArr = new ImageSource[this.Count];
            //this.CopyTo(iArr);

            for (int i = 0; i < this.Count; ++i)
            {
                hs.Add(this.ElementAt(i).Clone());
            }

            return hs;
        }

        /// <summary>
        /// 取下一帧
        /// </summary>
        /// <returns></returns>
        public ImageSource Next()
        {
            ImageSource s = this.ElementAt(_pointer);
            if (++_pointer == this.Count)
                _pointer = 0;
            return s;
        }

        /// <summary>
        /// 取上一帧
        /// </summary>
        /// <returns></returns>
        public ImageSource Previous() 
        {
            ImageSource s = this.ElementAt(_pointer);
            if (--_pointer == -1)
                _pointer = this.Count - 1;
            return s;
        }

        /// <summary>
        /// 添加一帧
        /// </summary>
        /// <param name="path">文件地址</param>
        public void Add(string path)
        {
            Add(new BitmapImage(new Uri(path)));
        }

        /// <summary>
        /// 添加一帧
        /// </summary>
        /// <param name="stream">文件流</param>
        public void Add(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            Add(GetImageSource(new Bitmap(stream)));
            stream.Close();
        }

        [DllImport("gdi32")]
        public static extern int DeleteObject(IntPtr o);
        /// <summary>
        /// 图片源头转换（Bitmap -> ImageSource）
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static ImageSource GetImageSource(System.Drawing.Bitmap bitmap)
        {
            IntPtr ip = bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                ip, IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(ip);
            return bitmapSource;
        }

        #endregion 
    }
}
