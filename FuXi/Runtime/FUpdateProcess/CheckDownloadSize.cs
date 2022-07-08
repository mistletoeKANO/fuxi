using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FuXi
{
    public class CheckDownloadSize : FxAsyncTask
    {
        private enum CheckSteps
        {
            CheckNormal,
            CheckPackage,
            checkValid,
            Finished
        }
        
        public DownloadInfo DownloadInfo;
        private Action<float> m_CheckUpdate;
        private CheckSteps m_CheckStep;
        private readonly bool m_ContainsPackage = false;
        private string[] m_Packages;
        private Queue<BundleManifest> m_BundleManifest;

        internal CheckDownloadSize(bool containsPackage = true, Action<float> action = null)
        {
            this.m_ContainsPackage = containsPackage;
            this.m_CheckUpdate = action;
        }

        internal CheckDownloadSize(string[] package, Action<float> action = null)
        {
            this.m_Packages = package;
            this.m_CheckUpdate = action;
        }

        internal override Task<FxAsyncTask> Execute()
        {
            base.Execute();
            this.DownloadInfo = new DownloadInfo{DownloadList = new Queue<BundleManifest>()};
            this.m_BundleManifest = new Queue<BundleManifest>();
            this.m_CheckStep = null != this.m_Packages ? CheckSteps.CheckPackage : CheckSteps.CheckNormal;
            return tcs.Task;
        }

        protected override void Update()
        {
            if (this.isDone) return;
            switch (this.m_CheckStep)
            {
                case CheckSteps.CheckNormal:
                    if (this.m_ContainsPackage)
                    {
                        foreach (var m in FuXiManager.ManifestVC.NewManifest.Bundles)
                            this.m_BundleManifest.Enqueue(m);
                    }
                    else
                    {
                        var packageBundles = FuXiManager.ManifestVC.GetPackagesBundle();
                        int length = FuXiManager.ManifestVC.NewManifest.Bundles.Length;
                        for (int i = 0; i < length; i++)
                        {
                            if (packageBundles.Contains(i)) continue;
                            this.m_BundleManifest.Enqueue(FuXiManager.ManifestVC.NewManifest.Bundles[i]);
                        }
                    }
                    this.progress = 0.1f;
                    this.m_CheckUpdate?.Invoke(this.progress);
                    this.m_CheckStep = CheckSteps.checkValid;
                    break;
                case CheckSteps.CheckPackage:
                    if (this.m_Packages.Length == 0)
                    {
                        this.m_CheckStep = CheckSteps.Finished;
                        break;
                    }
                    List<int> ids = FuXiManager.ManifestVC.GetPackagesBundle(this.m_Packages);
                    foreach (var i in ids)
                    {
                        this.m_BundleManifest.Enqueue(FuXiManager.ManifestVC.NewManifest.Bundles[i]);
                    }
                    this.progress = 0.1f;
                    this.m_CheckUpdate?.Invoke(this.progress);
                    this.m_CheckStep = CheckSteps.checkValid;
                    break;
                case CheckSteps.checkValid:
                    if (this.m_BundleManifest.Count == 0)
                    {
                        this.m_CheckStep = CheckSteps.Finished;
                        break;
                    }
                    while (this.m_BundleManifest.Count > 0)
                    {
                        var bm = this.m_BundleManifest.Dequeue();
                        var downloadState = FuXiManager.ManifestVC.Downloaded(bm.BundleHashName);
                        if (downloadState.Valid) continue;

                        this.DownloadInfo.DownloadSize += bm.Size - downloadState.Size;
                        this.DownloadInfo.DownloadList.Enqueue(bm);
                    }
                    break;
                case CheckSteps.Finished:
                    this.progress = 1f;
                    this.m_CheckUpdate?.Invoke(this.progress);
                    this.isDone = true;
                    this.tcs.SetResult(this);
                    break;
            }
        }

        protected override void Dispose()
        {
            this.m_BundleManifest.Clear();
            this.m_CheckUpdate = null;
            this.m_Packages = null;
        }
    }
}