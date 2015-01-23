//*-------------------------------------------------------------------
// Copyright (C) 天脉聚源传媒科技有限公司
// 版权所有。
//
// 文件名：IniFile.cs
// 文件功能描述：配置文件类
// 创建标识：Allen.H 2010/09 , hulunfu@tvmining.com
//--------------------------------------------------------------------- * /

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections;

namespace TVMWPFLab.Utils
{
    /// <summary>
    /// ini类型文件操作类
    /// </summary>
    public class IniFile
    {
        #region << 声明变量

        //文件ini名称 
        private string CONFIG_PATH = AppDomain.CurrentDomain.BaseDirectory + "config.cfg";
        //单条长度
        private int iSingle = 255 * 10;
        //片段长度
        private int iSection = 65535 * 10;
        //整篇长度
        //private int iWhole = 32767;
        //长度过短异常提示
        private string sErrorTooSmallBuffer = "buffer size too small";

        #endregion

        #region << 声明读写ini文件的api函数

        [DllImport("kernel32", EntryPoint = "GetPrivateProfileString")]
        private static extern int GetPrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpDefault,
            StringBuilder lpReturnedString,
            int nSize,
            string lpFileName);

        [DllImport("kernel32", EntryPoint = "WritePrivateProfileString")]
        private static extern bool WritePrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpString,
            string lpFileName);

        [DllImport("kernel32", EntryPoint = "GetPrivateProfileSection")]
        private static extern int GetPrivateProfileSection(
            string lpAppName,
            byte[] lpReturnedString,
            int nSize,
            string lpFileName);

        [DllImport("kernel32", EntryPoint = "WritePrivateProfileSection")]
        private static extern bool WritePrivateProfileSection(
          string lpAppName,
          string lpString,
          string lpFileName);

        [DllImport("kernel32", EntryPoint = "WritePrivateProfileSection")]
        private static extern bool WritePrivateProfileSection(
          string lpAppName,
          byte[] lpString,
          string lpFileName);

        [DllImport("kernel32", EntryPoint = "GetPrivateProfileSectionNames")]
        private static extern int GetPrivateProfileSectionNames(
            byte[] lpszReturnBuffer,
            int nSize,
            string lpFileName);

        #endregion

        #region <<  构造


        /// <summary>
        /// 默认构造

        /// </summary>
        public IniFile()
        {
            if (!File.Exists(CONFIG_PATH))
            {
                //throw new FileNotFoundException("配置文件\"" + CONFIG_PATH + "\"不存在!");
            }
        }

        /// <summary>
        /// 构造

        /// </summary>
        /// <param name="iniPath"></param>
        public IniFile(string cfgFileName)
        {
            CONFIG_PATH = AppDomain.CurrentDomain.BaseDirectory + cfgFileName;
            if (!File.Exists(CONFIG_PATH))
            {
                //throw new FileNotFoundException("配置文件\"" + CONFIG_PATH + "\"不存在!");
            }
        }

        #endregion

        #region << 对外接口

        /// <summary>
        /// 取得指定key值

        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(string section, string key)
        {
            StringBuilder sBuilder = new StringBuilder(iSingle);
            int result = GetPrivateProfileString(section, key, "", sBuilder, iSingle, this.CONFIG_PATH);
            if (result == iSingle - 1)
            {
                throw new Exception(sErrorTooSmallBuffer);
            }

            return sBuilder.ToString();
        }

        /// <summary>
        /// 设置指定key值

        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public bool SetValue(string section, string key, string value)
        {
            return WritePrivateProfileString(section, key, value, this.CONFIG_PATH);
        }

        /// <summary>
        /// 取得指定section
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public Hashtable GetSection(string section)
        {
            Hashtable item = new Hashtable();
            string key, value;
            byte[] buffer = new byte[iSection];
            StringBuilder sBuilder;
            string[] pair;

            int bufLen = GetPrivateProfileSection(section, buffer, buffer.GetUpperBound(0), this.CONFIG_PATH);
            string sSection = Encoding.Default.GetString(buffer);

            if (bufLen > 0)
            {
                sBuilder = new StringBuilder();
                for (int i = 0; i < bufLen; i++)
                {
                    if (sSection[i] != 0)
                    {
                        sBuilder.Append(sSection[i]);
                    }
                    else if (sBuilder.Length > 0)
                    {
                        pair = sBuilder.ToString().Split('=');

                        if (pair.Length > 1)
                        {
                            key = pair[0].Trim();
                            value = pair[1].Trim();

                            item.Add(key, value);
                        }

                        sBuilder = new StringBuilder();
                    }
                }
            }

            return item;
        }

        /// <summary>
        /// 取得指定section
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public List<string> GetINISection(String section)
        {
            List<string> items = new List<string>();
            byte[] buffer = new byte[iSection];
            int bufLen = GetPrivateProfileSection(section, buffer, buffer.GetUpperBound(0), this.CONFIG_PATH);
            if (bufLen > 0)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bufLen; i++)
                {
                    if (buffer[i] != 0)
                    {
                        sb.Append((char)buffer[i]);
                    }
                    else if (sb.Length > 0)
                    {
                        items.Add(sb.ToString());
                        sb = new StringBuilder();
                    }
                }
            }

            return items;
        }

        /// <summary>
        /// 设置指定section
        /// </summary>
        /// <param name="section"></param>
        /// <param name="hSection"></param>
        /// <returns></returns>
        public bool SetSection(string section, Hashtable hSection)
        {
            StringBuilder sBuilder = new StringBuilder();

            int i = 0;
            foreach (DictionaryEntry de in hSection)
            {
                sBuilder.Append(de.Key + "=" + de.Value);

                if (i < hSection.Count - 1)
                    sBuilder.Append("\r\n");

                i++;
            }

            return WritePrivateProfileSection(section, sBuilder.ToString(), this.CONFIG_PATH);
        }

        public bool SetSection2(string section, Hashtable hSection)
        {
            byte[] buffer = new byte[iSection];

            int index = 0;

            foreach (DictionaryEntry de in hSection)
            {
                foreach (byte bVaule in de.Key.ToString())
                {
                    buffer[index] = bVaule;
                    index++;
                }

                buffer[index] = 61;
                index++;

                foreach (byte bVaule in de.Value.ToString())
                {
                    buffer[index] = bVaule;
                    index++;
                }

                buffer[index] = 0;
                index++;
            }

            return WritePrivateProfileSection(section, buffer, this.CONFIG_PATH);
        }

        /// <summary>
        /// 取得所有section名

        /// </summary>
        /// <returns></returns>
        public List<string> GetSectionNames()
        {
            List<string> items = new List<string>();
            byte[] buffer = new byte[iSingle];
            int bufLen = GetPrivateProfileSectionNames(buffer, buffer.GetUpperBound(0), this.CONFIG_PATH);
            if (bufLen > 0)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bufLen; i++)
                {
                    if (buffer[i] != 0)
                    {
                        sb.Append((char)buffer[i]);
                    }
                    else if (sb.Length > 0)
                    {
                        items.Add(sb.ToString());
                        sb = new StringBuilder();
                    }
                }
            }

            return items;
        }

        #endregion
    }
}