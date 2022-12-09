using System.IO;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace FuXi
{
    public partial class DependBundleLoader
    {
        
        internal bool isDone;
        internal float progress;
        internal AssetBundle assetBundle;
        internal readonly FxReference fxReference;
        internal long size => this.m_BundleManifest.Size;

        private string m_PathOrURL;
        private AssetBundleCreateRequest m_BundleRequest;
        private readonly BundleManifest m_BundleManifest;

        private DependBundleLoader(BundleManifest bundleManifest)
        {
            this.m_BundleManifest = bundleManifest;
            this.fxReference = new FxReference();
        }

        internal void StartLoad(bool immediate = false)
        {
            this.isDone = false;
            this.m_PathOrURL = FuXiManager.ManifestVC.BundleRealLoadPath(this.m_BundleManifest);
            
            if (immediate)
                this.LoadBundleInternal();
            else
                this.LoadBundleAsyncInternal();
            this.isDone = immediate;
        }

        internal void Update()
        {
            if (this.isDone) return;
            
            this.progress = this.m_BundleRequest.progress;
            if (!this.m_BundleRequest.isDone) return;
            if (this.m_BundleRequest.assetBundle == null)
                FxDebug.ColorError(FX_LOG_CONTROL.Red, "Load Bundle {0} failure.", this.m_PathOrURL);
            else
            {
                FxDebug.ColorLog(FX_LOG_CONTROL.Cyan, "LoadBundle {0}", this.m_PathOrURL);
                this.assetBundle = this.m_BundleRequest.assetBundle;
            }
            this.isDone = true;
        }

        private void LoadBundleInternal()
        {
            if (FuXiManager.ManifestVC.GameEncrypt == null)
            {
                this.assetBundle = AssetBundle.LoadFromFile(this.m_PathOrURL, 0, 0);
                return;
            }
            if (FuXiManager.ManifestVC.GameEncrypt.EncryptMode == EncryptMode.OFFSET)
            {
                ulong offset = (ulong) FuXiManager.ManifestVC.GameEncrypt.HeadLength;
                this.assetBundle = AssetBundle.LoadFromFile(this.m_PathOrURL, 0, offset);
            }
            else if (FuXiManager.ManifestVC.GameEncrypt.EncryptMode == EncryptMode.XOR)
            {
                using (FileStream fileStream = new FileStream(this.m_PathOrURL, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[fileStream.Length];
                    fileStream.Read(buffer, 0, buffer.Length);
                    buffer = FuXiManager.ManifestVC.GameEncrypt.Decrypt(buffer);
                    this.assetBundle = AssetBundle.LoadFromMemory(buffer, 0);
                }
            }
        }

        private void LoadBundleAsyncInternal()
        {
            if (FuXiManager.ManifestVC.GameEncrypt == null)
            {
                this.m_BundleRequest = AssetBundle.LoadFromFileAsync(this.m_PathOrURL, 0, 0);
                return;
            }
            if (FuXiManager.ManifestVC.GameEncrypt.EncryptMode == EncryptMode.OFFSET)
            {
                ulong offset = (ulong) FuXiManager.ManifestVC.GameEncrypt.HeadLength;
                this.m_BundleRequest = AssetBundle.LoadFromFileAsync(this.m_PathOrURL, 0, offset);
            }
            else if (FuXiManager.ManifestVC.GameEncrypt.EncryptMode == EncryptMode.XOR)
            {
                using (FileStream fileStream = new FileStream(this.m_PathOrURL, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[fileStream.Length];
                    fileStream.Read(buffer, 0, buffer.Length);
                    buffer = FuXiManager.ManifestVC.GameEncrypt.Decrypt(buffer);
                    this.m_BundleRequest = AssetBundle.LoadFromMemoryAsync(buffer, 0);
                }
            }
        }

        private void AddReference()
        {
            this.fxReference.AddRef();
        }

        internal void SubReference()
        {
            if (!this.fxReference.SubRef()) return;
            if (this.fxReference.RefCount < 0)
                FxDebug.ColorWarning(FX_LOG_CONTROL.Orange, "Release over: {0}", this.m_BundleManifest.BundleHashName);
            ReleaseBundleLoader(this.m_BundleManifest);
        }
    }
}