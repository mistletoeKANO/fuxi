using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace FuXi
{
    public class ThreadDownloader
    {
        private const int m_BufferSize = 1024 * 4;
        private const string m_FtpUserName = "";
        private const string m_FtpPassword = "";
        
        private Thread m_Thread;
        private string m_URL;
        private string m_SavePath;
        
        private bool m_Running = false;
        
        private long m_MaxSize;
        private string m_Crc;
        private int m_RetryCount;

        internal string error;
        internal bool isDone = false;
        internal float progress = 0f;
        internal long m_DownloadedSize;
        internal readonly DownloadThreadSyncContext Context;

        internal ThreadDownloader() { this.Context = new DownloadThreadSyncContext(); }

        internal void Start(BundleManifest manifest)
        {
            this.m_URL = $"{FxManager.PlatformURL}{manifest.BundleHashName}";
            this.m_SavePath = FxPathHelper.PersistentLoadPath(manifest.BundleHashName);

            this.m_Crc = manifest.CRC;
            this.m_MaxSize = manifest.Size;
            this.m_DownloadedSize = 0;
            this.m_RetryCount = 2;

            this.StartThread();
        }

        private void StartThread()
        {
            this.isDone = false;
            this.m_Running = true;
            this.m_Thread = new Thread(this.RunThread) {IsBackground = true};
            this.m_Thread.Start();
        }

        private void RunThread()
        {
            FileStream fileStream = null; WebResponse webResponse = null; Stream respStream = null;
            try
            {
                fileStream = new FileStream(this.m_SavePath, FileMode.OpenOrCreate, FileAccess.Write);

                var resumeLength = fileStream.Length;
                // 注意：设置本地文件流的起始位置, 断点续传
                if (resumeLength > 0) fileStream.Seek(resumeLength, SeekOrigin.Begin);
                
                var webRequest = this.CreateWebRequest(resumeLength);
                webResponse = webRequest.GetResponse();
                respStream = webResponse.GetResponseStream();

                byte[] buffer = new byte[m_BufferSize];
                while (this.m_Running && respStream != null)
                {
                    int readLength = respStream.Read(buffer, 0, buffer.Length);
                    if (readLength <= 0) break;
                    fileStream.Write(buffer, 0, readLength);
                    
                    this.m_DownloadedSize += readLength;
                    this.progress = (float) this.m_DownloadedSize / this.m_MaxSize;
                }
            }
            catch (Exception e)
            {
                this.Context.Post(this.ThrowDownloadError, string.Concat($"文件下载出错:{e.Message}", "{0}"));
                this.error = e.Message;
            }
            finally
            {
                respStream?.Close();
                respStream?.Dispose();
                
                webResponse?.Close();
                webResponse?.Dispose();
                
                fileStream?.Close();
                fileStream?.Dispose();
                if (!string.IsNullOrEmpty(this.error) && this.m_RetryCount > 0)
                {
                    Thread.Sleep(1000);
                    this.m_RetryCount--;
                    this.StartThread();
                }
                else
                {
                    this.CheckDownloadedFileValid();
                    this.isDone = true;
                }
            }
        }

        /// <summary>
        /// 验证下载文件完整性
        /// </summary>
        private void CheckDownloadedFileValid()
        {
            if (!File.Exists(this.m_SavePath))
            {
                this.Context.Post(this.ThrowDownloadError, "下载文件不存在 {0}");
                return;
            }

            if (FxUtility.FileSize(this.m_SavePath) != this.m_MaxSize)
            {
                this.Context.Post(this.ThrowDownloadError, "下载文件大小不一致 {0}");
                return;
            }

            if (FxUtility.FileCrc32(this.m_SavePath) != this.m_Crc)
            {
                this.Context.Post(this.ThrowDownloadError, "下载文件CRC不一致 {0}");
            }
        }

        private void ThrowDownloadError(object path)
        {
            FxDebug.ColorError(FxDebug.ColorStyle.Red, (string) path, this.m_SavePath);
        }

        /// <summary>
        /// 创建下载请求
        /// </summary>
        /// <param name="offset">请求字节流偏移，断点续传</param>
        /// <returns></returns>
        private WebRequest CreateWebRequest(long offset)
        {
            if (this.m_URL.StartsWith("ftp", StringComparison.OrdinalIgnoreCase))
            {
                var ftpRequest = (FtpWebRequest) WebRequest.Create(this.m_URL);
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                if (!string.IsNullOrEmpty(m_FtpUserName))
                {
                    ftpRequest.Credentials = new NetworkCredential(m_FtpUserName, m_FtpPassword);
                }

                if (offset > 0) ftpRequest.ContentOffset = offset;
                return ftpRequest;
            }
            if (this.m_URL.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
            }

            var httpRequest = (HttpWebRequest) WebRequest.Create(this.m_URL);
            httpRequest.ProtocolVersion = HttpVersion.Version10;
            if (offset > 0) httpRequest.AddRange(offset);
            return httpRequest;
        }
        
        internal void Dispose()
        {
            this.m_Running = false;
            this.m_Thread = null;
        }
        
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors spe)
        {
            return true;
        }
    }
}