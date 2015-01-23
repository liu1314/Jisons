using ICE3.WPF.Linker;
using ICESetting.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace ICESetting
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Grid ShowMessageWin;
        public static MainWindow Win;
        public static bool isReboot = false;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;
            Win = this;
            ShowMessageWin = this.FindName("showMessageWin") as Grid;
            AddWPFLinker();
            Closed += MainWindow_Closed;
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            if (isReboot)
            {
                OpenSelfRebootExe();
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
        private void AddWPFLinker()
        {
            WPFLinker.GetCmd += WPFLinker_GetCmd;
            WPFLinker.GetCCLink += WPFLinker_GetCCLink;
            WPFLinker.GetSymbol += WPFLinker_GetSymbol;
            WPFLinker.GetUserMsg += WPFLinker_GetUserMsg;
            //WPFLinker.Login(false);
            //if (!WPFLinker.Login(false))
            //{
            //    WPFLinker.RunServer(Utility.INIFILE.GetValue("MAIN", "DeviceName"), true, System.IO.Path.GetFullPath(@"..\..\iVisionServer.exe"), false, false);
            //}
        }

        void WPFLinker_GetUserMsg(object sender, UserMsgEventArgs e)
        {
            myStage.DealInputText(e.Msg);
        }

        private void WPFLinker_GetCCLink(bool obj)
        {
            if (obj)
            {
                string deviceName = Utility.INIFILE.GetValue("MAIN", "DeviceName");
                myStage.red.Visibility = Visibility.Collapsed;
                myStage.green.Visibility = Visibility.Visible;
                Action<Dictionary<string, object>> action = (dic) =>
                        {
                            try
                            {
                                var serverInfo = StructCoverter.ToServerInfo(dic);
                                if (serverInfo.TvmID != deviceName)//不是自己需要的设备名
                                {
                                    WPFLinker.RunServer(deviceName, true, System.IO.Path.GetFullPath(@"..\..\iVisionServer.exe"), false, false);
                                }
                            }
                            catch { }
                        };
                WPFLinker.GetServerInfo(action);
            }
            else
            {
                myStage.red.Visibility = Visibility.Visible;
                myStage.green.Visibility = Visibility.Collapsed;
            }
        }
        private void WPFLinker_GetCmd(object sender, ControlCmdEventArgs e)
        {
            if (e.OneCmd != null)
            {
                myStage.DealCmd(e.OneCmd.CmdType.ToString());
            }
        }

        private void WPFLinker_GetSymbol(object sender, SymbolEventArgs e)
        {
        }
    }
}
