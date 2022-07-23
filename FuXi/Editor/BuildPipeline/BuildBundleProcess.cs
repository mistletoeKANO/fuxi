using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

// ReSharper disable once CheckNamespace
namespace FuXi.Editor
{
    internal class BuildBundleProcess : IBuild
    {
        private readonly List<string> m_MainAssets;
        private readonly List<BundlePackage> m_Packages;
        private readonly Dictionary<string, string> m_Asset2BundleName;
        private readonly Dictionary<string, BundleBuild> m_BundleName2Builds;
        private readonly Fx_BuildAsset buildAsset;
        private readonly Fx_BuildSetting buildSetting;
        private readonly List<Fx_BuildPackage> buildPackages;
        private readonly List<IBuildBundlePreprocess> bundlePreprocesses;
        
        private BaseEncrypt mEncrypt;
        private AssetBundleManifest manifest;
        private Dictionary<string, string> name2HashName;
        private bool cancel;
        
        internal BuildBundleProcess(Fx_BuildAsset asset)
        {
            this.buildAsset = asset;
            this.m_MainAssets = new List<string>();
            this.m_Packages = new List<BundlePackage>();
            this.m_BundleName2Builds = new Dictionary<string, BundleBuild>();
            this.m_Asset2BundleName = new Dictionary<string, string>();

            this.buildPackages = new List<Fx_BuildPackage>();
            var assetPath = AssetDatabase.GetAssetPath(this.buildAsset);
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (var o in assets)
            {
                if (o is Fx_BuildSetting setting) { this.buildSetting = setting; }
                if (o is Fx_BuildPackage package) { this.buildPackages.Add(package); }
            }
            this.bundlePreprocesses = ProcessingHelper.AcquireAllBundlePreProcess();
        }

