using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media;
using System.Xml;
using System.Drawing;
using System.ComponentModel;
using SevenZipLib;
using TVMWPFLab.Utils;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Threading;
namespace Metro.Utils
{
    public class ThemeEngine
    {
        public static Dictionary<string, string> ThemeMap = new Dictionary<string, string>(); //主题字典初始化
        public static Dictionary<string, Theme> ThemeDictionary = new Dictionary<string, Theme>();
        public static Theme BindingTheme;
        #region << 构造函数
        public ThemeEngine()
        {
            ThemeMap.Clear();
            BindingTheme = new Theme();
            string[] fileThemePath = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Theme\\");
            for (int i = 0; i < fileThemePath.Length; i++)
            {
                ThemeMap.Add(System.IO.Path.GetFileNameWithoutExtension(fileThemePath[i]), fileThemePath[i]);//主题字典初始化
            }
        }
        public void ReadAllTheme(string psw)
        {
            foreach (var item in ThemeMap)
            {
                using (SevenZipArchive archive = new SevenZipArchive(item.Value, ArchiveFormat.Unkown, psw))
                {
                    XmlTextReader xtr = new XmlTextReader(ExtractToStream(archive, "Theme.xml"));
                    Theme theme = GetThemeValue(archive, xtr, item.Key);
                    ThemeDictionary.Add(item.Key, theme);
                }
            }
        }
        /// <summary>
        /// 读取所有主题文件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="psw"></param>
        public void ReadTheme(string name, string psw)
        {
            using (SevenZipArchive archive = new SevenZipArchive(ThemeMap[name], ArchiveFormat.Unkown, psw))
            {
                XmlTextReader xtr = new XmlTextReader(ExtractToStream(archive, "Theme.xml"));
                GetThemeValue(archive, xtr, name);
            }
        }
        /// <summary>
        /// 获取主题皮肤路径
        /// </summary>
        /// <param name="xtr"></param>
        private Theme GetThemeValue(SevenZipArchive archive, XmlTextReader xtr, string bgName)
        {

            BindingTheme.Name = bgName;
            string content = null;
            while (xtr.Read())
            {
                switch (xtr.NodeType)
                {
                    case XmlNodeType.Element:
                        if (xtr.Name.Equals("Bg"))//背景
                        {
                            xtr.MoveToContent();
                            content = xtr.ReadString();
                            BindingTheme.VBG = ExtractToVideoSoruce(archive, content);
                        }
                        else if (xtr.Name.Equals("Image"))
                        {
                            if (xtr.HasAttributes)
                            {
                                for (int i = 0; i < xtr.AttributeCount; i++)
                                {
                                    xtr.MoveToAttribute(i);
                                    switch (xtr.Value)
                                    {
                                        #region MyRegion
                                        case "popBg":
                                            xtr.MoveToContent();
                                            content = xtr.ReadString();
                                            BindingTheme.PopBg = ExtractToImageSource(archive, content);
                                            break;
                                        case "bm":
                                            xtr.MoveToContent();
                                            content = xtr.ReadString();
                                            BindingTheme.Bm = ExtractToImageSource(archive, content);
                                            break;
                                        default:
                                            break;
                                        #endregion
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            xtr.Close();
            return BindingTheme;
        }

        #endregion

        #region << 私有方法

        /// <summary>
        /// 提取为流格式
        /// </summary>
        /// <param name="source">压缩源</param>
        /// <param name="path">关键字</param>
        /// <returns></returns>
        private Stream ExtractToStream(SevenZipArchive source, string path)
        {
            ArchiveEntry entry = source[path];
            MemoryStream stream = new MemoryStream();
            entry.Extract(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
        private string ExtractToVideoSoruce(SevenZipArchive source, string path, string extension, string moveName)
        {
            ArchiveEntry entry = source[path];
            string cachePath = Directory.GetCurrentDirectory() + "\\Cache";
            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }
            string cachestring = cachePath;
            entry.Extract(cachestring);
            string name = System.IO.Path.Combine(cachestring, path);
            string parserName = System.IO.Path.Combine(cachestring, moveName + extension);
            (new FileInfo(name)).MoveTo(parserName);
            return "\\Cache\\" + moveName + extension;
        }
        /// <summary>
        /// 提取视频格式
        /// </summary>
        /// <param name="source"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private Uri ExtractToVideoSoruce(SevenZipArchive source, string path)
        {
            ArchiveEntry entry = source[path];
            string cachestring = Directory.GetCurrentDirectory() + "\\Cache";
            entry.Extract(cachestring);
            string name = System.IO.Path.Combine(cachestring, path);
            string parserName = System.IO.Path.Combine(cachestring, "cache_" + GetSystemDateString() + Path.GetExtension(path));
            (new FileInfo(name)).MoveTo(parserName);

            return new Uri(parserName);
        }
        public static string GetSystemDateString()
        {
            string year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.Month.ToString();
            string day = DateTime.Now.Day.ToString();
            string time = DateTime.Now.ToLongTimeString();
            string ms = DateTime.Now.Millisecond.ToString();
            return string.Format("{0}-{1}-{2}-{3}-{4}", year, month, day, time, ms).Replace(':', '-');
        }
        /// <summary>
        /// 提取为序列图格式
        /// </summary>
        /// <param name="source">压缩源</param>
        /// <param name="path">关键字</param>
        /// <returns></returns>
        private ImageCollection ExtractToImageCollection(SevenZipArchive source, string path)
        {
            ImageCollection collection = new ImageCollection();
            MemoryStream stream;
            foreach (ArchiveEntry entry in source)
            {
                if (!entry.IsDirectory && entry.FileName.Contains(path))
                {
                    stream = new MemoryStream();
                    entry.Extract(stream);
                    collection.Add(stream);
                }
            }
            return collection;
        }

        /// <summary>
        /// 提取为图片源格式
        /// </summary>
        /// <param name="source">压缩源</param>
        /// <param name="path">关键字</param>
        /// <returns></returns>
        private ImageSource ExtractToImageSource(SevenZipArchive source, string path)
        {
            ArchiveEntry entry = source[path];
            MemoryStream stream = new MemoryStream();
            entry.Extract(stream);
            stream.Seek(0, SeekOrigin.Begin);
            ImageSourceConverter isc = new ImageSourceConverter();
            return (ImageSource)isc.ConvertFrom(stream);
        }
        #endregion
    }
    /// <summary>
    /// 主题类
    /// </summary>
    public class Theme : INotifyPropertyChanged
    {
        public Theme() { }
        /// <summary>
        /// 主题名
        /// </summary>
        public string Name { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #region << 主题元素
        /// <summary>
        /// 视频背景
        /// </summary>
        private Uri m_vbg;
        public Uri VBG
        {
            get
            {
                return m_vbg;
            }
            set
            {
                m_vbg = value;
                NotifyPropertyChanged("VBG");
            }
        }

        private ImageSource m_PopBg;

        public ImageSource PopBg
        {
            get { return m_PopBg; }
            set { m_PopBg = value; NotifyPropertyChanged("PopBg"); }
        }
        private ImageSource m_Bm;

        public ImageSource Bm
        {
            get { return m_Bm; }
            set { m_Bm = value; NotifyPropertyChanged("Bm"); }
        }
        #endregion
    }
}

