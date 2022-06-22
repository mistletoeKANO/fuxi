using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

// ReSharper disable once CheckNamespace
namespace FuXi.Editor
{
    internal static class BuildHelper
    {
        internal static bool IsMatchBuildPlatform(BuildPlateForm plateForm)
        {
            if (plateForm == BuildPlatform2SettingPlatform()) return true;
            FxDebug.ColorWarning(UnityEngine.Color.red,
                "Fx_Setting platform {0} is not match with current environment {1} !", 
                plateForm,
                EditorUserBuildSettings.activeBuildTarget);
            return false;
        }

        internal static Fx_BuildAsset GetBuildAsset(BuildPlateForm plateForm)
        {
            Fx_BuildAsset buildAsset = null;
            var asGuids = AssetDatabase.FindAssets("t:" + typeof(Fx_BuildAsset).FullName);
            foreach (var guid in asGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var buildSetting = AssetDatabase.LoadAssetAtPath<Fx_BuildSetting>(assetPath);
                
                if (buildSetting.FxPlatform != plateForm) continue;
                buildAsset = AssetDatabase.LoadAssetAtPath<Fx_BuildAsset>(assetPath);
                break;
            }
            return buildAsset;
        }

        private static BuildPlateForm BuildPlatform2SettingPlatform()
        {
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    return BuildPlateForm.Android;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return BuildPlateForm.Window;
                case BuildTarget.iOS:
                    return BuildPlateForm.IOS;
                case BuildTarget.StandaloneOSX:
                    return BuildPlateForm.MacOS;
                case BuildTarget.WebGL:
                    return BuildPlateForm.WebGL;
                default:
                    return BuildPlateForm.Window;
            }
        }

        #region 加密相关

        internal static bool CheckBundleFileValid(byte[] bundleBytes)
        {
            byte[] signArray = new byte[20];
            Array.Copy(bundleBytes, signArray, signArray.Length);
            string signature = System.Text.Encoding.UTF8.GetString(signArray);
            return signature.Contains("UnityFS") || signature.Contains("UnityRaw") ||
                   signature.Contains("UnityWeb") || signature.Contains("\xFA\xFA\xFA\xFA\xFA\xFA\xFA\xFA");
        }

        private static readonly List<string> IgnoreAssembles = new List<string>
        {
            "mscorlib", "UnityEngine", "System", "UnityEditor"
        };

        internal static string[] GetEncryptOptions()
        {
            List<string> options = new List<string> {"None"};
            var typeBase = typeof(IEncrypt);
            var assembles = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assembles)
            {
                if (CheckIgnore(assembly.GetName().Name)) continue;

                System.Type[] types = assembly.GetTypes();
                foreach (System.Type type in types)
                {
                    if (type.IsClass && !type.IsAbstract && typeBase.IsAssignableFrom(type))
                    {
                        options.Add(type.FullName);
                    }
                }
            }

            return options.ToArray();
        }

        internal static bool CheckIgnore(string name)
        {
            foreach (var assemble in IgnoreAssembles)
            {
                if (name.Contains(assemble)) return true;
            }

            return false;
        }

        internal static IEncrypt LoadEncryptObject(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return null;
            var assembles = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assembles)
            {
                var encryptType = assembly.GetType(typeName);
                if (encryptType == null) continue;

                return System.Activator.CreateInstance(encryptType) as IEncrypt;
            }

            return null;
        }

        #endregion
    }

    internal static class FxBuildPath
    {
        internal static readonly string BuildPlayers = "Players";

        /// <summary>
        /// 当前打包环境平台名称
        /// </summary>
        /// <returns></returns>
        internal static string BuildPlatformName()
        {
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android: return "Android";
                case BuildTarget.StandaloneOSX: return "OSX";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64: return "Windows";
                case BuildTarget.iOS: return "IOS";
                case BuildTarget.WebGL: return "WebGL";
            }

            return "Platform Not Support";
        }

        /// <summary>
        /// 安装包保存路径
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static string PlayerFullPath(string name)
        {
            return $"{PlayerRootPath()}/{name}";
        }

        /// <summary>
        /// 安装包存储根路径
        /// </summary>
        /// <returns></returns>
        internal static string PlayerRootPath()
        {
            var path = $"{ProjectRootPath()}./{FxBuildPath.BuildPlayers}/{BuildPlatformName()}";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// bundle文件保存路径
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static string BundleFullPath(string name)
        {
            return $"{BundleRootPath()}/{name}";
        }

        /// <summary>
        /// 打包数据存储路径
        /// </summary>
        /// <returns></returns>
        internal static string BundleRootPath()
        {
            var path = $"{ProjectRootPath()}/{FxPathHelper.BundlePathName}/{BuildPlatformName()}";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// 拷贝bundle资源保存路径
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static string CopyFullSavePath(string name)
        {
            return $"{CopyBundleRootPath()}/{name}";
        }

        /// <summary>
        /// 拷贝bundle资源保存根路径
        /// </summary>
        /// <returns></returns>
        internal static string CopyBundleRootPath()
        {
            var path = $"{CopyRootPath()}/{FxPathHelper.BundlePathName}";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// 内建资源根路径
        /// </summary>
        /// <returns></returns>
        internal static string CopyRootPath()
        {
            var path = "Assets/StreamingAssets";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// 项目工程路径
        /// </summary>
        /// <returns></returns>
        internal static string ProjectRootPath()
        {
            return Environment.CurrentDirectory;
        }
    }

    internal static class FxBuildCache
    {
        private static string BuildPlateFormName(BuildPlateForm platform)
        {
            switch (platform)
            {
                case BuildPlateForm.Android: return "Android";
                case BuildPlateForm.MacOS: return "OSX";
                case BuildPlateForm.Window: return "Windows";
                case BuildPlateForm.IOS: return "IOS";
                case BuildPlateForm.WebGL: return "WebGL";
            }
            return "Platform Not Support";
        }
        
        /// <summary>
        /// 清除所有AB
        /// </summary>
        /// <param name="platform"></param>
        internal static void DeleteAssetBundle(BuildPlateForm platform)
        {
            var path = $"{FxBuildPath.ProjectRootPath()}/{FxPathHelper.BundlePathName}/{BuildPlateFormName(platform)}";
            if (!Directory.Exists(path)) return;
            Directory.Delete(path, true);
            FxDebug.Log("Clear all assetBundle finished!");
        }

        /// <summary>
        /// 清除所有安装包
        /// </summary>
        /// <param name="platform"></param>
        internal static void DeletePlayer(BuildPlateForm platform)
        {
            var path = $"{FxBuildPath.ProjectRootPath()}/{FxBuildPath.BuildPlayers}/{BuildPlateFormName(platform)}";
            if (!Directory.Exists(path)) return;
            Directory.Delete(path, true);
            FxDebug.Log("Clear all player finished!");
        }
    }
}