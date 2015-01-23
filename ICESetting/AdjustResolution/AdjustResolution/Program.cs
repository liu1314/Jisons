using CoreAudioApi;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using TVMWPFLab.Control;
using TVMWPFLab.Utils;

namespace AdjustResolution
{
    class Program
    {
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
        static IniFile INIFILE = new IniFile("\\Config\\config.ini");
        static void Main(string[] args)
        {
            // Thread.Sleep(60000);
            UpdateResolution();
            UpdateVolumn();
            UpdateTime();
            return;
        }

        private static void UpdateResolution()
        {

            Resolution r = new Resolution();
            string rs = INIFILE.GetValue("MAIN", "Resolution");
            int width = Int32.Parse(rs.Split('*')[0]);
            int height = Int32.Parse(rs.Split('*')[1]);

            //var aaa = r.getAllResolution();
            //var last = aaa[251];
            //var ddd = r.setResolution(last.dmPelsWidth, last.dmPelsHeight, 60);
        }
        static MMDevice device;
        private static void UpdateVolumn()
        {
            float volumn = float.Parse(INIFILE.GetValue("MAIN", "SystemVolumn"));
            MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
            device = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
            device.AudioEndpointVolume.MasterVolumeLevelScalar = (volumn / 100.0f);
        }
        static void UpdateTime()
        {
            GetSystemTimeZone();
            DateTime dt = GetBeijingTime();
            string timeZone = INIFILE.GetValue("MAIN", "TimeZone");
            DateTime dtUTC = TimeZoneInfo.ConvertTimeToUtc(dt, TimeZoneInfo.FindSystemTimeZoneById(ListTimeMap[defaultTimeSpane]));
            dt = TimeZoneInfo.ConvertTimeFromUtc(dtUTC, TimeZoneInfo.FindSystemTimeZoneById(ListTimeMap.Values.ElementAt(GetIndex(timeZone))));
            SystemTime st = new SystemTime();
            st.FromDateTime(dt);
            SetLocalTime(ref st);
        }
        #region 时区
        static TimeSpan defaultTimeSpane = new TimeSpan(8, 0, 0);//东八区

        static Dictionary<TimeSpan, string> ListTimeMap = new Dictionary<TimeSpan, string>();
        static void GetSystemTimeZone()
        {
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
        static string ConvertTimeToShow(TimeSpan timeSpane)
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
        static DateTime GetBeijingTime()
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
    }
}
