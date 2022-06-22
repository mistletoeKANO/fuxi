using System;
using System.Collections.Generic;

namespace FuXi.Editor
{
    internal static class ProcessingHelper
    {
        /// <summary>
        /// 获取所有Bundle 预处理指令
        /// </summary>
        /// <returns></returns>
        internal static List<IBuildBundlePreprocess> AcquireAllBundlePreProcess()
        {
            return AcquirePreProcessInternal<IBuildBundlePreprocess>();
        }
        
        /// <summary>
        /// 获取所有Player 预处理指令
        /// </summary>
        /// <returns></returns>
        internal static List<IBuildPlayerPreprocess> AcquireAllPlayerPreProcess()
        {
            return AcquirePreProcessInternal<IBuildPlayerPreprocess>();
        }

        private static List<T> AcquirePreProcessInternal<T>()
        {
            List<T> preprocesses = new List<T>();
            var typeBase = typeof(T);
            var assembles = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assembles)
            {
                if (BuildHelper.CheckIgnore(assembly.GetName().Name)) continue;

                System.Type[] types = assembly.GetTypes();
                foreach (System.Type type in types)
                {
                    if (type.IsClass && !type.IsAbstract && typeBase.IsAssignableFrom(type))
                    {
                        var ins = (T) Activator.CreateInstance(type);
                        preprocesses.Add(ins);
                    }
                }
            }
            return preprocesses;
        }
    }
}