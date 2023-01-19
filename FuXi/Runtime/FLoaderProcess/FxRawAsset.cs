using System.Collections.Generic;
using UnityEngine.Networking;

namespace FuXi
{
    public partial class FxRawAsset : FxAsyncTask
    {
        protected enum LoadStep
        {
            Download,
            LoadFile,
        }
        private Downloader m_Downloader;
        private UnityWebRequest m_WebRequest;
        private UnityWebRequestAsyncOperation m_AsyncOperation;
        private BundleManifest m_BundleManifest;

        protected readonly List<FTask<FxRawAsset>> m_TcsList;
        protected string m_PathOrURL;
        protected LoadStep m_LoadStep;

        public byte[] Data;
        public string Text => System.Text.Encoding.Default.GetString(this.Data);

        protected FxRawAsset(string path) : base(false)
        {
            this.m_PathOrURL = path;
            this.m_TcsList = new List<FTask<FxRawAsset>>();
        }
        protected virtual FTask<FxRawAsset> Execute()
        {
            var tcs = FTask<FxRawAsset>.Create(true);
            this.m_TcsList.Add(tcs);
            if (!FuXiManager.ManifestVC.TryGetAssetManifest(this.m_PathOrURL, out var manifest))
            {
                this.LoadFinished();
            }
            FuXiManager.ManifestVC.TryGetBundleManifest(manifest.HoldBundle, out this.m_BundleManifest);
            this.m_PathOrURL = FuXiManager.ManifestVC.BundleRealLoadPath(this.m_BundleManifest, true);
            if (string.IsNullOrEmpty(this.m_PathOrURL))
            {
                this.m_Downloader = new Downloader(this.m_BundleManifest);
                this.m_Downloader.StartDownload();
                this.m_LoadStep = LoadStep.Download;
            }
            else
                this.LoadInternal();
            return tcs;
        }

        private FTask<FxRawAsset> GetRawAsset()
        {
            var tcs = FTask<FxRawAsset>.Create(true);
            this.m_TcsList.Add(tcs);
            return tcs;
        }

        protected override void Update()
        {
            if (this.isDone) return;
            switch (this.m_LoadStep)
            {
                case LoadStep.Download:
                    if (!this.m_Downloader.isDone)
                    {
                        this.m_Downloader.Update();
                        return;
                    }
                    this.LoadInternal();
                    break;
                case LoadStep.LoadFile:
                    if (!this.m_AsyncOperation.isDone) return;
                    this.Data = this.m_WebRequest.downloadHandler.data;
                    if (this.Data.Length > 0)
                    {
                        if (FuXiManager.ManifestVC.GameEncrypt != null && FuXiManager.ManifestVC.GameEncrypt.IsEncrypted(this.Data))
                        {
                            if (FuXiManager.ManifestVC.GameEncrypt.EncryptMode == EncryptMode.OFFSET)
                            {
                                var headerLength = FuXiManager.ManifestVC.GameEncrypt.HeadLength;
                                int newSize = this.Data.Length - headerLength;
                                System.Array.Copy(this.Data, headerLength, this.Data, 0, newSize);
                                System.Array.Resize(ref this.Data, newSize);
                            }
                            else if (FuXiManager.ManifestVC.GameEncrypt.EncryptMode == EncryptMode.XOR)
                            {
                                this.Data = FuXiManager.ManifestVC.GameEncrypt.Decrypt(this.Data);
                            }
                        }
                        FxDebug.ColorLog(FX_LOG_CONTROL.Cyan, "Load RawAsset {0}", this.m_PathOrURL);
                    }else
                        FxDebug.ColorError(FX_LOG_CONTROL.Red, "FxRawAsset read file {0} bytes failure", this.m_PathOrURL);
                    this.LoadFinished();
                    break;
            }
        }

        private void LoadInternal()
        {
            this.m_PathOrURL = FuXiManager.ManifestVC.BundleRealLoadPath(this.m_BundleManifest, true);
            this.m_WebRequest = UnityWebRequest.Get(this.m_PathOrURL);
            this.m_AsyncOperation = this.m_WebRequest.SendWebRequest();
            this.m_LoadStep = LoadStep.LoadFile;
        }

        protected void LoadFinished()
        {
            this.isDone = true;
            foreach (var task in this.m_TcsList)
            {
                task.SetResult(this);
            }
            this.m_TcsList.Clear();
        }

        protected override void Dispose()
        {
            this.m_WebRequest?.Dispose();
            this.m_WebRequest = null;
            this.Data = null;
        }
    }
}