        public void BeginBuild()
        {
            if (this.buildAsset == null) return;
            if (!BuildHelper.IsMatchBuildPlatform(this.buildSetting.FxPlatform)) return;
            EditorExtension.ClearConsole();
            try
            {
                this.BuildBundlePreProcess(); // 构建前预处理
                this.AnalysisMainAssets(); // 分析主要资产
                this.AnalysisDependenciesAssets(); // 分析依赖资产
                this.AnalysisPackage(); // 分析分包资产
                this.BuildBundles(); // 构建AssetBundle包
                this.AnalysisManifest(); // 分析清单文件
                this.EncryptBundles(); // 加密AssetBundle包
                this.AnalysisPackageDependencies(); // 分析分包依赖Bundle包
                this.WriteManifest(); // 生成版本文件
                this.CopyVersionFile(); // 拷贝版本差异文件
                this.RemoveUnityManifest();
                this.EndBuild(); // End
                this.BuildBundlePostProcess(); // 构建后处理
            }
            catch (Exception e)
            {
                Debug.LogError("Build failure!");
                throw new BuildFailedException(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            if (this.buildAsset != null) Selection.activeObject = this.buildAsset;
        }

        private void BuildBundlePreProcess()
        {
            foreach (var process in this.bundlePreprocesses)
                process.BuildBundlePre();
        }
        
        private void AnalysisMainAssets()
        {
            int count = this.buildAsset.fx_Objects.Count;
            for (int i = 0; i < count; i++)
            {
                var fo = this.buildAsset.fx_Objects[i];
                if (fo.folder == null)
                {
                    throw new Exception($"{this.buildAsset.name} assetList object reference is missing!!!");
                }
                var path = AssetDatabase.GetAssetPath(fo.folder);
                bool isRawFile = fo.bundleMode == BundleMode.PackByRaw;
                if (AssetDatabase.IsValidFolder(path))
                {
                    var guids = AssetDatabase.FindAssets("*", new[] {path});
                    for (int j = 0; j < guids.Length; j++)
                    {
                        var guid = guids[j];
                        var p = AssetDatabase.GUIDToAssetPath(guid).Replace("\\", "/");
                        
                        float progress = i / (float) count + j / (float) guids.Length;
                        this.cancel = EditorUtility.DisplayCancelableProgressBar("Analysis Main Assets", p, progress);
                        if (this.cancel) { throw new Exception("Cancel!!!");}
                        
                        if (AssetDatabase.IsValidFolder(p)) continue;
                        if (this.m_MainAssets.Contains(p)) continue;
                        if (!isRawFile) this.m_MainAssets.Add(p);
                        this.AddAsset2BuildRecorder(fo.bundleMode, path, p);
                    }
                }
                else
                {
                    if (this.m_MainAssets.Contains(path)) continue;
                    if (!isRawFile) this.m_MainAssets.Add(path);
                    this.AddAsset2BuildRecorder(isRawFile? BundleMode.PackByRaw : BundleMode.PackByFile, null, path);
                }
            }
        }
        
        private void AnalysisDependenciesAssets()
        {
            int count = this.m_MainAssets.Count;
            for (int i = 0; i < count; i++)
            {
                var asset = this.m_MainAssets[i];
                float progress = i / (float) count;
                this.cancel = EditorUtility.DisplayCancelableProgressBar($"Analysis Dependencies Assets {i}/{count}", asset, progress);
                if (this.cancel) { throw new Exception("Cancel!!!");}
                
                var dependencies = AssetDatabase.GetDependencies(asset);
                foreach (var depFile in dependencies)
                {
                    if (string.Equals(depFile, asset)) continue;
                    if (depFile.EndsWith(".cs") || this.buildSetting.ExcludeExtensions.Exists(depFile.EndsWith)) continue;
                    this.AddAsset2BuildRecorder(BundleMode.PackByDirectory, null, depFile);
                }
            }
        }

        private void AddAsset2BuildRecorder(BundleMode mode, string rootPath, string path)
        {
            var bundleName = this.GetBundleName(mode, rootPath, path);
            if (this.m_Asset2BundleName.ContainsKey(path)) return;
            this.m_Asset2BundleName.Add(path, bundleName);

            if (this.m_BundleName2Builds.TryGetValue(bundleName, out var builds))
            {
                if (!builds.assets.Contains(path)) { builds.assets.Add(path); }
            }
            else
            {
                builds = new BundleBuild() {bundleName = bundleName, assets = new List<string>()};
                builds.assets.Add(path);
            }
            builds.isRawFile = mode == BundleMode.PackByRaw;
            this.m_BundleName2Builds[bundleName] = builds;
        }
        
        private void AnalysisPackage()
        {
            int count = this.buildPackages.Count;
            for (int i = 0; i < count; i++)
            {
                var package = this.buildPackages[i];
                if (this.IsBundlePackageNameRepeated(package.name))
                {
                    throw new Exception($"Bundle package name is repeated: {package.name}");
                }
                var bundlePackage = new BundlePackage() {packageName = package.name, bundles = new List<string>()};
                if (package.PackageObjects == null) continue;
                int length = package.PackageObjects.Count;
                for (int j = 0; j < length; j++)
                {
                    var o = package.PackageObjects[j];
                    if (o == null)
                    {
                        Debug.LogWarningFormat("package: '{0}' object reference is missing!!!", package.name);
                        continue;
                    }
                    this.cancel = EditorUtility.DisplayCancelableProgressBar("Analysis Bundle Package", o.name, i / (float) count + j / (float) length);
                    if (this.cancel) { throw new Exception("Cancel!!!");}
                    
                    var assetPath = AssetDatabase.GetAssetPath(o);
                    if (AssetDatabase.IsValidFolder(assetPath))
                    {
                        var guids = AssetDatabase.FindAssets("*", new[] {assetPath});
                        foreach (var guid in guids)
                        {
                            var realPath = AssetDatabase.GUIDToAssetPath(guid);
                            if (AssetDatabase.IsValidFolder(realPath)) continue;

                            if (!this.m_Asset2BundleName.TryGetValue(realPath, out string bundleName))
                            {
                                Debug.LogError($"package: {package.name} asset: '{realPath}' is not contains in main assets");
                                continue;
                            }
                            if (!bundlePackage.bundles.Contains(bundleName))
                            {
                                bundlePackage.bundles.Add(bundleName);
                            }
                        }
                    }
                    else
                    {
                        if (!this.m_Asset2BundleName.TryGetValue(assetPath, out string bundleName))
                        {
                            Debug.LogError($"package: '{package.name}' asset: '{assetPath}' is not contains in main assets");
                            continue;
                        }
                        if (!bundlePackage.bundles.Contains(bundleName))
                        {
                            bundlePackage.bundles.Add(bundleName);
                        }
                    }
                }
                this.m_Packages.Add(bundlePackage);
            }
        }

        private void BuildBundles()
        {
            if (this.m_BundleName2Builds.Count == 0) return;
            EditorUtility.DisplayProgressBar("Prepare build assetBundle", "waiting...", 1);
            
            List<AssetBundleBuild> selects = new List<AssetBundleBuild>();
            foreach (var bundle in this.m_BundleName2Builds.Values)
            {
                if (bundle.isRawFile) continue;
                selects.Add(new AssetBundleBuild{assetNames = bundle.assets.ToArray(), assetBundleName = bundle.bundleName});
            }
            
            AssetBundleBuild[] builds = selects.ToArray();
            this.manifest = BuildPipeline.BuildAssetBundles(
                FxBuildPath.BundleRootPath(), 
                builds,
                this.buildAsset.buildAssetBundleOptions | BuildAssetBundleOptions.AppendHashToAssetBundleName,
                EditorUserBuildSettings.activeBuildTarget);
            Debug.Log("create assetBundle finished!");
        }

        private void AnalysisManifest()
        {
            if (this.manifest == null) return;
            this.name2HashName = new Dictionary<string, string>();
            var bundles = this.manifest.GetAllAssetBundles();
            if (bundles.Length == 0) return;
            
            foreach (var bundle in bundles)
            {
                var hash = this.manifest.GetAssetBundleHash(bundle);
                var bundleName = bundle.Replace($"_{hash}", "");
                this.name2HashName.Add(bundleName, bundle);
            }
            
            foreach (var rawFile in this.m_BundleName2Builds.Values)
            {
                if (!rawFile.isRawFile) continue;
                foreach (var asset in rawFile.assets)
                {
                    if (!this.m_Asset2BundleName.TryGetValue(asset, out var bundleName)) continue;
                    var destPath = FxBuildPath.BundleFullPath(bundleName);
                    File.Copy(asset, destPath, true);
                    this.name2HashName.Add(bundleName, bundleName);
                }
            }
        }
        
        private void EncryptBundles()
        {
            if (string.IsNullOrEmpty(this.buildSetting.EncryptType)) return;
            if (string.Equals(this.buildSetting.EncryptType, "None")) return;

            this.mEncrypt = BuildHelper.LoadEncryptObject(this.buildSetting.EncryptType);
            if (this.mEncrypt == null) return;
            
            float index = 1;
            foreach (var bundle in this.name2HashName)
            {
                EditorUtility.DisplayProgressBar("加密Bundle", bundle.Value, index++ / this.name2HashName.Count);
                var bundlePath = FxBuildPath.BundleFullPath(bundle.Value);
                var bundleBytes = File.ReadAllBytes(bundlePath);
                
                if (this.mEncrypt.IsEncrypted(bundleBytes)) continue;

                var encryptBytes = this.mEncrypt.Encrypt(bundleBytes);
                File.WriteAllBytes(bundlePath, encryptBytes);
            }
            Debug.Log($"加密Bundle, 采用加密接口: {this.buildSetting.EncryptType}");
        }
        
        private void AnalysisPackageDependencies()
        {
            foreach (var package in this.m_Packages)
            {
                if (package.bundles.Count == 0) continue;

                var mainBundles = package.bundles.ToArray();
                package.bundles.Clear();
                foreach (var bundle in mainBundles)
                {
                    var hsName = this.name2HashName[bundle];
                    if (!package.bundles.Contains(hsName))
                    {
                        package.bundles.Add(hsName);
                    }
                    var dependencies = this.manifest.GetAllDependencies(hsName);
                    foreach (var dep in dependencies)
                    {
                        if (package.bundles.Contains(dep)) continue;
                        package.bundles.Add(dep);
                    }
                }
            }
        }

        private void WriteManifest()
        {
            var fxManifest = new FxManifest
            {
                EncryptType = this.buildSetting.EncryptType,
                Bundles = new BundleManifest[this.m_BundleName2Builds.Count],
                Assets = new AssetManifest[this.m_Asset2BundleName.Count]
            };

            int index2Id = 0;
            Dictionary<string, int> hashName2Id = new Dictionary<string, int>();
            foreach (var bundle in this.m_BundleName2Builds)
            {
                EditorUtility.DisplayProgressBar("Write manifest", bundle.Key, index2Id / (float) this.m_BundleName2Builds.Count);
                var hsName = this.name2HashName[bundle.Key];
                var bundlePath = FxBuildPath.BundleFullPath(hsName);
                fxManifest.Bundles[index2Id].BundleHashName = hsName;
                fxManifest.Bundles[index2Id].Hash = FxUtility.FileMd5(bundlePath);
                fxManifest.Bundles[index2Id].CRC = FxUtility.FileCrc32(bundlePath);
                fxManifest.Bundles[index2Id].Size = FxUtility.FileSize(bundlePath);
                hashName2Id.Add(hsName, index2Id);
                index2Id++;
            }
            index2Id = 0;
            foreach (var asset in this.m_Asset2BundleName)
            {
                var hashName = this.name2HashName[asset.Value];
                var build = this.m_BundleName2Builds[asset.Value];
                var dependBundles = this.manifest.GetAllDependencies(hashName);
                fxManifest.Assets[index2Id].Path = asset.Key;
                fxManifest.Assets[index2Id].IsRawFile = build.isRawFile;
                fxManifest.Assets[index2Id].HoldBundle = hashName2Id[hashName];
                fxManifest.Assets[index2Id].DependBundles = new int[dependBundles.Length];
                for (int i = 0; i < dependBundles.Length; i++)
                {
                    fxManifest.Assets[index2Id].DependBundles[i] = hashName2Id[dependBundles[i]];
                }
                index2Id++;
            }

            int pLength = this.m_Packages.Count;
            fxManifest.Packages = new PackageManifest[pLength];
            for (int i = 0; i < pLength; i++)
            {
                var package = this.m_Packages[i];
                fxManifest.Packages[i].PackageName = package.packageName;
                fxManifest.Packages[i].Bundles = new int[package.bundles.Count];
                for (int j = 0; j < package.bundles.Count; j++)
                {
                    fxManifest.Packages[i].Bundles[j] = hashName2Id[package.bundles[j]];
                }
            }
            this.buildAsset.bundleVersion++;
            fxManifest.ResVersion = this.buildAsset.bundleVersion;
            fxManifest.AppVersion = this.buildAsset.playerVersion;
            
            var jsonContent = JsonUtility.ToJson(fxManifest, true);
            var trimName = this.buildAsset.name.Replace(" ", "");
            
            var manifestSavePath = FxBuildPath.BundleFullPath($"{trimName}{FxPathHelper.ManifestFileExtension}");
            File.WriteAllText(manifestSavePath, jsonContent);
            Debug.Log($"生成版本清单文件: {manifestSavePath}");

            var vManifestSavePath = FxBuildPath.BundleFullPath(
                    $"{trimName}_V{this.buildAsset.bundleVersion}{FxPathHelper.ManifestFileExtension}");
            File.WriteAllText(vManifestSavePath, jsonContent);

            var manifestHash = FxUtility.FileMd5(manifestSavePath);
            var hashSavePath = FxBuildPath.BundleFullPath($"{trimName}{FxPathHelper.VersionFileExtension}");
            File.WriteAllText(hashSavePath, manifestHash);
            Debug.Log($"生成版本清单Hash文件: {hashSavePath}");
        }

        /// <summary>
        /// 拷贝当前版本差异文件
        /// </summary>
        private void CopyVersionFile()
        {
            EditorUtility.DisplayProgressBar("Copy version difference bundle.", "waiting...", 0);
            var trimName = this.buildAsset.name.Replace(" ", "");
            var oldV = this.buildAsset.bundleVersion - 1;
            var oldFile = FxBuildPath.BundleFullPath($"{trimName}_V{oldV}{FxPathHelper.ManifestFileExtension}");
            if (!File.Exists(oldFile))
                oldFile = FxBuildPath.BundleFullPath($"{trimName}{FxPathHelper.ManifestFileExtension}");
            var oldManifest = FxManifest.Parse(File.ReadAllText(oldFile, Encoding.UTF8));
            var newFile = FxBuildPath.BundleFullPath($"{trimName}_V{this.buildAsset.bundleVersion}{FxPathHelper.ManifestFileExtension}");
            var newManifest = FxManifest.Parse(File.ReadAllText(newFile, Encoding.UTF8));

            var verDir = FxBuildPath.BundleFullPath($"{trimName}_V{this.buildAsset.bundleVersion}");
            if (!Directory.Exists(verDir))
                Directory.CreateDirectory(verDir);

            var diffs = newManifest.Name2BundleManifest.Keys.Except(oldManifest.Name2BundleManifest.Keys).ToList();
            for (int i = 0; i < diffs.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Copy version difference bundle.", diffs[i], i / (float) diffs.Count);
                var path = FxBuildPath.BundleFullPath(diffs[i]);
                var save = $"{verDir}/{diffs[i]}";
                File.Copy(path, save, true);
            }
            var manifestSourcePath = FxBuildPath.BundleFullPath($"{trimName}{FxPathHelper.ManifestFileExtension}");
            var manifestSavePath = $"{verDir}/{trimName}{FxPathHelper.ManifestFileExtension}";
            File.Copy(manifestSourcePath, manifestSavePath);
            
            var verSourcePath = FxBuildPath.BundleFullPath($"{trimName}{FxPathHelper.VersionFileExtension}");
            var verSavePath = $"{verDir}/{trimName}{FxPathHelper.VersionFileExtension}";
            File.Copy(verSourcePath, verSavePath);
        }

        private void RemoveUnityManifest()
        {
            var removeFilePath = FxBuildPath.BundleFullPath(FxBuildPath.BuildPlatformName());
            if (File.Exists(removeFilePath))
            {
                File.Delete(removeFilePath);
            }
            var removeManifest = $"{removeFilePath}.manifest";
            if (File.Exists(removeManifest))
            {
                File.Delete(removeManifest);
            }
        }

        private bool IsBundlePackageNameRepeated(string name)
        {
            foreach (var package in this.m_Packages)
            {
                if (package.packageName == name) return true;
            }
            return false;
        }
        
        /// <summary>
        /// 获取资产 所属 Bundle 包名
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="rootPath"></param>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        private string GetBundleName(BundleMode mode, string rootPath, string assetPath)
        {
            var bundleName = assetPath;
            if (bundleName.EndsWith(".unity") && mode != BundleMode.PackByRaw) mode = BundleMode.PackByFile;
            switch (mode)
            {
                case BundleMode.PackByFile:
                    bundleName = assetPath;
                    break;
                case BundleMode.PackTogether:
                    bundleName = rootPath;
                    break;
                case BundleMode.PackByDirectory:
                    var dir = Path.GetDirectoryName(assetPath);
                    bundleName = !string.IsNullOrEmpty(dir) ? dir.Replace("\\", "/") : string.Empty;
                    break;
                case BundleMode.PackByTopDirectory:
                    int startIndex = assetPath.IndexOf('/', rootPath.Length + 1);
                    bundleName = startIndex == -1 ? rootPath : assetPath.Substring(0, startIndex);
                    break;
                case BundleMode.PackByRaw:
                    var extension = Path.GetExtension(assetPath);
                    var hash = FxUtility.FileMd5(assetPath);
                    bundleName = assetPath.Replace(extension, $".{hash}.raw");
                    break;
            }
            bundleName = bundleName.Replace(this.buildSetting.BundleRootPath, "");
            bundleName = bundleName.Replace("/", "_");
            bundleName = bundleName.Replace(".", "_");
            bundleName = bundleName.Replace(" ", "_");
            bundleName = bundleName.ToLower();
            if (!string.IsNullOrEmpty(this.buildSetting.ExtensionName))
            {
                bundleName = $"{bundleName}{this.buildSetting.ExtensionName}";
            }
            return bundleName;
        }
        
        public void EndBuild()
        {
            this.OnAssetValueChanged();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("build bundle succeed!");
        }

        private void BuildBundlePostProcess()
        {
            foreach (var process in this.bundlePreprocesses)
                process.BuildBundlePost();
        }
        
        public void OnAssetValueChanged() => EditorUtility.SetDirty(this.buildAsset);

        public void Dispose()
        {
            this.manifest = null;
            EditorUtility.ClearProgressBar();
        }

        private struct BundleBuild
        {
            internal string bundleName;
            internal bool isRawFile;
            internal List<string> assets;
        }
        
        private struct BundlePackage
        {
            internal string packageName;
            internal List<string> bundles;
        }
    }
}