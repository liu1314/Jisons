using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Windows.Interop;
using ICESetting;
using CoreAudioApi;
using UserMessageBox;
using TVMWPFLab.Control;
using ICESetting.Utils;
using ICE3.WPF.Linker;
using System.Threading;
using System.Diagnostics;
using ICESetting.Control;
using System.Windows.Media.Animation;
using TVM.WPF.Library.Animation;
using SevenZipLib;
using Tvm.WPF;
using System.Net.Sockets;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
namespace ICESetting.Stage
{
    /// <summary>
    /// SettingStage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingStage : UserControl
    {
        #region 变量
        bool isTest = false;
        private MMDevice device;
        IntPtr handle;
        DispatcherTimer dtTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
        public static WarningPlate Warning;
        public static CoolProgressBar CoolBar;
        public static Grid PopGrid;//进度条所在Grid
        List<FrameworkElement> ListObject = new List<FrameworkElement>();
        public static StageEnum CurrentStage = StageEnum.MainFace;
        SelectedBorder mySelectedBorder;
        string version_config;
        public static SettingStage Stage;
        PopPanel popPanel = null;



        string Meetingpath = @"D:\Ivision3.0ForWin\ICEForWin3.01\ICEForWin3.01\BonjourForWin3.01\bonjour_name.dat";


        #endregion

        #region 构造
        public SettingStage()
        {
            InitializeComponent();
            this.Loaded += SettingStage_Loaded;
        }
        void SettingStage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                Loaded -= SettingStage_Loaded;
                //TvmUpdate update = new TvmUpdate("ice", "pc", false, "3.5.0.5");
                string Product1 = Utility.INIFILE.GetValue("MAIN", "Product");
                string Device1 = Utility.INIFILE.GetValue("MAIN", "Device");
                string version1 = Utility.INIFILE.GetValue("MAIN", "version");

                //TimeZoneInfo 
                //string xx = File.ReadAllText(Meetingpath, UnicodeEncoding.GetEncoding("GB2312"));

                //if (!File.Exists(Meetingpath))
                //{
                //    Console.WriteLine(Meetingpath + " 找不到...");
                //    return;
                //}

                //string xx = File.ReadAllText(Meetingpath, UnicodeEncoding.UTF8);
                //meetingName.Text = xx;
                currentVersion.Text = "版本号：" + version1;
                update = new TvmUpdate(Product1, Device1, false, version1);
                Warning = this.FindName("warningPlate") as WarningPlate;
                PopGrid = this.FindName("popGrid") as Grid;
                Stage = this;
                this._scale1.ScaleX = SystemParameters.PrimaryScreenWidth / 1920d;
                this._scale1.ScaleY = SystemParameters.PrimaryScreenHeight / 1080d;
                MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
                device = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
                _volume.ValueChanged += Slider_ValueChanged;
                _volume.Value = (int)(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                device.AudioEndpointVolume.OnVolumeNotification += new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification);

                dtTimer.Tick += dt_Tick;
                dtTimer.Start();

                resolutionUIGrid.Width = resolutionUIGrid.ActualWidth;
                resolutionUIGrid.Height = resolutionUIGrid.ActualHeight;
                handle = (IntPtr)new WindowInteropHelper(App.Current.MainWindow).Handle.ToInt32();
                ListObject.Add(_volume);//0
                ListObject.Add(internetTime);//1

                ListObject.Add(_year);//2
                ListObject.Add(_month);//3
                ListObject.Add(_day);//4
                ListObject.Add(_hour);//5
                ListObject.Add(_minute);//6
                ListObject.Add(_second);//7


                ListObject.Add(meetingName);//8
                ListObject.Add(updateBox);//9
                ListObject.Add(appDesc);//10

                ListObject.Add(myLawText);//11

                ListObject.Add(resolutionUIGrid);//12
                ListObject.Add(timeZone);//13
                InitializeSelectedBorder();//

                DispatcherTimer dtUpdate = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(5) };
                dtUpdate.Tick += delegate
                {
                    dtUpdate.Stop();
                    CheckVersion();
                };
                dtUpdate.Start();
                Utility.DeleteFolder(Directory.GetCurrentDirectory() + "\\ICEPatch\\");
                CreateLog();
                resolutionUI.UpdateResolution += resolutionUI_UpdateResolution;

