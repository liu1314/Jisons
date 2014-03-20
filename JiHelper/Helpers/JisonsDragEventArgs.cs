/* 迹I柳燕
 * 
 * FileName:   JisonsDragEventArgs.cs
 * Version:    1.0
 * Date:       2014.03.20
 * Author:     Ji
 * 
 *========================================
 * @namespace  Jisons 
 * @class      JisonsDragEventArgs
 * @extends    
 * 
 *             对于 DragEventArgs 数据的扩展处理
 * 
 *========================================
 * Hi,小喵喵...
 * Copyright © 迹I柳燕
 * 
 * 转载请保留...
 * 
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Jisons
{
    public static class JisonsDragEventArgs
    {

        /// <summary> 从 DragEventArgs 中获取拖拽的数据 </summary>
        /// <param name="args"> 拖拽的参数 </param>
        /// <returns> 查询到的数据 </returns>
        public static IList<object> GetDatas(this DragEventArgs args)
        {
            List<object> datas = new List<object>();

            var data = (DataObject)args.Data;
            var dataFormateList = data.GetFormats();
            if (dataFormateList.Count() > 0)
            {
                var dataList = data.GetData(dataFormateList[0]) as IEnumerable;
                IEnumerator enumerator = dataList.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current != null)
                    {
                        datas.Add(enumerator.Current);
                    }
                }
            }

            return datas;
        }

        /// <summary> 从 DragEventArgs 中获取指定类型的拖拽数据 </summary>
        /// <typeparam name="T"> 获取的数据类型 </typeparam>
        /// <param name="args"> 拖拽的参数 </param>
        /// <returns> 查询到的数据 </returns>
        public static IList<object> GetDatas<T>(this DragEventArgs args)
        {
            List<object> datas = new List<object>();

            var data = (DataObject)args.Data;
            var dataFormateList = data.GetFormats();
            if (dataFormateList.Count() > 0)
            {
                var dataList = data.GetData(dataFormateList[0]) as IEnumerable;
                IEnumerator enumerator = dataList.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current != null && enumerator.Current is T)
                    {
                        datas.Add((T)enumerator.Current);
                    }
                }
            }

            return datas;
        }

    }
}
