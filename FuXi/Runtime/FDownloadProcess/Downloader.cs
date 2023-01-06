
namespace FuXi
{
    public class Downloader
    {
        internal bool isDone;
        internal string error;

        /// <summary>
        /// 超过规定时间下载大小 未变化 则按超时处理
        /// </summary>
        private readonly float timeout;
        private float curTime;
        private long lastDownloadSize;
        internal long DownloadSize => this.m_ThreadDownloader.m_DownloadedSize;
        
        private readonly ThreadDownloader m_ThreadDownloader;
        internal readonly BundleManifest m_BundleManifest;

        internal Downloader(BundleManifest bundleManifest)
        {
            this.timeout = 10;
            this.m_BundleManifest = bundleManifest;
            this.m_ThreadDownloader = new ThreadDownloader();
        }

        internal void StartDownload()
        {
            this.m_ThreadDownloader.Start(this.m_BundleManifest);
            this.curTime = UnityEngine.Time.realtimeSinceStartup;
        }

        internal void Update()
        {
            if (this.isDone) 
                return;
            this.isDone = this.m_ThreadDownloader.isDone;

            if (this.isDone)
            {
                if (!string.IsNullOrEmpty(this.m_ThreadDownloader.error))
                {
                    this.error = this.m_ThreadDownloader.error;
                    FxDebug.ColorError(FX_LOG_CONTROL.Red, this.m_ThreadDownloader.error);
                }
                else
                    FxDebug.ColorLog(FX_LOG_CONTROL.Green, "Download bundle {0}", this.m_BundleManifest.BundleHashName);
                this.m_ThreadDownloader.Dispose();
            }
            if (UnityEngine.Time.realtimeSinceStartup - this.curTime > this.timeout)
            {
                this.isDone = true;
                this.error = "Download timeout.";
                this.m_ThreadDownloader.Abort();
                return;
            }
            if (this.lastDownloadSize != DownloadSize)
            {
                this.lastDownloadSize = DownloadSize;
                this.curTime = UnityEngine.Time.realtimeSinceStartup;
            }
        }
    }
}