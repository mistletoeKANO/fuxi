using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace FuXi
{
    public class ThreadDownloader
    {
        private const int m_BufferSize = 1024 * 4;
        private const string m_FtpUserName = "";
        private const string m_FtpPassword = "";
        
        private Task m_Task;
        private string m_URL;
        private string m_SavePath;

        private long m_MaxSize;
        private string m_Crc;

        internal string error;
        internal bool isDone = false;
        internal long m_DownloadedSize;

        internal void Start(BundleManifest manifest)
        {
            this.m_URL = $"{FuXiManager.PlatformURL}{manifest.BundleHashName}";
            this.m_SavePath = FxPathHelper.PersistentLoadPath(manifest.BundleHashName);

            this.m_Crc = manifest.CRC;
            this.m_MaxSize = manifest.Size;
            this.m_DownloadedSize = 0;

            this.StartThread();
        }

        private void StartThread()
        {
            this.isDone = false;
            this.error = String.Empty;
            this.m_Task = Task.Factory.StartNew(RunThread);
        }

        private async void RunThread()
        {
            FileStream fileStream = null;
            WebResponse response = null;
            Stream respStream = null;
            
            try
            {
                FileInfo fileInfo = new FileInfo(this.m_SavePath);
                if (fileInfo.Exists && (fileInfo.Length > this.m_MaxSize || !FuXiManager.ManifestVC.NewManifest.OpenBreakResume))
                {
                    File.Delete(this.m_SavePath);
                }

                fileStream = new FileStream(this.m_SavePath, FileMode.OpenOrCreate, FileAccess.Write);
                var resumeLength = fileStream.Length;
                // 注意：设置本地文件流的起始位置, 断点续传
                if (resumeLength > 0 && FuXiManager.ManifestVC.NewManifest.OpenBreakResume)
                    fileStream.Seek(resumeLength, SeekOrigin.Begin);
                else
                    resumeLength = 0;
                
                var webRequest = this.CreateWebRequest(resumeLength);
                if (webRequest == null)
                    throw new WebException("创建下载请求失败");
                response = await webRequest.GetResponseAsync();
                respStream = response.GetResponseStream();

                byte[] buffer = new byte[m_BufferSize];
                while (respStream != null && !this.isDone)
                {
                    int readLength = await respStream.ReadAsync(buffer, 0, buffer.Length);
                    if (readLength <= 0) 
                        break;
                    fileStream.Write(buffer, 0, readLength);
                    
                    this.m_DownloadedSize += readLength;
                }
                this.isDone = true;
            }
            catch (Exception e)
            {
                this.isDone = true;
                this.error = $"下载资源异常:{e.Message}";
            }
            finally
            {
                respStream?.Close();
                respStream?.Dispose();
                
                response?.Close();
                response?.Dispose();

                fileStream?.Close();
                fileStream?.Dispose();
                
                this.CheckDownloadedFileValid();
                this.isDone = true;
            }
        }

        internal void Abort()
        {
            this.isDone = true;
            this.m_Task.Dispose();
        }

        /// <summary>
        /// 验证下载文件完整性
        /// </summary>
        private void CheckDownloadedFileValid()
        {
            if (!File.Exists(this.m_SavePath))
            {
                this.error = $"下载文件不存在 {this.m_URL}";
                return;
            }

            long downloadSize = FxUtility.FileSize(this.m_SavePath);
            if (downloadSize != this.m_MaxSize)
            {
                this.error = $"下载文件 {this.m_URL} 大小不一致 {downloadSize}/{this.m_MaxSize}";
                File.Delete(this.m_SavePath);
                return;
            }

            if (FxUtility.FileCrc32(this.m_SavePath) != this.m_Crc)
            {
                this.error = $"下载文件CRC不一致 {this.m_URL}";
                File.Delete(this.m_SavePath);
            }
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
            this.m_Task.Dispose();
            this.m_Task = null;
        }
        
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors spe)
        {
            return true;
        }
    }
}