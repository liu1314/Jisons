/* 
 * 
 * FileName:   JisonsIEnumerable.cs
 * Version:    1.0
 * Date:       2014.03.18
 * Author:     Ji
 * 
 *========================================
 * @namespace  Jisons 
 * @class      JisonsIEnumerable
 * @extends    
 * 
 *             对于IEnumerable 的Linq扩展查询
 * 
 *========================================
 * 
 * Copyright © 迹I柳燕
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jisons
{
    public static class JisonsIEnumerable
    {

        /// <summary>
        /// 具有锁定检查的遍历执行函数体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">欲执行循环的列表,继承IEnumerable</param>
        /// <param name="action">执行循环的函数体</param>
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action.Invoke(item);
            }
        }
    }
}
