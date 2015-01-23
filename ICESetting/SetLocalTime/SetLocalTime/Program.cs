using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using TVMWPFLab.Utils;
namespace SetLocalTime
{
    class Program
    {
        static IniFile INIFILE = new IniFile("\\Config\\config.ini");

        static void Main(string[] args)
        {
            GetSystemTimeZone();
            PressOnTimerBtn();

        }
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
        #region 获取网络时间
        static DateTime GetInternetTime()
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
        enum jimmy
        {
            year, month, day, hour, minute, second
        }

        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int connectionDescription, int reservedValue);
        public static bool IsConnected()
        {
            int I = 0;
            bool state = InternetGetConnectedState(out I, 0);
            return state;
        }
        static void PressOnTimerBtn()
        {
            try
            {

                if (IsConnected())
                {
                    DateTime dtUTC = GetInternetTime();//0时区时间
                    ///0时区标准时间
                    DateTime dt = TimeZoneInfo.ConvertTimeFromUtc(dtUTC, TimeZoneInfo.FindSystemTimeZoneById(ListTimeMap.Values.ElementAt(currentTimeZoneIndex)));
                    SystemTime st = new SystemTime();
                    st.FromDateTime(dt);
                    SetLocalTime(ref st);
                }
                else
                {
                    //Warning.WarningText = "网络连接不存在，不能同步网络时间。";
                    //MessageBox.Show("网络无法连接");
                    //Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("网络无法连接");
            }
        }
        private void UpdateTime(jimmy myjimmy, string msg)
        {
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
        }

        #endregion
        #region 时区
        static TimeSpan defaultTimeSpane = new TimeSpan(8, 0, 0);//东八区
        static int currentTimeZoneIndex;
        static Dictionary<TimeSpan, string> ListTimeMap = new Dictionary<TimeSpan, string>();

        static void GetSystemTimeZone()
        {

            string timeZone = INIFILE.GetValue("MAIN", "TimeZone");
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

        }
        static int GetIndex(string timeZone)
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

        #endregion
    }
}
