using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Jisons
{
    public static class JisonsFindClassInfo
    {

        /// <summary> 从当前类型一直向上查找指定名称的字段 </summary>
        /// <param name="type"> 查找的起始类型 </param>
        /// <param name="fieldName"> 字段的名称 </param>
        /// <param name="bindingAttr"> 搜索的标志 </param>
        /// <returns> 返回查询到的  FieldInfo </returns>
        public static FieldInfo FindField(this Type type, string fieldName, BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                var field = type.GetField(fieldName, bindingAttr);
                if (field != null)
                {
                    return field;
                }
                else if (type.BaseType != null)
                {
                    return type.BaseType.FindField(fieldName, bindingAttr);
                }
            }
            return null;
        }

        /// <summary> 查询当前程序集所拥有的指定类型字段字段元数据集合 </summary>
        /// <typeparam name="T"> 指定查询的类型 </typeparam>
        /// <param name="assembly"> 查选的程序集 </param>
        /// <param name="bindingAttr"> 查询的指定参数 </param>
        /// <returns> 查询到的字段元数据集合 </returns>
        public static IList<FieldInfo> FindFieldInAssembly<T>(this Assembly assembly, BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic) where T : class
        {
            IList<FieldInfo> datas = new List<FieldInfo>();

            var classTypes = assembly.GetTypes();
            foreach (var item in classTypes)
            {
                var fields = item.GetFields(bindingAttr);
                if (fields.Count() > 0)
                {
                    var fieldType = typeof(T);
                    //此处在大量数据时曾试过并行获取 Parallel.ForEach
                    //不幸的发现会更慢
                    foreach (var field in fields)
                    {
                        if (field.FieldType.Equals(fieldType))
                        {
                            datas.Add(field);
                        }
                    }
                }
            }

            return datas;
        }

        /// <summary> 查询当前程序集所拥有的指定类型静态字段集合 </summary>
        /// <typeparam name="T"> 指定查询的类型 </typeparam>
        /// <param name="assembly"> 查选的程序集 </param>
        /// <param name="bindingAttr"> 查询的指定参数 </param>
        /// <returns>查询到指定静态类型的值集合</returns>
        public static IList<T> FindStaticFieldValueInAssembly<T>(this Assembly assembly, BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.Public) where T : class
        {
            IList<T> datas = new List<T>();

            var fieldList = assembly.FindFieldInAssembly<T>(bindingAttr);
            if (fieldList.Count > 0)
            {
                //此处在大量数据时曾试过并行获取 Parallel.ForEach
                //不幸的发现会更慢
                fieldList.ForEach(i =>
                {
                    var data = i.GetValue(null) as T;
                    if (data != null)
                    {
                        datas.Add(data);
                    }
                });
            }

            return datas;
        }

    }
}
