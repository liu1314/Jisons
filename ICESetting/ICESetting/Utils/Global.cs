/* * * * * * * * * * * * * ** * * * * * ** * * * * * ** * * * * * ** * * * * * *
 * Copyright (C) 天脉聚源传媒科技有限公司
 * 版权所有。
 * 文件名：Global.cs
 * 文件功能描述：全局类
 * 创建标识：Jimmy.Bright 2011/09/27 , chujinming@tvmining.com
 * 开发须知：在进行较大逻辑改动之前请联系原作者
 * * * * * * * * * * * * * ** * * * * * ** * * * * * ** * * * * * ** * * * * * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using IWshRuntimeLibrary;
using System.Windows;
using System.Windows.Media;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Xml;
using System.Windows.Media.Media3D;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Diagnostics;
using TVMWPFLab.Utils;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using Microsoft.VisualBasic.FileIO;
using System.Net;
using System.Windows.Ink;
using System.Windows.Data;
namespace TVMWPFLab.Utils
{

    public class Global
    {
        #region 全局变量
        public static Window WHD = Application.Current.MainWindow;
        public static double SCREEN_WIDTH = SystemParameters.PrimaryScreenWidth;
        public static double SCREEN_HEIGHT = SystemParameters.PrimaryScreenHeight;
        public static IniFile INIFILE = new IniFile("\\Config\\config.ini");
        public static Random myRandom = new Random();
        #endregion

        #region 通用画笔属性
        public static DrawingAttributes DrawAttr = new DrawingAttributes()
        {
            Color = Colors.Red,
            Width = 2,
            Height = 4,
            StylusTipTransform = new System.Windows.Media.Matrix(1, 1.5, 2.2, 1, 0, 0),
            StylusTip = StylusTip.Rectangle,
            IsHighlighter = true,
            IgnorePressure = true,
            FitToCurve = true
        };
        //inkCanvas.DefaultDrawingAttributes = DrawAttr;
        #endregion

        #region 获取double类型随机数
        /// <summary>
        /// 获取任意double小数之间的随机double小数
        /// </summary>
        /// <param name="from">小数最小值</param>
        /// <param name="to">小数最大值</param>
        /// <param name="randomHelp">随机系数＞0，越大，随机种类越多</param>
        /// <returns></returns>
        public static double GetRandomDoubleNumber(double minDouble, double maxDouble, int randomHelp)
        {
            int min = (int)(minDouble * randomHelp);
            int max = (int)(maxDouble * randomHelp);
            return (myRandom.Next(min, max) + myRandom.NextDouble()) / randomHelp;
        }
        #endregion

        #region 获取系统当前时间字符串

        public static string GetSystemDateString()
        {
            string year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.Month.ToString();
            string day = DateTime.Now.Day.ToString();
            string time = DateTime.Now.ToLongTimeString();
            string ms = DateTime.Now.Millisecond.ToString();

            return string.Format("{0}-{1}-{2}-{3}-{4}", year, month, day, time, ms).Replace(':', '-');
        }

        #endregion

        #region 判断网络地址是否存在
        /// <summary>
        /// 返回200表示地址可访问
        /// </summary>
        /// <param name="curl"></param>
        /// <returns></returns>
        public static int GetUrlError(string curl)
        {
            int num = 0;
            if (curl.Contains("http:"))
            {
                num = 200;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(curl));
                ServicePointManager.Expect100Continue = false;
                try
                {
                    ((HttpWebResponse)request.GetResponse()).Close();
                }
                catch (WebException exception)
                {
                    if (exception.Status != WebExceptionStatus.ProtocolError)
                    {
                        return num;
                    }
                    if (exception.Message.IndexOf("500 ") > 0)
                    {
                        return 500;
                    }
                    if (exception.Message.IndexOf("401 ") > 0)
                    {
                        return 401;
                    }
                    if (exception.Message.IndexOf("404") > 0)
                    {
                        num = 404;
                    }
                }
            }
            else if (File.Exists(curl))
            {
                num = 200;
            }
            else
            {
                num = 404;
            }
            return num;

        }
        #endregion

        #region 网络文件读取为流文件
        //[DllImport("gdi32.dll")]
        //public static extern int DeleteObject(IntPtr o);
        public static ImageSource GetWebImageSource(string urlSource)
        {
            WebClient wc = new WebClient();
            byte[] bt = wc.DownloadData(urlSource);
            MemoryStream fs = new MemoryStream(bt);

            //FileStream fs = new FileStream(urlSource, FileMode.Open, FileAccess.Read);
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(fs);
            fs.Close();
            IntPtr ip = bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(ip);
            return bitmapSource;
        }
        #endregion

        #region 隐藏鼠标

        [DllImport("user32.dll", EntryPoint = "ShowCursor", CharSet = CharSet.Auto)]
        public static extern void ShowCursor(int status); // 0:hidden,1:visible

        #endregion

        #region 内存垃圾回收

        [DllImport("kernel32.dll")]
        public static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

        public static void FlushMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }

        #endregion

        #region 颜色字符串转颜色
        /// <summary>
        /// 16进制颜色在CS端转换
        /// </summary>
        /// <param name="colorName">十六进制颜色，，例如半透明黑色：#800C0303</param>
        /// <returns></returns>
        public static Color StringToColor(string colorName)
        {
            BrushConverter conv = new BrushConverter();
            SolidColorBrush brush = conv.ConvertFromString(colorName) as SolidColorBrush;
            return brush.Color;
        }

        #endregion

        #region Visual的点击测试
        /// <summary>
        /// 二维点击测试
        /// </summary>
        /// <param name="e">鼠标点击事件</param>
        /// <param name="panel">鼠标点击的容器</param>
        /// <returns>返回点击测试获得的UIElement</returns>
        public static UIElement GetHitTestResult(MouseButtonEventArgs e, Panel panel)
        {
            HitTestResult result = VisualTreeHelper.HitTest(panel, e.GetPosition(panel));
            if (result != null)
            {
                if (result.VisualHit is UIElement)
                {
                    return result.VisualHit as UIElement;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 三维点击测试
        /// </summary>
        /// <param name="args">鼠标点击事件</param>
        /// <param name="myViewport">三维容器</param>
        public void HitTest(MouseButtonEventArgs args, Viewport3D myViewport)
        {
            Point mouseposition = args.GetPosition(myViewport);
            Point3D testpoint3D = new Point3D(mouseposition.X, mouseposition.Y, 0);
            Vector3D testdirection = new Vector3D(mouseposition.X, mouseposition.Y, 10);
            PointHitTestParameters pointparams = new PointHitTestParameters(mouseposition);
            RayHitTestParameters rayparams = new RayHitTestParameters(testpoint3D, testdirection);
            VisualTreeHelper.HitTest(myViewport, null, HTResult, pointparams);
        }
        public HitTestResultBehavior HTResult(HitTestResult rawresult)
        {
            RayHitTestResult rayResult = rawresult as RayHitTestResult;
            if (rayResult != null)
            {
                RayMeshGeometry3DHitTestResult rayMeshResult = rayResult as RayMeshGeometry3DHitTestResult;
                if (rayMeshResult != null)
                {
                    GeometryModel3D hitgeo = rayMeshResult.ModelHit as GeometryModel3D;
                }
            }
            return HitTestResultBehavior.Continue;
        }
        #endregion

        #region 标准XML读取写入操作
        public static XmlDocument _xmlDoc;
        public static XmlNode _Node;
        public static String _string;
        public class ItemInfo
        {
            public string ID;
            public string Title;
            public string Thumb;
            public string Source;
            public string Desc;
        }
        /// <summary>
        /// 创建标准XML文件
        /// </summary>
        /// <param name="xmlName"></param>
        public static void CreateXMLFile(string xmlName)
        {
            FileStream myFs = new FileStream(Directory.GetCurrentDirectory() + @"\XML\" + xmlName + ".xml", FileMode.Create);
            StreamWriter sw = new StreamWriter(myFs);
            sw.Write("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            sw.Write("<Root>");
            sw.Write("</Root>");
            sw.Close();
            myFs.Close();
        }
        public static void CreateXMLFile1(string xmlName)
        {
            XmlTextWriter xw = new XmlTextWriter(Directory.GetCurrentDirectory() + @"\XML\" + xmlName + ".xml", Encoding.UTF8);
            xw.Formatting = Formatting.Indented;
            xw.WriteStartDocument();
            xw.WriteStartElement("Root");
            xw.Flush();
            xw.Close();
        }
        /// <summary>
        /// 对指定XML添加一个结点
        /// </summary>
        /// <param name="xmlPath"></param>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="thumb"></param>
        /// <param name="source"></param>
        /// <param name="desc"></param>
        public static void WriteXml(string xmlPath, string id, string title, string thumb, string source, string desc)
        {
            _xmlDoc = new XmlDocument();
            _string = xmlPath;
            _xmlDoc.Load(_string);
            _Node = _xmlDoc.LastChild;
            XmlElement xmlElementNew = _xmlDoc.CreateElement("ItemNode");
            xmlElementNew.SetAttribute("ID", id);
            XmlElement xmlChild0 = _xmlDoc.CreateElement("Title");
            XmlElement xmlChild1 = _xmlDoc.CreateElement("Thumb");
            XmlElement xmlChild2 = _xmlDoc.CreateElement("Source");
            XmlElement xmlChild3 = _xmlDoc.CreateElement("Desc");
            xmlChild0.InnerText = title;
            xmlChild1.InnerText = thumb;
            xmlChild2.InnerText = source;
            xmlChild3.InnerText = desc;
            xmlElementNew.AppendChild(xmlChild0);
            xmlElementNew.AppendChild(xmlChild1);
            xmlElementNew.AppendChild(xmlChild2);
            xmlElementNew.AppendChild(xmlChild3);
            _Node.AppendChild(xmlElementNew);
            _xmlDoc.Save(_string);
        }
        /// <summary>
        /// xml结点存放列表
        /// </summary>
        public static List<ItemInfo> ListItemInfo = new List<ItemInfo>();
        /// <summary>
        /// 读取XML，存放所有结点列表
        /// </summary>
        /// <param name="xmlPath"></param>
        public static void ReadXml(string xmlPath)
        {
            ListItemInfo.Clear();
            _xmlDoc = new XmlDocument();
            _string = xmlPath;
            _xmlDoc.Load(_string);
            _Node = _xmlDoc.LastChild;
            ItemInfo _itemInfo;
            for (int i = 0; i < _Node.ChildNodes.Count; i++)
            {
                _itemInfo = new ItemInfo();
                _itemInfo.ID = (_Node.ChildNodes[i] as XmlElement).GetAttribute("ID");

                _itemInfo.Title = GetNodeContent(_Node.ChildNodes[i], "Title");
                _itemInfo.Thumb = GetNodeContent(_Node.ChildNodes[i], "Thumb");
                _itemInfo.Source = GetNodeContent(_Node.ChildNodes[i], "Source");
                _itemInfo.Desc = GetNodeContent(_Node.ChildNodes[i], "Desc");
                ListItemInfo.Add(_itemInfo);
            }
        }
        private static string GetNodeContent(XmlNode xmlNode, string name)
        {
            string content = "";
            for (int i = 0; i < xmlNode.ChildNodes.Count; i++)
            {
                if (xmlNode.ChildNodes[i].Name == name)
                {
                    content = xmlNode.ChildNodes[i].InnerText;
                    break;
                }
            }
            return content;
        }
        #endregion

        #region 创建快捷方式

        ///// <summary>
        ///// 给指定的应用程序创建快捷方式
        ///// </summary>
        ///// <param name="LnkPath">快捷方式要放置的完全路径如(@"C:\Users\Public\Desktop\魔兽争霸.lnk")</param>
        ///// <param name="targetPath">目标应用程序的绝对路径(@"D:\游戏\Warcraft III\Frozen Throne.exe")</param>
        //public static void CreateShortCut(string LnkPath, string targetPath)
        //{
        //    WshShell shell = new WshShell();
        //    IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(LnkPath);
        //    shortcut.TargetPath = targetPath;
        //    shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(targetPath);
        //    shortcut.WindowStyle = 1;
        //    shortcut.Description = "";
        //    shortcut.Save();
        //}
        #endregion

        #region 文件操作

        /// <summary>
        /// 删除本地某个文件
        /// </summary>
        /// <param name="filePath">要删除的文件的绝对路径</param>
        public static void DeleteLocalFile(string filePath)
        {
            FileSystem.DeleteFile(filePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);

            //System.IO.File.Delete(filePath);
        }
        /// <summary>
        /// 清空某个文件目录中的资源
        /// </summary>
        /// <param name="folderPath">文件目录绝对路径</param>
        public static void ClearFolder(string folderPath)
        {
            string[] filePath = Directory.GetFiles(folderPath);
            for (int i = 0; i < filePath.Length; i++)
            {
                //System.IO.File.Delete(filePath[i]);
                FileSystem.DeleteFile(filePath[i], UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);

            }
        }

        /// <summary>
        /// 删除本地文件夹
        /// </summary>
        /// <param name="foldelPath">要删除的文件夹的绝对路径</param>
        public static void DeleteLocalFolder(string foldelPath)
        {
            string[] filePath = Directory.GetFiles(foldelPath);
            for (int i = 0; i < filePath.Length; i++)
            {
                //System.IO.File.Delete(filePath[i]);
                FileSystem.DeleteFile(filePath[i], UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);

            }
            Directory.Delete(foldelPath);//清空文件之后删除文件夹
        }
        /// <summary>
        /// 检测是否有相同文件，有相同文件则导入失败
        /// </summary>
        /// <param name="arrayImport">要复制的文件的数组</param>
        /// <param name="destFilePath">目标文件夹路径</param>
        public static bool IsHasSameNameFile(string[] arrayImport, string destFilePath)
        {
            bool isHasSameName = false;
            string[] filePath = Directory.GetFiles(destFilePath);
            for (int i = 0; i < arrayImport.Length; i++)
            {
                for (int j = 0; j < filePath.Length; j++)
                {
                    if (System.IO.Path.GetFileName(arrayImport[i]) == System.IO.Path.GetFileName(filePath[j]))
                    {
                        isHasSameName = true;
                        break;
                    }
                }
            }
            return isHasSameName;
        }
        /// <summary>
        /// 获取目录下文件夹取的名字
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static int GetFolderName(string dir)
        {
            string[] filePath = Directory.GetDirectories(dir);
            List<int> NumList = new List<int>();
            foreach (var item in filePath)
            {
                string xxx = item.Replace(dir, "");

                NumList.Add(Convert.ToInt32(xxx));
            }
            if (NumList.Count == 0)
            {
                return 0;
            }
            else
            {
                return NumList.Max() + 1;
            }
        }
        /// <summary>
        /// 递归删除文件夹和文件夹中所有子文件
        /// </summary>
        /// <param name="dir">要删除的文件绝对路径</param>
        public static void DeleteFolder(string dir)
        {
            if (Directory.Exists(dir))
            {
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    if (File.Exists(d))
                    {
                        //File.Delete(d);
                        FileSystem.DeleteFile(d, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);

                    }
                    else
                    {
                        DeleteFolder(d);
                    }
                }
                Directory.Delete(dir);
            }
            else
            {
                //MessageBox.Show(dir + "该文件夹不存在");
            }
        }
        /// <summary>
        /// 文件重命名
        /// </summary>
        /// <param name="sourceFile">源文件路径</param>
        /// <param name="destFile">重命名之后的文件新路径</param>
        public static void ReNameFileOrFolder(string sourceFile, string destFile)
        {
            Directory.Move(sourceFile, destFile);
        }
        /// <summary>
        /// 复制文件时，遇到文件名重复自动生成副本名称
        /// </summary>
        public static int Num = 0;//进入递归的次数,每次调用递归需要首先赋值为0；
        public static string OriginName;//要复制的源文件的原始名字
        /// <param name="DestFile">复制到该文件夹下</param>
        /// <param name="sourceFileName">源文件名称</param>
        /// <returns>文件新的副本名称，带初始后缀名</returns>
        public static string GetNewFileName(string DestFile, string sourceFileName)
        {
            Num++;
            if (Num == 1)
            {
                OriginName = sourceFileName;
            }
            string[] fileArray = Directory.GetFiles(DestFile);
            string a = DestFile + sourceFileName;
            if (System.IO.File.Exists(a))
            {
                if (Num == 1)
                {
                    sourceFileName = System.IO.Path.GetFileNameWithoutExtension(OriginName) + " - 副本" + System.IO.Path.GetExtension(sourceFileName);
                }
                else
                {
                    sourceFileName = System.IO.Path.GetFileNameWithoutExtension(OriginName) + " - 副本 (" + Num.ToString() + ")" + System.IO.Path.GetExtension(sourceFileName);
                }
                return GetNewFileName(DestFile, sourceFileName);
            }
            else
            {
                Num = 0;
                return sourceFileName;
            }
        }
        #endregion

        #region 递归遍历文件夹下左右子文件
        /// <summary>
        /// 遍历文件夹下所有文件
        /// </summary>
        /// <param name="DirectoryPath">文件根目录路径</param>
        /// <returns>所有文件列表</returns>
        public static List<string> BrowserFilesInFolder(string DirectoryPath)
        {
            ListFile.Clear();
            GetFolderFiles(DirectoryPath);
            return ListFile;
        }
        private static List<string> ListFile = new List<string>();
        private static void GetFolderFiles(string filePath)
        {
            string[] files = Directory.GetFiles(filePath);
            foreach (var item in files)
            {
                ListFile.Add(item);
            }
            string[] dirs = Directory.GetDirectories(filePath);
            foreach (var item in dirs)
            {

                GetFolderFiles(item);
            }
            if (dirs.Length == 0)
            {
                return;
            }
        }
        #endregion

        #region 元素置于顶端
        /// <summary>
        /// 元素置于顶端
        /// </summary>
        /// <param name="uiElement"></param>
        public static void BringUIElementTop(FrameworkElement uiElement)
        {
            var children =
            (from FrameworkElement child in (uiElement.Parent as Panel).Children
             where child != uiElement
             orderby Panel.GetZIndex(child)
             select child).ToArray();
            for (int i = 0; i < children.Length; i++)
            {
                Panel.SetZIndex(children[i], i);
            }
            Panel.SetZIndex(uiElement, children.Length);
        }
        #endregion

        #region 视频截图
        /// <summary>
        /// 视频截取第一帧
        /// </summary>
        /// <param name="videopath">要截屏的视频路径</param>
        /// <param name="savepath">要把截屏保存到得路径</param>
        /// <param name="rect">截屏所得图片框大小</param>
        public static void GetVideoThumb(string videopath, string savepath, Rect rect)
        {
            //打开视频
            MediaPlayer _player = new MediaPlayer();
            _player.Volume = 0;
            _player.Open(new Uri(videopath));
            _player.Play();

            //截取视频第一帧
            Thread.Sleep(1300);
            RenderTargetBitmap target = new RenderTargetBitmap((int)rect.Width, (int)rect.Height, 1 / 100, 1 / 100, PixelFormats.Pbgra32);
            DrawingVisual visual = new DrawingVisual();
            DrawingContext context = visual.RenderOpen();
            context.DrawVideo(_player, new Rect(0, 0, (int)rect.Width, (int)rect.Height));
            context.Close();

            target.Render(visual);

            //移除视频
            _player.Stop();
            _player.Position = TimeSpan.FromSeconds(0);
            _player.Close();
            _player = null;

            //保存第一帧
            BitmapEncoder encoder = new TiffBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(target));

            FileStream fs = new FileStream(savepath, FileMode.Create);
            encoder.Save(fs);
            fs.Close();
        }
        /// <summary>
        /// 判断资源路径是否视频
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool IsVideo(string url)
        {
            bool isVideo = false;
            if (url.EndsWith(".avi") || url.EndsWith(".mp4") || url.EndsWith(".wmv") || url.EndsWith(".mov") || url.EndsWith(".flv") || url.EndsWith(".3gp"))
            {
                isVideo = true;
            }
            return isVideo;
        }
        #endregion

        #region 判断是否是视频
        public static bool isVideo(string path)
        {
            bool _isVideo = false;
            if (path.EndsWith(".wmv") || path.EndsWith(".avi") || path.EndsWith(".mp4") || path.EndsWith(".3gp") || path.EndsWith("mov"))
            {
                _isVideo = true;
            }
            else
            {
                _isVideo = false;
            }
            return _isVideo;
        }
        #endregion

        #region  处理图片缩略图

        /// <summary>
        /// 获取等比例缩放图片
        /// </summary>
        /// <param name="imgPath">待缩放图片路径</param>
        /// <param name="format">缩放图片保存的格式</param>
        /// <param name="scaling">要保持的宽度或高度</param>
        /// <param name="keepWidthOrHeight">
        /// 如果为true则保持宽度为scaling，否则保持高度为scaling</param>
        /// <returns>System.Drawing.Bitmap 类型，不受多线程UI限制</returns>
        public static System.Drawing.Bitmap GetThumbnail(
            string imgPath,
            System.Drawing.Imaging.ImageFormat format,
            int scaling,
            bool keepWidthOrHeight)
        {
            try
            {
                System.Drawing.Bitmap source;
                using (System.Drawing.Bitmap myBitmap = new System.Drawing.Bitmap(imgPath))
                {
                    int width = 0;
                    int height = 0;
                    int tw = myBitmap.Width;//图像的实际宽度
                    int th = myBitmap.Height;//图像的实际高度
                    if (keepWidthOrHeight)//保持宽度
                    {
                        #region 自动保持宽度
                        if (scaling >= tw)
                        {
                            width = tw;
                            height = th;
                        }
                        else
                        {
                            double ti = Convert.ToDouble(tw) / Convert.ToDouble(scaling);
                            if (ti == 0d)
                            {
                                width = tw;
                                height = th;
                            }
                            else
                            {
                                width = scaling;
                                height = Convert.ToInt32(Convert.ToDouble(th) / ti);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region 自动保持高度
                        if (scaling >= th)
                        {
                            width = tw;
                            height = th;
                        }
                        else
                        {
                            double ti = Convert.ToDouble(th) / Convert.ToDouble(scaling);
                            if (ti == 0d)
                            {
                                width = tw;
                                height = th;
                            }
                            else
                            {
                                width = Convert.ToInt32(Convert.ToDouble(tw) / ti);
                                height = scaling;
                            }
                        }
                        #endregion
                    }
                    using (System.Drawing.Image myThumbnail = myBitmap.GetThumbnailImage(
                        width, height, () => { return false; }, IntPtr.Zero))
                    {
                        MemoryStream ms = new MemoryStream();
                        myThumbnail.Save(ms, format);
                        source = new System.Drawing.Bitmap(ms);
                        ms.Close();
                    }
                }
                return source;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 控件截屏

        /// <summary>
        /// 控件截屏
        /// </summary>
        /// <param name="ele">要截屏元素</param>
        /// <param name="parent">截屏父元素</param>
        /// <param name="offsetX">截屏X方向偏移量</param>
        /// <param name="offsetY">截屏Y方向偏移量</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="isVisual">是否是Visual</param>
        public static void ControlClipScreen(FrameworkElement ele, Visual parent, double offsetX, double offsetY, string savePath, bool isVisual)
        {
            if (isVisual) //WPF类型控件
            {
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)ele.ActualWidth, (int)ele.ActualHeight, 96d, 96d, PixelFormats.Default);
                rtb.Render(ele);
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));
                FileStream fs = System.IO.File.Open(savePath, FileMode.Create);
                encoder.Save(fs);
                fs.Close();
            }
            else //Winform控件
            {
                Point childPoint = ele.TransformToVisual(parent).Transform(new Point(0, 0));
                System.Drawing.Bitmap myImage = new System.Drawing.Bitmap((int)(ele.ActualWidth + offsetX), (int)(ele.ActualHeight + offsetY));
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(myImage);
                g.CopyFromScreen(new System.Drawing.Point((int)childPoint.X, (int)childPoint.Y),
                    new System.Drawing.Point(0, 0),
                    new System.Drawing.Size((int)(ele.ActualWidth - offsetX), (int)(ele.ActualHeight - offsetY)));
                IntPtr dc1 = g.GetHdc();
                g.ReleaseHdc(dc1);
                g.Dispose();
                myImage.Save(savePath);
            }
        }
        /// <summary>
        /// WPF控件保存为图片
        /// </summary>
        /// <param name="ui">需要保存为图片的控件</param>
        /// <param name="fileName">图片存放路径</param>
        public static void SaveToImage(FrameworkElement ui, string fileName)
        {
            System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create);
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)ui.Width, (int)ui.Height, 96d, 96d, PixelFormats.Pbgra32);
            bmp.Render(ui);
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            encoder.Save(fs);
            fs.Close();
        }
        /// <summary>
        /// 控件保存为内存图片流(Bitmap)
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public static Stream GetImageFromControl(FrameworkElement control)
        {
            MemoryStream ms = null;
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                VisualBrush brush = new VisualBrush(control) { Stretch = Stretch.None };
                context.DrawRectangle(brush, null, new Rect(0, 0, control.Width, control.Height));
                context.Close();
            }
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)control.Width, (int)control.Height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);
            PngBitmapEncoder encode = new PngBitmapEncoder();
            encode.Frames.Add(BitmapFrame.Create(bitmap));
            ms = new MemoryStream();
            encode.Save(ms);
            return ms;
        }

        public static Stream BitmapStreamFromControl(FrameworkElement control)
        {
            MemoryStream ms = null;
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)control.Width, (int)control.Height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(control);
            JpegBitmapEncoder encode = new JpegBitmapEncoder();
            encode.Frames.Add(BitmapFrame.Create(bitmap));
            ms = new MemoryStream();
            encode.Save(ms);
            return ms;
        }
        #endregion

        #region 全屏幕截屏
        /// <summary>
        /// 屏幕截屏：ScreenSnapShot(new Rect(0, 0, 1440, 900), Directory.GetCurrentDirectory() + "\\test.png");
        /// </summary>
        /// <param name="rect">截屏矩形</param>
        /// <param name="savePath">图片保存路径</param>
        /// <returns>不可用则将代码复制到调用的文件下</returns>
        public static System.Drawing.Bitmap ScreenSnapShot(Rect rect, String savePath)
        {
            var bitmap = new System.Drawing.Bitmap((int)rect.Width, (int)rect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen((int)rect.Left, (int)rect.Top, 0, 0, new System.Drawing.Size((int)rect.Width, (int)rect.Height), System.Drawing.CopyPixelOperation.SourceCopy);
                g.Dispose();
            }
            bitmap.Save(savePath);
            return bitmap;
        }
        #endregion

        #region 鼠标拖拽
        /// <summary>
        /// 鼠标交互
        /// </summary>
        /// <param name="frameworkElement"></param>
        public static Point DragMove(UIElement frameworkElement)
        {
            bool isDown = false;
            Point start = new Point(0, 0);
            Point move = new Point(0, 0);
            Point returnPoint = new Point(0, 0);
            TransformGroup transformGroup = new TransformGroup();
            TranslateTransform _translate = new TranslateTransform();
            ScaleTransform _scale = new ScaleTransform(1, 1);
            transformGroup.Children.Add(_translate);
            transformGroup.Children.Add(_scale);
            frameworkElement.RenderTransform = transformGroup;
            frameworkElement.MouseWheel += (s0, e0) =>
            {
                Point _point = e0.GetPosition((UIElement)s0);
                _scale.CenterX = _point.X;
                _scale.CenterY = _point.Y;
                if (e0.Delta > 0)
                {
                    if (!(_scale.ScaleX * 1.05 > 2))
                    {
                        _scale.ScaleX *= 1.05;
                        _scale.ScaleY *= 1.05;
                    }
                }
                else if (e0.Delta < 0)
                {
                    if (!(_scale.ScaleX * 0.95 < 0.5))
                    {
                        _scale.ScaleX *= 0.95;
                        _scale.ScaleY *= 0.95;
                    }
                }
            };

            frameworkElement.MouseLeftButtonDown += (s, e) =>
            {
                if (!isDown)
                {
                    isDown = true;
                    start = e.GetPosition((UIElement)s);
                    Mouse.Capture((UIElement)s);
                }
            };
            frameworkElement.MouseLeftButtonUp += delegate
            {
                if (isDown)
                {
                    isDown = false;
                    Mouse.Capture(null);
                }
            };
            frameworkElement.MouseMove += (s1, e1) =>
            {
                if (isDown)
                {
                    move = e1.GetPosition((UIElement)s1);
                    _translate.X += move.X - start.X;
                    _translate.Y += move.Y - start.Y;
                    returnPoint = new Point(move.X - start.X, move.Y - start.Y);
                    //start = move;
                    move = start;
                }
            };
            return returnPoint;
        }
        #endregion

        #region 鼠标右键拖动
        /// <summary>
        /// 添加右键对单元移动
        /// </summary>
        /// <param name="body">移动单元</param>
        /// <param name="element">移动单元所在的容器</param>
        void AddMoblieBodyMouseAction(FrameworkElement body, FrameworkElement element)
        {
            MouseButtonEventHandler rightButtonDown = null;
            MouseEventHandler mouseMove = null;
            MouseButtonEventHandler rightButtonUp = null;
            Point ptDown = new Point();
            Point ptMove = new Point();
            Panel parentCanvas = (body.Parent as Panel);
            rightButtonDown = (s, e) =>
            {
                body.MouseRightButtonDown -= rightButtonDown;
                body.MouseRightButtonUp += rightButtonUp;
                body.MouseMove += mouseMove;
                ptDown = e.GetPosition(body);
                body.CaptureMouse();
                BringUIElementTop(body);
            };
            mouseMove = (s, e) =>
            {
                ptMove = e.GetPosition(element);
                //显示当前单元在容器中的位置
                Point currentPoint = new Point(ptMove.X - ptDown.X, ptMove.Y - ptDown.Y);
                Canvas.SetLeft(body, currentPoint.X);
                Canvas.SetTop(body, currentPoint.Y);
                //Point releativePoint = parentCanvas.TranslatePoint(currentPoint, BackgroundCanvasA);
                //debugText.Text = string.Format("({0},{1})", Math.Round(releativePoint.X), Math.Round(releativePoint.Y));
            };
            rightButtonUp = (s, e) =>
            {
                body.MouseRightButtonDown += rightButtonDown;
                body.MouseRightButtonUp -= rightButtonUp;
                body.MouseMove -= mouseMove;
                body.ReleaseMouseCapture();
            };
            body.MouseRightButtonDown += rightButtonDown;
        }
        #endregion

        #region 排序
        /// <summary>
        /// 降序排列
        /// </summary>
        /// <param name="list"></param>
        public static List<double> BubbleSortByAscending(List<double> list)
        {
            int n = list.Count;
            double temp = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = n - 1; j - i > 0; j--)
                {
                    if (list[i] <= list[j])
                    {
                        temp = list[i];
                        list[i] = list[j];
                        list[j] = temp;
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// 升序排列
        /// </summary>
        /// <param name="list"></param>
        public static List<double> BubbleSortByDescending(List<double> list)
        {
            int n = list.Count;
            double temp = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = n - 1; j - i > 0; j--)
                {
                    if (list[i] >= list[j])
                    {
                        temp = list[i];
                        list[i] = list[j];
                        list[j] = temp;
                    }
                }
            }
            return list;
        }
        #endregion

        #region 获取图片指定像素颜色值

        /// <summary>
        /// 获取某点鼠标点击颜色值
        /// </summary>
        /// <param name="bitmapsource">当前点击图片</param>
        /// <param name="x">鼠标点击坐标</param>
        /// <param name="y">鼠标点击坐标</param>
        /// <returns>是否有效点击</returns>
        public static bool IsEffectiveHitOnPngImage(BitmapSource bitmapsource, double x, double y)
        {
            bool isEffective = false;
            CroppedBitmap crop = new CroppedBitmap(bitmapsource as BitmapSource, new Int32Rect((int)x, (int)y, 1, 1));
            byte[] pixels = new byte[4];
            try
            {
                crop.CopyPixels(pixels, 4, 0);
                crop = null;
            }
            catch (Exception e)
            {
                //MessageBox.Show(ee.ToString());
            }
            Color tempColor = Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
            if (tempColor.ToString() == "#00000000")
            {
                isEffective = false;//点击到非有效部分
            }
            else
            {
                isEffective = true;//点击到有效部分
            }
            return isEffective;
        }
        #endregion

        #region 是否能连接到服务器
        /// <summary>
        /// </summary>
        /// <param name="url">服务器地址</param>
        /// <returns>连接成功布尔值</returns>
        public static bool IsAccessServer(string url)
        {
            Ping p = new Ping();
            PingReply pr = p.Send(url);

            if (pr.Status == IPStatus.Success)
            {
                Console.WriteLine("网络连接成功");
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 应用总在最前

        [DllImport("user32.dll ", EntryPoint = "ShowWindow")]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        public static void SetTopMost(string processName)
        {
            foreach (Process p in Process.GetProcesses())
            {
                if (p.ProcessName.ToString().ToLower() == processName)
                {
                    ShowWindow(p.MainWindowHandle, 1);
                    break;
                }
            }
        }

        #endregion

        #region 常用正则表达式

        public static bool IsNumber(String strNumber)
        {
            Regex floatNumberPattern = new Regex(@"^([1-9][0-9]*(.[0-9]*[1-9])?|0\.([0-9]*[1-9]))$");
            return floatNumberPattern.IsMatch(strNumber);
        }
        /// <summary>
        /// 验证Email格式
        /// </summary>
        /// <param name="str_Email"></param>
        /// <returns></returns>
        public static bool IsEmail(string str_Email)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str_Email, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9] {1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\)?]$");
        }
        /// <summary>
        /// 验证IP地址格式
        /// </summary>
        /// <param name="IP"></param>
        /// <returns></returns>
        public static bool IPCheck(string IP)
        {
            string num = "(25[0-5]|2[0-4]\\d|[0-1]\\d{2}|[1-9]?\\d)";
            return Regex.IsMatch(IP, ("^" + num + "\\." + num + "\\." + num + "\\." + num + "$"));
        }
        /// <summary>
        /// 验证URl网址格式
        /// </summary>
        /// <param name="str_url"></param>
        /// <returns></returns>
        public static bool IsUrl(string str_url)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str_url, @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
        }
        /// <summary>
        /// 判断是否含有小数点
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNumberOrLetter(string str)
        {
            Regex reg = new Regex(@"[A-Za-z0-9\u4e00-\u9fa5]");
            return reg.IsMatch(str);
        }
        #endregion

        #region 坐标转换
        /// <summary>
        /// visual0上任意坐标换算到visual1上
        /// </summary>
        /// <param name="visual0">visual0</param>
        /// <param name="visual1">visual1</param>
        /// <param name="visual0Point">visual0上某点坐标</param>
        /// <returns>返回visual0上指定点相对Visual1上坐标</returns>
        public static Point Visual0TranstromToVisual1(UIElement visual0, UIElement visual1, Point visual0Point)
        {
            Point tempPoint = visual0.TransformToVisual(visual1).Transform(visual0Point);
            return tempPoint;
        }
        /// <summary>
        /// visual0上左上角坐标换算到visual1上，visual0上坐标缺省
        /// </summary>
        /// <param name="visual0">visual0</param>
        /// <param name="visual1">visual1</param>
        /// <returns>返回visual0相对在visual1上的坐标</returns>
        public static Point Visual0TranstromToVisual1(UIElement visual0, UIElement visual1)
        {
            Point tempPoint = visual0.TransformToVisual(visual1).Transform(new Point(0, 0));
            return tempPoint;
        }
        #endregion

        #region 动画延迟
        public static void SetBeginTime(Timeline anim, int charIndex)
        {
            double totalMs = anim.Duration.TimeSpan.TotalMilliseconds;
            double offset = totalMs / 10;
            double resolvedOffset = offset * charIndex;
            anim.BeginTime = TimeSpan.FromMilliseconds(resolvedOffset);
        }
        #endregion

        #region 进程

        #region 关闭指定进程

        public static void CloseProcess(string currentProcess)
        {
            Process[] processArray = Process.GetProcesses();

            foreach (Process item in processArray)
            {
                if (item.ProcessName == currentProcess)
                {
                    item.Kill();
                }
            }
        }
        #endregion

        #region 检测进程是否存在
        public static bool CheckProcessRun(string currentProcess)
        {
            bool isExist = false;
            Process[] processArray = Process.GetProcesses();
            foreach (Process item in processArray)
            {

                if (item.ProcessName == currentProcess)
                {
                    isExist = true;
                }
            }
            return isExist;
        }
        #endregion

        #region 打开指定进程exe
        public static void OpenProcess(string exePath)
        {
            string previous_Dir = Environment.CurrentDirectory;
            Process process = new Process();
            Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(exePath);
            process.StartInfo.FileName = exePath;
            process.StartInfo.Arguments = System.IO.Path.GetFileNameWithoutExtension(exePath);
            process.Start();
            Environment.CurrentDirectory = previous_Dir;
        }
        #endregion

        #region 获取进程exe信息
        public FileVersionInfo GetProcessInfo(Process ps)
        {
            return ps.MainModule.FileVersionInfo;
        }
        #endregion
        #endregion

        #region Bitmap转化Image
        [DllImport("gdi32.dll")]
        public static extern int DeleteObject(IntPtr o);
        /// <summary>
        /// Bitmap文件转化为Image文件
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static ImageSource GetImageSourceFromBitmap(System.Drawing.Bitmap bitmap)
        {
            IntPtr ip = bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(ip);
            return bitmapSource;

        }
        /// <summary>
        /// 本地文件路径转化为文件流路径
        /// </summary>
        /// <param name="urlSource">图片真实路径</param>
        /// <returns>返回流的图片来源</returns>
        public static ImageSource GetImageSource(string urlSource)
        {
            FileStream fs = new FileStream(urlSource, FileMode.Open, FileAccess.Read);
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(fs);
            fs.Close();
            IntPtr ip = bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(ip);
            return bitmapSource;
        }
        #endregion

        #region 打开资源选择对话框

        public static string[] OpenFileMyDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            //dialog.InitialDirectory = "C:";
            dialog.Filter = "中控资源格式:(*.png, *.jpg,*.wmv,*.avi,*.mp4,*mov,*.3gp)|*.png;*.jpg;*.wmv;*.avi;*.mp4;*.mov;*.3gp";
            dialog.FilterIndex = 1;
            dialog.RestoreDirectory = true;
            dialog.Title = "导入中控资源";
            dialog.FileName = "选择中控资源";
            dialog.Multiselect = true;
            dialog.ShowDialog();
            if (dialog.FileName != "选择中控资源")
            {
                return dialog.FileNames;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region ZoomableCanvas操作
        //Point LastMousePosition;
        //protected override void OnPreviewMouseMove(MouseEventArgs e)
        //{
        //    var position = e.GetPosition(_grid);
        //    if (e.LeftButton == MouseButtonState.Pressed)
        //    {
        //        CaptureMouse();
        //        MyCanvas.Offset -= position - LastMousePosition;
        //        e.Handled = true;
        //    }
        //    else
        //    {
        //        ReleaseMouseCapture();
        //    }
        //    LastMousePosition = position;
        //}
        //protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        //{
        //    var x = Math.Pow(2, e.Delta / 3.0 / Mouse.MouseWheelDeltaForOneLine);
        //    MyCanvas.Scale *= x;
        //    var position = (Vector)e.GetPosition(_grid);
        //    MyCanvas.Offset = (Point)((Vector)(MyCanvas.Offset + position) * x - position);
        //    e.Handled = true;
        //}
        #endregion

        #region pack://application:,,,/Assets/
        public static ImageSource GetReleativeImageSource(string zamlUrl)
        {
            ImageSource source = new BitmapImage(new Uri(@"pack://application:,,," + zamlUrl, UriKind.RelativeOrAbsolute));
            return source;
            //ImageSource source = new CroppedBitmap(new BitmapImage(new Uri(@"pack://application:,,,/Assets/Btn/editMenu.png", UriKind.RelativeOrAbsolute)), new Int32Rect(i * 30, 0, 30, 30));
        }
        #endregion

        #region 获取图片尺寸
        /// <summary>
        /// 获取本地图片的尺寸
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Size GetLocalImageSize(string url)
        {
            BitmapSource bitmap = new BitmapImage(new Uri(url, UriKind.RelativeOrAbsolute));
            return new Size(bitmap.PixelWidth, bitmap.PixelHeight);
        }
        /// <summary>
        /// 获取网络图片的尺寸
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private System.Drawing.Size GetWebImageSize(string url)
        {
            System.Drawing.Size mysize = new System.Drawing.Size();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            byte[] buffer = new byte[1024];
            Stream outStream = new MemoryStream();
            Stream inStream = response.GetResponseStream();
            int bufferLength;
            do
            {
                bufferLength = inStream.Read(buffer, 0, buffer.Length);
                if (bufferLength > 0)
                    outStream.Write(buffer, 0, bufferLength);
            }
            while (bufferLength > 0);
            outStream.Flush();
            outStream.Seek(0, SeekOrigin.Begin);
            inStream.Close();
            System.Drawing.Image image = System.Drawing.Image.FromStream(outStream);
            outStream.Close();
            mysize = image.Size;
            image.Dispose();
            return mysize;
        }
        #endregion

        #region Linq
        private void GetLinqList(List<Point> validPoint)
        {
            var topestPointYCollection =
                                                        from myPoint in validPoint
                                                        orderby myPoint.Y ascending
                                                        select myPoint.Y;

            double minY = topestPointYCollection.Min();
            var topsetPointCollection =
                                                        from myPoint1 in validPoint
                                                        where myPoint1.Y == minY
                                                        orderby myPoint1.X ascending
                                                        select myPoint1;
        }
        #endregion

     

        #region 删除UIelement
        public static void RemoveUIElement(FrameworkElement element)
        {
            if (VisualTreeHelper.GetParent(element) != null)
            {
                (VisualTreeHelper.GetParent(element) as Panel).Children.Remove(element);
            }
        }
        #endregion

        #region 检测config下项目名是否存在
        public static bool IsLastProjectExist(string name)
        {
            string[] xmlPath = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\XML");
            List<string> xmlName = new List<string>();
            foreach (string item in xmlPath)
            {
                xmlName.Add(Path.GetFileNameWithoutExtension(item));
            }
            if (xmlName.Contains(name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 检查URL是否存在
        private bool UrlExistsUsingSockets(string url)
        {
            if (url.StartsWith("http://"))
                url = url.Remove(0, "http://".Length);
            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry(url);
                return true;
            }
            catch (System.Net.Sockets.SocketException se)
            {
                System.Diagnostics.Trace.Write(se.Message);
                return false;
            }
        }
        #endregion

        #region BitmapSource转化Bitmap
        public static System.Drawing.Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            System.Drawing.Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
            }
            return bitmap;
        }
        #endregion

        #region 裁切矩形框选定的图片区域
        /// <summary>
        /// 裁切选择的区域保存为图片
        /// </summary>
        /// <param name="upperLeft">裁切区域左上角坐标</param>
        /// <param name="rightBottom">裁切区域右下角坐标</param>
        /// <param name="visualImageSize">图片当前显示的实际尺寸</param>
        /// <param name="sourceFilePath">图片源路径</param>
        /// <param name="desFilePath">裁切后保存到路径</param>
        /// <param name="maxSize">裁切后图片限制最大长宽分辨率</param>
        public static void CropSelectedRegion(Point upperLeft, Point rightBottom, Size visualImageSize, string sourceFilePath, string desFilePath, int maxSize)
        {
            #region 获取裁切区域
            BitmapSource source = new BitmapImage(new Uri(sourceFilePath, UriKind.RelativeOrAbsolute));
            double originWidth = source.PixelWidth;
            double originHeight = source.PixelHeight;
            int x = (int)(Math.Round(upperLeft.X) * (originWidth / visualImageSize.Width));
            int y = (int)(Math.Round(upperLeft.Y) * (originHeight / visualImageSize.Height));
            int width = (int)(Math.Round(rightBottom.X - upperLeft.X) * (originWidth / visualImageSize.Width));
            int height = (int)(Math.Round(rightBottom.Y - upperLeft.Y) * (originHeight / visualImageSize.Height));
            int BitmapWidth;
            int BitmapHeight;
            CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(x, y, width, height));
            #endregion
            #region 限制裁切图片分辨率
            if (width > maxSize || height > maxSize)
            {
                if (width > height)
                {
                    BitmapWidth = maxSize;
                    BitmapHeight = (int)Math.Round(BitmapWidth * (height / (double)width));
                }
                else if (width < height)
                {
                    BitmapHeight = maxSize;
                    BitmapWidth = (int)Math.Round(BitmapHeight * (width / (double)height));
                }
                else
                {
                    BitmapHeight = maxSize;
                    BitmapWidth = maxSize;
                }
            }
            else
            {
                BitmapHeight = height;
                BitmapWidth = width;
            }
            #endregion
            #region  保存为图片
            MemoryStream ms = new MemoryStream();
            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(croppedBitmap));
            enc.Save(ms);
            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(img, BitmapWidth, BitmapHeight);
            bitmap.Save(desFilePath);
            #endregion
        }
        #endregion

        #region 文件或文件夹批量重命名
        /// <summary>
        /// 文件或文件夹批量重命名
        /// </summary>
        /// <param name="oldPath">当前文件名数组</param>
        /// <param name="newPath">命名后的文件名数组</param>
        public static void ReNameFileQuantity(string[] oldPath, string[] newPath)
        {
            for (int i = 0; i < oldPath.Length; i++)
            {
                string oldName = oldPath[i];
                string newName = newPath[i];
                string st = System.IO.Path.GetExtension(oldName);
                if (oldPath.Contains(newName))
                {
                    int index = 0;
                    for (int k = 0; k < oldPath.Length; k++)
                    {
                        if (oldPath[k].ToLower() == newName.ToLower())
                        {
                            index = k;
                        }
                    }
                    if (index != i)
                    {
                        oldPath[i] = newName;
                        oldPath[index] = oldName;
                        if (st == string.Empty)//文件夹目录重命名
                        {
                            st = System.IO.Path.GetDirectoryName(newName) + "\\#CacheImage_JimmyBright#";
                            Directory.Move(newName, st);
                            Directory.Move(oldName, newName);
                            Directory.Move(st, oldName);
                        }
                        else//文件重命名
                        {
                            st = System.IO.Path.GetDirectoryName(newName) + "\\#CacheImage_JimmyBright#" + System.IO.Path.GetExtension(oldName);
                            File.Move(newName, st);
                            File.Move(oldName, newName);
                            File.Move(st, oldName);
                        }
                    }
                }
                else
                {
                    oldPath[i] = newName;
                    if (st == string.Empty)//文件夹目录重命名
                    {
                        Directory.Move(oldName, newName);
                    }
                    else
                    {
                        File.Move(oldName, newName);
                    }
                }
            }
        }
        #endregion

        #region 文件下载
        public static void DownLoadedFile(string url,string savePath)
        {
            WebClient client = new WebClient();
            client.DownloadFile(url, savePath);
        }
        #endregion
        #region 验证网络链接状态
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int connectionDescription, int reservedValue);
        public static bool IsConnected()
        {
            int I = 0;
            bool state = InternetGetConnectedState(out I, 0);
            return state;
        }
        #endregion

        #region 获取远程文件大小
        
        //Shuttle2DNode myShuttle = myList.Find(delegate(Shuttle2DNode item) { return item.ID == id; });
        /// <summary>
        /// 得到一个地址的文件长度,单位MB
        /// </summary>
        /// <param name="serverURL"></param>
        /// <returns></returns>
        public static double GetWebUrlFileLength(String webUrl)
        {
            HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(webUrl);
            HttpWebResponse httpResponse;

            long contentLength = 0;
            try
            {
                httpRequest.Method = "HEAD";
                httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    httpResponse.Close();
                    return contentLength;
                }
                contentLength = httpResponse.ContentLength;
            }
            catch (Exception e)
            {
                return contentLength;
            }
            httpResponse.Close();
            return Math.Round(contentLength / (1024 * 1024.0), 2);
        }
        #endregion

    }
    #region 常用绑定转换器

    [ValueConversion(typeof(double), typeof(double))]
    public class WidthBindingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return double.Parse(value.ToString()) + 10;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    [ValueConversion(typeof(double), typeof(double))]
    public class PopBgWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return double.Parse(value.ToString()) * 2;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    [ValueConversion(typeof(double), typeof(double))]
    public class HeightBindingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return double.Parse(value.ToString()) - 10;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    [ValueConversion(typeof(double), typeof(string))]
    public class TileSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            string size = "\"" + Math.Round(double.Parse(value.ToString())).ToString() + "\"";
            return "TileSize=" + size;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    [ValueConversion(typeof(ImageSource), typeof(ImageBrush))]
    public class ImageBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CroppedBitmap cb = new CroppedBitmap();
            cb.BeginInit();
            cb.Source = (BitmapSource)value;

            cb.SourceRect = new Int32Rect(int.Parse(parameter.ToString()), 0, 256, 256);
            cb.EndInit();
            return new ImageBrush((ImageSource)cb);
            //return (ImageSource)cb;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
