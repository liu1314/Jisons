using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVMWPFLab.Utils;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Net;
using System.Media;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ICESetting.Utils
{
    enum SelectedControl
    {
        Volumn,
        WebTime,
        MeetingName,
        CheckUpdate,
        ResetDevice
    }
    public class ResolutionStruct
    {
        public int width;
        public int height;
        public int frequence;
        public int bitNum;
    }
    /// <summary>
    /// 舞台场景
    /// </summary>
    public enum StageEnum
    {
        MainFace,
        PopWinFace,
        LoadRarFace,
        DescFace,
        LawTextFace
    }
    /// <summary>
    /// 资源展示项结构
    /// </summary>
    public class ItemInfo
    {
        public string ID;
        public Size CellSize;
        public bool ShowThumb = true;
        public string Title;
        public string Thumb;
        public string Source;
        public string Desc;
        public string Type;
        public string UID;
    }
    public enum Msg
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        FIRST,
        LAST,
        FORWARD,
        BACKWARD,
        ZOOMIN,
        ZOOMOUT,
        OK,
        CANCEL,
        FUNCTION1,
        FUNCTION2,
        TOUCH,
        ERROR
    }
    enum PopBoxEnum
    {
        Ok,
        Cancel
    }
    public enum TypeEnum
    {
        IMAGE,
        VIDEO,
        文本,
        网页资源,
        流媒体
    }
    class Utility : Global
    {
        #region 变量
        public static List<SelectedControl> ListSelectedControl = new List<SelectedControl>() { SelectedControl.Volumn, SelectedControl.WebTime, SelectedControl.MeetingName, SelectedControl.CheckUpdate, SelectedControl.ResetDevice };
        public static List<PopBoxEnum> ListPopBoxEnum = new List<PopBoxEnum>() { PopBoxEnum.Ok, PopBoxEnum.Cancel };
        public static double PopCellHeightMax = 900;//弹出MetroTile最大高度
        public static bool isManuStopLoad = false;
        public static double LargeScreenWidth = 1920;
        public static double LargeScreenHeight = 1080;
        #endregion

        public static Msg ConvertFromStringToMsg(string msg)
        {
            msg = msg.ToLower();
            switch (msg)
            {
                case "up":
                    return Msg.UP;
                case "down":
                    return Msg.DOWN;
                case "left":
                    return Msg.LEFT;
                case "right":
                    return Msg.RIGHT;
                case "first":
                    return Msg.FIRST;
                case "last":
                    return Msg.LAST;
                case "forward":
                    return Msg.FORWARD;
                case "backward":
                    return Msg.BACKWARD;
                case "zoomin":
                    return Msg.ZOOMIN;
                case "zoomout":
                    return Msg.ZOOMOUT;
                case "ok":
                    return Msg.OK;
                case "cancel":
                    return Msg.CANCEL;
                case "function1":
                    return Msg.FUNCTION1;
                case "function2":
                    return Msg.FUNCTION2;
                case "touch":
                    return Msg.TOUCH;
                default:
                    return Msg.ERROR;
            }
        }
        public static void PlaySound(string name)
        {
            string path = Directory.GetCurrentDirectory() + "\\Resources\\Sounds\\";
            try
            {
                SoundPlayer sp = new SoundPlayer();
                string soundPath = path + name + ".wav";
                if (File.Exists(soundPath))
                {
                    sp.SoundLocation = soundPath;
                    sp.Play();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "Global.cs  -773");
            }
        }
        public static bool isImage(string filePath)
        {
            filePath = filePath.ToLower();
            if (filePath.EndsWith(".jpg") || filePath.EndsWith(".bmp") || filePath.EndsWith(".jpeg") || filePath.EndsWith(".png"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #region 下载
        /// <summary>
        /// 下载队列
        /// </summary>
        public static Queue<string> QueueAddress = new Queue<string>();
        /// <summary>
        /// 本地保存队列
        /// </summary>
        public static Queue<string> QueueSavePath = new Queue<string>();
        public delegate void LoadFileDelegate(string st, double percent);
        public delegate void DownLoadIDDelegate(int number);
        /// <summary>
        ///下载实时进度
        /// </summary>
        public static event LoadFileDelegate DownLoadFileProgress;
        /// <summary>
        /// 下载结束
        /// </summary>
        public static event EventHandler DownLoadFileFinished;
        public static event DownLoadIDDelegate DownLoadNumber;
        public static bool isDownLoading = false;
        static int DownLoadID = 0;
        static WebClient client;

        static void DownLoadFileInBackground(string address, string savePath)
        {
            //if (QueueAddress.Count != 0)
            //{
            isDownLoading = true;
            client = new WebClient();
            Uri uri = new Uri(address);
            LoadUrl = address;
            client.DownloadFileCompleted += client_DownloadFileCompleted;
            client.DownloadProgressChanged += client_DownloadProgressChanged;
            client.DownloadFileAsync(uri, savePath);

            DownLoadID++;
            if (DownLoadNumber != null)
            {
                DownLoadNumber(DownLoadID);
            }
            //}
        }
        /// <summary>
        /// 自动顺序下载数据
        /// </summary>
        public static void DownLoadDataAuto()
        {
            DownLoadID = 0;

            if (QueueAddress.Count != 0 && QueueSavePath.Count != 0)
            {
                DownLoadFileInBackground(QueueAddress.Dequeue(), QueueSavePath.Dequeue());
            }
        }

        /// <summary>
        /// 停止下载
        /// </summary>
        public static void StopDownLoadData()
        {
            QueueAddress.Clear();
            QueueSavePath.Clear();
            if (client != null)
            {
                client.CancelAsync();
            }
        }
        static string LoadUrl = "";
        static void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double cent = e.ProgressPercentage;
            if (DownLoadFileProgress != null)
            {
                DownLoadFileProgress(LoadUrl, cent);
            }
        }
        static void client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {

            if (QueueAddress.Count != 0)
            {
                client = new WebClient();

                string url = QueueAddress.Dequeue();
                Uri uri = new Uri(url);
                LoadUrl = url;
                client.DownloadFileCompleted += client_DownloadFileCompleted;
                client.DownloadProgressChanged += client_DownloadProgressChanged;
                client.DownloadFileAsync(uri, QueueSavePath.Dequeue());
                DownLoadID++;
                if (DownLoadNumber != null)
                {
                    DownLoadNumber(DownLoadID);
                }
            }
            else
            {
                if (DownLoadFileFinished != null)
                {
                    DownLoadFileFinished(null, new EventArgs());
                }
                isDownLoading = false;
            }
        }
        #endregion
        #region 转换器

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
        public static string ToFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return "";
            }
            StringBuilder sb = new StringBuilder(fileName.Length);
            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
            foreach (var item in fileName.Trim().ToCharArray())
            {
                if (invalidFileNameChars.Contains(item))
                {
                    sb.Append('_');
                    continue;
                }
                sb.Append(item);
            }
            return sb.ToString();
        }

    }
}