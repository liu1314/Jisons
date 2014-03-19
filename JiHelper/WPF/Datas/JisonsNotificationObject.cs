﻿/* 迹I柳燕
 * 
 * FileName:   JisonsNotificationObject.cs
 * Version:    1.0
 * Date:       2014.03.19
 * Author:     Ji
 * 
 *========================================
 * @namespace  Jisons 
 * @class      JisonsNotificationObject
 * @extends    
 * 
 *             对于实现了 INotifyPropertyChanged 接口的类，可以直接扩展通知当前的线程通知
 *             
 * 
 *========================================
 * Hi,小喵喵...
 * Copyright © 迹I柳燕
 * 
 * 转载请保留...
 * 
 */

using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Jisons
{
    public static class JisonsNotificationObject
    {

        /// <summary> 增加对与 实现 INotifyPropertyChanged 接口的扩展线程通知 </summary>
        /// <typeparam name="D">当前被绑定的类型</typeparam>
        /// <typeparam name="T">当前发送线程通知的类型</typeparam>
        /// <param name="data">当前被绑定的数据</param>
        /// <param name="propertyExpression">反射获取类型名称的委托</param>
        public static void RaisePropertyChanged<D, T>(this D data, Expression<Func<T>> propertyExpression) where D : INotifyPropertyChanged
        {
            if (data != null)
            {
                var multicastDelegateFieldInfo = data.GetType().FindField("PropertyChanged");
                if (multicastDelegateFieldInfo != null)
                {
                    var multicastDelegateValue = multicastDelegateFieldInfo.GetValue(data);
                    if (multicastDelegateValue != null)
                    {
                        var multicastDelegate = (MulticastDelegate)multicastDelegateValue;
                        Delegate[] delegates = multicastDelegate.GetInvocationList();
                        var propertyName = ExtractPropertyName<T>(propertyExpression);
                        if (delegates.Count() > 0 && !string.IsNullOrWhiteSpace(propertyName))
                        {
                            var propertyChangedEventArgs = new PropertyChangedEventArgs(propertyName);
                            foreach (Delegate delegateItem in delegates)
                            {
                                //在此动态的调用其方法
                                //因此需要需要保证其方法参数的正确性
                                var propertyChangedEventHandler = delegateItem as PropertyChangedEventHandler;
                                if (propertyChangedEventHandler != null)
                                {
                                    propertyChangedEventHandler.DynamicInvoke(data, propertyChangedEventArgs);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary> 获取当前的委托名称 </summary>
        /// <typeparam name="T">当前发送线程通知的类型</typeparam>
        /// <param name="propertyExpression">反射获取类型名称的委托</param>
        /// <returns> 当前的委托名称 </returns>
        public static string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression != null)
            {
                var memberExpression = propertyExpression.Body as MemberExpression;
                var propertyInfo = memberExpression.Member as PropertyInfo;
                var getMethod = propertyInfo.GetGetMethod(true);
                return memberExpression.Member.Name;
            }
            return null;
        }
    }

}
