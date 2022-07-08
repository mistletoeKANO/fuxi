using System.Collections.Generic;

namespace FuXi
{
    public class BundleLoader
    {
        internal bool isDone;
        internal float progress;
        internal DependBundleLoader mainLoader;
        private List<DependBundleLoader> m_LoaderList;

        internal void StartLoad(AssetManifest manifest, bool immediate)
        {
            if (manifest.IsRawFile)
            {
                FxDebug.ColorError(FxDebug.ColorStyle.Orange, "Raw File {0} cant load as bundle!", manifest.Path);
                this.isDone = true;
                return;
            }
            this.m_LoaderList = new List<DependBundleLoader>();
            if (FuXiManager.ManifestVC.TryGetBundleManifest(manifest.HoldBundle, out var bundleManifest))
            {
                if (!DependBundleLoader.TryReferenceBundle(bundleManifest, out this.mainLoader))
                    this.mainLoader.StartLoad(immediate);
                this.m_LoaderList.Add(this.mainLoader);
            }
            if (manifest.DependBundles.Length > 0)
            {
                foreach (var index in manifest.DependBundles)
                {
                    if (!FuXiManager.ManifestVC.TryGetBundleManifest(index, out bundleManifest)) continue;
                    if (!DependBundleLoader.TryReferenceBundle(bundleManifest, out var bundleLoader))
                        bundleLoader.StartLoad(immediate);
                    this.m_LoaderList.Add(bundleLoader);
                }
            }
            this.isDone = immediate;
        }

        internal void Update()
        {
            if (this.isDone) return;
            bool isFinished = true;
            float pro = 0.0f;
            foreach (var bundleLoader in this.m_LoaderList)
            {
                pro += bundleLoader.progress;
                if (bundleLoader.isDone) continue;
                bundleLoader.Update();
                isFinished = bundleLoader.isDone;
            }
            this.progress = pro / this.m_LoaderList.Count;
            this.isDone = isFinished;
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