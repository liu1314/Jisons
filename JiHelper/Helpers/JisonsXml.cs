/* 迹I柳燕
 * 
 * FileName:   JisonsXml.cs
 * Version:    1.0
 * Date:       2014.03.18
 * Author:     Ji
 * 
 *========================================
 * @namespace  Jisons 
 * @class      JisonsXml 
 *             JisonsXmlOfClass<C> where C : class 
 *             JisonsXmlOfStruct<S> where S : struct
 * @extends    
 * 
 *             对于Xml的处理
 * 
 *========================================
 * Hi,小喵喵...
 * Copyright © 迹I柳燕
 * 
 * 转载请保留...
 * 
 */

using System;
using System.IO;

namespace Jisons
{
    /// <summary> 只支持class类型 因为此项在struct返回默认的之后无法判断是否成功 </summary>
    public static class JisonsXmlOfClass
    {

        /// <summary> 从XML读取数据 </summary>
        /// <typeparam name="C">读取的数据类型</typeparam>
        /// <param name="fileInfo">包含数据的文件 FileInfo 信息</param>
        /// <returns>返回为null的时候读取失败</returns>
        public static C ReadFromXml<C>(this FileInfo fileInfo) where C : class
        {
            return ReadFromXml<C>(fileInfo.FullName);
        }

        /// <summary> 从XML读取数据 </summary>
        /// <typeparam name="C">读取的数据类型</typeparam>
        /// /// <param name="FullPath">包含数据的文件路径</param>
        /// <returns>C ， 返回为null的时候读取失败</returns>
        public static C ReadFromXml<C>(this string FullPath) where C : class
        {
            return JisonsXml.Read(FullPath, typeof(C)) as C;
        }

        /// <summary> 从XML读取数据 </summary>
        /// <typeparam name="C">读取的数据类型</typeparam>
        /// <param name="stream">包含数据的数据流</param>
        /// <returns>C ， 返回为null的时候读取失败</returns>
        public static C ReadFromXml<C>(this Stream stream) where C : class
        {
            return JisonsXml.Read(stream, typeof(C)) as C;
        }

        /// <summary> 写入数据到XML</summary>
        /// <typeparam name="C">读取的数据类型</typeparam>
        /// <param name="obj">将要写入的数据</param>
        /// <param name="fullpath">写入的文件路径</param>
        /// <returns>返回为null的时候写入成功</returns>
        public static string WriteToXml<C>(this C obj, string fullpath) where C : class
        {
            try
            {
                JisonsXml.Save(obj, fullpath);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }

    /// <summary> struct类型进行读取时，不确保一定成功 如果读取不成功会返回default(S) </summary>
    public static class JisonsXmlOfStruct
    {

        /// <summary> 从XML读取数据 </summary>
        /// <typeparam name="S">读取的数据类型</typeparam>
        /// <param name="fileInfo">包含数据的文件 FileInfo 信息</param>
        /// <returns>返回为默认值的时候读取失败</returns>
        public static S ReadFromXml<S>(this FileInfo fileInfo) where S : struct
        {
            return ReadFromXml<S>(fileInfo.FullName);
        }

        /// <summary> 从XML读取数据 </summary>
        /// <typeparam name="S">读取的数据类型</typeparam>
        /// <param name="fileInfo">包含数据的文件 FileInfo 信息</param>
        /// <returns>返回为默认值的时候读取失败</returns>
        public static S ReadFromXml<S>(this string FullPath) where S : struct
        {
            var data = JisonsXml.Read(FullPath, typeof(S));
            return data != null ? (S)data : default(S);
        }

        /// <summary> 从XML读取数据 ，返回为默认值的时候读取失败 </summary>
        /// <typeparam name="S">读取的数据类型</typeparam> 
        /// <param name="FullPath"></param>
        /// <returns></returns>
        public static S ReadFromXml<S>(this Stream stream) where S : struct
        {
            var data = JisonsXml.Read(stream, typeof(S));
            return data != null ? (S)data : default(S);
        }

        /// <summary> 写入数据到XML </summary>
        /// <typeparam name="S">写入的数据类型</typeparam> 
        /// <param name="obj"></param>
        /// <param name="fullpath">写入的路径</param>
        /// <returns>返回为null的时候写入成功</returns>
        public static string WriteToXml<S>(this S obj, string fullpath) where S : struct
        {
            try
            {
                JisonsXml.Save(obj, fullpath);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }

    /// <summary> 封装对与Xml读写操作的类 </summary>
    internal static class JisonsXml
    {

        internal static void Save(object obj, string filePath)
        {
            Save(obj, filePath, obj.GetType());
        }

        internal static void Save(object obj, string filePath, System.Type type)
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath))
            {
                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(type);
                xs.Serialize(writer, obj);
                writer.Close();
            }
        }

        internal static object Read(string filePath, System.Type type)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return null;
            }

            using (System.IO.StreamReader reader = new System.IO.StreamReader(filePath))
            {
                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(type);
                object obj = xs.Deserialize(reader);
                reader.Close();
                return obj;
            }
        }

        internal static object Read(Stream stream, System.Type type)
        {
            if (stream == null || stream.Length == 0)
            {
                return null;
            }

            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(type);
            object obj = xs.Deserialize(stream);
            return obj;
        }
    }
}