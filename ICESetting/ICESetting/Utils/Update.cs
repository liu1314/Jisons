using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net;
using System.Web.Script.Serialization;
using System.IO;
using ICESetting.Utils;

//using System.ComponentModel;

namespace Tvm.WPF
{
    public class TvmUpdate
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="product">产品名</param>
        /// <param name="device">设备名</param>
        /// <param name="isTest">是否测试</param>
        /// <param name="version">指定版本,可选,默认时取当前程序的productVersion</param>
        public TvmUpdate(string product, string device, bool isTest = false, string version = null)
        {
            if (string.IsNullOrEmpty(product) || string.IsNullOrEmpty(device))
            {
                throw new ArgumentNullException("product device");
            }
            if (string.IsNullOrEmpty(version))
            {
                Version = GetDefaultVersion();
            }
            else
            {
                Version = version;
            }
            //_isTest = isTest;

            _checkUrl = string.Format(
                "{0}/SoftUpdateServer/update/update?product={1}&device={2}&version={3}_release&format=json{4}",
                SERVER_IP, product, device, Version, isTest ? "&releasetype=test" : "");
        }

        public string Version { get; private set; }

        public CheckResult Status { get; set; }

        public static string GetDefaultVersion()
        {
            return Process.GetCurrentProcess().MainModule.FileVersionInfo.ProductVersion;
        }

        /// <summary>
        /// 检查是否需要更新
        /// </summary>
        /// <param name="callback"></param>
        public void BeginCheck(Action<CheckResult, string> callback = null)
        {
            #region <<
            Status = CheckResult.None;
            using (System.ComponentModel.BackgroundWorker bw = new System.ComponentModel.BackgroundWorker())
            {
                bw.DoWork += (s, v) =>
                    {
                        v.Result = needUpdate();
                    };
                bw.RunWorkerCompleted += (s, v) =>
                    {
                        string msg = "";
                        if (v.Error == null)
                        {
                            try
                            {
                                msg = Data.msg;
                            }
                            catch { }

                            if ((bool)v.Result)
                            {
                                Status = CheckResult.HasNew;
                            }
                            else
                            {
                                Status = CheckResult.HasNotNew;
                            }
                            //else if (string.IsNullOrEmpty(msg))
                            //{
                            //    Status = CheckResult.HasNotNew;
                            //}
                            //else
                            //{
                            //    Status = CheckResult.Error;
                            //}
                        }
                        else
                        {
                            Status = CheckResult.TimeOut;
                            msg = v.Error.Message;
                        }

                        if (callback != null)
                        {
                            callback(Status, msg);
                        }
                    };
                bw.RunWorkerAsync();
            }
            #endregion
        }

        /// <summary>
        /// 下载新包到指定目录
        /// </summary>
        /// <param name="dirPath">指定下载目录</param>
        /// <param name="progress">下载进度</param>
        /// <param name="complete">下载结束</param>
        public void DownloadNewPack(string dirPath, Action<double> progress = null, Action<bool, string> complete = null)
        {
            if (!Directory.Exists(dirPath))
            {
                throw new DirectoryNotFoundException(dirPath);
            }
            if (Data != null && Data.status == UpdateStatus.SUCCESS && Data.versionlist != null && Data.versionlist.Length > 0)
            {
                WebClient wb = null;
                try
                {
                    string file = Data.versionlist[0].addr;
                    var downPath = Path.Combine(dirPath, string.Format("{0}_{1}", Utility.ToFileName(Data.versionlist[0].version), Path.GetFileName(file)));
                    if (File.Exists(downPath))
                    {
                        if (progress != null)
                        {
                            progress(1);
                        }
                        if (complete != null)
                        {
                            complete(true, downPath);
                        }
                    }
                    else
                    {
                        string tmpPath = string.Concat(downPath, ".tmp");
                        wb = new WebClient();
                        if (progress != null)
                        {
                            wb.DownloadProgressChanged += (s, v) =>
                            {
                                progress(v.ProgressPercentage * .01);
                            };
                        }

                        if (complete != null)
                        {
                            wb.DownloadFileCompleted += (s, v) =>
                            {
                                var suc = !v.Cancelled && v.Error == null;
                                if (suc)
                                {
                                    try
                                    {
                                        File.Move(tmpPath, downPath);
                                    }
                                    catch
                                    {
                                        suc = false;
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        File.Delete(tmpPath);
                                    }
                                    catch { }
                                }
                                if (complete != null)
                                {
                                    complete(suc, downPath);
                                }
                            };
                        }

                        //wb.DownloadFileAsync(new Uri(file), downPath);
                        wb.DownloadFileAsync(new Uri(file), tmpPath);
                    }
                }
                finally
                {
                    if (wb != null)
                    {
                        wb.Dispose();
                    }
                }
            }
            else
            {
                throw new DecoderFallbackException("Nothing to download");
            }
        }

        public string GetReleaseNote()
        {
            if (Data == null || Data.status == UpdateStatus.FAILED || Data.versionlist == null || Data.versionlist.Length < 1)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            for (int i = Data.versionlist.Length - 1; i >= 0; i--)
            {
                var item = Data.versionlist[i];
                sb.Append(item.version);
                sb.AppendLine(":");
                sb.AppendLine(item.describe);
            }
            return sb.ToString();
        }

        #region <<

        bool needUpdate()
        {
            bool res = false;
            string str = null;
            WebClient wc = new WebClient();
            try
            {
                var da = wc.DownloadData(_checkUrl);
                str = Encoding.UTF8.GetString(da);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (wc != null)
                {
                    wc.Dispose();
                }
            }

            if (!string.IsNullOrEmpty(str))
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();
                Data = jss.Deserialize<updateResult>(str);
                if (Data.status == UpdateStatus.SUCCESS)
                {
                    res = true;
                }
            }

            return res;
        }

        static readonly string SERVER_IP = "http://update.tvmining.com";
        //static readonly int TIME_OUT = 5000;

        static string _checkUrl;
        public updateResult Data { get; private set; }

        #region << 内部结构

        public class updateResult
        {
            /// <summary>
            /// 支持的设备
            /// </summary>
            public string device { get; set; }
            /// <summary>
            /// 产品名称
            /// </summary>
            public string product { get; set; }
            /// <summary>
            /// 成功或失败状态
            /// </summary>
            public UpdateStatus status { get; set; }

            /// <summary>
            /// 返回信息
            /// </summary>
            public string msg { get; set; }
            /// <summary>
            /// 各版本
            /// </summary>
            public vs[] versionlist { get; set; }
        }
        public enum UpdateStatus { FAILED, SUCCESS }
        public enum vsRule { optional, required }
        public enum vsRollback { no, yes }

        public class vs
        {
            public string version { get; set; }
            /// <summary>
            /// 是否强制更新
            /// </summary>
            public vsRule rule { get; set; }
            /// <summary>
            /// 是否回滚
            /// </summary>
            public vsRollback isrollback { get; set; }
            /// <summary>
            /// 回滚版本
            /// </summary>
            public string rollbackver { get; set; }
            /// <summary>
            /// 更新描述
            /// </summary>
            public string describe { get; set; }
            /// <summary>
            /// 更新包下载地址
            /// </summary>
            public string addr { get; set; }
            /// <summary>
            /// 免责声明
            /// </summary>
            public string disclaimer { get; set; }
        }
        #endregion

        #endregion

    }

    public enum CheckResult { None, HasNew, HasNotNew, TimeOut }
}
