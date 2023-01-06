using System;
using System.Collections.Generic;

namespace FuXi
{
    public class CheckDownloadBundle : FxAsyncTask
    {
        private enum CheckDownloadStep
        {
            Downloading,
            Finished,
        }
        
        private readonly Action<CheckDownloadBundle> m_CheckDownload;
        private readonly Queue<BundleManifest> m_BundleList;
        private List<Downloader> m_Downloading;
        private List<Downloader> m_DownloadFinished;
        private List<Downloader> m_Temp;

        private CheckDownloadStep m_DownloadStep;

        /// <summary>
        /// 最大同时下载个数
        /// </summary>
        private readonly int m_MaxDownloadCount;
        private readonly long m_DownloadSize;
        private long m_CurDownloadSize;
        
        public int DownloadCount => this.m_BundleList.Count;
        public int DownloadedCount => this.m_DownloadFinished.Count;
        public long DownloadSize => this.m_DownloadSize;
        public long DownloadedSize => this.m_CurDownloadSize;
        public string FormatDownloadSize => FxUtility.FormatBytes(this.m_DownloadSize);
        public string FormatCurDownloadSize => FxUtility.FormatBytes(this.m_CurDownloadSize);
        
        internal CheckDownloadBundle(DownloadInfo downloadInfo, Action<CheckDownloadBundle> checkDownload)
        {
            this.m_BundleList = downloadInfo.DownloadList;
            this.m_DownloadSize = downloadInfo.DownloadSize;
            this.m_MaxDownloadCount = 6;
            this.m_CheckDownload = checkDownload;
            this.m_Downloading = new List<Downloader>(this.m_MaxDownloadCount);
            this.m_DownloadFinished = new List<Downloader>();
            this.m_Temp = new List<Downloader>();
        }

        internal override FTask<FxAsyncTask> Execute()
        {
            base.Execute();
            this.m_CurDownloadSize = 0;
            this.m_DownloadStep = CheckDownloadStep.Downloading;
            return tcs;
        }

        protected override void Update()
        {
            if (this.isDone) return;

            switch (this.m_DownloadStep)
            {
                case CheckDownloadStep.Downloading:
                    while (this.m_Downloading.Count < this.m_MaxDownloadCount && this.m_BundleList.Count > 0)
                    {
                        if (UnityEngine.Application.internetReachability == UnityEngine.NetworkReachability.NotReachable)
                            break;
                        var d = new Downloader(this.m_BundleList.Dequeue());
                        this.m_Downloading.Add(d);
                        d.StartDownload();
                    }

                    this.m_CurDownloadSize = 0;
                    foreach (var d in this.m_DownloadFinished)
                    {
                        this.m_CurDownloadSize += d.DownloadSize;
                    }
                    
                    foreach (var downloader in this.m_Downloading)
                    {
                        this.m_CurDownloadSize += downloader.DownloadSize;
                        downloader.Update();
                        if (!downloader.isDone) continue;
                        
                        this.m_Temp.Add(downloader);
                        if (string.IsNullOrEmpty(downloader.error))
                            this.m_DownloadFinished.Add(downloader);
                        else
                            this.m_BundleList.Enqueue(downloader.m_BundleManifest);
                    }
                    foreach (var down in m_Temp)
                        this.m_Downloading.Remove(down);
                    this.m_Temp.Clear();
                    
                    this.progress = (float) this.m_CurDownloadSize / (float) this.m_DownloadSize;
                    this.m_CheckDownload?.Invoke(this);
                    
                    if (this.m_Downloading.Count <= 0 && this.m_BundleList.Count <= 0)
                        this.m_DownloadStep = CheckDownloadStep.Finished;
                    break;
                case CheckDownloadStep.Finished:
                    FuXiManager.ManifestVC.OverrideManifest();
                    this.isDone = true;
                    this.tcs.SetResult(this);
                    break;
            }
        }

        protected override void Dispose()
        {
            base.Dispose();
            this.m_Downloading.Clear();
            this.m_DownloadFinished.Clear();
            this.m_BundleList.Clear();
            this.m_Temp.Clear();

            this.m_Downloading = null;
            this.m_DownloadFinished = null;
            this.m_Temp = null;
        }
    }
}