/* 迹I柳燕
 * 
 * FileName:   JisonsNotificationObject.cs
 * Version:    1.0
 * Date:       2014.03.19
 * Author:     Ji
 * 
 *========================================
 * @namespace  Jisons 
 * @class      JisonsNotificationObject
 * @extends    INotifyPropertyChanged
 * 
 *             对于 INotifyPropertyChanged 的补充，在此可以省略与每次自己写通知函数
 *             不过在不能继承此类的时候还是需要自己写
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
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Jisons
{
    public static class JisonsNotificationObject
    {
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

        /// <summary>
        /// 获取当前的 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
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
