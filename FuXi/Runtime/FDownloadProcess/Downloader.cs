
namespace FuXi
{
    public class Downloader
    {
        internal bool isDone;
        internal float progress;
        internal long DownloadSize => this.m_ThreadDownloader.m_DownloadedSize;
        
        private ThreadDownloader m_ThreadDownloader;
        private readonly BundleManifest m_BundleManifest;

        internal Downloader(BundleManifest bundleManifest)
        {
            this.m_BundleManifest = bundleManifest;
            this.m_ThreadDownloader = new ThreadDownloader();
        }

        internal void StartDownload()
        {
            this.m_ThreadDownloader.Start(this.m_BundleManifest);
            FxDebug.ColorLog(FX_LOG_CONTROL.Green, "Download bundle {0}", this.m_BundleManifest.BundleHashName);
        }

        internal void Update()
        {
            if (this.isDone) return;
            
            this.m_ThreadDownloader.Context.Update();
            this.progress = this.m_ThreadDownloader.progress;
            
            this.isDone = this.m_ThreadDownloader.isDone;
        }

        internal void Dispose()
        {
            this.m_ThreadDownloader.Dispose();
            this.m_ThreadDownloader = null;
        }
    }
}