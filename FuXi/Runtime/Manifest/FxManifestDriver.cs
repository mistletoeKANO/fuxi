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

        private Type encryptType;
        internal BaseEncrypt GameEncrypt;
        internal FxManifestDriver(string name, Type encryptType)
        {
            this.ManifestName = $"{name.Trim()}{FxPathHelper.ManifestFileExtension}";
            this.VersionName = $"{name.Trim()}{FxPathHelper.VersionFileExtension}";
            this.encryptType = encryptType;
        }

        internal string CombineAssetPath(string loadPath)
        {
            return $"{this.NewManifest.RootPath}{loadPath}";
        }
        /// <summary>
        /// 初始化解密接口
        /// </summary>
        internal void InitEncrypt()
        {
            if (this.NewManifest == null) return;
            if (string.IsNullOrEmpty(this.NewManifest.EncryptType)) return;
            if (this.NewManifest.EncryptType.Equals("None")) return;

            if (this.encryptType != null) 
            {
                this.GameEncrypt = (BaseEncrypt) Activator.CreateInstance(this.encryptType);
                FxDebug.ColorLog(FX_LOG_CONTROL.Cyan,"解密接口 {0}", this.encryptType);
                return;
            }
            var assembles = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assemble in assembles)
            {
                this.encryptType = assemble.GetType(this.NewManifest.EncryptType);
                if (this.encryptType != null) break;
            }
            if (this.encryptType != null)
            {
                this.GameEncrypt = (BaseEncrypt) Activator.CreateInstance(this.encryptType);
                FxDebug.ColorLog(FX_LOG_CONTROL.Cyan,"解密接口 {0}", this.NewManifest.EncryptType);
            }
            else
                FxDebug.ColorError(FX_LOG_CONTROL.Red,"未发现解密接口 {0}", this.NewManifest.EncryptType);
        }
        
        internal bool TryGetAssetManifest(string assetPath, out AssetManifest manifest)
        {
            if (!this.NewManifest.Path2AssetManifest.TryGetValue(assetPath, out manifest))
            {
                FxDebug.ColorError(FX_LOG_CONTROL.Red, "File {0} not found", assetPath);
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
                FxDebug.ColorError(FX_LOG_CONTROL.Red, "Load bundle index {0} is out of range!", index);
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
                //内置DLC不作为 独立 DLC单独下载, 在主更新下载中检查更新
                if (package.IsBuiltin)
                    continue;
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
                FxDebug.ColorWarning(FX_LOG_CONTROL.Orange, "bundle {0} is not valid", bundleName);
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
        /// <param name="rawFile"></param>
        /// <returns></returns>
        internal string BundleRealLoadPath(BundleManifest manifest, bool rawFile = false)
        {
            var path = FxPathHelper.PersistentLoadPath(manifest.BundleHashName);
            if (File.Exists(path) && FxUtility.FileMd5(path) == manifest.Hash)
                return !rawFile? path : FxPathHelper.PersistentLoadURL(manifest.BundleHashName);
            if (!this.OldManifest.Name2BundleManifest.TryGetValue(manifest.BundleHashName, out var oldManifest)) 
                return String.Empty;
            if (manifest.IsBuiltin && oldManifest.Hash == manifest.Hash)
            {
                return !rawFile
                    ? FxPathHelper.StreamingLoadPath(manifest.BundleHashName)
                    : FxPathHelper.StreamingLoadURL(manifest.BundleHashName);
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
                FxDebug.ColorLog(FX_LOG_CONTROL.Orange, "Override VersionFile! new hash {0}", this.NewHash);
            
                var manifestDest = FxPathHelper.PersistentLoadPath(this.ManifestName);
                string content = JsonUtility.ToJson(this.NewManifest);
                File.WriteAllText(manifestDest, content);
                FxDebug.ColorLog(FX_LOG_CONTROL.Orange, "Override Manifest! new version {0}", this.NewManifest.ResVersion);
            }
            catch (Exception e)
            {
                FxDebug.LogError("覆盖版本文件出错 {0}", e.Message);
            }
        }
    }
}