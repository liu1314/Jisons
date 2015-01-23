
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SelfUpdate
{
    class Program
    {
        static void Main(string[] args)
        {

            Thread.Sleep(1000);
            string selfPatch = Directory.GetCurrentDirectory() + "\\seftpatch";
            if (Directory.Exists(selfPatch))//存在设置程序补丁
            {
                //复制
                //MessageBox.Show("进入");
                //try
                //{
                List<string> patchList = BrowserFilesInFolder(selfPatch);

                foreach (var item in patchList)
                {
                    string sorcePath = item.Replace(selfPatch, "");
                    string desPath = @"D:" + sorcePath;

                    string directory = System.IO.Path.GetDirectoryName(desPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    if (File.Exists(desPath))
                    {
                        File.Delete(desPath);
                        WriteLog("删除：" + desPath);
                    }
                    File.Move(item, desPath);
                    WriteLog("移动文件" + item + "到" + desPath);
                }

                //Reboot();
            }
            else//不存在该补丁
            {
                //Reboot();
            }
        }
        #region 日志
        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="log"></param>
        static void WriteLog(string log)
        {
            try
            {
                if (!File.Exists(Directory.GetCurrentDirectory() + "\\config\\log1.txt"))
                {
                    StreamWriter sw = File.CreateText(Directory.GetCurrentDirectory() + "\\config\\log1.txt");
                    sw.Close();
                    sw.Dispose();
                }
                else
                {
                    FileInfo info = new FileInfo(Directory.GetCurrentDirectory() + "\\config\\log1.txt");
                    double length = info.Length / 1024 * 1024;
                    if (length > 1)
                    {
                        File.Delete(Directory.GetCurrentDirectory() + "\\config\\log1.txt");
                        StreamWriter sw = File.CreateText(Directory.GetCurrentDirectory() + "\\config\\log1.txt");
                        sw.Close();
                        sw.Dispose();
                    }
                }
                FileStream fsapp = new FileStream(Directory.GetCurrentDirectory() + "\\config\\log1.txt", FileMode.Append, FileAccess.Write);
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

        static void CreateLog()
        {

            if (!File.Exists(Directory.GetCurrentDirectory() + "\\config\\log1.txt"))
            {
                StreamWriter sw = File.CreateText(Directory.GetCurrentDirectory() + "\\config\\log1.txt");
                sw.Close();
                sw.Dispose();
            }
            FileStream fsapp = new FileStream(Directory.GetCurrentDirectory() + "\\config\\log1.txt", FileMode.Append, FileAccess.Write);
            StreamWriter sw1 = new StreamWriter(fsapp);
            string dateTime = DateTime.Now.ToLongDateString() + "  " + DateTime.Now.ToLongTimeString();
            sw1.WriteLine("***************************************" + DateTime.Now.ToLongDateString() + "  " + DateTime.Now.ToLongTimeString() + "***************************************");
            sw1.Close();
            fsapp.Close();
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
            //MessageBox.Show("重启计算机");
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


        #endregion
    }
}
