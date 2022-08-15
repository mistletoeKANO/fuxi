using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace FuXi.Editor
{
    internal class BuildPlayerProcess : IBuild
    {
        private readonly Fx_BuildAsset buildAsset;
        private readonly Fx_BuildSetting buildSetting;
        private readonly List<IBuildPlayerPreprocess> playerPreprocesses;

        internal BuildPlayerProcess(Fx_BuildAsset asset = null)
        {
            if (asset == null) return;
            this.buildAsset = asset;
            var assetPath = AssetDatabase.GetAssetPath(asset);
            this.buildSetting = AssetDatabase.LoadAssetAtPath<Fx_BuildSetting>(assetPath);
            this.playerPreprocesses = ProcessingHelper.AcquireAllPlayerPreProcess();
        }

        public void BeginBuild()
        {
            if (this.buildAsset == null) return;
            if (!BuildHelper.IsMatchBuildPlatform(this.buildSetting.FxPlatform)) return;
            EditorExtension.ClearConsole();
            try
            {
                this.BuildPlayerPreProcess();
                this.CopyBuiltinBundles();
                this.BuildPlayer();
                this.EndBuild();
                this.BuildBundlePostProcess();
            }
            catch (Exception e)
            {
                Debug.LogError($"build player failure with error: {e.Message}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            Debug.Log("build player succeed!");
            if (this.buildAsset != null) Selection.activeObject = this.buildAsset;
        }

        public void BeginCopyBundle()
        {
            if (this.buildAsset == null) return;
            if (!BuildHelper.IsMatchBuildPlatform(this.buildSetting.FxPlatform)) return;
            EditorExtension.ClearConsole();
            try
            {
                this.CopyBuiltinBundles();
                AssetDatabase.ImportAsset(FxBuildPath.CopyBundleRootPath());
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError($"copy bundle failure with error: {e.Message}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            Debug.Log("copy bundle finished!");
        }

        public void BeginClearStreamingAssets()
        {
            this.EndBuild();
            Debug.Log("clear finished!");
        }
        
        private void BuildPlayerPreProcess()
        {
            foreach (var process in this.playerPreprocesses)
                process.BuildPlayerPre();
        }

        /// <summary>
        /// 拷贝 初始内建 Bundle 包 和 版本文件
        /// </summary>
        private void CopyBuiltinBundles()
        {
            if (this.buildSetting == null) return;

            var manifestName = this.buildAsset.name.Replace(" ", "");
            var manifestPath = FxBuildPath.BundleFullPath($"{manifestName}{FxPathHelper.ManifestFileExtension}");
            if (!File.Exists(manifestPath)) return;

            var readStr = File.ReadAllText(manifestPath, Encoding.UTF8);
            if (string.IsNullOrEmpty(readStr)) return;
            
            var manifestDest = FxBuildPath.CopyFullSavePath($"{manifestName}{FxPathHelper.ManifestFileExtension}");
            var versionFile = FxBuildPath.BundleFullPath($"{manifestName}{FxPathHelper.VersionFileExtension}");
            var versionDest = FxBuildPath.CopyFullSavePath($"{manifestName}{FxPathHelper.VersionFileExtension}");
            if (this.buildSetting.BuiltinPackages.Count == 0 && !this.buildSetting.CopyAllBundle2Player)
            {
                File.Copy(manifestPath, manifestDest, true);
                File.Copy(versionFile, versionDest, true);
                return;
            }

            var manifest = JsonUtility.FromJson<FxManifest>(readStr);
            for (int i = 0; i < manifest.Bundles.Length; i++)
            {
                manifest.Bundles[i].IsBuiltin = false;
            }
            if (manifest.Packages.Length == 0)
            {
                Debug.LogWarning("manifest package array is empty!");
                return;
            }
            
            var encrypt = BuildHelper.LoadEncryptObject(this.buildSetting.EncryptType);
            if (null != encrypt && encrypt.EncryptMode == EncryptMode.XOR)
            {
                File.Copy(manifestPath, manifestDest, true);
                File.Copy(versionFile, versionDest, true);
                return;
            }

            //拷贝 Bundle 文件
            var builtinBundles = new List<string>();
            if (this.buildSetting.CopyAllBundle2Player)
            {
                for (int i = 0; i < manifest.Bundles.Length; i++)
                {
                    builtinBundles.Add(manifest.Bundles[i].BundleHashName);
                    manifest.Bundles[i].IsBuiltin = true;
                }
            }
            else
            {
                foreach (var p in this.buildSetting.BuiltinPackages)
                {
                    if (p == null)
                    {
                        Debug.LogWarning("build setting package reference is missing!");
                        continue;
                    }
                    if (!this.TryGetBuiltinBundles(manifest, p.name, out var bundles)) continue;
                    foreach (var bundleId in bundles)
                    {
                        var bundleName = manifest.Bundles[bundleId].BundleHashName;
                        if (builtinBundles.Contains(bundleName)) continue;
                        manifest.Bundles[bundleId].IsBuiltin = true;
                        builtinBundles.Add(bundleName);
                    }
                }
            }

            //覆盖 manifest 版本文件
            var manifestText = JsonUtility.ToJson(manifest, true);
            File.WriteAllText(manifestPath, manifestText);
            //覆盖 manifest hash 值文件
            var manifestHash = FxUtility.FileMd5(manifestPath);
            File.WriteAllText(versionFile, manifestHash);
            
            //拷贝 manifest 版本管理文件
            File.Copy(manifestPath, manifestDest, true);
            //拷贝 manifest hash 值文件
            File.Copy(versionFile, versionDest, true);

            if (builtinBundles.Count == 0) return;
            foreach (var bundleName in builtinBundles)
            {
                var bundlePath = FxBuildPath.BundleFullPath(bundleName);
                if (!File.Exists(bundlePath))
                {
                    Debug.LogWarningFormat("bundle file: {0} is not exist", bundlePath);
                    continue;
                }
                var destPath = FxBuildPath.CopyFullSavePath(bundleName);
                File.Copy(bundlePath, destPath, true);
            }
        }

        private bool TryGetBuiltinBundles(FxManifest manifest, string name, out int[] bundles)
        {
            foreach (var pManifest in manifest.Packages)
            {
                if (string.Equals(pManifest.PackageName, name))
                {
                    bundles = pManifest.Bundles;
                    return true;
                }
            }
            bundles = new int[0];
            return false;
        }

        private void BuildPlayer()
        {
            List<string> buildScenes = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled) continue;
                buildScenes.Add(scene.path);
            }
            if (buildScenes.Count == 0)
            {
                Debug.LogError("EditorBuildSettings scenes is empty!");
                return;
            }
            var buildOption = new BuildPlayerOptions
            {
                scenes = buildScenes.ToArray(),
                target = EditorUserBuildSettings.activeBuildTarget,
                locationPathName = FxBuildPath.PlayerFullPath(BuildHelper.GetPlayerName()),
                options = EditorUserBuildSettings.development? BuildOptions.Development : BuildOptions.None
            };
            var buildReport = BuildPipeline.BuildPlayer(buildOption);
            if (buildReport.summary.result == BuildResult.Succeeded)
            {
                this.buildAsset.playerVersion = PlayerSettings.bundleVersion;
                this.OnAssetValueChanged();
            }
        }

        public void EndBuild()
        {
            AssetDatabase.DeleteAsset(FxBuildPath.CopyBundleRootPath());
            if (AssetDatabase.FindAssets("*", new []{FxBuildPath.CopyRootPath()}).Length == 0) 
                AssetDatabase.DeleteAsset(FxBuildPath.CopyRootPath());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        private void BuildBundlePostProcess()
        {
            foreach (var process in this.playerPreprocesses)
                process.BuildPlayerPost();
        }

        public void OnAssetValueChanged() => EditorUtility.SetDirty(this.buildAsset);

        public void Dispose()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}