using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jisons
{
    public static class JisonsAssembly
    {
        /// <summary> 获取指定程序域里所有的自定义类型 T </summary>
        /// <typeparam name="T"> 将要查询的类型 T </typeparam>
        /// <param name="currentAssembly"> 指定查询的程序集 </param>
        /// <param name="bindingAttr"> 查询的指定参数 </param>
        /// <returns> 查询到的指定程序域所含有的所有自定义类型 T </returns>
        public static IList<T> FindAllStaticTypesInAssemblyDomain<T>(this Assembly currentAssembly, BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.Public) where T : class
        {
            List<T> retDatas = new List<T>();
            var referencedAssemblies = currentAssembly.GetReferencedAssemblies();

            //获取当前 程序集所自定义的 T
            var datas = currentAssembly.FindStaticFieldValueInAssembly<T>(bindingAttr);
            if (datas != null)
            {
                retDatas.AddRange(datas);
            }

            //获取当前程序集所引用的所有程序集包含的所有自定义 T 
            Parallel.ForEach(referencedAssemblies, referencedAssembly =>
               {
                   var assembly = Assembly.Load(referencedAssembly);
                   datas = assembly.FindStaticFieldValueInAssembly<T>(bindingAttr);
                   if (datas != null)
                   {
                       retDatas.AddRange(datas);
                   }
               });

            return retDatas;
        }

    }
}
