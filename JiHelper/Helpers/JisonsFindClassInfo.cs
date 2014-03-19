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
        /// <param name="type">查找的起始类型</param>
        /// <param name="fieldName">字段的名称</param>
        /// <param name="bindingAttr">搜索的标志</param>
        /// <returns>返回查询到的  FieldInfo</returns>
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

    }
}
