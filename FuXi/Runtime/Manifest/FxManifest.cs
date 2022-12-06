using System;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace FuXi
{
    [Serializable]
    public class FxManifest
    {
        //资源版本号
        public int ResVersion;
        //APP版本号
        public string AppVersion;
        //加密算法
        public string EncryptType;
        //资源根路径
        public string RootPath;
        //是否开启断点续传
        public bool OpenBreakResume;
        
        public AssetManifest[] Assets;
        public BundleManifest[] Bundles;
        public PackageManifest[] Packages;
        
        internal readonly Dictionary<string, AssetManifest> Path2AssetManifest = new Dictionary<string, AssetManifest>();
        internal readonly Dictionary<string, BundleManifest> Name2BundleManifest = new Dictionary<string, BundleManifest>();
        internal readonly Dictionary<string, PackageManifest> Name2PackageManifest = new Dictionary<string, PackageManifest>();
        internal static FxManifest Parse(string jsonContent)
        {
            if (string.IsNullOrEmpty(jsonContent))
            {
                FxDebug.LogError("manifest json content is null or empty!");
                return null;
            }
            try
            {
                var manifest = JsonUtility.FromJson<FxManifest>(jsonContent);
                foreach (var asManifest in manifest.Assets) manifest.Path2AssetManifest.Add(asManifest.Path, asManifest);
                foreach (var bdManifest in manifest.Bundles) manifest.Name2BundleManifest.Add(bdManifest.BundleHashName, bdManifest);
                foreach (var pManifest in manifest.Packages) manifest.Name2PackageManifest.Add(pManifest.PackageName, pManifest);
                return manifest;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }

    /// <summary>
    /// 资产信息
    /// </summary>
    [Serializable]
    public struct AssetManifest
    {
        /// <summary>
        /// 资产路径
        /// </summary>
        public string Path;
        /// <summary>
        /// 是否是原生资产
        /// </summary>
        public bool IsRawFile;
        /// <summary>
        /// 资产所属Bundle ID
        /// </summary>
        public int HoldBundle;
        /// <summary>
        /// 资产依赖Bundle ID
        /// </summary>
        public int[] DependBundles;
    }

    /// <summary>
    /// 分包信息 DLC
    /// </summary>
    [Serializable]
    public struct PackageManifest
    {
        /// <summary>
        /// 分包名称
        /// </summary>
        public string PackageName;
        /// <summary>
        /// 分包包含的Bundle ID 只包含独有资源Bundle, 如果有依赖共享的则在公共部分
        /// </summary>
        public int[] Bundles;
        /// <summary>
        /// 是否是内置DLC
        /// </summary>
        public bool IsBuiltin;
    }
    
    /// <summary>
    /// AB 包 信息
    /// </summary>
    [Serializable]
    public struct BundleManifest
    {
        public string BundleHashName;
        public string CRC;
        public string Hash;
        /// <summary>
        /// 只在 是否 拷贝进 安装包时 确定是否是 内置资源
        /// </summary>
        public bool IsBuiltin;
        public long Size;
    }
}