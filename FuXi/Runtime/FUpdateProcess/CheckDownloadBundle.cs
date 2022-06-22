using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FuXi
{
    public class CheckDownloadBundle : FxAsyncTask
    {
        private enum CheckDownloadStep
        {
            Downloading,
            Finished,
        }
        
        private readonly Action<float> m_CheckDownload;
        private readonly Queue<BundleManifest> m_BundleList;
        private List<Downloader> m_Downloading;
        private List<Downloader> m_DownloadFinished;

        private CheckDownloadStep m_DownloadStep;

        /// <summary>
        /// 最大同时下载个数
        /// </summary>
        private readonly int m_MaxDownloadCount;
        private readonly long m_DownloadSize;
        private long m_CurDownloadSize;
        
        public string FormatDownloadSize => FxUtility.FormatBytes(this.m_DownloadSize);
        public string FormatCurDownloadSize => FxUtility.FormatBytes(this.m_CurDownloadSize);

        internal CheckDownloadBundle(DownloadInfo downloadInfo, Action<float> checkDownload)
        {
            this.m_BundleList = downloadInfo.DownloadList;
            this.m_DownloadSize = downloadInfo.DownloadSize;
            this.m_MaxDownloadCount = 6;
            this.m_CheckDownload = checkDownload;
            this.m_Downloading = new List<Downloader>(this.m_MaxDownloadCount);
            this.m_DownloadFinished = new List<Downloader>();
        }

        internal override Task<FxAsyncTask> Execute()
        {
            base.Execute();
            this.m_CurDownloadSize = 0;
            this.m_DownloadStep = CheckDownloadStep.Downloading;
            return tcs.Task;
        }

        protected override void Update()
        {
            if (this.isDone) return;

            switch (this.m_DownloadStep)
            {
                case CheckDownloadStep.Downloading:
                    while (this.m_Downloading.Count < this.m_MaxDownloadCount && this.m_BundleList.Count > 0)
                    {
                        var d = new Downloader(this.m_BundleList.Dequeue());
                        this.m_Downloading.Add(d);
                        d.StartDownload();
                    }

                    this.m_CurDownloadSize = 0;
                    foreach (var d in this.m_DownloadFinished)
                    {
                        this.m_CurDownloadSize += d.DownloadSize;
                    }
                    
                    for (int i = 0; i < this.m_Downloading.Count; i++)
                    {
                        var downloader = this.m_Downloading[i];
                        this.m_CurDownloadSize += downloader.DownloadSize;
                        
                        downloader.Update();
                        if (!downloader.isDone) continue;
                        
                        this.m_Downloading.RemoveAt(i);
                        this.m_DownloadFinished.Add(downloader);
                    }

                    if (this.m_Downloading.Count == 0 && this.m_BundleList.Count == 0)
                    {
                        this.m_DownloadStep = CheckDownloadStep.Finished;
                    }
                    this.progress = (float) this.m_CurDownloadSize / (float) this.m_DownloadSize;
                    this.m_CheckDownload?.Invoke(this.progress);
                    break;
                case CheckDownloadStep.Finished:
                    FxManager.ManifestVC.OverrideManifest();
                    this.isDone = true;
                    this.tcs.SetResult(this);
                    break;
            }
        }

        protected override void Dispose()
        {
            base.Dispose();
            foreach (var d in this.m_DownloadFinished) d.Dispose();
            foreach (var d in this.m_Downloading) d.Dispose();
            
            this.m_Downloading.Clear();
            this.m_DownloadFinished.Clear();
            this.m_BundleList.Clear();

            this.m_Downloading = null;
            this.m_DownloadFinished = null;
        }
    }
}