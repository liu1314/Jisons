using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Threading;
using System.IO;
using System.Diagnostics;

namespace ICE3.WPF.Linker
{
    public class WPFLinker
    {
        #region <<初始化
        static WPFLinker()
        {
            WIN_TITLE = Application.Current.MainWindow.Title;
            if (WIN_TITLE.Contains(' '))
            {
                WIN_TITLE = WIN_TITLE.Replace(' ', '_');
                Application.Current.MainWindow.Title = WIN_TITLE;
            }
            _dispatcher = Application.Current.MainWindow.Dispatcher;
            initialWndProc();

            #region <<如果你有程序已经加了钩子就注销掉这些代码,并把在钩子中调用WndProc
            if (Application.Current.MainWindow.IsLoaded)
            {
                (PresentationSource.FromVisual(Application.Current.MainWindow) as HwndSource).AddHook(new HwndSourceHook(WndProc));
            }
            else
            {
                Application.Current.MainWindow.Loaded += MainWindow_Loaded;
            }
            #endregion
        }

        static void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Application.Current.MainWindow.Loaded -= MainWindow_Loaded;
            (PresentationSource.FromVisual(Application.Current.MainWindow) as HwndSource).AddHook(new HwndSourceHook(WndProc));
        }

        public static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_COPYDATA)
            {
                CopyDataStruct cds =
                    (CopyDataStruct)Marshal.PtrToStructure(lParam, typeof(CopyDataStruct));
                _msgQueue.Enqueue(cds.lpData);
                _msgDH.Set();
                handled = true;
            }
            return hwnd;
        }

        static void initialWndProc()
        {
            _msgQueue = new Queue<string>();
            _msgDH = new EventWaitHandle(false, EventResetMode.AutoReset);
            _msgDispatcher = new Thread(dispatchMsg);
            _msgDispatcher.IsBackground = true;
            _msgDispatcher.Name = "dispatchMsg";
            _msgDispatcher.Priority = ThreadPriority.Highest;
            _msgDispatcher.Start();
        }

        static Thread _msgDispatcher;
        static EventWaitHandle _msgDH;
        static Queue<string> _msgQueue;
        static Dispatcher _dispatcher;

        static readonly string BC_TITLE = "TVM.ICE.iVisionServer";
        static string WIN_TITLE = "";
        #endregion

        #region <<方法

        /// <summary>
        /// 登录,在注册所有需要的事件之后调用
        /// </summary>
        /// <param name="needProtect">是否需要进程保护</param>
        /// <returns></returns>
        public static bool Login(bool needProtect = false)
        {
            var dic = new Dictionary<string, object>();
            dic["subscription"] = getSubscription();
            dic["path"] = needProtect ? System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName : "";
            return sendInterprocessMsg(CmdType.Login, null, dic, null);
        }

        static string getSubscription()
        {
            StringBuilder sb = new StringBuilder(20);
            if (Reset != null)
            {
                sb.Append(EventType.Reset.ToSubscriptionCode());
            }
            if (PushMedia != null)
            {
                sb.Append(EventType.PushResource.ToSubscriptionCode());
            }
            if (GetCmd != null)
            {
                sb.Append(EventType.Command.ToSubscriptionCode());
            }
            if (RequirePassword != null)
            {
                sb.Append(EventType.SurportPassword.ToSubscriptionCode());
            }
            if (UpdateResource != null)
            {
                sb.Append(EventType.ResourceUpdate.ToSubscriptionCode());
            }
            if (GetExtendCmd != null)
            {
                sb.Append(EventType.SupportExtend.ToSubscriptionCode());
            }
            if (LinkState != null)
            {
                sb.Append(EventType.LinkState.ToSubscriptionCode());
            }
            if (GetError != null)
            {
                sb.Append(EventType.Error.ToSubscriptionCode());
            }
            if (GetSync != null)
            {
                sb.Append(EventType.Sync.ToSubscriptionCode());
            }
            if (GetFollow != null)
            {
                sb.Append(EventType.Follow.ToSubscriptionCode());
            }
            if (GetSymbol != null)
            {
                sb.Append(EventType.Symbol.ToSubscriptionCode());
            }
            if (GetUserMsg != null)
            {
                sb.Append(EventType.UserMsg.ToSubscriptionCode());
            }
            if (UserApproaching != null)
            {
                sb.Append(EventType.Approaching.ToSubscriptionCode());
            }
            if (GetLock != null)
            {
                sb.Append(EventType.Lock.ToSubscriptionCode());
            }
            if (GetForce != null)
            {
                sb.Append(EventType.Force.ToSubscriptionCode());
            }
            if (GetCCLink != null)
            {
                sb.Append(EventType.CCLinkState.ToSubscriptionCode());
            }
            if (FailedToSendMsg != null)
            {
                sb.Append(EventType.FailedToSendMsg.ToSubscriptionCode());
            }
            if (GetRunApp != null)
            {
                sb.Append(EventType.RunApp.ToSubscriptionCode());
            }
            if (GetRaw != null)
            {
                sb.Append(EventType.Raw.ToSubscriptionCode());
            }
            if (GetOtherCmd != null)
            {
                sb.Append(EventType.OtherCmd.ToSubscriptionCode());
            }

            return sb.ToString();
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        public static bool Logout()
        {
            return sendInterprocessMsg(CmdType.Logout, null, null, null);
        }

        /// <summary>
        /// 取Server的信息
        /// </summary>
        /// <param name="feedBack"></param>
        /// <returns></returns>
        public static bool GetServerInfo(Action<Dictionary<string, object>> feedBack)
        {
            var evidence = pushAction(feedBack);
            return sendInterprocessMsg(CmdType.GetServerInfo, evidence, null, null);
        }

        /// <summary>
        /// 设置Serve登录的中控密码
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool SetPassword(string password)
        {
            return sendInterprocessMsg(CmdType.SetPassword, null, new Dictionary<string, object> { { "password", password } }, null);
        }

        /// <summary>
        /// 取可以发送和接收消息的列表
        /// </summary>
        /// <param name="feedBack"></param>
        /// <returns></returns>
        public static bool GetSendRecvList(Action<Dictionary<string, object>> feedBack)
        {
            var evidence = pushAction(feedBack);
            return sendInterprocessMsg(CmdType.GetSendAndRecvList, evidence, null, null);
        }
        public static bool GetAllOnline(Action<Dictionary<string, object>> feedBack)
        {
            var evidence = pushAction(feedBack);
            return sendInterprocessMsg(CmdType.GetSendAndRecvList, evidence, null, null);
        }


        /// <summary>
        /// 取设备的AutoPlay列表
        /// </summary>
        /// <param name="feedBack"></param>
        /// <returns></returns>
        public static bool GetAutoPlay(Action<Dictionary<string, object>> feedBack)
        {
            var evidence = pushAction(feedBack);
            return sendInterprocessMsg(CmdType.GetAutoPlay, evidence, null, null);
        }

        /// <summary>
        /// 包名是否可用
        /// </summary>
        /// <param name="feedBack"></param>
        /// <returns></returns>
        public static bool IsPkgNameAvailable(string pkgName, Action<Dictionary<string, object>> feedBack)
        {
            var evidence = pushAction(feedBack);
            var dic = new Dictionary<string, object>
            {
                {"name",pkgName},
            };
            return sendInterprocessMsg(CmdType.CheckPkgName, evidence, dic, null);
        }

        public static bool GetPrivatePkg(Action<Dictionary<string, object>> feedBack)
        {
            var evidence = pushAction(feedBack);
            return sendInterprocessMsg(CmdType.GetPrivatePkg, evidence, null, null);
        }
        /// <summary>
        /// 发送控制命令
        /// </summary>
        /// <returns></returns>
        public static bool SendCmd(ControlCmd controlCmd, string data)
        {
            var value = new Dictionary<string, object>
            {
                    {"receiver", null},
                    {"cmd",controlCmd.ToString()},
                    {"body",data},
            };
            return sendInterprocessMsg(CmdType.SendCmd, null, value, null);
        }

        /// <summary>
        /// 发送多个命令
        /// </summary>
        /// <param name="cmds"></param>
        /// <returns></returns>
        public static bool SendCmds(params ControlCmd[] cmds)
        {
            //string[] recv = new string[] { "123", "32" };
            string[] cmdsArray = null;
            try
            {
                cmdsArray = cmds.Select(item => item.ToString().ToLower()).ToArray();
            }
            catch { }
            var dic = new Dictionary<string, object>
                {
                    {"cmds",cmdsArray},
                };
            var value = new Dictionary<string, object>
            {
                    {"receiver", null},
            };
            return sendInterprocessMsg(CmdType.SendCmds, null, value, dic);
        }

        /// <summary>
        /// 推送多资源
        /// </summary>
        /// <param name="prd"></param>
        /// <param name="receivers"></param>
        /// <param name="direction">从X轴到出现方向的夹角,从右入为0度,上为90度,下为-90/270,左为180/-180</param>
        /// <returns></returns>
        public static bool PushResource(PushResourceData[] prd, string[] receivers, int direction = 0)
        {
            var dic = new Dictionary<string, object>
                {
                    {"type","resource"},
                    {"detail",prd},
                    {"direction",direction}
                };
            var value = new Dictionary<string, object>
            {
                    {"receiver", receivers},        
            };

            return sendInterprocessMsg(CmdType.Push, null, value, dic);
        }
        /// <summary>
        /// 推网页资源
        /// </summary>
        /// <param name="pwd"></param>
        /// <param name="receivers"></param>
        /// <param name="direction">从X轴到出现方向的夹角,从右入为0度,上为90度,下为-90/270,左为180/-180</param>
        /// <returns></returns>
        public static bool PushWeb(PushWebData[] pwd, string[] receivers, int direction = 0)
        {
            var dic = new Dictionary<string, object>
                {
                    {"type","web"},
                    {"detail",pwd},
                    {"direction",direction}
                };
            var value = new Dictionary<string, object>
            {
                    {"receiver", receivers},        
            };
            return sendInterprocessMsg(CmdType.Push, null, value, dic);
        }
        /// <summary>
        /// 推送流
        /// </summary>
        /// <param name="streamUrl"></param>
        /// <param name="start"></param>
        /// <param name="title"></param>
        /// <param name="receivers"></param>
        /// <param name="direction">从X轴到出现方向的夹角,从右入为0度,上为90度,下为-90/270,左为180/-180</param>
        /// <returns></returns>
        public static bool PushStream(string streamUrl, bool start, string title, string[] receivers, int direction = 0)
        {
            var dic = new Dictionary<string, object>
                {
                    {"type","stream"},
                    {"url",streamUrl},
                    {"start", start},
                    {"title", title},
                    {"direction",direction},
                };
            var value = new Dictionary<string, object>
            {
                    {"receiver", receivers},        
            };
            return sendInterprocessMsg(CmdType.Push, null, value, dic);
        }
        /// <summary>
        /// 上传交流 并 推送
        /// </summary>
        /// <param name="rs"></param>
        /// <param name="receivers"></param>
        /// <returns></returns>
        public static bool PushFile(ResourceStruct rs, string[] receivers)
        {
            var value = new Dictionary<string, object>
            {
                    {"receiver", receivers},        
                    {"detail", rs},
            };
            return sendInterprocessMsg(CmdType.PushFile, null, value, null);
        }

        /// <summary>
        /// 上传资源包
        /// </summary>
        /// <param name="rcs"></param>
        /// <param name="progress"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public static bool UploadPackage(ResourceContainerStruct rcs, Action<Dictionary<string, object>> progress,
            Action<Dictionary<string, object>> complete, bool toVirtualOnly = false)
        {
            Debug.Assert(rcs != null);
            string pe = "";
            if (complete != null)
            {
                pe = pushAction(complete);
                if (progress != null)
                {
                    pushProgressAction(pe, progress);
                }
            }
            var value = new Dictionary<string, object>
                {
                    {"package",rcs},
                    {"virtual",toVirtualOnly},//是否同时copy到虚拟中控中，
                };
            return sendInterprocessMsg(CmdType.UploadPackage, pe, value, null);
        }

        /// <summary>
        /// 取中控所有包
        /// </summary>
        /// <param name="feedBack"></param>
        /// <returns></returns>
        public static bool GetAllPkg(Action<Dictionary<string, object>> feedBack)
        {
            string evidence = "";
            if (feedBack != null)
            {
                evidence = pushAction(feedBack);
            }
            return sendInterprocessMsg(CmdType.GetAllPkg, evidence, null, null);
        }

        /// <summary>
        /// 取虚拟中控所有包
        /// </summary>
        /// <param name="feedBack"></param>
        /// <returns></returns>
        public static bool GetAllPkgOnVirtual(Action<Dictionary<string, object>> feedBack)
        {
            string evidence = "";
            if (feedBack != null)
            {
                evidence = pushAction(feedBack);
            }
            return sendInterprocessMsg(CmdType.GetAllPkgInVirtual, evidence, null, null);
        }

        /// <summary>
        /// 搜索资源
        /// </summary>
        /// <param name="feedBack"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static bool Search(Action<Dictionary<string, object>> feedBack, params string[] ids)
        {
            string evidence = "";
            if (feedBack != null)
            {
                evidence = pushAction(feedBack);
            }
            var value = new Dictionary<string, object>
                {
                    {"ids",ids},
                };
            return sendInterprocessMsg(CmdType.Search, evidence, value, null);
        }

        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="feedBack">回调</param>
        /// <param name="id">搜索的id</param>
        /// <param name="isICEID">是ICEID还是TVMID</param>
        /// <returns></returns>
        public static bool SearchUser(Action<Dictionary<string, object>> feedBack, string id, bool isICEID)
        {
            string evidence = "";
            if (feedBack != null)
            {
                evidence = pushAction(feedBack);
            }
            var value = new Dictionary<string, object>(1);
            if (isICEID)
            {
                value["iceid"] = id;
                value["tvmid"] = null;
            }
            else
            {
                value["iceid"] = null;
                value["tvmid"] = id;
            }
            return sendInterprocessMsg(CmdType.SearchUser, evidence, value, null);
        }

        public static bool SearchUserOnline(Action<Dictionary<string, object>> feedBack)
        {
            string evidence = "";
            if (feedBack != null)
            {
                evidence = pushAction(feedBack);
            }
            var value = new Dictionary<string, object>(1);
            value["iceid"] = SEARCH_USER_ONLINE;
            value["tvmid"] = "";
            return sendInterprocessMsg(CmdType.SearchUser, evidence, value, null);
        }
        static string SEARCH_USER_ONLINE = "USER_ONLINE";

        internal static bool SearchAllUser(Action<Dictionary<string, object>> feedBack)
        {
            string evidence = "";
            if (feedBack != null)
            {
                evidence = pushAction(feedBack);
            }
            var value = new Dictionary<string, object>(1);
            value["iceid"] = null;
            value["tvmid"] = null;
            return sendInterprocessMsg(CmdType.SearchUser, evidence, value, null);
        }

        public static bool GetCCLinkState(Action<Dictionary<string, object>> feedBack)
        {
            string evidence = "";
            if (feedBack != null)
            {
                evidence = pushAction(feedBack);
            }
            return sendInterprocessMsg(CmdType.CCLinkState, evidence, null, null);
        }


        public static bool Cache(Action<Dictionary<string, object>> progress, Action<Dictionary<string, object>> complete, params string[] ids)
        {
            string pe = "";
            if (complete != null)
            {
                pe = pushAction(complete);
                if (progress != null)
                {
                    pushProgressAction(pe, progress);
                }
            }
            var value = new Dictionary<string, object>
                {
                    {"ids",new HashSet<string>(ids)},
                };
            return sendInterprocessMsg(CmdType.Cache, pe, value, null);
        }

        /// <summary>
        /// 发送扩展命令
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="receivers"></param>
        /// <returns></returns>
        public static bool SendExtendCmd(string msg, string[] receivers)
        {
            var dic = new Dictionary<string, object>
                {
                    {"extend",msg},
                };
            var value = new Dictionary<string, object>
                {
                    {"receiver",receivers},
                };

            return sendInterprocessMsg(CmdType.Extend, null, value, dic);
        }

        /// <summary>
        /// 发送命令以让另一端的Serve个程序运行
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="receivers"></param>
        /// <returns></returns>
        public static bool RunApp(string appName, string[] receivers)
        {
            if (string.IsNullOrEmpty(appName))
            {
                throw new ArgumentNullException("appName");
            }

            var dic = new Dictionary<string, object>
                {
                    {"app",appName},
                };
            var value = new Dictionary<string, object>
                {
                    {"receiver",receivers},
                };

            return sendInterprocessMsg(CmdType.RunApp, null, value, dic);
        }

        /// <summary>
        /// 清空中控
        /// </summary>
        /// <returns></returns>
        public static bool ResetCC()
        {
            return sendInterprocessMsg(CmdType.Reset, null, null, null);
        }

        /// <summary>
        /// 删除包
        /// </summary>
        /// <param name="pkgName"></param>
        /// <returns></returns>
        public static bool DeletePkg(string pkgName, HandleCC hc = HandleCC.Current)
        {
            var value = new Dictionary<string, object>
                {
                    {"pkg",pkgName},
                    {"base",hc},
                };
            return sendInterprocessMsg(CmdType.DeletePkg, null, value, null);
        }

        /// <summary>
        /// 发送同步命令
        /// </summary>
        /// <param name="pkgName"></param>
        /// <param name="guid"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static bool SendSync(string pkgName, string guid, string tag)
        {
            var dic = new Dictionary<string, object>
                {
                    {"pkg",pkgName},
                    {"guid",guid},
                    {"tag",tag},
                };
            return sendInterprocessMsg(CmdType.Sync, null, null, dic);
        }

        /// <summary>
        /// 发送跟随命令
        /// </summary>
        /// <param name="source"></param>
        /// <param name="thumb"></param>
        /// <param name="title"></param>
        /// <param name="position">视频位置(单位:秒)</param>
        /// <returns></returns>
        public static bool SendFollow(string source, string thumb, string title, string position, string guid, string pkgName, string relay)
        {
            var dic = new Dictionary<string, object>
                {
                    {"source",source},
                    {"thumb",thumb},
                    {"title",title},
                    {"position",position},
                    {"guid",guid},
                    {"pkg",pkgName},
                    {"relay",relay},
                };
            return sendInterprocessMsg(CmdType.SendFollow, null, null, dic);
        }

        /// <summary>
        /// 发字符消息
        /// </summary>
        /// <param name="symbol">有效的值为0-9 A-D</param>
        /// <param name="receivers">接收者TVM_ID</param>
        /// <returns></returns>
        public static bool SendSymbol(char symbol, string qustionGuid = null, string[] receivers = null)
        {

            var dic = new Dictionary<string, object>
                {
                    {"symbol",symbol},
                    {"qustionguid",qustionGuid ?? ""},
                };
            var value = new Dictionary<string, object>
                {
                    {"receiver",receivers},
                };

            return sendInterprocessMsg(CmdType.Symbol, null, value, dic);

        }

        /// <summary>
        /// 更新用户资料
        /// </summary>
        /// <param name="ifs"></param>
        /// <returns></returns>
        public static bool UploadUserInfo(UserInforUpload ifs, Action<Dictionary<string, object>> feedBack = null)
        {
            string pe = "";
            if (feedBack != null)
            {
                pe = pushAction(feedBack);
            }
            var value = new Dictionary<string, object>
                {
                    {"userinfo",ifs},
                };

            return sendInterprocessMsg(CmdType.UploadUserInfo, pe, value, null);
        }

        internal static bool ChangeOrder(PackageResourceOrder pro, HandleCC hc = HandleCC.Current)
        {
            var value = new Dictionary<string, object>
                {
                    {"order",pro},
                    {"base",hc},
                };

            return sendInterprocessMsg(CmdType.Sort, null, value, null);
        }
        public static string GetRegistriedServer()
        {
            string path = null;
            Microsoft.Win32.RegistryKey rkey = null;
            try
            {
                rkey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\TVMining\Wify Plus\iVisionServer");
                if (rkey != null)
                {
                    path = (string)rkey.GetValue("Path", "");
                }
                else
                {
#if RELEASE
                                        throw new AccessViolationException("服务未注册.");
#endif
                }
            }
            finally
            {
                if (rkey != null)
                {
                    rkey.Close();
                }
            }
            return path;
        }

        /// <summary>
        /// 启动Server并登录
        /// </summary>
        /// <param name="tvmID"></param>
        /// <param name="isDevice"></param>
        /// <param name="path"></param>
        /// <param name="withVirtual"></param>
        /// <param name="needProtect"></param>
        public static void RunServer(string tvmID = null, bool isDevice = false, string path = null, bool withVirtual = false, bool needProtect = false)
        {
            if (string.IsNullOrEmpty(tvmID) && IsServerRunning())//不提供Tvmid 且 程序已经运行.
            {
                return;
            }
            if (string.IsNullOrEmpty(path))
            {
                path = GetRegistriedServer();
            }

            if (File.Exists(path))
            {
                Process p = new Process();
                p.StartInfo.FileName = path;
                p.StartInfo.WorkingDirectory = Path.GetDirectoryName(path);

                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrEmpty(tvmID))
                {
                    sb.Append(isDevice ? "dn:" : "id:");
                    sb.Append(tvmID);
                    sb.Append(' ');
                }

                sb.Append("tl:");
                sb.Append(WIN_TITLE);
                sb.Append(' ');

                sb.Append("sb:");
                sb.Append(getSubscription());
                sb.Append(' ');

                if (withVirtual)
                {
                    sb.Append("v");
                    sb.Append(' ');
                }

                if (needProtect)
                {
                    sb.Append("pp:");
                    sb.Append(Uri.EscapeUriString(Process.GetCurrentProcess().MainModule.FileName));
                }
                p.StartInfo.Arguments = sb.ToString();

                p.Start();
                try
                {
                    p.WaitForInputIdle();
                }
                catch { }
                p = null;
            }
            else
            {
                throw new FileNotFoundException("文件不存在.", "服务 Path");
            }
        }

        /// <summary>
        /// Server是否已经在运行
        /// </summary>
        /// <returns></returns>
        public static bool IsServerRunning()
        {
            IntPtr hwnd = FindWindow(null, BC_TITLE);
            return hwnd != IntPtr.Zero;
        }

        /// <summary>
        /// 正常关闭Server,以保存Server相应信息
        /// </summary>
        /// <returns></returns>
        public static bool ShutdownServer(bool join = false)
        {
            var res = sendInterprocessMsg(CmdType.Shutdown, null, null, null);
            if (res && join)
            {
                //SpinWait.SpinUntil(() =>
                //{
                //    return FindWindow(null, BC_TITLE) == IntPtr.Zero;
                //});
                int maxCount = 10;
                while (FindWindow(null, BC_TITLE) != IntPtr.Zero)
                {
                    if (maxCount < 0)
                    {
                        throw new TimeoutException();
                    }
                    Thread.Sleep(500);
                }
            }
            return res;
        }

        public static bool SendUserMsg(string msg, string[] receivers = null)
        {
            var dic = new Dictionary<string, object>
                {
                    {"msg",msg},
                };
            var value = new Dictionary<string, object>
                {
                    {"receiver",receivers},
                };

            return sendInterprocessMsg(CmdType.UserMsg, null, value, dic);
        }
        public static bool SwitchVirtual(bool toVirtual)
        {
            var value = new Dictionary<string, object>
                {
                    {"VirtualOn",toVirtual},
                };

            return sendInterprocessMsg(CmdType.Virtual, null, value, null);
        }

        public static bool SendLockCmd(bool toLock, string[] receivers = null)
        {
            var value = new Dictionary<string, object>
                {
                    {"receiver",receivers},
                };
            var msg = new Dictionary<string, object>
                {
                    {"power",toLock ? "on":"off"},
                };

            return sendInterprocessMsg(CmdType.Lock, null, value, msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toForce"></param>
        /// <param name="pkg"></param>
        /// <param name="guid"></param>
        /// <param name="action">follow/answer</param>
        /// <returns></returns>
        public static bool SendForceCmd(bool toForce, ForeceCmdAction action, string guid = null, string[] receivers = null)
        {
            var value = new Dictionary<string, object>
                {
                    {"receiver",receivers},
                };
            var msg = new Dictionary<string, object>
                {
                    {"power",toForce ? "on":"off"},
                    {"action",action.ToString()},
                    {"guid",guid??""},
                };

            return sendInterprocessMsg(CmdType.Force, null, value, msg);
        }

        public static bool SendOtherCmd(OtherCmd oc, string[] receivers = null)
        {
            var value = new Dictionary<string, object>
            {
                    {"receiver", receivers},
                    {"cmdType",oc.cmdType},
                    {"cmdBody",oc.cmdBody},
            };
            return sendInterprocessMsg(CmdType.OtherCmd, null, value, null);
        }

        public static bool SendOtherCmd(string cmdType, string cmdBody, string[] receivers = null)
        {
            var value = new Dictionary<string, object>
            {
                    {"receiver", receivers},
                    {"cmdType",cmdType},
                    {"cmdBody",cmdBody},
            };
            return sendInterprocessMsg(CmdType.OtherCmd, null, value, null);
        }


        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="guids">GUID</param>
        /// <param name="hc">针对的中控 虚拟中控 ? 当前中控 ? Both ?</param>
        /// <returns></returns>
        public static bool DeleteFiles(string[] guids, HandleCC hc = HandleCC.Current)
        {
            var value = new Dictionary<string, object>
                {
                    {"guids",guids},
                    {"base",hc}
                };
            return sendInterprocessMsg(CmdType.DeleteFiles, null, value, null);
        }

        public static bool SendUserApproach()
        {
            return sendInterprocessMsg(CmdType.UserApproach, null, null, null);
        }

        internal static string GetEvidence(Action<Dictionary<string, object>> pac)
        {
            string evi = null;
            if (pac == null)
            {
                evi = null;
            }
            else
            {
                try
                {
                    var d = (from x in _delegateDict
                             where x.Value == pac
                             select x).ToArray();
                    evi = d[0].Key;
                }
                catch { }
            }
            return evi;

        }

        internal static bool CancelCallBack(string evidence)
        {
            removeAction(evidence);
            removeAction(string.Concat(PROGRESS, evidence));
            var value = new Dictionary<string, object>
                {
                    {"evidence",evidence},
                };
            return sendInterprocessMsg(CmdType.Abort, null, value, null);
        }

        #endregion

        #region <<事件
        /// <summary>
        /// 重置
        /// </summary>
        public static event EventHandler Reset;

        /// <summary>
        /// 推送,甩屏
        /// </summary>
        public static event EventHandler<PushMediaEventArgs> PushMedia;

        /// <summary>
        /// 需要密码
        /// </summary>
        public static event EventHandler RequirePassword;

        /// <summary>
        /// 得到命令
        /// </summary>
        public static event EventHandler<ControlCmdEventArgs> GetCmd;

        /// <summary>
        /// 资源更新
        /// </summary>
        public static event EventHandler<ResourceUpdateEventArgs> UpdateResource;

        /// <summary>
        /// 扩展命令
        /// </summary>
        public static event EventHandler<ExtendEventArgs> GetExtendCmd;

        /// <summary>
        /// Server连接状态
        /// </summary>
        public static event EventHandler<LinkStateEventArgs> LinkState;

        /// <summary>
        /// Server出错信息
        /// </summary>
        public static event EventHandler<ServerErrorEventArgs> GetError;

        /// <summary>
        /// 同步命令
        /// </summary>
        public static event EventHandler<SyncEventArgs> GetSync;

        /// <summary>
        /// 跟随
        /// </summary>
        public static event EventHandler<FollowEventArgs> GetFollow;

        /// <summary>
        /// 接收字符
        /// </summary>
        public static event EventHandler<SymbolEventArgs> GetSymbol;

        /// <summary>
        /// 用户消息
        /// </summary>
        public static event EventHandler<UserMsgEventArgs> GetUserMsg;

        /// <summary>
        /// 用户漫游至
        /// </summary>
        public static event EventHandler<UserInfoStruct> UserApproaching;

        /// <summary>
        /// 锁定
        /// </summary>
        public static event EventHandler<LockEventArgs> GetLock;

        /// <summary>
        /// 强制
        /// </summary>
        public static event EventHandler<ForceEventArgs> GetForce;

        public static event Action<bool> GetCCLink;

        public static event Action<string> FailedToSendMsg;

        public static event EventHandler<RunAppArgs> GetRunApp;

        public static event Action<string> GetRaw;

        public static event Action<OtherCmd> GetOtherCmd;

        /// <summary>
        /// 发不出消息了,找不到server
        /// </summary>
        public static event Action ServerDown;

        #endregion

        #region <<内部方法

        enum CmdType
        {
            None = 0,
            Login,
            Logout,
            GetServerInfo,
            SetPassword,
            GetSendAndRecvList,
            GetAutoPlay,

            SendCmd,
            SendCmds,
            Push,
            PushFile,
            UploadPackage,
            GetAllPkg,
            Search,
            Reset,
            DeletePkg,
            Extend,
            RunApp,
            Sync,
            SendFollow,
            UploadUserInfo,
            Shutdown,
            Symbol,
            CheckPkgName,
            GetPrivatePkg,
            SearchUser,
            UserMsg,
            Cache,
            //3.4
            Virtual,
            GetAllPkgInVirtual,

            Lock,
            Force,
            DeleteFiles,
            UserApproach,
            CCLinkState,
            Abort,
            Sort,
            OtherCmd,
        }

        #region <<消息处理
        class InterprocessStruct
        {
            public string Sender;
            public string Evidence;
            public string CmdType;
            public Dictionary<string, object> Values;
            public Dictionary<string, object> Msgs;
        }
        static Dictionary<string, Action<Dictionary<string, object>>> _delegateDict = new Dictionary<string, Action<Dictionary<string, object>>>();
        static void dispatchMsg()
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            jss.MaxJsonLength = int.MaxValue;
            while (true)
            {
                try
                {
                    _msgDH.WaitOne();
                    while (_msgQueue.Count > 0)
                    {
                        string msg = _msgQueue.Dequeue();
                        if (!string.IsNullOrEmpty(msg))
                        {
                            InterprocessStruct detail = null;
                            try { detail = jss.Deserialize<InterprocessStruct>(msg); }
                            catch { }
                            if (detail != null)
                            {
                                if (string.IsNullOrEmpty(detail.Evidence))//广播消息,触发相应的事件
                                {
                                    trigerEvent(detail);
                                }
                                else//回应消息
                                {
                                    if (detail.Evidence.StartsWith(PROGRESS))
                                    {
                                        var pac = useProgressAction(detail.Evidence);
                                        if (pac != null)
                                        {
                                            invokeMethod(pac, detail.Values);
                                        }
                                    }
                                    else
                                    {
                                        var rac = popAction(detail.Evidence);
                                        if (rac != null)
                                        {
                                            invokeMethod(rac, detail.Values);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    MessageBox.Show(string.Format("{0}:{1}", "dispatchMsg", ex.Message));
#endif
                }
            }
        }

        [Conditional("DEBUG")]
        static void error(EventType et, Exception ex)
        {
            MessageBox.Show(string.Format("WPFLinker:\t{0}:\t{1}", et.ToString(), ex.Message));
        }
        private static void trigerEvent(InterprocessStruct detail)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            Action ac;
            EventType et = (EventType)Enum.Parse(typeof(EventType), detail.CmdType);
            switch (et)
            {
                case EventType.Reset://Reset
                    #region <<
                    if (Reset != null)
                    {
                        ac = delegate
                        {
                            Reset(null, null);
                        };
                        _dispatcher.Invoke(DispatcherPriority.Send, ac);
                    }
                    break;
                    #endregion
                case EventType.PushResource://PushMedia
                    #region <<
                    if (PushMedia != null)
                    {
                        PushMediaEventArgs pmea = new PushMediaEventArgs();
                        try
                        {
                            var type = detail.Values["type"].ToString();
                            if (detail.Values.ContainsKey("iceid"))
                            {
                                pmea.iceid = (string)detail.Values["iceid"];
                            }
                            switch (type)
                            {
                                case "resource"://中控资源
                                    pmea.Type = PushMediaType.Resource;
                                    pmea.Resource = jss.Deserialize<PushResourceConainer[]>(jss.Serialize(detail.Values["detail"]));
                                    break;
                                case "stream"://流
                                    pmea.Type = PushMediaType.Stream;
                                    pmea.Stream = jss.Deserialize<PushStreamData>(jss.Serialize(detail.Values));
                                    break;
                                case "web"://网页资源
                                    pmea.Type = PushMediaType.Web;
                                    pmea.Web = jss.Deserialize<PushWebData[]>(jss.Serialize(detail.Values["detail"]));
                                    break;
                                default:
                                    break;
                            }
                            if (detail.Values.ContainsKey("direction"))
                            {
                                try
                                {
                                    pmea.direction = (int)detail.Values["direction"];
                                }
                                catch
                                {
                                    try
                                    {
                                        pmea.direction = int.Parse(detail.Values["direction"].ToString());
                                    }
                                    catch { }
                                }
                            }

                            ac = delegate
                            {
                                PushMedia(detail.Sender, pmea);
                            };
                            _dispatcher.Invoke(DispatcherPriority.Send, ac);
                        }
                        catch (Exception ex)
                        {
                            error(et, ex);
                        }
                    }
                    break;
                    #endregion
                case EventType.Command://Cmd
                    #region <<
                    if (GetCmd != null)
                    {
                        ControlCmdEventArgs ccea = new ControlCmdEventArgs();
                        try
                        {
                            if (detail.Values.ContainsKey("iceid"))
                            {
                                ccea.iceid = (string)detail.Values["iceid"];
                            }
                            if (detail.Values.ContainsKey("cmd"))//单命令
                            {
                                var cmd = detail.Values["cmd"].ToString().ToUpper();
                                ccea.OneCmd = new ControlCmdStruct();
                                ccea.OneCmd.CmdType = (ControlCmd)Enum.Parse(typeof(ControlCmd), cmd);
                                ccea.OneCmd.Data = detail.Values["body"].ToString();
                            }
                            else
                            {
                                ControlCmd[] cmds = jss.Deserialize<ControlCmd[]>(jss.Serialize(detail.Values["cmds"]));
                                ccea.MultiCmds = cmds;
                            }

                            ac = delegate
                            {
                                GetCmd(detail.Sender, ccea);
                            };
                            _dispatcher.Invoke(DispatcherPriority.Send, ac);
                        }
                        catch (Exception ex)
                        {
                            error(et, ex);
                        }
                    }
                    break;
                    #endregion
                case EventType.ResourceUpdate://ResourceUpdate
                    #region <<
                    if (UpdateResource != null)
                    {
                        var arg = new ResourceUpdateEventArgs();
                        try
                        {
                            arg.Action = jss.Deserialize<ResourceAction>(detail.Values["action"].ToString());
                            arg.Data = jss.Deserialize<ResourceContainerStruct>(jss.Serialize(detail.Values["data"]));
                            ac = delegate
                            {
                                UpdateResource(detail.Sender, arg);
                            };
                            _dispatcher.Invoke(DispatcherPriority.Send, ac);
                        }
                        catch (Exception ex)
                        {
                            error(et, ex);
                        }
                    }
                    break;
                    #endregion
                case EventType.SupportExtend://Extend
                    #region <<
                    if (GetExtendCmd != null)
                    {
                        var arg = new ExtendEventArgs();
                        try
                        {
                            if (detail.Values.ContainsKey("iceid"))
                            {
                                arg.iceid = (string)detail.Values["iceid"];
                            }
                            if (detail.Values["extend"] is string)
                            {
                                arg.Msg = (string)detail.Values["extend"];
                            }
                            else
                            {
                                arg.Msg = jss.Serialize(detail.Values["extend"]);
                            }
                            ac = delegate
                            {
                                GetExtendCmd(detail.Sender, arg);
                            };
                            _dispatcher.Invoke(DispatcherPriority.Send, ac);
                        }
                        catch (Exception ex)
                        {
                            error(et, ex);
                        }
                    }
                    break;
                    #endregion
                case EventType.SurportPassword://RequirePassword
                    #region <<
                    if (RequirePassword != null)
                    {
                        ac = delegate
                        {
                            RequirePassword(null, null);
                        };
                        _dispatcher.Invoke(DispatcherPriority.Send, ac);
                    }
                    break;
                    #endregion
                case EventType.LinkState://LinkState
                    if (LinkState != null)
                    {
                        var arg = new LinkStateEventArgs();
                        try
                        {
                            arg.LinkState = (ServerLinkState)detail.Values["state"];
                            ac = delegate
                            {
                                LinkState(detail.Sender, arg);
                            };
                            _dispatcher.Invoke(DispatcherPriority.Send, ac);
                        }
                        catch (Exception ex)
                        {
                            error(et, ex);
                        }

                    }
                    break;
                case EventType.Error://Error
                    if (GetError != null)
                    {
                        var arg = new ServerErrorEventArgs();
                        try
                        {
                            arg.ErrorType = (ServerError)detail.Values["code"];
                            arg.ErrorMsg = (string)detail.Values["msg"];
                            ac = delegate
                            {
                                GetError(detail.Sender, arg);
                            };
                            _dispatcher.Invoke(DispatcherPriority.Send, ac);
                        }
                        catch (Exception ex)
                        {
                            error(et, ex);
                        }
                    }
                    break;
                case EventType.Sync://Sync
                    if (GetSync != null)
                    {
                        var arg = new SyncEventArgs();
                        try
                        {
                            if (detail.Values.ContainsKey("iceid"))
                            {
                                arg.iceid = (string)detail.Values["iceid"];
                            }
                            arg.PkgName = detail.Values["pkg"].ToString();
                            arg.Guid = detail.Values["guid"].ToString();
                            arg.Tag = detail.Values["tag"].ToString();
                            ac = delegate
                            {
                                GetSync(detail.Sender, arg);
                            };
                            _dispatcher.Invoke(DispatcherPriority.Send, ac);
                        }
                        catch (Exception ex)
                        {
                            error(et, ex);
                        }
                    }
                    break;
                case EventType.Follow://Follow
                    if (GetFollow != null)
                    {
                        FollowEventArgs fea = jss.Deserialize<FollowEventArgs>(jss.Serialize(detail.Values));
                        ac = delegate
                        {
                            GetFollow(detail.Sender, fea);
                        };
                        _dispatcher.Invoke(DispatcherPriority.Send, ac);
                    }
                    break;
                case EventType.Symbol://Symbol
                    if (GetSymbol != null)
                    {
                        SymbolEventArgs sea = jss.Deserialize<SymbolEventArgs>(jss.Serialize(detail.Values));
                        ac = delegate
                        {
                            GetSymbol(detail.Sender, sea);
                        };
                        _dispatcher.Invoke(DispatcherPriority.Send, ac);
                    }
                    break;
                case EventType.UserMsg:
                    if (GetUserMsg != null)
                    {
                        var arg = new UserMsgEventArgs();
                        try
                        {
                            if (detail.Values.ContainsKey("iceid"))
                            {
                                arg.iceid = (string)detail.Values["iceid"];
                            }
                            arg.Msg = (string)detail.Values["msg"];
                            ac = delegate
                            {
                                GetUserMsg(detail.Sender, arg);
                            };
                            _dispatcher.Invoke(DispatcherPriority.Send, ac);
                        }
                        catch (Exception ex)
                        {
                            error(et, ex);
                        }
                    }
                    break;
                case EventType.Approaching:
                    if (UserApproaching != null)
                    {
                        try
                        {
                            UserInfoStruct sea = jss.Deserialize<UserInfoStruct>(jss.Serialize(detail.Values["result"]));
                            ac = delegate
                            {
                                UserApproaching(detail.Sender, sea);
                            };
                            _dispatcher.Invoke(DispatcherPriority.Send, ac);

                        }
                        catch (Exception ex)
                        {
                            error(et, ex);
                        }
                    }
                    break;
                case EventType.Lock:
                    if (GetLock != null)
                    {
                        try
                        {
                            LockEventArgs sea = new LockEventArgs
                            {
                                power = (string)detail.Values["power"] == "on",
                                iceid = (string)detail.Values["iceid"],
                            };
                            ac = delegate
                            {
                                GetLock(detail.Sender, sea);
                            };
                            _dispatcher.Invoke(DispatcherPriority.Send, ac);

                        }
                        catch (Exception ex)
                        {
                            error(et, ex);
                        }
                    }
                    break;
                case EventType.Force:
                    if (GetForce != null)
                    {
                        try
                        {
                            ForceEventArgs sea = new ForceEventArgs
                            {
                                iceid = (string)detail.Values["iceid"],
                                action = (ForeceCmdAction)Enum.Parse(typeof(ForeceCmdAction), (string)detail.Values["action"]),
                                guid = (string)detail.Values["guid"],
                                power = (string)detail.Values["power"] == "on",
                            };
                            ac = delegate
                            {
                                GetForce(detail.Sender, sea);
                            };
                            _dispatcher.Invoke(DispatcherPriority.Send, ac);

                        }
                        catch (Exception ex)
                        {
                            error(et, ex);
                        }
                    } break;
                case EventType.CCLinkState:
                    if (GetCCLink != null)
                    {
                        bool connect = false;
                        try
                        {
                            connect = (bool)detail.Values["connect"];
                            ac = delegate
                            {
                                GetCCLink(connect);
                            };
                            _dispatcher.Invoke(DispatcherPriority.Send, ac);
                        }
                        catch { }
                    }
                    break;
                case EventType.FailedToSendMsg:
                    if (FailedToSendMsg != null)
                    {
                        string msg = "";
                        try
                        {
                            msg = (string)detail.Values["msg"];
                            ac = delegate
                            {
                                FailedToSendMsg(msg);
                            };
                            _dispatcher.Invoke(DispatcherPriority.Send, ac);
                        }
                        catch { }
                    }
                    break;
                case EventType.RunApp:
                    if (GetRunApp != null)
                    {
                        try
                        {
                            //RunAppArgs sea = jss.Deserialize<RunAppArgs>(jss.Serialize(detail.Values["result"]));
                            RunAppArgs raa = new RunAppArgs
                            {
                                iceid = (string)detail.Values["iceid"],
                                key = (string)detail.Values["app"],
                            };
                            ac = delegate
                            {
                                GetRunApp(detail.Sender, raa);
                            };
                            _dispatcher.Invoke(DispatcherPriority.Send, ac);

                        }
                        catch (Exception ex)
                        {
                            error(et, ex);
                        }
                    }
                    break;
                case EventType.Raw:
                    if (GetRaw != null)
                    {
                        try
                        {
                            string body = jss.Serialize(detail.Values);
                            ac = delegate
                            {
                                GetRaw(body);
                            };
                            _dispatcher.Invoke(DispatcherPriority.Send, ac);
                        }
                        catch { }
                    }
                    break;
                case EventType.OtherCmd:
                    if (GetOtherCmd != null)
                    {
                        try
                        {
                            OtherCmd oc = jss.Deserialize<OtherCmd>(jss.Serialize(detail.Values));
                            ac = delegate
                            {
                                GetOtherCmd(oc);
                            };
                            _dispatcher.Invoke(DispatcherPriority.Send, ac);
                        }
                        catch { }
                    }
                    break;
                default:
                    break;
            }
        }
        static readonly string PROGRESS = "p";
        static Action<Dictionary<string, object>> popAction(string evidence)
        {
            Action<Dictionary<string, object>> ac = null;
            try
            {
                Monitor.Enter(_delegateDict);
                ac = _delegateDict[evidence];
                _delegateDict.Remove(evidence);
                _delegateDict.Remove(string.Concat(PROGRESS, evidence));
            }
            catch { }
            finally
            {
                Monitor.Exit(_delegateDict);
            }
            return ac;
        }
        static string pushAction(Action<Dictionary<string, object>> ac)
        {
            string guid = "";
            try
            {
                Monitor.Enter(_delegateDict);
                guid = Guid.NewGuid().ToString();
                _delegateDict[guid] = ac;
            }
            catch { }
            finally
            {
                Monitor.Exit(_delegateDict);
            }
            return guid;
        }
        static void pushProgressAction(string evidence, Action<Dictionary<string, object>> ac)
        {
            try
            {
                Monitor.Enter(_delegateDict);
                _delegateDict[string.Concat(PROGRESS, evidence)] = ac;
            }
            catch { }
            finally
            {
                Monitor.Exit(_delegateDict);
            }
        }
        static Action<Dictionary<string, object>> useProgressAction(string evidence)
        {
            Action<Dictionary<string, object>> ac = null;
            try
            {
                Monitor.Enter(_delegateDict);
                ac = _delegateDict[evidence];
            }
            catch { }
            finally
            {
                Monitor.Exit(_delegateDict);
            }
            return ac;
        }
        static void removeAction(string evidence)
        {
            try
            {
                Monitor.Enter(_delegateDict);
                _delegateDict.Remove(evidence);
            }
            catch { }
            finally
            {
                Monitor.Exit(_delegateDict);
            }
        }

        private static void invokeMethod(Delegate ac, Dictionary<string, object> p)
        {
            _dispatcher.Invoke(DispatcherPriority.Send, ac, p);
        }

        #endregion

        public static string GetStartTime()
        {
            IntPtr hwnd = FindWindow(null, BC_TITLE);
            string res = "";
            if (hwnd != IntPtr.Zero)
            {
                int pid = 0;
                GetWindowThreadProcessId(hwnd, out pid);
                if (pid > 0)
                {
                    var pc = Process.GetProcessById(pid);
                    res = pc.StartTime.ToString();
                }
            }
            return res;
        }
        #region << 发送消息
        [DllImport("user32")]
        public static extern int GetWindowThreadProcessId(IntPtr handle, out int pid);


        static bool sendInterprocessMsg(CmdType ct, string evidence, Dictionary<string, object> value, Dictionary<string, object> msg)
        {
            IntPtr hwnd = FindWindow(null, BC_TITLE);
            if (hwnd == IntPtr.Zero)
            {
                if (ServerDown != null)
                {
                    ServerDown();
                }
                return false;
            }
            else
            {
                InterprocessStruct interP = new InterprocessStruct()
                {
                    Sender = WIN_TITLE,
                    Evidence = evidence,
                    CmdType = ((int)ct).ToString(),
                    Values = value,
                    Msgs = msg,
                };
                JavaScriptSerializer jss = new JavaScriptSerializer();
                jss.MaxJsonLength = int.MaxValue;
                var str = jss.Serialize(interP);
                sendMessage(hwnd, str);
                return true;
            }
        }

        static void sendMessage(IntPtr hwnd, string strMsg)
        {
            CopyDataStruct cds;
            cds.dwData = IntPtr.Zero;
            cds.lpData = strMsg;

            //注意：长度为字节数
            cds.cbData = System.Text.Encoding.Default.GetBytes(strMsg).Length + 1;

            SendMessage(hwnd, WM_COPYDATA, 0, ref cds);
        }

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage
        (
            IntPtr hWnd,                  //目标窗体句柄
            int Msg,                      //WM_COPYDATA
            int wParam,                   //自定义数值
            ref  CopyDataStruct lParam    //结构体
        );

        [StructLayout(LayoutKind.Sequential)]
        public struct CopyDataStruct
        {
            public IntPtr dwData;
            public int cbData;//字符串长度
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;//字符串
        }
        const int WM_COPYDATA = 0x004A;

        #endregion

        #endregion

        public static void FitImageToContainer(
            double containerWidth, double containerHeight,
            int imageWidth, int imageHeight,
            out int fitWidth, out int fitHeight)
        {
            fitWidth = imageWidth;
            fitHeight = imageHeight;
            double scale = Math.Min(containerHeight / imageHeight, containerWidth / imageWidth);
            if (scale < 1)
            {
                fitHeight = (int)(scale * imageHeight);
                fitWidth = (int)(scale * imageWidth);
            }
        }
        public static string COM_PKG_NAME = "交流";

        //internal static void FitImageToContainer(
        //    double containerWidth, double containerHeight,
        //    double imageWidth, double imageHeight,
        //    out double fitWidth, out double fitHeight)
        //{
        //    fitWidth = imageWidth;
        //    fitHeight = imageHeight;
        //    double scale = Math.Min(containerHeight / imageHeight, containerWidth / imageWidth);
        //    if (scale < 1)
        //    {
        //        fitHeight = (scale * imageHeight);
        //        fitWidth = (scale * imageWidth);
        //    }
        //}
    }

    public static class StructCoverter
    {
        static Dictionary<EventType, char> SUBSCRIBE_DIC = new Dictionary<EventType, char>()
        {
            {EventType.Reset,'0'},
            {EventType.PushResource,'1'},
            {EventType.Command,'2'},
            {EventType.ResourceUpdate,'3'},
            {EventType.SupportExtend,'4'},
            {EventType.SurportPassword,'5'},
            {EventType.LinkState,'6'},
            {EventType.Error,'7'},
            {EventType.Sync,'8'},
            {EventType.Follow,'9'},
            {EventType.Symbol,'a'},
            {EventType.UserMsg,'b'},
            {EventType.Approaching,'c'},
       
                    //3.4
            {EventType.Lock,'d'},
            {EventType.Force,'e'},     
            {EventType.CCLinkState,'f'} , 
            {EventType.FailedToSendMsg,'g'},
            {EventType.RunApp,'h'},
            {EventType.Raw,'i'},
            {EventType.OtherCmd,'j'},
        };
        public static char ToSubscriptionCode(this EventType et)
        {
            return SUBSCRIBE_DIC[et];
        }
        internal static ServerInfoStruct ToServerInfo(Dictionary<string, object> dic)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            if (string.IsNullOrEmpty((string)dic["Power"]))
            {
                dic["Power"] = 1;
            }
            ServerInfoStruct sif = jss.Deserialize<ServerInfoStruct>(jss.Serialize(dic));
            return sif;
        }

        public static string[] ToSendList(Dictionary<string, object> dic)
        {
            List<string> tsl = new List<string>();
            JavaScriptSerializer jss = new JavaScriptSerializer();
            try
            {
                var upr = jss.Deserialize<Dictionary<string, string>[]>(jss.Serialize(dic[UserTag.cansend.ToString()]));
                if (upr != null && upr.Length > 0)
                {
                    foreach (var item in upr)
                    {
                        if (item["type"] == DEVICE)
                        {
                            tsl.Add(item["tvmid"]);
                        }
                    }
                }
            }
            catch { }
            return tsl.ToArray();
        }
        public const string DEVICE = "1";

        public static string[] ToRecvList(Dictionary<string, object> dic)
        {
            List<string> tsl = new List<string>();
            JavaScriptSerializer jss = new JavaScriptSerializer();
            try
            {
                var upr = jss.Deserialize<Dictionary<string, string>[]>(jss.Serialize(dic[UserTag.canrecv.ToString()]));
                if (upr != null && upr.Length > 0)
                {
                    foreach (var item in upr)
                    {
                        if (item["type"] == DEVICE)
                        {
                            tsl.Add(item["tvmid"]);
                        }
                    }
                }
            }
            catch { }
            return tsl.ToArray();
        }

        public static ResourceContainerStruct ToUploadResult(Dictionary<string, object> dic)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            jss.MaxJsonLength = int.MaxValue;
            var upr = jss.Deserialize<ResourceContainerStruct>(jss.Serialize(dic["result"]));
            return upr;
        }

        public static ResourceContainerStruct ToPrivatePkg(Dictionary<string, object> dic)
        {
            return ToUploadResult(dic);
        }

        public static Dictionary<string, ResourceContainerStruct> ToAllPkg(Dictionary<string, object> dic)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            jss.MaxJsonLength = int.MaxValue;
            var upr = jss.Deserialize<Dictionary<string, ResourceContainerStruct>>(jss.Serialize(dic["result"]));
            return upr;
        }

        public static ResourceContainerStruct[] ToSearchResult(Dictionary<string, object> dic)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            jss.MaxJsonLength = int.MaxValue;
            var upr = jss.Deserialize<ResourceContainerStruct[]>(jss.Serialize(dic["result"]));
            return upr;
        }

        public static ResourceContainerStruct[] ToAutoPlayList(Dictionary<string, object> dic)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            var upr = jss.Deserialize<ResourceContainerStruct[]>(jss.Serialize(dic["autoplay"]));
            return upr;
        }

        public static ResourceType ToResourceType(string filePath)
        {
            ResourceType ret = ResourceType.TEXT;
            #region <<
            try
            {
                var ext = Path.GetExtension(filePath).ToLower();
                switch (ext)
                {
                    case ".jpg":
                    case ".jpeg":
                    case ".bmp":
                    case ".png":
                    case ".tif":
                    case ".gif":
                        ret = ResourceType.IMAGE;
                        break;
                    case ".avi":
                    case ".wmv":
                    case ".mpg":
                    case ".mov":
                    case ".asf":
                    case ".mp4":
                    case ".3gp":
                    case ".ts":
                    case ".mkv":
                    case ".mp3":
                        ret = ResourceType.VIDEO;
                        break;
                    case ".ppt":
                    case ".pptx":
                    case ".pps":
                        ret = ResourceType.PPT;
                        break;
                    case ".pdf":
                        ret = ResourceType.PDF;
                        break;
                    case ".doc":
                    case ".docx":
                        ret = ResourceType.WORD;
                        break;
                    default:
                        break;
                }
            }
            catch { }
            #endregion
            return ret;
        }


        internal static double ToUploadProgress(Dictionary<string, object> dic)
        {
            return double.Parse(dic["progress"].ToString());
        }

        internal static double ToCacheProgress(Dictionary<string, object> dic)
        {
            return double.Parse(dic["progress"].ToString());
        }

        public static Dictionary<string, ResourceStruct> ToCacheResult(Dictionary<string, object> dic)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            jss.MaxJsonLength = int.MaxValue;
            var upr = jss.Deserialize<Dictionary<string, ResourceStruct>>(jss.Serialize(dic["result"]));
            return upr;
        }

        internal static bool ToPkgNameAvailable(Dictionary<string, object> dic)
        {
            return (bool)dic["available"];
        }

        internal static bool ToUploadUserInfoSucceeded(Dictionary<string, object> dic)
        {
            return (bool)dic["success"];
        }

        internal static UserInfoStruct ToUserInfo(Dictionary<string, object> dic)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            var upr = jss.Deserialize<UserInfoStruct>(jss.Serialize(dic["result"]));
            return upr;
        }

        internal static UserInfoStruct[] ToUserInfoArray(Dictionary<string, object> dic, UserTag userTag)
        {
            try
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();
                var upr = jss.Deserialize<UserInfoStruct[]>(jss.Serialize(dic[userTag.ToString()]));
                return upr;
            }
            catch { }
            return null;
        }

        internal static CCLinkState ToCCLinkeState(Dictionary<string, object> dic)
        {
            CCLinkState state = CCLinkState.OffLine;
            try
            {
                state = (CCLinkState)dic["state"];
            }
            catch { }
            return state;
        }
    }

    public enum CCLinkState { Online = 1, OffLine }
    public enum EventType
    {
        Reset = 0,
        PushResource,
        Command,
        ResourceUpdate,
        SupportExtend,
        SurportPassword,
        LinkState,
        Error,
        Sync,
        Follow,
        Symbol,
        UserMsg,
        Approaching,

        //3.4
        Lock,
        Force,
        CCLinkState,
        FailedToSendMsg,
        RunApp,
        Raw,
        OtherCmd,
    }

    public class ResourceContainerStruct
    {
        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                try
                {
                    RCType = (ResourceContainerType)Enum.Parse(typeof(ResourceContainerType), this.Type);
                }
                catch
                {
                    RCType = ResourceContainerType.TEXT;
                }
            }
        }
        string _type;
        public string PackageName { get; set; }
        public List<ResourceStruct> Datas { get; set; }
        public string Thumb { get; set; }
        public PowerColor Power { get; set; }
        public ResourceContainerType RCType { get; set; }
        public object TmpProperty { get; set; }

        internal ResourceContainerStruct Clone()
        {
            ResourceContainerStruct rs = new ResourceContainerStruct()
            {
                PackageName = PackageName,
                Power = Power,
                _type = _type,
                RCType = RCType,
                Thumb = Thumb,
                TmpProperty = TmpProperty,
            };
            try
            {
                rs.Datas = new List<ResourceStruct>(Datas.Count);
                rs.Datas.AddRange(Datas);
            }
            catch { }
            return rs;
        }
    }
    public enum PowerColor
    {
        Green = 1, Yellow, Red
    }

    public enum ControlCmd
    {
        UP, DOWN, LEFT, RIGHT, FIRST, LAST, FORWARD, BACKWARD, ZOOMIN, ZOOMOUT, OK, CANCEL, FUNCTION1, FUNCTION2, TOUCH, MENU, HOME
    }

    public class ControlCmdStruct
    {
        public ControlCmd CmdType { get; set; }
        public string Data { get; set; }
    }

    public class ResourceStruct
    {
        public string ID { get; set; }
        public string Type
        {
            get { return _typeStr; }
            set
            {
                _typeStr = value;
                try
                {
                    RType = (ResourceType)Enum.Parse(typeof(ResourceType), this.Type);
                }
                catch
                {
                    RType = ResourceType.TEXT;
                }
            }
        }
        string _typeStr;
        public string Title { get; set; }
        public string Description { get; set; }
        public string Tag { get; set; }
        public string Source { get; set; }
        public ResourceType RType { get; set; }
        public string OriginalSource { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string SubmitTime { get; set; }
        public string OwnerTvmID { get; set; }

        public object TmpProperty { get; set; }

        public static string GetThumb(string source)
        {
            return GetThumb(400, 300, ThumbMethod.Resample, source);
        }

        /// <summary>
        /// 取缩略图
        /// </summary>
        /// <param name="width">缩略图高</param>
        /// <param name="height">缩略图宽</param>
        /// <param name="crop">缩略方法</param>
        /// <returns>缩略图路径</returns>
        public static string GetThumb(int width, int height, ThumbMethod method, string source)
        {
            string temp = "";
            if (!string.IsNullOrEmpty(source))
            {
                if (File.Exists(source))//虚拟中控 缓存
                {
                    temp = string.Concat(source, ".jpg");
                }
                else //中控网络资源
                {
                    try
                    {
                        temp = Path.GetExtension(source);
                        string tmstr = "";
                        switch (method)
                        {
                            case ThumbMethod.Resample:
                                tmstr = "_";
                                break;
                            case ThumbMethod.Crop:
                                tmstr = "x";
                                break;
                            default:
                                tmstr = "_";
                                break;
                        }
                        temp = string.Format("{0}.jpg{1}{2}_{3}.jpg", source.Substring(0, source.LastIndexOf(temp)),
                            tmstr, width, height);
                    }
                    catch { }
                }
            }
            return temp;
        }

        string _thumb;
        /// <summary>
        /// 取默认缩略图
        /// </summary>
        /// <returns></returns>
        public string GetThumb()
        {
            if (string.IsNullOrEmpty(_thumb))
            {
                _thumb = GetThumb(400, 300, ThumbMethod.Resample);
            }
            return _thumb;
        }

        public string GetThumb(int width, int height, ThumbMethod method = ThumbMethod.Resample)
        {
            return GetThumb(width, height, method, Source);
        }

        public enum ThumbMethod
        {
            Resample, Crop,
        }


        internal ResourceStruct Clone()
        {
            var rs = new ResourceStruct
                {
                    Description = this.Description,
                    Height = this.Height,
                    ID = this.ID,
                    OriginalSource = this.OriginalSource,
                    OwnerTvmID = this.OwnerTvmID,
                    RType = this.RType,
                    Source = this.Source,
                    SubmitTime = this.SubmitTime,
                    Tag = this.Tag,
                    Title = this.Title,
                    Type = this.Type,
                    Width = this.Width,
                };
            return rs;
        }
    }

    public class ServerInfoStruct
    {
        public ServerLinkState State { get; set; }
        public string VirtualPath { get; set; }
        public PowerColor Power { get; set; }
        public string CCIP { get; set; }
        public bool IsDevice { get; set; }
        public string TvmID { get; set; }
        public string ICEName { get; set; }
        public CCState StateOfCC { get; set; }
        public string IceID { get; set; }
    }

    public enum CCState
    {
        None = -1, NotStarted, SearchingICE, Finding, Found, WaitingPassword, LoggingIn, Initializing, ClearingCache, Succeed
    }

    public class ControlCmdEventArgs : EventArgs
    {
        public ControlCmdStruct OneCmd { get; set; }
        public ControlCmd[] MultiCmds { get; set; }
        public string iceid { get; set; }
    }

    public class ResourceUpdateEventArgs : EventArgs
    {
        public ResourceAction Action { get; set; }
        public ResourceContainerStruct Data { get; set; }
    }

    public enum ResourceAction
    {
        Update,
        AddFile,
        AddPackage,
        [Obsolete("3.4后没有再使用了.")]
        DeleteFile,
        DeletePackage,
    }

    #region <<
    public enum PushMediaType
    {
        Other = 0, Resource, Stream, Web,
    }
    public enum ServerLinkState
    {
        /// <summary>
        /// 无连接
        /// </summary>
        None = 0,
        /// <summary>
        /// 真空控,在线
        /// </summary>
        Online,
        /// <summary>
        /// 虚拟中控在线
        /// </summary>
        Virtual
    }

    public struct PushResourceData
    {
        public string id { get; set; }
        /// <summary>
        /// 位置参数
        /// </summary>
        public string position { get; set; }
    }
    public struct PushWebData
    {
        public string url { get; set; }
        public string title { get; set; }
    }
    public class PushResourceConainer
    {
        public string position { get; set; }
        public ResourceStruct data { get; set; }
    }
    public class PushStreamData
    {
        public string url { get; set; }
        public bool start { get; set; }
        public string title { get; set; }
        public string token { get; set; }
    }

    public class PushMediaEventArgs : EventArgs
    {
        public PushMediaType Type { get; set; }
        /// <summary>
        /// 中控资源列表
        /// </summary>
        public PushResourceConainer[] Resource { get; set; }

        public PushStreamData Stream { get; set; }

        public PushWebData[] Web { get; set; }

        public string iceid { get; set; }

        /// <summary>
        /// 指定出现方向,从X轴到出现方向的夹角,从右入为0度,上为90度,下为-90/270,左为180/-180
        /// </summary>
        public int direction { get; set; }
    }

    public class ExtendEventArgs : EventArgs
    {
        public string Msg { get; set; }
        public string iceid { get; set; }
    }

    public class LinkStateEventArgs : EventArgs
    {
        public ServerLinkState LinkState { get; set; }
    }

    public class UserMsgEventArgs : ExtendEventArgs
    {
    }
    #endregion

    public enum ServerError
    {
        /// <summary>
        /// 未定义的错误
        /// </summary>
        None = 0,
        /// <summary>
        /// 无法取得中控信息
        /// </summary>
        NoCCInfo,
        /// <summary>
        /// 尚未找到指定中控
        /// </summary>
        NoRequiredCC,
        /// <summary>
        /// 中控登录密码不正确
        /// </summary>
        CCPasswordError,
        /// <summary>
        /// 中控拒绝接入
        /// </summary>
        CCDenyAccess,
        /// <summary>
        /// 中控初始化失败
        /// </summary>
        CCInitializeError,
    }

    public class ServerErrorEventArgs : EventArgs
    {
        public ServerError ErrorType { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class SyncEventArgs : EventArgs
    {
        public string PkgName { get; set; }
        public string Guid { get; set; }
        public string Tag { get; set; }
        public string iceid { get; set; }
    }

    public class FollowEventArgs : EventArgs
    {
        /// <summary>
        /// 包名
        /// </summary>
        public string pkg { get; set; }

        /// <summary>
        /// 资源guid
        /// </summary>
        public string guid { get; set; }

        /// <summary>
        /// 发起者TVMID
        /// </summary>
        public string from { get; set; }
        /// <summary>
        /// 发起者的ICEID
        /// </summary>
        public string iceid { get; set; }

        /// <summary>
        /// 命令的接力ICEID
        /// </summary>
        public string relay { get; set; }
        /// <summary>
        /// 路径
        /// </summary>
        public string source
        {
            get { return _source; }
            set
            {
                _source = value;
                rtype = StructCoverter.ToResourceType(_source);
            }
        }
        string _source;
        /// <summary>
        /// 缩略图
        /// </summary>
        public string thumb { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public ResourceType rtype { get; set; }
        /// <summary>
        /// 视频起始播放时间点
        /// </summary>
        public string position { get; set; }

    }

    public class SymbolEventArgs : EventArgs
    {
        /// <summary>
        /// 有效值为0-9 A-D
        /// </summary>
        public char symbol { get; set; }
        public string iceid { get; set; }
        public string questionguid { get; set; }
    }

    public enum ResourceContainerType
    {
        TEXT = 0, PPT, WORD, PDF, IMAGE, VIDEO, MIX, COMMUNICATION, KEYNOTE
    }
    public enum ResourceType
    {
        TEXT = 0, PPT, WORD, PDF, IMAGE, VIDEO, KEYNOTE
    }

    public class UserInforUpload : EventArgs
    {
        /// <summary>
        /// 移动电话
        /// </summary>
        public String mobile;

        /// <summary>
        /// 天脉号
        /// </summary>
        public String tvmid;

        /// <summary>
        /// 应用名
        /// </summary>
        public String appname;

        /// <summary>
        /// 公司名
        /// </summary>
        public String company;

        /// <summary>
        /// 电子邮件
        /// </summary>
        public String email;

        /// <summary>
        /// 头像路径
        /// </summary>
        public String face;

        /// <summary>
        /// 用户昵称
        /// </summary>
        public String nickname;

        /// <summary>
        /// 拼音
        /// </summary>
        public string spell;

        /// <summary>
        /// value
        /// </summary>
        public String value;

        /// <summary>
        /// key
        /// </summary>
        public String key;

    }

    public class UserInfoStruct : UserInforUpload
    {
        /// <summary>
        /// 天脉号
        /// </summary>
        public String iceid;

        /// <summary>
        /// 提交日期
        /// </summary>
        public String submit_date;

        /// <summary>
        /// 这个用户的类型
        /// </summary>
        public String type;

        /// <summary>
        /// 组号
        /// </summary>
        public string group_id;

        public PowerColor Power
        {
            get
            {
                int it = 0;
                try
                {
                    it = int.Parse(group_id);
                }
                catch
                {
                }
                return (PowerColor)it;
            }
        }
        public bool IsDevice { get { return type == StructCoverter.DEVICE; } }
    }
    public enum ForeceCmdAction
    {
        answer, follow
    }

    public class LockEventArgs : EventArgs
    {
        public bool power { get; set; }
        public string iceid { get; set; }
    }
    public class ForceEventArgs : EventArgs
    {
        public bool power { get; set; }
        public ForeceCmdAction action { get; set; }
        public string guid { get; set; }
        public string iceid { get; set; }
    }
    public class RunAppArgs : EventArgs
    {
        public string iceid { get; set; }
        public string key { get; set; }
    }
    public enum HandleCC
    {
        Current, Virtual, Both,
    }
    public enum UserTag
    {
        cansend, canrecv
    }

    public class PackageResourceOrder
    {
        public PackageResourceOrder()
        {
            OrderInfo = new Dictionary<string, int>();
        }
        public string PackageName { get; set; }
        public Dictionary<string, int> OrderInfo { get; set; }
    }

    public class OtherCmd
    {
        public string cmdType { get; set; }
        public string cmdBody { get; set; }
        public string iceid { get; set; }
    }
}
