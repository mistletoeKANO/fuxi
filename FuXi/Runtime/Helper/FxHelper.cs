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

        internal static string StreamingLoadURL(string path)
        {
            var strPath = StreamingLoadPath(path);
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return strPath;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return $"file:///{strPath}";
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    return $"file://{strPath}";
            }
            return strPath;
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

        internal static string PersistentLoadURL(string path)
        {
            var perStr = PersistentLoadPath(path);
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return $"file://{perStr}";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return perStr;
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    return $"file://{perStr}";
            }
            return perStr;
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