using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace FuXi
{
    /// <summary>
    /// 伏羲 资源管理器
    /// </summary>
    public static class FuXiManager
    {
        /// <summary>
        /// 版本管理器
        /// </summary>
        internal static FxManifestDriver ManifestVC;
        internal static Func<FxManifest> ParseManifestCallback;  // 用在Unity编辑器下初始化本地配置
        internal static string PlatformURL;
        /// <summary>
        /// 运行模式
        /// </summary>
        public static RuntimeMode RuntimeMode = RuntimeMode.Editor;
        
        /// <summary>
        /// FuXi启动器
        /// </summary>
        /// <param name="versionFileName">版本文件名称</param>
        /// <param name="url">资源服务器地址</param>
        /// <param name="runtimeMode">运行模式</param>
        /// <returns></returns>
        public static async FTask FxLauncherAsync (
            string versionFileName,
            string url,
            RuntimeMode runtimeMode = RuntimeMode.Editor)
        {
            try
            {
                InitInternal(versionFileName, url, runtimeMode);
                if (FuXiManager.RuntimeMode == RuntimeMode.Editor)
                {
                    FuXiManager.ManifestVC.NewManifest = ParseManifestCallback?.Invoke();
                    FuXiManager.ManifestVC.InitEncrypt();
                }
                else
                {
                    FxAsset.FxAssetCreate = FxAsset.CreateAsset;
                    FxScene.FxSceneCreate = FxScene.CreateScene;
                    FxRawAsset.FxRawAssetCreate = FxRawAsset.CreateRawAsset;
                    await new CheckLocalManifest().Execute();
                }
            }
            catch (Exception e)
            {
                FxDebug.LogError(e.Message);
            }
        }

        public static CheckLocalManifest FxLauncherAsyncCo(
            string versionFileName,
            string url,
            RuntimeMode runtimeMode = RuntimeMode.Editor)
        {
            CheckLocalManifest check = null;
            InitInternal(versionFileName, url, runtimeMode);
            if (FuXiManager.RuntimeMode == RuntimeMode.Editor)
            {
                FuXiManager.ManifestVC.NewManifest = ParseManifestCallback?.Invoke();
                FuXiManager.ManifestVC.InitEncrypt();
                check = new CheckLocalManifest{isDone = true};
            }
            else
            {
                FxAsset.FxAssetCreate = FxAsset.CreateAsset;
                FxScene.FxSceneCreate = FxScene.CreateScene;
                FxRawAsset.FxRawAssetCreate = FxRawAsset.CreateRawAsset;
                check = new CheckLocalManifest();
                check.Execute();
            }
            return check;
        }

        private static void InitInternal(
            string versionFileName,
            string url,
            RuntimeMode runtimeMode)
        {
            FuXiManager.ManifestVC = new FxManifestDriver(versionFileName);
            FuXiManager.PlatformURL = url;
            FuXiManager.RuntimeMode = runtimeMode;
            if (FuXiManager.RuntimeMode == RuntimeMode.Editor && !Application.isEditor)
            {
                FuXiManager.RuntimeMode = RuntimeMode.Offline;
            }
            var root = GameObject.Find("__FuXi Asset Manager__");
            if (root == null)
            {
                root = new GameObject("__FuXi Asset Manager__");
                root.AddComponent<AssetPolling>();
            }
            UnityEngine.Object.DontDestroyOnLoad(root);
        }
        
        public static void ResetDownLoadURL(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            PlatformURL = url;
        }
        
        #region Task 版本
        
        /// <summary>
        /// 检查服务器版本文件
        /// </summary>
        /// <param name="checkUpdate"></param>
        /// <returns></returns>
        public static async FTask FxCheckUpdate(Action<float> checkUpdate = null)
        {
            if (FuXiManager.RuntimeMode != RuntimeMode.Runtime) return;
            try
            {
                await new CheckWWWManifest(checkUpdate).Execute();
                FxDebug.Log("Check update finished!");
            }
            catch (Exception e)
            {
                FxDebug.LogError(e.Message);
            }
        }

        /// <summary>
        /// 获取更新大小，分包更新
        /// </summary>
        /// <param name="packages">分包列表</param>
        /// <param name="checkDownload"></param>
        /// <returns></returns>
        public static async FTask<DownloadInfo> FxCheckDownloadSize(string[] packages, Action<float> checkDownload = null)
        {
            if (FuXiManager.RuntimeMode != RuntimeMode.Runtime) return default;
            CheckDownloadSize c = (CheckDownloadSize) await new CheckDownloadSize(packages, checkDownload).Execute();
            return c.DownloadInfo;
        }

        /// <summary>
        /// 获取更新大小，默认全局更新
        /// </summary>
        /// <param name="containsPackage">是否包含分包</param>
        /// <param name="checkDownload"></param>
        /// <returns></returns>
        public static async FTask<DownloadInfo> FxCheckDownloadSize(bool containsPackage = false, Action<float> checkDownload = null)
        {
            if (FuXiManager.RuntimeMode != RuntimeMode.Runtime) return default;
            CheckDownloadSize c = (CheckDownloadSize) await new CheckDownloadSize(containsPackage, checkDownload).Execute();
            return c.DownloadInfo;
        }

        /// <summary>
        /// 检查下载
        /// </summary>
        /// <param name="downloadInfo"></param>
        /// <param name="checkDownload"></param>
        /// <returns></returns>
        public static async FTask FxCheckDownload(DownloadInfo downloadInfo, Action<CheckDownloadBundle> checkDownload = null)
        {
            if (FuXiManager.RuntimeMode != RuntimeMode.Runtime) return;
            try
            {
                await new CheckDownloadBundle(downloadInfo, checkDownload).Execute();
            }
            catch (Exception e)
            {
                FxDebug.LogError(e.Message);
            }
        }

        #endregion

        #region Coroutine 协程版本

        /// <summary>
        /// 检查服务器版本文件 协程版本
        /// </summary>
        /// <param name="checkUpdate"></param>
        /// <returns></returns>
        public static CheckWWWManifest FxCheckUpdateCo(Action<float> checkUpdate = null)
        {
            if (FuXiManager.RuntimeMode != RuntimeMode.Runtime) return default;
            var check = new CheckWWWManifest(checkUpdate);
            check.Execute();
            return check;
        }
        
        /// <summary>
        /// 获取更新大小，分包更新 协程版本
        /// </summary>
        /// <param name="packages"></param>
        /// <param name="checkDownload"></param>
        /// <returns></returns>
        public static CheckDownloadSize FxCheckDownloadSizeCo(string[] packages, Action<float> checkDownload = null)
        {
            if (FuXiManager.RuntimeMode != RuntimeMode.Runtime) return default;
            var check = new CheckDownloadSize(packages, checkDownload);
            check.Execute();
            return check;
        }
        
        /// <summary>
        /// 获取更新大小，默认全局更新 协程版本
        /// </summary>
        /// <param name="containsPackage">是否包含分包</param>
        /// <param name="checkDownload"></param>
        /// <returns></returns>
        public static CheckDownloadSize FxCheckDownloadSizeCo(bool containsPackage = false, Action<float> checkDownload = null)
        {
            if (FuXiManager.RuntimeMode != RuntimeMode.Runtime) return new CheckDownloadSize {isDone = true};
            var check = new CheckDownloadSize(containsPackage, checkDownload);
            check.Execute();
            return check;
        }
        
        /// <summary>
        /// 检查下载 协程版本
        /// </summary>
        /// <param name="downloadInfo"></param>
        /// <param name="checkDownload"></param>
        /// <returns></returns>
        public static CheckDownloadBundle FxCheckDownloadCo(DownloadInfo downloadInfo, Action<CheckDownloadBundle> checkDownload = null)
        {
            if (FuXiManager.RuntimeMode != RuntimeMode.Runtime)
                return new CheckDownloadBundle(default, null) {isDone = true};
            var check = new CheckDownloadBundle(downloadInfo, checkDownload);
            check.Execute();
            return check;
        }

        #endregion
    }
}