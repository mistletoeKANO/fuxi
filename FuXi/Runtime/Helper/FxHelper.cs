using System.IO;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace FuXi
{
    internal static class FxPathHelper
    {
        internal static readonly string ManifestFileExtension = ".json";
        internal static readonly string VersionFileExtension = ".hash";
        
        internal static readonly string BundlePathName = "Bundles";
        private  static readonly string BundleCacheDir = "BundleCache";
        /// <summary>
        /// 获取规范化的路径
        /// </summary>
        private static string GetRegularPath(string path)
        {
            return path.Replace('\\', '/').Replace("\\", "/"); //替换为Linux路径格式
        }

        /// <summary>
        /// 获取文件所在的目录路径（Linux格式）
        /// </summary>
        internal static string GetDirectory(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            return GetRegularPath(directory);
        }

        /// <summary>
        /// 获取基于流文件夹的加载路径
        /// </summary>
        internal static string StreamingLoadPath(string path)
        {
            return $"{StreamingRootPath()}/{path}";
        }

        internal static string StreamingRootPath()
        {
            return $"{UnityEngine.Application.streamingAssetsPath}/{BundlePathName}";
        }

        /// <summary>
        /// 获取基于bundle缓存文件夹的加载路径
        /// </summary>
        internal static string PersistentLoadPath(string path)
        {
            return $"{PersistentRootPath()}/{path}";
        }

        /// <summary>
        /// 获取bundle缓存文件夹路径
        /// </summary>
        internal static string PersistentRootPath()
        {
            var path = $"{UnityEngine.Application.persistentDataPath}/{BundleCacheDir}";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }
        
        /// <summary>
        /// application.persistentDataPath 的UnityWebRequest.Get
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static string PersistentLoadURL(string path)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return $"file://{path}";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return $"file:///{path}";
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    return $"file://{path}";
            }
            return path;
        }

        /// <summary>
        /// 获取网络资源加载路径
        /// </summary>
        internal static string ConvertToWWWPath(string path)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.IPhonePlayer:
                    return $"file://{path}";
            }
            return path;
        }
    }

    /// <summary>
    /// 下载的 缓存资源文件管理
    /// </summary>
    internal static class FxCacheHelper
    {
        /// <summary>
        /// 清除缓存资源
        /// </summary>
        internal static void ClearBundleCache()
        {
            var cachePath = FxPathHelper.PersistentRootPath();
            if (!Directory.Exists(cachePath)) return;
            Directory.Delete(cachePath, true);
            FxDebug.Log("Clear all download cache finished!");
        }

        /// <summary>
        /// 删除缓存指定文件
        /// </summary>
        /// <param name="name"></param>
        internal static void DeleteCacheFile(string name)
        {
            var cacheFile = FxPathHelper.PersistentLoadPath(name);
            if (!File.Exists(cacheFile)) return;
            File.Delete(cacheFile);
        }
    }
}