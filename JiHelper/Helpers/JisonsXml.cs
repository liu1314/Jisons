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
 * 
 * Copyright © 迹I柳燕
 * 
 * 转载请保留...
 * 
 */


using System;
using System.IO;

namespace Jisons
{
    /// <summary>
    /// 只支持class类型
    /// 因为此项在struct返回默认的之后无法判断是否成功
    /// </summary>
    /// <typeparam name="C"></typeparam>
    public static class JisonsXmlOfClass<C> where C : class
    {

        /// <summary> 读取数据 </summary>
        /// <param name="FullPath">包含数据的文件路径</param>
        /// <returns>C ， 返回为null的时候读取失败</returns>
        public static C ReadData(string FullPath)
        {
            return JisonsXml.Read(FullPath, typeof(C)) as C;
        }

        /// <summary> 读取数据 </summary>
        /// <param name="stream">包含数据的数据流</param>
        /// <returns>C ， 返回为null的时候读取失败</returns>
        public static C ReadData(Stream stream)
        {
            return JisonsXml.Read(stream, typeof(C)) as C;
        }

        /// <summary> 写入数据</summary>
        /// <param name="obj">将要写入的数据</param>
        /// <param name="fullpath">写入的文件路径</param>
        /// <returns>返回为null的时候写入成功</returns>
        public static string WriteData(C obj, string fullpath)
        {
            try
            {
                JisonsXml.Save(fullpath, obj);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }

    /// <summary>
    /// struct类型进行读取时，不确保一定成功
    /// 如果读取不成功会返回default(S)
    /// </summary>
    /// <typeparam name="S"></typeparam>
    public static class JisonsXmlOfStruct<S> where S : struct
    {

        /// <summary> 读取数据 ，返回为null的时候读取失败 </summary>
        /// <param name="FullPath"></param>
        /// <returns></returns>
        public static S Read(string FullPath)
        {
            var data = JisonsXml.Read(FullPath, typeof(S));
            return data != null ? (S)data : default(S);
        }

        /// <summary>  </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static S Read(Stream stream)
        {
            var data = JisonsXml.Read(stream, typeof(S));
            return data != null ? (S)data : default(S);
        }

        /// <summary> 写入数据,返回为null的时候写入成功 </summary>
        /// <param name="obj"></param>
        /// <param name="fullpath"></param>
        /// <returns></returns>
        public static string Write(S obj, string fullpath)
        {
            try
            {
                JisonsXml.Save(fullpath, obj);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }

    public static class JisonsXml
    {
        public static void Save(string filePath, object obj)
        {
            Save(filePath, obj, obj.GetType());
        }

        public static void Save(string filePath, object obj, System.Type type)
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath))
            {
                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(type);
                xs.Serialize(writer, obj);
                writer.Close();
            }
        }

        public static object Read(string filePath, System.Type type)
        {
            if (!System.IO.File.Exists(filePath))
                return null;

            using (System.IO.StreamReader reader = new System.IO.StreamReader(filePath))
            {
                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(type);
                object obj = xs.Deserialize(reader);
                reader.Close();
                return obj;
            }
        }

        public static object Read(Stream stream, System.Type type)
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