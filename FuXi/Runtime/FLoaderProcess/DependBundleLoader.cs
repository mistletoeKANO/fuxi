using System.IO;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace FuXi
{
    public partial class DependBundleLoader
    {
        private enum LoadStep
        {
            DownLoad,
            LoadBundle,
        }
        internal bool isDone;
        internal float progress;
        internal AssetBundle assetBundle;
        internal readonly FxReference fxReference;
        internal long size => this.m_BundleManifest.Size;

        private string m_PathOrURL;
        private AssetBundleCreateRequest m_BundleRequest;
        private readonly BundleManifest m_BundleManifest;
        private Downloader m_Downloader;
        private LoadStep m_LoadStep;

        private DependBundleLoader(BundleManifest bundleManifest)
        {
            this.m_BundleManifest = bundleManifest;
            this.fxReference = new FxReference();
        }

        internal void StartLoad(bool immediate = false)
        {
            this.isDone = false;
            this.m_PathOrURL = FxManager.ManifestVC.BundleRealLoadPath(this.m_BundleManifest);
            if (string.IsNullOrEmpty(this.m_PathOrURL))
            {
                if (immediate)
                {
                    this.m_Downloader = new Downloader(this.m_BundleManifest);
                    this.m_Downloader.StartDownloadAwait().ConfigureAwait(false).GetAwaiter();
                }
                else
                {
                    this.m_Downloader = new Downloader(this.m_BundleManifest);
                    this.m_Downloader.StartDownload();
                    this.m_LoadStep = LoadStep.DownLoad;
                    return;
                }
            }
            if (immediate)
                this.LoadBundleInternal();
            else
            {
                this.LoadBundleAsyncInternal();
                this.m_LoadStep = LoadStep.LoadBundle;
            }
            this.isDone = immediate;
        }

        internal void Update()
        {
            if (this.isDone) return;

            switch (this.m_LoadStep)
            {
                case LoadStep.DownLoad:
                    this.progress = 0.3f * this.m_Downloader.progress;
                    if (!this.m_Downloader.isDone)
                    {
                        this.m_Downloader.Update();
                        return;
                    }
                    this.m_Downloader.Dispose();
                    this.m_PathOrURL = FxManager.ManifestVC.BundleRealLoadPath(this.m_BundleManifest);
                    this.LoadBundleAsyncInternal();
                    this.m_LoadStep = LoadStep.LoadBundle;
                    break;
                case LoadStep.LoadBundle:
                    this.progress = 0.3f + this.m_BundleRequest.progress * 0.7f;
                    if (!this.m_BundleRequest.isDone) return;
                    FxDebug.ColorLog(FxDebug.ColorStyle.Cyan, "LoadBundle {0}", this.m_PathOrURL);
                    this.assetBundle = this.m_BundleRequest.assetBundle;
                    this.isDone = true;
                    break;
            }
        }

        private void LoadBundleInternal()
        {
            if (FxManager.ManifestVC.GameEncrypt != null)
            {
                if (FxManager.ManifestVC.GameEncrypt.EncryptMode == EncryptMode.OFFSET)
                {
                    ulong offset = (ulong) FxManager.ManifestVC.GameEncrypt.EncryptOffset().Length;
                    this.assetBundle = AssetBundle.LoadFromFile(this.m_PathOrURL, 0, offset);
                }else if (FxManager.ManifestVC.GameEncrypt.EncryptMode == EncryptMode.XOR)
                {
                    using (FileStream fileStream = new FileStream(this.m_PathOrURL, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[fileStream.Length];
                        fileStream.Read(buffer, 0, buffer.Length);
                        buffer = FxManager.ManifestVC.GameEncrypt.DeEncrypt(buffer);
                        this.assetBundle = AssetBundle.LoadFromMemory(buffer, 0);
                    }
                }
            }
            else
                this.assetBundle = AssetBundle.LoadFromFile(this.m_PathOrURL, 0, 0);
        }

        private void LoadBundleAsyncInternal()
        {
            if (FxManager.ManifestVC.GameEncrypt != null)
            {
                if (FxManager.ManifestVC.GameEncrypt.EncryptMode == EncryptMode.OFFSET)
                {
                    ulong offset = (ulong) FxManager.ManifestVC.GameEncrypt.EncryptOffset().Length;
                    this.m_BundleRequest = AssetBundle.LoadFromFileAsync(this.m_PathOrURL, 0, offset);
                }else if (FxManager.ManifestVC.GameEncrypt.EncryptMode == EncryptMode.XOR)
                {
                    using (FileStream fileStream = new FileStream(this.m_PathOrURL, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[fileStream.Length];
                        fileStream.Read(buffer, 0, buffer.Length);
                        buffer = FxManager.ManifestVC.GameEncrypt.DeEncrypt(buffer);
                        this.m_BundleRequest = AssetBundle.LoadFromMemoryAsync(buffer, 0);
                    }
                }
            }
            else
                this.m_BundleRequest = AssetBundle.LoadFromFileAsync(this.m_PathOrURL, 0, 0);
        }

        private void AddReference()
        {
            this.fxReference.AddRef();
        }

        internal void SubReference()
        {
            if (!this.fxReference.SubRef()) return;
            if (this.fxReference.RefCount < 0)
                FxDebug.ColorWarning(FxDebug.ColorStyle.Orange, "Release over: {0}", this.m_BundleManifest.BundleHashName);
            ReleaseBundleLoader(this.m_BundleManifest);
        }
    }
}