using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace FuXi
{
    /// <summary>
    /// 下载信息
    /// </summary>
    public struct DownloadInfo
    {
        public long DownloadSize;
        public Queue<BundleManifest> DownloadList;
        public string FormatSize => FxUtility.FormatBytes(this.DownloadSize);
    }

    /// <summary>
    /// 下载文件有效状态
    /// </summary>
    public struct DownloadState
    {
        public bool Valid;
        //文件已下载大小，断点续传记录大小
        public long Size;
    }

    /// <summary>
    /// 运行配置管理，管理更新下载和资源加载配置
    /// </summary>
    public class FxManifestDriver
    {
        //本地配置
        public FxManifest OldManifest;
        //资源服务器下载的最新配置
        ///<see cref = "CheckWWWManifest"/>
        public FxManifest NewManifest;
        
        internal readonly string ManifestName;
        internal readonly string VersionName;
        internal string NewHash = String.Empty;

        internal IEncrypt GameEncrypt;
        internal FxManifestDriver(string name)
        {
            this.ManifestName = $"{name.Trim()}{FxPathHelper.ManifestFileExtension}";
            this.VersionName = $"{name.Trim()}{FxPathHelper.VersionFileExtension}";
        }

        /// <summary>
        /// 初始化解密接口
        /// </summary>
        internal void InitEncrypt()
        {
            if (string.IsNullOrEmpty(this.NewManifest.EncryptType)) return;
            if (this.NewManifest.EncryptType.Equals("None")) return;
            
            Type encryptType = null;
            var assembles = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assemble in assembles)
            {
                encryptType = assemble.GetType(this.NewManifest.EncryptType);
                if (encryptType != null) break;
            }
            if (encryptType != null)
            {
                this.GameEncrypt = (IEncrypt) Activator.CreateInstance(encryptType);
                FxDebug.ColorLog(FxDebug.ColorStyle.Cyan,"解密接口 {0}", this.NewManifest.EncryptType);
            }
            else
                FxDebug.ColorError(FxDebug.ColorStyle.Red,"未发现解密接口 {0}", this.NewManifest.EncryptType);
        }
        
        internal bool TryGetAssetManifest(string assetPath, out AssetManifest manifest)
        {
            if (!this.NewManifest.Path2AssetManifest.TryGetValue(assetPath, out manifest))
            {
                FxDebug.ColorError(FxDebug.ColorStyle.Red, "File {0} not found", assetPath);
                return false;
            }
            return true;
        }
        internal bool TryGetBundleManifest(int index, out BundleManifest manifest)
        {
            manifest = default;
            if (this.NewManifest.Bundles == null) return false;
            if (index >= this.NewManifest.Bundles.Length || index < 0)
            {
                FxDebug.ColorError(FxDebug.ColorStyle.Red, "Load bundle index {0} is out of range!", index);
                return false;
            }
            manifest = this.NewManifest.Bundles[index];
            return true;
        }
        /// <summary>
        /// 获取分包信息
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        private PackageManifest GetPackageWithName(string packageName)
        {
            foreach (var p in this.NewManifest.Packages)
            {
                if (p.PackageName == packageName)
                {
                    return p;
                }
            }
            FxDebug.ColorError(Color.red, "PackageName is not exist --------> {0}", packageName);
            return default;
        }

        /// <summary>
        /// 获取分包列表包含的bundle包
        /// </summary>
        /// <param name="packages"></param>
        /// <returns></returns>
        internal List<int> GetPackagesBundle(string[] packages)
        {
            List<int> res = new List<int>();
            foreach (var pName in packages)
            {
                var package = this.GetPackageWithName(pName);
                foreach (var index in package.Bundles)
                {
                    if (res.Contains(index)) continue;
                    res.Add(index);
                }
            }
            return res;
        }

        /// <summary>
        /// 获取所有分包 的 bundle 下标列表
        /// </summary>
        /// <returns></returns>
        internal List<int> GetPackagesBundle()
        {
            List<int> res = new List<int>();
            foreach (var package in this.NewManifest.Packages)
            {
                foreach (var index in package.Bundles)
                {
                    if (res.Contains(index)) continue;
                    res.Add(index);
                }
            }
            return res;
        }
        
        /// <summary>
        /// 验证文件下载状态
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        internal DownloadState Downloaded(string bundleName)
        {
            if (!this.NewManifest.Name2BundleManifest.TryGetValue(bundleName, out var manifest))
            {
                FxDebug.ColorWarning(FxDebug.ColorStyle.Orange, "bundle {0} is not valid", bundleName);
                return default;
            }
            var path = FxPathHelper.PersistentLoadPath(bundleName);
            if (File.Exists(path))
            {
                if (FxUtility.FileMd5(path) == manifest.Hash)
                    return new DownloadState {Valid = true};
                else
                    return new DownloadState {Valid = false, Size = FxUtility.FileSize(path)};
            }
            if (!this.OldManifest.Name2BundleManifest.TryGetValue(bundleName, out var oldManifest)) return default;
            if (oldManifest.IsBuiltin && oldManifest.Hash == manifest.Hash)
            {
                return new DownloadState {Valid = true};
            }
            return default;
        }

        /// <summary>
        /// 获取Bundle加载路径
        /// </summary>
        /// <param name="manifest"></param>
        /// <returns></returns>
        internal string BundleRealLoadPath(BundleManifest manifest)
        {
            var path = FxPathHelper.PersistentLoadPath(manifest.BundleHashName);
            if (File.Exists(path) && FxUtility.FileMd5(path) == manifest.Hash)
                return path;
            if (!this.OldManifest.Name2BundleManifest.TryGetValue(manifest.BundleHashName, out var oldManifest)) return default;
            if (oldManifest.IsBuiltin && oldManifest.Hash == manifest.Hash)
            {
                return FxPathHelper.StreamingLoadPath(manifest.BundleHashName);
            }
            return String.Empty;
        }

        /// <summary>
        /// 覆盖版本文件
        /// </summary>
        internal void OverrideManifest()
        {
            if (this.NewManifest == null) return;
            try
            {
                var versionDest = FxPathHelper.PersistentLoadPath(this.VersionName);
                File.WriteAllText(versionDest, this.NewHash);
            
                var manifestDest = FxPathHelper.PersistentLoadPath(this.ManifestName);
                string content = JsonUtility.ToJson(this.NewManifest);
                File.WriteAllText(manifestDest, content);
            }
            catch (Exception e)
            {
                FxDebug.LogError("覆盖版本文件出错 {0}", e.Message);
            }
        }
    }
}