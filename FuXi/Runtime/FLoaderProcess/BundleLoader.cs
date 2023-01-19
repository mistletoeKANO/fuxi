using System.Collections.Generic;

namespace FuXi
{
    public class BundleLoader
    {
        private enum LoadStep
        {
            CheckDownload,
            LoadBundle,
        }
        
        internal bool isDone;
        internal float progress;
        internal DependBundleLoader mainLoader;

        private LoadStep m_Step;
        private List<Downloader> m_Downloader;
        private List<DependBundleLoader> m_LoaderList;

        internal void StartLoad(AssetManifest manifest, bool immediate)
        {
            if (manifest.IsRawFile)
            {
                FxDebug.ColorError(FX_LOG_CONTROL.Orange, "Raw File {0} cant load as bundle!", manifest.Path);
                this.isDone = true;
                return;
            }
            this.m_Downloader = new List<Downloader>();
            this.m_LoaderList = new List<DependBundleLoader>();
            
            if (FuXiManager.ManifestVC.TryGetBundleManifest(manifest.HoldBundle, out var bundleManifest))
            {
                DependBundleLoader.ReferenceBundle(bundleManifest, out this.mainLoader);
                this.m_LoaderList.Add(this.mainLoader);

                var loadPath = FuXiManager.ManifestVC.BundleRealLoadPath(bundleManifest);
                if (string.IsNullOrEmpty(loadPath))
                    this.m_Downloader.Add(new Downloader(bundleManifest));
            }
            if (manifest.DependBundles.Length > 0)
            {
                foreach (var index in manifest.DependBundles)
                {
                    if (!FuXiManager.ManifestVC.TryGetBundleManifest(index, out bundleManifest)) 
                        continue;
                    DependBundleLoader.ReferenceBundle(bundleManifest, out var bundleLoader);
                    this.m_LoaderList.Add(bundleLoader);

                    var loadPath = FuXiManager.ManifestVC.BundleRealLoadPath(bundleManifest);
                    if (string.IsNullOrEmpty(loadPath))
                        this.m_Downloader.Add(new Downloader(bundleManifest));
                }
            }
            
            if (this.m_Downloader.Count > 0)
            {
                if (immediate)
                    FxDebug.ColorError(FX_LOG_CONTROL.Red, "Asset's {0} Bundle or depend bundle is not downloaded, cant load immediate!",
                        manifest.Path);
                else
                {
                    foreach (var downloader in m_Downloader)
                        downloader.StartDownload();
                    this.m_Step = LoadStep.CheckDownload;
                }
            }
            else
            {
                foreach (var loader in m_LoaderList)
                {
                    loader.StartLoad(immediate);
                }
                this.m_Step = LoadStep.LoadBundle;
            }
            this.isDone = immediate;
        }

        internal void Update()
        {
            if (this.isDone) return;

            switch (this.m_Step)
            {
                case LoadStep.CheckDownload:
                    bool isFinishedDownload = true;
                    foreach (var download in m_Downloader)
                    {
                        if (download.isDone) continue;
                        isFinishedDownload = false;
                        download.Update();
                    }
                    if (!isFinishedDownload)
                        break;
                    this.m_Downloader.Clear();
                    foreach (var loader in m_LoaderList)
                    {
                        loader.StartLoad();
                    }
                    this.m_Step = LoadStep.LoadBundle;
                    break;
                case LoadStep.LoadBundle:
                    isFinishedDownload = true;
                    float progressBase = 0.0f;
                    foreach (var bundleLoader in m_LoaderList)
                    {
                        progressBase += bundleLoader.progress;
                        if (bundleLoader.isDone) continue;
                        bundleLoader.Update();
                        isFinishedDownload = false;
                    }
                    this.progress = progressBase / this.m_LoaderList.Count;
                    if (!isFinishedDownload)
                        break;
                    this.isDone = true;
                    break;
            }
        }

        internal void Release()
        {
            if (this.m_LoaderList == null) return;
            foreach (var refLoader in this.m_LoaderList)
                refLoader.SubReference();
        }

        internal void Dispose()
        {
            this.mainLoader = null;
            this.m_LoaderList?.Clear();
        }
    }
}