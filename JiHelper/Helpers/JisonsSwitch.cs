/* 迹I柳燕
 * 
 * FileName:   JisonsSwitch.cs
 * Version:    1.0
 * Date:       2014.03.18
 * Author:     Ji
 * 
 *========================================
 * @namespace  Jisons 
 * @class      JisonsSwitch
 * @extends    
 * 
 *             对于 Switch 的扩展，主要用于不想写那么多的case
 * 
 *========================================
 * Hi,小喵喵...
 * Copyright © 迹I柳燕
 * 
 * 转载请保留...
 * 
 */

using System;

namespace Jisons
{
    public static class JisonsSwitch
    {

        /// <summary> 创建 switch 中的单项 case 判断方法</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumData"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static SwitchItem<T> CreatSwitchItem<T>(this T enumData, Action action)
        {
            return new SwitchItem<T>(enumData, action);
        }

        /// <summary> 执行 switch 中 case 的判断</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="judge"></param>
        /// <param name="jition"></param>
        /// <returns></returns>
        public static bool DoAction<T>(this T judge, params SwitchItem<T>[] jition)
        {
            bool isdo = false;
            if (jition != null)
            {
                foreach (var item in jition)
                {
                    if (item.Data != null && item.Data.Equals(judge) && item.Action != null)
                    {
                        isdo = true;
                        item.Action();
                    }
                }
            }
            return isdo;
        }

        /// <summary> switch Item 模版 </summary>
        /// <typeparam name="T"></typeparam>
        public class SwitchItem<T>
        {
            public SwitchItem(T enumData, Action action)
            {
                this.Data = enumData;
                this.Action = action;
            }

            public T Data { get; set; }
            public Action Action { get; set; }
        }

    }
}
