/* 迹I柳燕
 * 
 * FileName:   JisonsImageSource.cs
 * Version:    1.0
 * Date:       2014.03.18
 * Author:     Ji
 * 
 *========================================
 * @namespace  Jisons 
 * @class      JisonsImageSource
 * @extends    
 * 
 *             对于JisonsImageSource的处理
 * 
 *========================================
 * Hi,小喵喵...
 * Copyright © 迹I柳燕
 * 
 * 转载请保留...
 * 
 */

using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Jisons
{
    public static class JisonsImageSource
    {

        /// <summary> 转换Icon到ImageSource </summary>
        /// <param name="icon">传入的Icon</param>
        /// <returns>ImageSource</returns>
        public static ImageSource ConverterToImageSource(this Icon icon)
        {
            return icon != null ? Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()) : null;
        }

    }
}
