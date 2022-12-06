using System;
using UnityEngine.Networking;

// ReSharper disable once CheckNamespace
namespace FuXi
{
    public class CheckWWWManifest : FxAsyncTask
    {
        private enum CheckVersionSteps
        {
            DownloadLocalVersion,
            DownloadServerVersion,
            CompareVersionHash,
            DownloadServerManifest,
        }
        
        private UnityWebRequestAsyncOperation m_AsyncOperation;
        private UnityWebRequest m_UnityWebRequest;
        private CheckVersionSteps m_CurStep;
        
        private readonly string m_LocalVersion;
        private readonly string m_ServerVersion;
        private readonly string m_ServerManifest;

        private string m_LocalVer;
        private string m_ServerVer;

        private int m_RetryCount = 3;
        private int m_CurRetryCount = 0;
        private string m_CurUrl = String.Empty;

        private readonly System.Action<float> m_UpdateProgress;
        private int m_StepNum = 0;

        public CheckWWWManifest(System.Action<float> updateProgress)
        {
            this.m_UpdateProgress = updateProgress;
            this.m_LocalVersion = FxPathHelper.PersistentLoadURL(FuXiManager.ManifestVC.VersionName);
            if (!System.IO.File.Exists(this.m_LocalVersion))
                this.m_LocalVersion = FxPathHelper.StreamingLoadURL(FuXiManager.ManifestVC.VersionName);
            this.m_ServerVersion = $"{FuXiManager.PlatformURL}{FuXiManager.ManifestVC.VersionName}";
            this.m_ServerManifest = $"{FuXiManager.PlatformURL}{FuXiManager.ManifestVC.ManifestName}";
        }

        internal override FTask<FxAsyncTask> Execute()
        {
            base.Execute();
            this.m_StepNum = 0;
            this.m_CurUrl = this.m_LocalVersion;
            this.SendWebRequest(this.m_LocalVersion);
            this.m_CurStep = CheckVersionSteps.DownloadLocalVersion;
            return tcs;
        }

        private void SendWebRequest(string url)
        {
            this.m_UnityWebRequest?.Dispose();
            this.m_UnityWebRequest = UnityWebRequest.Get(url);
            this.m_UnityWebRequest.timeout = 6;
            this.m_UnityWebRequest.disposeDownloadHandlerOnDispose = true;
            this.m_AsyncOperation = this.m_UnityWebRequest.SendWebRequest();
        }

        protected override void Update()
        {
            if (this.isDone) return;
            this.progress = this.m_UnityWebRequest.downloadProgress * 0.25f + this.m_StepNum / 4f;
            this.m_UpdateProgress?.Invoke(this.progress);
            if (!this.m_AsyncOperation.isDone) return;
            
            switch (this.m_CurStep)
            {
                case CheckVersionSteps.DownloadLocalVersion:
                    if (!string.IsNullOrEmpty(this.m_UnityWebRequest.error))
                    {
                        if (this.CheckRetry())
                            break;
                        FxDebug.LogError($"Load local version hash with error: {this.m_UnityWebRequest.error}");
                        this.m_LocalVer = "errorLocalVer";
                    }else
                    {
                        this.m_LocalVer = System.Text.Encoding.UTF8.GetString(this.m_UnityWebRequest.downloadHandler.data);
                        FxDebug.Log($"Local Version Hash:{this.m_LocalVer}");
                    }

                    this.m_CurUrl = this.m_ServerVersion;
                    this.SendWebRequest(this.m_ServerVersion);
                    this.m_CurStep = CheckVersionSteps.DownloadServerVersion;
                    this.m_StepNum++;
                    break;
                case CheckVersionSteps.DownloadServerVersion:
                    if (!string.IsNullOrEmpty(this.m_UnityWebRequest.error))
                    {
                        if (this.CheckRetry())
                            break;
                        FxDebug.LogError($"Load server version hash with error: {this.m_UnityWebRequest.error}");
                        this.m_ServerVer = "errorServerVer";
                    }else
                    {
                        this.m_ServerVer = System.Text.Encoding.UTF8.GetString(this.m_UnityWebRequest.downloadHandler.data);
                        FxDebug.Log($"Server Version Hash:{this.m_ServerVer}");
                    }
                    
                    FuXiManager.ManifestVC.NewHash = this.m_ServerVer;
                    this.m_CurStep = CheckVersionSteps.CompareVersionHash;
                    this.m_StepNum++;
                    break;
                case CheckVersionSteps.CompareVersionHash:
                    if (this.m_LocalVer == this.m_ServerVer)
                    {
                        this.isDone = true;
                        this.tcs.SetResult(this);
                    }else
                    {
                        this.m_CurUrl = this.m_ServerManifest;
                        this.SendWebRequest(this.m_ServerManifest);
                        this.m_CurStep = CheckVersionSteps.DownloadServerManifest;
                    }
                    this.m_StepNum++;
                    break;
                case CheckVersionSteps.DownloadServerManifest:
                    if (!string.IsNullOrEmpty(this.m_UnityWebRequest.error))
                    {
                        if (this.CheckRetry())
                            break;
                        FxDebug.ColorError(FX_LOG_CONTROL.Red, "Download Server Manifest {0} with error: {1}",
                            this.m_CurUrl, this.m_UnityWebRequest.error);
                    }else
                    {
                        var readValue = System.Text.Encoding.UTF8.GetString(this.m_UnityWebRequest.downloadHandler.data);
                        FuXiManager.ManifestVC.NewManifest = FxManifest.Parse(readValue);
                        FuXiManager.ManifestVC.InitEncrypt();
                        FxDebug.Log($"Download Server Manifest: {this.m_ServerManifest}");
                    }

                    this.progress = 1;
                    this.isDone = true;
                    this.tcs.SetResult(this);
                    break;
            }
        }

        private bool CheckRetry()
        {
            if (this.m_CurRetryCount >= this.m_RetryCount)
            {
                this.m_CurRetryCount = 0;
                return false;
            }
            
            this.m_CurRetryCount++;
            FxDebug.ColorLog(FX_LOG_CONTROL.Red, "Retry download {0} retry count: {1}.", this.m_CurUrl, this.m_CurRetryCount);
            this.SendWebRequest(this.m_CurUrl);
            return true;
        }

        protected override void Dispose()
        {
            this.m_UnityWebRequest?.Dispose();
            this.m_UnityWebRequest = null;
            this.m_AsyncOperation = null;
        }
    }
}