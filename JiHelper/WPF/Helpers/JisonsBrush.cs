/* 迹I柳燕
 * 
 * FileName:   JisonsBrush.cs
 * Version:    1.0
 * Date:       2014.03.18
 * Author:     Ji
 * 
 *========================================
 * @namespace  Jisons 
 * @class      JisonsBrush
 * @extends    
 *             
 *             WPF 扩展
 *             对于Brush的扩展方法
 * 
 *========================================
 * Hi,小喵喵...
 * Copyright © 迹I柳燕
 * 
 * 转载请保留...
 * 
 */

using System.Windows.Media;

namespace Jisons
{
    public static class JisonsBrush
    {

        /// <summary> 十六进制颜色值转换为Brush </summary>
        /// <param name="brushStr">传入的颜色字符串</param>
        /// <returns>Brush</returns>
        public static Brush StringConvertToBrush(this string brushStr)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(brushStr));
        }

    }
}
