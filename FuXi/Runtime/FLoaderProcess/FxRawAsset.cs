using UnityEngine.Networking;

namespace FuXi
{
    public partial class FxRawAsset : FxAsyncTask
    {
        private enum LoadStep
        {
            Download,
            LoadFile,
        }
        private Downloader m_Downloader;
        private UnityWebRequest m_WebRequest;
        private UnityWebRequestAsyncOperation m_AsyncOperation;
        private BundleManifest m_BundleManifest;
        
        protected string m_PathOrURL;
        private LoadStep m_LoadStep;

        public byte[] Data;
        public string Text => System.Text.Encoding.Default.GetString(this.Data);

        protected FxRawAsset(string path)
        {
            this.m_PathOrURL = path;
        }
        internal override FTask<FxAsyncTask> Execute()
        {
            base.Execute();
            if (FuXiManager.RuntimeMode == RuntimeMode.Editor) return null;
            if (!FuXiManager.ManifestVC.TryGetAssetManifest(this.m_PathOrURL, out var manifest))
            {
                this.tcs.SetResult(this);
                this.isDone = true;
                return this.tcs;
            }
            FuXiManager.ManifestVC.TryGetBundleManifest(manifest.HoldBundle, out this.m_BundleManifest);
            this.m_PathOrURL = FuXiManager.ManifestVC.BundleRealLoadPath(this.m_BundleManifest);
            if (string.IsNullOrEmpty(this.m_PathOrURL))
            {
                this.m_Downloader = new Downloader(this.m_BundleManifest);
                this.m_Downloader.StartDownload();
                this.m_LoadStep = LoadStep.Download;
            }
            else
                this.LoadInternal();
            return this.tcs;
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
                    this.m_Downloader.Dispose();
                    this.LoadInternal();
                    this.m_LoadStep = LoadStep.LoadFile;
                    break;
                case LoadStep.LoadFile:
                    if (!this.m_AsyncOperation.isDone) return;
                    this.Data = this.m_WebRequest.downloadHandler.data;
                    if (this.Data.Length == 0)
                    {
                        FxDebug.ColorWarning(FxDebug.ColorStyle.Orange, "FxRawAsset read file {0} bytes failure", this.m_PathOrURL);
                        return;
                    }
                    if (FuXiManager.ManifestVC.GameEncrypt != null && FuXiManager.ManifestVC.GameEncrypt.IsEncrypted(this.Data))
                    {
                        if (FuXiManager.ManifestVC.GameEncrypt.EncryptMode == EncryptMode.OFFSET)
                        {
                            var header = FuXiManager.ManifestVC.GameEncrypt.EncryptOffset();
                            int newSize = this.Data.Length - header.Length;
                            System.Array.Copy(this.Data, header.Length, this.Data, 0, newSize);
                            System.Array.Resize(ref this.Data, newSize);
                        }
                        else if (FuXiManager.ManifestVC.GameEncrypt.EncryptMode == EncryptMode.XOR)
                        {
                            this.Data = FuXiManager.ManifestVC.GameEncrypt.DeEncrypt(this.Data);
                        }
                    }
                    
                    this.isDone = true;
                    this.tcs.SetResult(this);
                    break;
            }
        }

        private void LoadInternal()
        {
            this.m_PathOrURL = FuXiManager.ManifestVC.BundleRealLoadPath(this.m_BundleManifest);
            this.m_WebRequest = UnityWebRequest.Get(this.m_PathOrURL);
            this.m_AsyncOperation = this.m_WebRequest.SendWebRequest();
            this.m_LoadStep = LoadStep.LoadFile;
        }

        protected override void Dispose()
        {
            this.m_WebRequest?.Dispose();
            this.m_WebRequest = null;
            this.Data = null;
        }
    }
}