                GetSystemTimeZone();
            }
            catch { }
        }
        void resolutionUI_UpdateResolution(object sender, EventArgs e)
        {
            this._scale1.ScaleX = SystemParameters.PrimaryScreenWidth / 1920d;
            this._scale1.ScaleY = SystemParameters.PrimaryScreenHeight / 1080d;
        }
        #endregion

        #region 系统时间

        [DllImport("Kernel32.dll")]
        public static extern bool SetSystemTime(ref SystemTime sysTime);
        [DllImport("Kernel32.dll")]
        public static extern bool SetLocalTime(ref SystemTime sysTime);
        [StructLayout(LayoutKind.Sequential)]
        public struct SystemTime
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
            public void FromDateTime(DateTime time)
            {
                wYear = (ushort)time.Year;
                wMonth = (ushort)time.Month;
                wDayOfWeek = (ushort)time.DayOfWeek;
                wDay = (ushort)time.Day;
                wHour = (ushort)time.Hour;
                wMinute = (ushort)time.Minute;
                wSecond = (ushort)time.Second;
                wMilliseconds = (ushort)time.Millisecond;
            }
        }

        #endregion

        #region 音量
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            device.AudioEndpointVolume.MasterVolumeLevelScalar = ((float)_volume.Value / 100.0f);
            _volumeImg.Width = 318 * _volume.Value / 100d;
            Utility.INIFILE.SetValue("MAIN", "SystemVolumn", _volume.Value.ToString());

        }
        void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {

            //Dispatcher.BeginInvoke(DispatcherPriority.Background, new AudioEndpointVolumeDelegate(AudioEndpointVolume), data);


            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                object[] Params = new object[1];
                Params[0] = data;
                Dispatcher.Invoke(new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification), Params);
            }
            else
            {
                //_volume.Value = (int)(data.MasterVolume * 100);
            }

        }
        delegate void AudioEndpointVolumeDelegate(AudioVolumeNotificationData data);
        private void AudioEndpointVolume(AudioVolumeNotificationData data)
        {
            _volume.Value = (int)(data.MasterVolume * 100.0);
        }

        #endregion

        #region 获取网络时间
        public DateTime GetInternetTime()
        {

            var ntpData = new byte[48];
            ntpData[0] = 0x1B; //LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)
            var addresses = IPAddress.Parse("58.215.39.11");
            var ipEndPoint = new IPEndPoint(addresses, 123);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(ipEndPoint);
            socket.Send(ntpData);
            socket.Receive(ntpData);
            socket.Close();
            ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
            ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];
            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);
            return networkDateTime;


        }
        //public static DateTime GetInternetTime()
        //{
        //    const string ntpServer = "pool.ntp.org";
        //    var ntpData = new byte[48];
        //    ntpData[0] = 0x1B; //LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)
        //    var addresses = Dns.GetHostEntry(ntpServer).AddressList;
        //    var ipEndPoint = new IPEndPoint(addresses[1], 123);
        //    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //    socket.Connect(ipEndPoint);
        //    socket.Send(ntpData);
        //    socket.Receive(ntpData);
        //    socket.Close();

        //    ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
        //    ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];

        //    var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
        //    var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);

        //    return networkDateTime;
        //}
        /// <summary>  
        /// 获取标准北京时间，读取http://www.beijing-time.org/time.asp  
        /// </summary>  
        /// <returns>返回网络时间</returns>  
        public DateTime GetBeijingTime()
        {
            DateTime dt;
            WebRequest wrt = null;
            WebResponse wrp = null;
            try
            {
                wrt = WebRequest.Create("http://www.beijing-time.org/time.asp");
                //wrt = WebRequest.Create("http://time.windows.com");
                wrp = wrt.GetResponse();

                string html = string.Empty;
                using (Stream stream = wrp.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
                    {
                        html = sr.ReadToEnd();
                    }
                }

                string[] tempArray = html.Split(';');
                for (int i = 0; i < tempArray.Length; i++)
                {
                    tempArray[i] = tempArray[i].Replace("\r\n", "");
                }

                string year = tempArray[1].Split('=')[1];
                string month = tempArray[2].Split('=')[1];
                string day = tempArray[3].Split('=')[1];
                string hour = tempArray[5].Split('=')[1];
                string minite = tempArray[6].Split('=')[1];
                string second = tempArray[7].Split('=')[1];

                dt = DateTime.Parse(year + "-" + month + "-" + day + " " + hour + ":" + minite + ":" + second);
            }
            catch (WebException)
            {
                return DateTime.Parse("2011-1-1");
            }
            catch (Exception)
            {
                return DateTime.Parse("2011-1-1");
            }
            finally
            {
                if (wrp != null)
                    wrp.Close();
                if (wrt != null)
                    wrt.Abort();
            }
            return dt;
        }
        #region 输入文字
        internal void DealInputText(string msg)
        {
            switch (SelectedIndex)
            {
                case 2:
                    dtTimer.Stop();
                    _year._time.Text = msg + "年";
                    UpdateTime(jimmy.year, msg);
                    break;
                case 3:
                    _month._time.Text = msg + "月";
                    UpdateTime(jimmy.month, msg);

                    break;
                case 4:
                    _day._time.Text = msg + "日";
                    UpdateTime(jimmy.day, msg);

                    break;
                case 5:
                    _hour._time.Text = msg + "时";
                    UpdateTime(jimmy.hour, msg);
                    break;
                case 6:
                    _minute._time.Text = msg + "分";
                    UpdateTime(jimmy.minute, msg);
                    break;
                case 7:
                    _second._time.Text = msg + "秒";
                    UpdateTime(jimmy.second, msg);
                    break;
                case 8:
                    if (msg.Contains(" ") || msg.Contains("_"))
                    {
                        Warning.WarningText = "会议名称不能包含空格或下划线";
                    }
                    else
                    {
                        meetingName.Text = msg;

                        //File.WriteAllText(Meetingpath, msg, Encoding.UTF8);这样写不是utf8格式

                        StreamWriter sw = new StreamWriter(Meetingpath);
                        sw.Write(msg);
                        sw.Close();




                        string exeName = "r BonjourForWin3.01.exe";
                        byte[] exebtye = Encoding.UTF8.GetBytes(exeName);
                        UdpClient udp = new UdpClient("127.0.0.1", 8899);
                        udp.Send(exebtye, exebtye.Length);
                        WriteLog("发送中控重启命令，中转会议号");
                        udp.Close();
                    }
                    break;
                default:
                    break;
            }
        }

        #endregion

        enum jimmy
        {
            year, month, day, hour, minute, second
        }
        private void RealButton_Click(object sender, EventArgs e)
        {
            PressOnTimerBtn();
        }
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int connectionDescription, int reservedValue);
        public static bool IsConnected()
        {
            int I = 0;
            bool state = InternetGetConnectedState(out I, 0);
            return state;
        }
        void PressOnTimerBtn()
        {
            try
            {
                //Ping ping = new Ping();
                //PingReply replay = ping.Send("202.108.22.5");
                if (IsConnected())
                //if (replay.Status == IPStatus.Success)
                {
                    dtTimer.Stop();
                    internetTime.RemoteClick();
                    WriteLog("1111111111111111111111111111111111111111111111111");
                    //DateTime dt = GetBeijingTime();
                    DateTime dtUTC = GetInternetTime();//0时区时间
                    ///0时区标准时间
                    //DateTime dtUTC = TimeZoneInfo.ConvertTimeToUtc(dt, TimeZoneInfo.FindSystemTimeZoneById(ListTimeMap[defaultTimeSpane]));
                    DateTime dt = TimeZoneInfo.ConvertTimeFromUtc(dtUTC, TimeZoneInfo.FindSystemTimeZoneById(ListTimeMap.Values.ElementAt(currentTimeZoneIndex)));
                    WriteLog("2222222222222222222222222222222222222222222");

                    _year._time.Text = dt.Year.ToString() + "年";
                    _month._time.Text = dt.Month.ToString() + "月";
                    _day._time.Text = dt.Day.ToString() + "日";
                    _hour._time.Text = dt.Hour.ToString() + "时";
                    _minute._time.Text = dt.Minute.ToString() + "分";
                    _second._time.Text = dt.Second.ToString() + "秒";
                    SystemTime st = new SystemTime();
                    st.FromDateTime(dt);
                    WriteLog("333333333333333333333333333333333333");

                    SetLocalTime(ref st);
                    WriteLog("44444444444444444444444444444444444");

                    //SetMyLocalTime(dt);
                    Warning.WarningText = "系统时间更新成功。";
                    dtTimer.Start();
                    WriteLog("5555555555555555555555555");

                    //Thread.Sleep(500);
                }
                else
                {
                    Warning.WarningText = "网络连接不存在，不能同步网络时间。";
                }
            }
            catch (Exception ex)
            {
                Warning.WarningText = "网络连接不存在，不能同步网络时间。";
            }
        }
        private void UpdateTime(jimmy myjimmy, string msg)
        {
            dtTimer.Stop();
            DateTime dt = DateTime.Now;
            DateTime dt1 = DateTime.Now;
            try
            {
                switch (myjimmy)
                {
                    case jimmy.year:
                        dt1 = new DateTime((int)Int32.Parse(msg), dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                        break;
                    case jimmy.month:
                        dt1 = new DateTime(dt.Year, (int)Int32.Parse(msg), dt.Day, dt.Hour, dt.Minute, dt.Second);
                        break;
                    case jimmy.day:
                        dt1 = new DateTime(dt.Year, dt.Month, (int)Int32.Parse(msg), dt.Hour, dt.Minute, dt.Second);
                        break;
                    case jimmy.hour:
                        dt1 = new DateTime(dt.Year, dt.Month, dt.Day, (int)Int32.Parse(msg), dt.Minute, dt.Second);
                        break;
                    case jimmy.minute:
                        dt1 = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (int)Int32.Parse(msg), dt.Second);
                        break;
                    case jimmy.second:
                        dt1 = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, (int)Int32.Parse(msg));
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {

            }
            SystemTime st = new SystemTime();
            st.FromDateTime(dt1);
            SetLocalTime(ref st);
            Warning.WarningText = "系统时间更新成功。";
            dtTimer.Start();
        }
        void dt_Tick(object sender, EventArgs e)
        {
            DateTime dt = DateTime.Now;
            _year._time.Text = dt.Year.ToString() + "年";
            _month._time.Text = dt.Month.ToString() + "月";
            _day._time.Text = dt.Day.ToString() + "日";
            _hour._time.Text = dt.Hour.ToString() + "时";
            _minute._time.Text = dt.Minute.ToString() + "分";
            _second._time.Text = dt.Second.ToString() + "秒";
        }
        #endregion

        #region 检查系统更新
        private void CheckVersion()
        {
            string version = string.Empty;
            //检测版本号，如果有更新则以红色标注，如果没有更新则不显示
            Action<CheckResult, string> action = (dic, msg) =>
            {
                switch (dic)
                {
                    case CheckResult.None:
                        break;
                    case CheckResult.HasNew:
                        version = update.Data.versionlist.First().version;

                        if (version != string.Empty)
                        {
                            text0.Visibility = Visibility.Visible;
                            text0.Text = string.Format("({0})", version);
                            //if (version.ToLower().Contains("_release"))
                            //{
                            //    version.Replace("_release", "");
                            //}
                            version_config = version.Split('_')[0];
                        }
                        else
                        {
                            text0.Visibility = Visibility.Collapsed;
                        }
                        break;
                    case CheckResult.HasNotNew:

                        break;
                    case CheckResult.TimeOut:
                        break;
                    default:
                        break;
                }
            };
            update.BeginCheck(action);
        }
        /// <summary>
        /// 检查ICE更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateBox_Click(object sender, EventArgs e)
        {
            //CheckUpdate();
            GoCheckUpdate();
        }
        private void appDesc_Click(object sender, EventArgs e)
        {
            OpenICEZaker();
        }
        /// <summary>
        /// 检查更新
        /// </summary>
        private void CheckVersion(string updateVersion, List<string> listUrl)
        {
            CurrentStage = StageEnum.PopWinFace;
            //updateVersion = GoCheckUpdate();
            if (updateVersion == string.Empty)
            {
                Warning.WarningText = "当前已经是最新版本。";
            }
            else
            {
                MessageWin wm = new MessageWin("检测到最新版本：" + updateVersion + "，是否更新(更新期间会关闭所有应用程序)？");
                wm.Show();
                CurrentMessageWin = wm;
                wm.ClickYesBtn += delegate
                {
                    //MessageBox.Show("开始更新");
                    CurrentStage = StageEnum.LoadRarFace;
                    //List<string> zipList = new List<string>();
                    //zipList.Add(updateAdress);
                    BeginDownLoadUpdateFiles(listUrl);
                };
            }
        }
        /// <summary>
        /// 更新查询结果
        /// </summary>
        Tvm.WPF.TvmUpdate.vs UpdateResult;
        TvmUpdate update = new TvmUpdate("ice", "pc", false, "3.5.0.5");
        private void GoCheckUpdate()
        {
            updateBox.RemoteClick();
            string version = string.Empty;
            /**************************************/
            Action<CheckResult, string> action = (dic, msg) =>
            {
                switch (dic)
                {
                    case CheckResult.None:

                        break;
                    case CheckResult.HasNew:
                        UpdateResult = update.Data.versionlist.First();
                        //修改更新结果顺序错误问题
                        version = UpdateResult.version;
                        //if (version.Contains("_release"))
                        //{
                        //    version.Replace("_release", "");
                        //}
                        version = version.Split('_')[0];
                        //ICE测试
                        List<string> ListUrl = new List<string>();
                        if (isTest)
                        {
                            ListUrl.Add(update.Data.versionlist[3].addr);
                            ListUrl.Add(update.Data.versionlist[3].addr);
                            ListUrl.Add(update.Data.versionlist[3].addr);
                            ListUrl.Add(update.Data.versionlist[3].addr);
                            ListUrl.Add(update.Data.versionlist[3].addr);
                        }
                        else
                        {
                            List<Tvm.WPF.TvmUpdate.vs> ListVs = new List<TvmUpdate.vs>();
                            foreach (var item in update.Data.versionlist)
                            {
                                ListVs.Add(item);
                            }
                            ListVs.Reverse();
                            foreach (var item in ListVs)
                            {
                                ListUrl.Add(item.addr);
                            }
                        }
                        //测试zip

                        CheckVersion(UpdateResult.version, ListUrl);

                        break;
                    case CheckResult.HasNotNew:
                        Warning.WarningText = "当前没有可用的更新补丁。";
                        int a = 3;
                        break;
                    case CheckResult.TimeOut:
                        Warning.WarningText = "请求超时。";
                        break;
                    default:
                        break;
                }
            };
            update.BeginCheck(action);
            /***************************************/
        }
        #endregion

        #region 更新进度条

        private void BeginDownLoadUpdateFiles(List<string> zipUrlList)
        {
            Utility.DownLoadFileProgress -= Utility_DownLoadFileProgress;
            Utility.DownLoadFileFinished -= Utility_DownLoadFileFinished;
            Utility.DownLoadFileProgress += Utility_DownLoadFileProgress;
            Utility.DownLoadFileFinished += Utility_DownLoadFileFinished;
            Utility.DownLoadNumber -= Utility_DownLoadNumber;
            Utility.DownLoadNumber += Utility_DownLoadNumber;
            AddCoolBar(zipUrlList.Count, "正在努力更新，请勿关闭程序或切断电源！");
            CoolBar.label.Visibility = Visibility.Visible;
            CoolBar.BarReadyEvent += delegate
            {
                InitializeDownLoadQueue(zipUrlList);
                Utility.DownLoadDataAuto();
            };
        }
        int currentNumber = 0;
        void Utility_DownLoadNumber(int number)
        {
            CoolBar.ImportValue = number;
            currentNumber = number;
        }
        private void InitializeDownLoadQueue(List<string> zipUrlList)
        {
            Utility.QueueAddress.Clear();
            Utility.QueueSavePath.Clear();
            ListLocalZip.Clear();
            for (int i = 0; i < zipUrlList.Count; i++)
            {
                string item = zipUrlList[i];
                Utility.QueueAddress.Enqueue(item);
                if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\ICEPatch\\"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\ICEPatch\\");
                }
                string newDir = Directory.GetCurrentDirectory() + "\\ICEPatch\\" + System.Guid.NewGuid().ToString() + "\\";
                Directory.CreateDirectory(newDir);
                string urlSave = newDir + System.IO.Path.GetFileName(item);
                Utility.QueueSavePath.Enqueue(urlSave);
                ListLocalZip.Add(urlSave);
            }
        }
        void Utility_DownLoadFileFinished(object sender, EventArgs e)
        {
            if (Utility.isManuStopLoad)
            {
                Utility.isManuStopLoad = false;
                CoolBar.BarGo();
            }
            else
            {
                CoolBar.label.Visibility = Visibility.Collapsed;
                ///下载完成之后关闭所有进程
                CloseICEProcess();

                DispatcherTimer dtx = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
                dtx.Tick += delegate
                {
                    dtx.Stop();
                    Begin7Zip();
                };
                dtx.Start();
            }
        }

        void Utility_DownLoadFileProgress(string st, double percent)
        {
            CoolBar.ProgressBarContent = "下载更新包:" + System.IO.Path.GetFileName(st);
            CoolBar.perCent.Text = string.Format("({0}/{1}){2}%", currentNumber, CoolBar.MaxMaximum, percent);

        }
        //void CoolBar_PreviewLoadFinished(object sender, EventArgs e)
        //{
        //    CoolBar.LoadFinished -= CoolBar_PreviewLoadFinished;
        //    Warning.WarningText = "更新完成，正在为您启动服务，请稍后几秒。";
        //}
        void CoolBarContentUpdate(string text, string percent)
        {
            CoolBar.ProgressBarContent = text;
            //CoolBar.perCent.Text = percent + "%";
        }
        /// <summary>
        /// 添加进度条
        /// </summary>
        /// <param name="Max"></param>
        /// <param name="name"></param>
        void AddCoolBar(int Max, string name)
        {
            CurrentStage = StageEnum.LoadRarFace;
            PopGrid.Visibility = Visibility.Visible;
            CoolBar = new CoolProgressBar(Max); ;
            CoolBar.titleBlock.Text = name;
            PopGrid.Children.Add(CoolBar);
        }
        /// <summary>
        /// 移除进度条
        /// </summary>
        void RemoveCoolBar()
        {
            if (PopGrid.Children.Contains(CoolBar))
            {
                CoolBar.BarGo();
            }
        }

        #endregion

        #region 进程相关

        /// <summary>
        /// 关闭所有进程
        /// </summary>
        private void CloseICEProcess()
        {
            string[] line = File.ReadAllLines(@"c:\GuideConfig.conf", UnicodeEncoding.GetEncoding("GB2312"));
            foreach (var item in line)
            {
                string exeName = "t" + " " + System.IO.Path.GetFileName(item);
                if (exeName.EndsWith(".exe"))
                {
                    byte[] exebtye = Encoding.UTF8.GetBytes(exeName);
                    UdpClient udp = new UdpClient("127.0.0.1", 8899);
                    udp.Send(exebtye, exebtye.Length);
                    WriteLog("发送中控关闭进程命令，" + exeName);
                    udp.Close();
                }
            }
            DispatcherTimer dt = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
            dt.Tick += delegate
            {
                dt.Stop();
                #region MyRegion

                Process[] processArray = Process.GetProcesses();

                string name0 = "iVisionServer";
                string name1 = "Launcher";
                string name2 = "ApacheMonitor";
                string name3 = "E-MeetingCover";
                List<Process> ListProcess = new List<Process>();


                //Process pc = processArray.ToList().Find(delegate(Process process) {return process.ProcessName==name3 });


                foreach (Process item in processArray)//杀iVisionServer
                {
                    try
                    {
                        if (item.ProcessName == name0)
                        {
                            ListProcess.Add(item);
                        }
                    }
                    catch (Exception ex0)
                    {
                    }
                }
                foreach (Process item in processArray)//杀Launcher
                {
                    try
                    {
                        if (item.ProcessName == name1)
                        {
                            ListProcess.Add(item);
                        }
                    }
                    catch (Exception ex1)
                    {
                    }
                }
                foreach (Process item in processArray)//杀ApacheMonitor
                {
                    try
                    {
                        if (item.ProcessName == name2)
                        {
                            ListProcess.Add(item);
                        }
                    }
                    catch (Exception ex2)
                    {
                    }
                }
                foreach (Process item in processArray)///杀E-MeetingCover
                {
                    try
                    {
                        if (item.ProcessName == name3)
                        {
                            ListProcess.Add(item);
                        }
                    }
                    catch (Exception ex3)
                    {
                    }
                }
                string name = Process.GetCurrentProcess().MainModule.FileName.ToLower();
                foreach (Process item in processArray)
                {
                    try
                    {
                        if (!ListProcess.Contains(item))
                        {
                            if (item.MainModule.FileName.StartsWith("D:"))
                            {
                                if (item.MainModule.FileName.ToLower() != name)
                                {
                                    ListProcess.Add(item);
                                }
                            }
                        }

                    }
                    catch (Exception ex2)
                    {
                    }
                }

                for (int i = 0; i < 10; i++)
                {
                    foreach (Process item in ListProcess)
                    {
                        try
                        {
                            if (item.MainModule.FileName.ToLower() != name)
                            {
                                item.Kill();
                            }
                        }
                        catch (Exception EX)
                        {


                        }
                    }
                }

                foreach (Process item in ListProcess)
                {
                    try
                    {
                        if (item.ProcessName == name3)
                        {
                            item.Kill();
                            break;
                        }
                    }
                    catch (Exception EX)
                    {


                    }
                }
                #endregion
            };
            dt.Start();
        }
        /// <summary>
        /// 重启之前立即检查是否还有进程没有关闭，关闭之
        /// </summary>
        private void CloseICEProcessImmediately()
        {
            string[] line = File.ReadAllLines(@"c:\GuideConfig.conf", UnicodeEncoding.GetEncoding("GB2312"));
            foreach (var item in line)
            {
                string exeName = "t" + " " + System.IO.Path.GetFileName(item);
                if (exeName.EndsWith(".exe"))
                {
                    byte[] exebtye = Encoding.UTF8.GetBytes(exeName);
                    UdpClient udp = new UdpClient("127.0.0.1", 8899);
                    udp.Send(exebtye, exebtye.Length);
                    WriteLog("发送中控关闭进程命令，" + exeName);
                    udp.Close();
                }
            }
            #region MyRegion

            Process[] processArray = Process.GetProcesses();

            string name0 = "iVisionServer";
            string name1 = "Launcher";
            string name2 = "ApacheMonitor";
            string name3 = "E-MeetingCover";
            List<Process> ListProcess = new List<Process>();


            //Process pc = processArray.ToList().Find(delegate(Process process) {return process.ProcessName==name3 });


            foreach (Process item in processArray)//杀iVisionServer
            {
                try
                {
                    if (item.ProcessName == name0)
                    {
                        ListProcess.Add(item);
                    }
                }
                catch (Exception ex0)
                {
                }
            }
            foreach (Process item in processArray)//杀Launcher
            {
                try
                {
                    if (item.ProcessName == name1)
                    {
                        ListProcess.Add(item);
                    }
                }
                catch (Exception ex1)
                {
                }
            }
            foreach (Process item in processArray)//杀ApacheMonitor
            {
                try
                {
                    if (item.ProcessName == name2)
                    {
                        ListProcess.Add(item);
                    }
                }
                catch (Exception ex2)
                {
                }
            }
            foreach (Process item in processArray)///杀E-MeetingCover
            {
                try
                {
                    if (item.ProcessName == name3)
                    {
                        ListProcess.Add(item);
                    }
                }
                catch (Exception ex3)
                {
                }
            }
            string name = Process.GetCurrentProcess().MainModule.FileName.ToLower();
            foreach (Process item in processArray)
            {
                try
                {
                    if (!ListProcess.Contains(item))
                    {
                        if (item.MainModule.FileName.StartsWith("D:"))
                        {
                            if (item.MainModule.FileName.ToLower() != name)
                            {
                                ListProcess.Add(item);
                            }
                        }
                    }

                }
                catch (Exception ex2)
                {
                }
            }

            for (int i = 0; i < 10; i++)
            {
                foreach (Process item in ListProcess)
                {
                    try
                    {
                        if (item.MainModule.FileName.ToLower() != name)
                        {
                            item.Kill();
                        }
                    }
                    catch (Exception EX)
                    {


                    }
                }
            }
            #endregion
        }
        #endregion

        #region 选择框逻辑
        private void DealMsg(Msg msg)
        {

            switch (CurrentStage)
            {
                case StageEnum.MainFace:
                    switch (SelectedIndex)
                    {
                        case 0:
                            DealVolumnSlider(msg);
                            break;
                        case 1:
                            DealInternetTime(msg);
                            break;
                        case 2:
                            DealYear(msg);
                            break;
                        case 3:
                            DealMonth(msg);
                            break;
                        case 4:
                            DealDay(msg);
                            break;
                        case 5:
                            DealHour(msg);
                            break;
                        case 6:
                            DealMinute(msg);
                            break;
                        case 7:
                            DealSecond(msg);
                            break;
                        case 8:
                            DealMeeting(msg);
                            break;
                        case 9:
                            DealUpdate(msg);
                            break;
                        case 10:
                            DealAppDesc(msg);
                            break;
                        case 11:
                            DealLawText(msg);
                            break;
                        case 12:
                            DealResolution(msg);
                            break;
                        case 13:
                            DealSelectTimeZone(msg);
                            break;
                        default:
                            break;
                    }
                    break;
                case StageEnum.PopWinFace:
                    CurrentMessageWin.DealCmd(msg);
                    break;
                case StageEnum.LoadRarFace:
                    CoolBar.DealCmd(msg);
                    break;
                case StageEnum.DescFace:
                    if (popPanel != null)
                    {
                        popPanel.DealCmd(msg);
                    }
                    //OpenICEZaker();
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 时区选择遥控
        /// </summary>
        /// <param name="msg"></param>
        private void DealSelectTimeZone(Msg msg)
        {
            switch (msg)
            {
                case Msg.UP:
                    GoTimeZoneUp();
                    break;
                case Msg.DOWN:
                    GoTimeZoneDown();
                    break;
                case Msg.RIGHT:
                    GoIndex(2);
                    break;
                case Msg.LEFT:
                    GoIndex(1);
                    break;
                default:
                    break;
            }
        }
        #region 时间设置

        private void DealYear(Msg msg)
        {
            switch (msg)
            {
                case Msg.UP:

                    GoIndex(1);
                    break;
                case Msg.DOWN:
                    GoIndex(5);
                    break;
                case Msg.RIGHT:
                    GoIndex(3);
                    break;
                default:
                    break;
            }
        }
        private void DealMonth(Msg msg)
        {
            switch (msg)
            {
                case Msg.LEFT:
                    GoIndex(2);

                    break;
                case Msg.DOWN:
                    GoIndex(6);
                    break;
                case Msg.UP:
                    GoIndex(1);
                    break;

                case Msg.RIGHT:
                    GoIndex(4);
                    break;
                default:
                    break;
            }
        }
        private void DealDay(Msg msg)
        {
            switch (msg)
            {
                case Msg.LEFT:
                    GoIndex(3);
                    break;
                case Msg.DOWN:
                    GoIndex(7);
                    break;
                case Msg.UP:
                    GoIndex(13);
                    break;
                case Msg.RIGHT:
                    GoIndex(11);
                    break;
                default:
                    break;
            }
        }
        private void DealHour(Msg msg)
        {
            switch (msg)
            {
                case Msg.LEFT:
                    GoIndex(4);
                    break;
                case Msg.DOWN:
                    GoIndex(12);
                    break;
                case Msg.UP:
                    GoIndex(2);
                    break;
                case Msg.RIGHT:
                    GoIndex(6);
                    break;
                default:
                    break;
            }
        }
        private void DealMinute(Msg msg)
        {
            switch (msg)
            {
                case Msg.LEFT:
                    GoIndex(5);
                    break;
                case Msg.DOWN:
                    GoIndex(12);
                    break;
                case Msg.RIGHT:
                    GoIndex(7);
                    break;
                case Msg.UP:
                    GoIndex(3);

                    break;
                default:
                    break;
            }
        }
        private void DealSecond(Msg msg)
        {
            switch (msg)
            {
                case Msg.LEFT:
                    GoIndex(6);
                    break;
                case Msg.RIGHT:
                    GoIndex(11);
                    break;
                case Msg.DOWN:
                    GoIndex(12);
                    break;
                case Msg.UP:
                    GoIndex(4);
                    break;
                default:
                    break;
            }
        }
        private void DealResolution(Msg msg)
        {
            switch (msg)
            {
                case Msg.LEFT:
                    resolutionUI.GoPre();
                    break;
                case Msg.RIGHT:
                    resolutionUI.GoNext();
                    break;
                case Msg.DOWN:
                    GoIndex(8);
                    break;
                case Msg.UP:
                    GoIndex(5);
                    break;
                case Msg.OK:
                    resolutionUI.SetResolution();
                    break;
                default:
                    break;
            }
        }

        private void DealAppDesc(Msg msg)
        {
            switch (msg)
            {
                case Msg.LEFT:
                    GoIndex(9);
                    break;
                case Msg.RIGHT:
                    GoIndex(11);
                    break;
                case Msg.UP:
                    GoIndex(8);
                    break;

                case Msg.OK:
                    OpenICEZaker();
                    break;
                default:
                    break;
            }
        }
        private void DealLawText(Msg msg)
        {
            switch (msg)
            {
                case Msg.LEFT:
                    GoIndex(10);
                    break;
                case Msg.RIGHT:
                    GoIndex(0);

                    break;
                case Msg.UP:
                    MoveTextUp();
                    break;
                case Msg.DOWN:
                    MoveTextDown();
                    break;
                case Msg.OK:
                    OpenICEZaker();
                    break;
                default:
                    break;
            }
        }

        private void MoveTextDown()
        {
            myLawText.GoDown();
        }

        private void MoveTextUp()
        {
            myLawText.GoUp();
        }
        private void DealUpdate(Msg msg)
        {
            switch (msg)
            {
                case Msg.UP:
                    GoIndex(8);
                    break;
                case Msg.RIGHT:
                    GoIndex(10);
                    break;
                case Msg.OK:
                    GoCheckUpdate();
                    break;

                default:
                    break;
            }
        }

        private void DealMeeting(Msg msg)
        {
            switch (msg)
            {
                case Msg.UP:
                    GoIndex(12);
                    break;
                case Msg.DOWN:
                    GoIndex(9);
                    break;
                case Msg.RIGHT:
                    GoIndex(11);
                    break;
                default:
                    break;
            }
        }

        private void DealInternetTime(Msg msg)
        {
            switch (msg)
            {
                case Msg.UP:
                    GoIndex(0);
                    break;
                case Msg.RIGHT:
                    GoIndex(13);
                    break;
                case Msg.DOWN:
                    GoIndex(2);
                    break;
                case Msg.OK:
                    PressOnTimerBtn();
                    break;

                default:
                    break;
            }
        }
        #endregion

        /// <summary>
        /// 音量调节
        /// </summary>
        /// <param name="msg"></param>
        private void DealVolumnSlider(Msg msg)
        {
            switch (msg)
            {
                case Msg.UP:
                    GoIndex(11);
                    break;
                case Msg.DOWN:
                    GoIndex(1);
                    break;
                case Msg.LEFT:
                    if (isOrdersValid)
                    {
                        DecreaseVolumn();
                    }
                    break;
                case Msg.RIGHT:
                    if (isOrdersValid)
                    {
                        IncreaseVolumn();
                    }
                    break;
            }
        }
        /// <summary>
        /// 音量增加
        /// </summary>
        private void IncreaseVolumn()
        {
            OrdersIntervial(0.2);
            if (_volume.Value > 100)
            {
                _volume.Value = 100;
            }
            else
            {
                if (_volume.Value + 3 > 100)
                {
                    _volume.Value = 100;
                }
                else
                {
                    _volume.Value += 3;
                }
            }
        }
        /// <summary>
        /// 音量降低
        /// </summary>
        private void DecreaseVolumn()
        {
            OrdersIntervial(0.2);
            double minVolumn = 3;
            if (_volume.Value <= 0)
            {
                _volume.Value = 0;
            }
            else
            {
                if (_volume.Value - minVolumn < 0)
                {
                    _volume.Value = 0;
                }
                else
                {
                    _volume.Value -= minVolumn;
                }
            }

        }

        int SelectedIndex = 0;
        private void InitializeSelectedBorder()
        {
            mySelectedBorder = new SelectedBorder();
            selectedCanvas.Children.Add(mySelectedBorder);
            MoveBoderTo();
        }

        /// <summary>
        /// 移动到指定空间焦点
        /// </summary>
        /// <param name="objectIndex"></param>
        private void GoIndex(int objectIndex)
        {
            SelectedIndex = objectIndex;
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
            double time = 0.35;
            DoubleAnimation daX = new DoubleAnimation()
            {
                To = toX,
                //Equation = Equations.QuartEaseOut,
                Duration = TimeSpan.FromSeconds(time)
            };
            DoubleAnimation daY = new DoubleAnimation()
            {
                To = toY,
                //Equation = Equations.QuartEaseOut,

                Duration = TimeSpan.FromSeconds(time)
            };
            DoubleAnimation daW = new DoubleAnimation()
             {
                 To = width,
                 //Equation = Equations.QuintEaseOut,
                 Duration = TimeSpan.FromSeconds(time)
             };
            DoubleAnimation daH = new DoubleAnimation()
             {
                 To = height,
                 //Equation = Equations.QuintEaseOut,

                 Duration = TimeSpan.FromSeconds(time)
             };
            element.BeginAnimation(Canvas.LeftProperty, daX);
            element.BeginAnimation(Canvas.TopProperty, daY);
            element.BeginAnimation(WidthProperty, daW);
            element.BeginAnimation(HeightProperty, daH);
        }
        #endregion

        #region 测试按钮

        /// <summary>
        /// 向前
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            //GoForward();
            resolutionUI.GoNext();
        }
        /// <summary>
        /// 向后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //GoBackward();
            resolutionUI.GoPre();
        }
        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            resolutionUI.SetResolution();
        }
        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region 解压文件
        int zipIndex = 0;
        event EventHandler ZipFinishedEvent;
        List<string> ListLocalZip = new List<string>();
        DispatcherTimer dtProgressBar;
        Thread thread;
        bool isFinish = false;
        /// <summary>
        /// 开始解压缩
        /// </summary>
        private void Begin7Zip()
        {
            zipIndex = 0;
            thread = new Thread(new ParameterizedThreadStart(ZipThread));
            thread.IsBackground = true;
            thread.Start(zipIndex);
            dtProgressBar = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.05) };
            dtProgressBar.Tick += delegate
            {
                CoolBar._loadText.Visibility = Visibility.Visible;
                CoolBar.MaxMaximum = CoolBar_MaxMaximum;
                CoolBar.bar.Maximum = CoolBar.MaxMaximum;
                CoolBar.bar.Value = currentNumber;
                CoolBar.ProgressBarContent = CoolBar_ProgressBarContent;
                CoolBar.perCent.Text = CoolBar_perCent_Text;
                CoolBar.perCent1.Text = CoolBar_perCent1_Text;
                //if (currentNumber == CoolBar.MaxMaximum)
                //{
                try
                {
                    if (zipIndex == ListLocalZip.Count && !isFinish)
                    {
                        if (ZipFinishedEvent != null)
                        {
                            ZipFinishedEvent(this, new EventArgs());
                        }

                        thread.Abort();
                        CoolBar.BarGo();
                        version_config = version_config.Split('_')[0];

                        Utility.INIFILE.SetValue("MAIN", "version", version_config);
                        Warning.WarningText = "更新完成，立即重新启动您的设备，稍后。";
                        isReadyReboot = true;
                        MainWindow.isReboot = true;
                        DispatcherTimer dtReboot = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };
                        dtReboot.Tick += delegate
                        {
                            dtReboot.Stop();
                            WriteLog("关闭程序，开始重启");
                            CloseICEProcessImmediately();
                            App.Current.MainWindow.Close();
                        };
                        dtReboot.Start();
                        isFinish = true;
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }

                //}
            };
            dtProgressBar.Start();
        }

        bool isReadyReboot = false;
        List<DispatcherTimer> ListZipTimer = new List<DispatcherTimer>();
        List<Thread> ListThread = new List<Thread>();
        #region MyRegion

        //private void OneZipThread(object msg)
        //{
        //    string localZipUrl = ListLocalZip[zipIndex];
        //    //StopThread();
        //    //Thread thread = new Thread(new ParameterizedThreadStart(ZipThread));
        //    //thread.IsBackground = true;
        //    //thread.Start(localZipUrl);
        //    CoolBar._loadText.Visibility = Visibility.Collapsed;
        //    StopZipTimers();
        //    DispatcherTimer dt = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.01) };
        //    dt.Tick += delegate
        //    {
        //        CoolBar._loadText.Visibility = Visibility.Visible;
        //        CoolBar.MaxMaximum = CoolBar_MaxMaximum;
        //        CoolBar.bar.Maximum = CoolBar.MaxMaximum;
        //        CoolBar.bar.Value = currentNumber;
        //        CoolBar.ProgressBarContent = CoolBar_ProgressBarContent;
        //        CoolBar.perCent.Text = CoolBar_perCent_Text;
        //        CoolBar.perCent1.Text = CoolBar_perCent1_Text;
        //        if (currentNumber == CoolBar.MaxMaximum)
        //        {
        //            dt.Stop();
        //            zipIndex++;
        //            if (zipIndex == ListLocalZip.Count)
        //            {
        //                if (ZipFinishedEvent != null)
        //                {
        //                    ZipFinishedEvent(this, new EventArgs());
        //                }
        //                CoolBar.BarGo();
        //                if (version_config.Contains("_release"))
        //                {
        //                    version_config = version_config.Replace("_release", "");
        //                }
        //                Utility.INIFILE.SetValue("MAIN", "version", version_config);
        //                Warning.WarningText = "更新完成，立即重新启动您的设备，请稍后。";
        //                isReadyReboot = true;
        //                MainWindow.isReboot = true;

        //                DispatcherTimer dtReboot = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };
        //                dtReboot.Tick += delegate
        //                {
        //                    dtReboot.Stop();
        //                    App.Current.MainWindow.Close();
        //                };
        //                dtReboot.Start();
        //            }
        //            else
        //            {
        //                OneZipThread(zipIndex);
        //            }
        //        }
        //    };
        //    ListZipTimer.Add(dt);
        //    dt.Start();
        //}
        #endregion

        public void StopZipTimers()
        {
            foreach (var item in ListZipTimer)
            {
                item.Stop();
            }
            ListZipTimer.Clear();
        }
        public void StopThread()
        {
            foreach (var item in ListThread)
            {
                item.Abort();
            }
        }
        int CoolBar_MaxMaximum;
        string CoolBar_ProgressBarContent;
        string CoolBar_perCent_Text;
        string CoolBar_perCent1_Text;
        private void ZipThread(object msg)
        {
            int index = Int32.Parse(msg.ToString());
            string zipPath = ListLocalZip[index];
            using (SevenZipArchive archive = new SevenZipArchive(zipPath.ToString(), ArchiveFormat.Unkown, ""))
            {
                CoolBar_MaxMaximum = archive.Count;
                for (int i = 0; i < archive.Count; i++)
                {
                    ArchiveEntry entry = archive[i];
                    string desDir = @"D:\";//文件解压到D盘
                    string selfUpdatePath = Directory.GetCurrentDirectory() + "\\seftpatch\\";//自更新的文件临时存放在程序目录下，待程序关闭由其他exe执行复制操作，随后重启计算机
                    if (!Directory.Exists(selfUpdatePath))
                    {
                        Directory.CreateDirectory(selfUpdatePath);//创建临时存放目录
                    }
                    if (entry.FileName.ToLower().Contains("ICESetting".ToLower()))//ICESetting的更新文件
                    {
                        string st = Directory.GetCurrentDirectory() + "\\seftpatch\\" + entry.FileName;
                        if (entry.IsDirectory)//解压为目录
                        {
                            entry.Extract(selfUpdatePath);
                        }
                        else//解压为文件
                        {
                            int n = 0;
                            while (true)
                            {
                                if (File.Exists(st))
                                {
                                    File.Delete(st);
                                    WriteLog("已经存在设置文件:需要删除：" + st + ":");
                                }
                                else
                                {
                                    break;
                                }
                                n++;
                                if (n > 20)
                                {
                                    WriteLog("系统不允许删除");
                                    MessageBox.Show("系统不允许删除。");
                                }
                            }
                            try
                            {
                                entry.Extract(selfUpdatePath);
                                WriteLog("写入设置文件：" + selfUpdatePath);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                    }
                    else//其他程序的更新文件
                    {
                        string desSt = desDir + entry.FileName;
                        if (entry.IsDirectory)//是目录
                        {
                            entry.Extract(desDir);
                        }
                        else//是文件
                        {
                            int n = 0;
                            while (true)
                            {
                                if (File.Exists(desSt))
                                {
                                    File.Delete(desSt);
                                }
                                else
                                {
                                    break;
                                }
                                n++;
                                if (n > 20)
                                {
                                    MessageBox.Show("系统不允许删除。");
                                }
                            }
                            try
                            {
                                entry.Extract(desDir);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                            WriteLog("解压到其他程序中文件：" + desDir + entry.FileName);
                        }
                    }
                    CoolBar_ProgressBarContent = "更新文件:" + System.IO.Path.GetFileName(entry.FileName);
                    currentNumber = i + 1;
                    CoolBar_perCent_Text = string.Format("({0}/{1})", currentNumber, CoolBar.MaxMaximum);
                    CoolBar_perCent1_Text = string.Format("({0})", currentNumber, CoolBar.MaxMaximum);
                }
                zipIndex++;
                Console.WriteLine("记录zipIndex:" + zipIndex);
                //WriteLog();
                if (zipIndex != ListLocalZip.Count)
                {
                    ZipThread(zipIndex.ToString());
                }
            }
        }

        #endregion

        #region 重启机器
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }
        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref   IntPtr phtok);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref   long pluid);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
        ref   TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool ExitWindowsEx(int flg, int rea);

        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const int TOKEN_QUERY = 0x00000008;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        internal const int EWX_LOGOFF = 0x00000000;
        internal const int EWX_SHUTDOWN = 0x00000001;
        internal const int EWX_REBOOT = 0x00000002;
        internal const int EWX_FORCE = 0x00000004;
        internal const int EWX_POWEROFF = 0x00000008;
        internal const int EWX_FORCEIFHUNG = 0x00000010;

        public static void Reboot()
        {
            DoExitWin(EWX_FORCE | EWX_REBOOT);
        }

        public static void PowerOff()
        {
            DoExitWin(EWX_FORCE | EWX_POWEROFF);
        }

        public static void LogoOff()
        {
            DoExitWin(EWX_FORCE | EWX_LOGOFF);
        }
        private static void DoExitWin(int flg)
        {
            bool ok;
            TokPriv1Luid tp;
            IntPtr hproc = GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;
            ok = OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref   htok);
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;
            ok = LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref   tp.Luid);
            ok = AdjustTokenPrivileges(htok, false, ref   tp, 0, IntPtr.Zero, IntPtr.Zero);
            ok = ExitWindowsEx(flg, 0);
        }
        private void Restart()
        {
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = "shutdown.exe -r";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message); //显示错误信息。 
            }
        }
        private void OpenSelfRebootExe()
        {
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\SelfUpdate.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message); //显示错误信息。 
            }
        }
        #endregion

        #region 打开ICE程序引导
        /// <summary>
        /// zaker引导
        /// </summary>
        private void OpenICEZaker()
        {
            appDesc.RemoteClick();
            CurrentStage = StageEnum.DescFace;
            OrdersIntervial(1);
            popPanel = new PopPanel();
            popPanel.ClosePopPanel += popPanel_ClosePopPanel;
            List<string> ListAddress = new List<string>();

            var appDescPath = Directory.GetCurrentDirectory() + "\\AppDesc\\";
            if (!Directory.Exists(appDescPath))
            {
                MessageBox.Show("找不到文件夹 " + appDescPath);
                return;
            }

            string[] filePath = Directory.GetFiles(appDescPath);
            foreach (var item in filePath)
            {
                if (Utility.isImage(item))
                {
                    ListAddress.Add(item);
                }
            }
            popPanel.InitializeStack(ListAddress);
            griPop.Children.Add(popPanel);
        }

        void popPanel_ClosePopPanel(object sender, EventArgs e)
        {
            (sender as PopPanel).ClosePopPanel -= popPanel_ClosePopPanel;
            griPop.Children.Clear();
            popPanel = null;
            CurrentStage = StageEnum.MainFace;
        }
        #endregion

        #region 日志
        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="log"></param>
        void WriteLog(string log)
        {
            try
            {
                if (!File.Exists(Directory.GetCurrentDirectory() + "\\config\\log.txt"))
                {
                    StreamWriter sw = File.CreateText(Directory.GetCurrentDirectory() + "\\config\\log.txt");
                    sw.Close();
                    sw.Dispose();
                }
                else
                {
                    FileInfo info = new FileInfo(Directory.GetCurrentDirectory() + "\\config\\log.txt");
                    double length = info.Length / 1024 * 1024;
                    if (length > 1)
                    {
                        File.Delete(Directory.GetCurrentDirectory() + "\\config\\log.txt");
                        StreamWriter sw = File.CreateText(Directory.GetCurrentDirectory() + "\\config\\log.txt");
                        sw.Close();
                        sw.Dispose();
                    }
                }
                FileStream fsapp = new FileStream(Directory.GetCurrentDirectory() + "\\config\\log.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw1 = new StreamWriter(fsapp);
                sw1.WriteLine(log);
                sw1.Close();
                fsapp.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void CreateLog()
        {

            if (!File.Exists(Directory.GetCurrentDirectory() + "\\config\\log.txt"))
            {
                StreamWriter sw = File.CreateText(Directory.GetCurrentDirectory() + "\\config\\log.txt");
                sw.Close();
                sw.Dispose();
            }
            FileStream fsapp = new FileStream(Directory.GetCurrentDirectory() + "\\config\\log.txt", FileMode.Append, FileAccess.Write);
            StreamWriter sw1 = new StreamWriter(fsapp);
            string dateTime = DateTime.Now.ToLongDateString() + "  " + DateTime.Now.ToLongTimeString();
            sw1.WriteLine("***************************************" + DateTime.Now.ToLongDateString() + "  " + DateTime.Now.ToLongTimeString() + "***************************************");
            sw1.Close();
            fsapp.Close();
        }

        #endregion

        #region 时区
        TimeSpan defaultTimeSpane = new TimeSpan(8, 0, 0);//东八区
        int currentTimeZoneIndex;
        Dictionary<TimeSpan, string> ListTimeMap = new Dictionary<TimeSpan, string>();
        private void GetSystemTimeZone()
        {

            string timeZone = Utility.INIFILE.GetValue("MAIN", "TimeZone");
            ReadOnlyCollection<TimeZoneInfo> lst = TimeZoneInfo.GetSystemTimeZones();
            foreach (var item in lst)
            {
                TimeSpan time = item.BaseUtcOffset;
                string zoneID = item.Id;
                if (!ListTimeMap.Keys.Contains(time))
                {
                    if (time.ToString().EndsWith("00:00"))
                    {
                        ListTimeMap.Add(time, zoneID);
                    }
                }
            }
            currentTimeZoneIndex = GetIndex(timeZone);
            lableTime.Content = timeZone;
        }
        int GetIndex(string timeZone)
        {
            timeZone = timeZone.Replace("UTC", "");
            int index = 0;
            for (int i = 0; i < ListTimeMap.Count; i++)
            {
                string time = ListTimeMap.Keys.ElementAt(i).ToString();
                double hour = double.Parse(time.Split(':')[0]);
                if (hour > 0)
                {
                    time = "+" + time;
                }

                if (time.StartsWith(timeZone))
                {
                    index = i;
                    break;
                }
            }

            return index;
        }
        /// <summary>
        /// 时区秒去掉
        /// </summary>
        /// <param name="timeSpane"></param>
        /// <returns></returns>
        private string ConvertTimeToShow(TimeSpan timeSpane)
        {
            string time = timeSpane.ToString();
            string st0 = time.Split(':')[0];
            string st1 = time.Split(':')[1];
            double hour = double.Parse(st0);
            if (hour >= 0)
            {
                return "UTC+" + st0 + ":" + st1;
            }
            else
            {
                return "UTC" + st0 + ":" + st1;
            }
        }
        /// <summary>
        /// 向上切换时区
        /// </summary>
        private void GoTimeZoneUp()
        {
            timeRegion.RemoteClick();
            if (currentTimeZoneIndex > 0)
            {
                currentTimeZoneIndex--;
            }
            else
            {
                currentTimeZoneIndex = ListTimeMap.Count - 1;
            }
            lableTime.Content = ConvertTimeToShow(ListTimeMap.Keys.ElementAt(currentTimeZoneIndex));
            Utility.INIFILE.SetValue("MAIN", "TimeZone", lableTime.Content.ToString());
        }
        /// <summary>
        /// 向下切换时区
        /// </summary>
        private void GoTimeZoneDown()
        {
            timeRegion.RemoteClick();
            if (currentTimeZoneIndex < ListTimeMap.Count - 1)
            {
                currentTimeZoneIndex++;
            }
            else
            {
                currentTimeZoneIndex = 0;
            }
            lableTime.Content = ConvertTimeToShow(ListTimeMap.Keys.ElementAt(currentTimeZoneIndex));
            Utility.INIFILE.SetValue("MAIN", "TimeZone", lableTime.Content.ToString());
        }
        #endregion
    }
}