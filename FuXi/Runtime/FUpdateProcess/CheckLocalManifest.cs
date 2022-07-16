using UnityEngine.Networking;

// ReSharper disable once CheckNamespace
namespace FuXi
{
    public class CheckLocalManifest : FxAsyncTask
    {
        private enum CheckLocalMStep
        {
            CheckFile,
            LoadFile,
            ParseManifest,
        }
        
        private UnityWebRequestAsyncOperation m_AsyncOperation;
        private UnityWebRequest m_UnityWebRequest;
        private string m_UrlOrPath;
        private CheckLocalMStep m_Step;

        internal CheckLocalManifest() { }
        internal override FTask<FxAsyncTask> Execute()
        {
            base.Execute();
            this.m_Step = CheckLocalMStep.CheckFile;
            return tcs;
        }

        protected override void Update()
        {
            if (this.isDone) return;

            switch (this.m_Step)
            {
                case CheckLocalMStep.CheckFile:
                    var manifestPath = FxPathHelper.PersistentLoadPath(FuXiManager.ManifestVC.ManifestName);
                    if (!System.IO.File.Exists(manifestPath))
                    {
                        manifestPath = FxPathHelper.StreamingLoadPath(FuXiManager.ManifestVC.ManifestName);
                        manifestPath = FxPathHelper.ConvertToWWWPath(manifestPath);
                    }else
                        manifestPath = FxPathHelper.PersistentLoadURL(manifestPath);
                    this.m_UrlOrPath = manifestPath;
                    this.m_Step = CheckLocalMStep.LoadFile;
                    break;
                case CheckLocalMStep.LoadFile:
                    this.m_UnityWebRequest = UnityWebRequest.Get(this.m_UrlOrPath);
                    this.m_UnityWebRequest.disposeDownloadHandlerOnDispose = true;
                    this.m_UnityWebRequest.timeout = 60;
                    this.m_AsyncOperation = this.m_UnityWebRequest.SendWebRequest();
                    this.m_Step = CheckLocalMStep.ParseManifest;
                    break;
                case CheckLocalMStep.ParseManifest:
                    this.progress = this.m_AsyncOperation.progress;
                    if (!this.m_UnityWebRequest.isDone) return;
                    if (string.IsNullOrEmpty(this.m_UnityWebRequest.error))
                    {
                        FxDebug.Log($"Load local manifest file: {this.m_UrlOrPath}");
                        var readValue = System.Text.Encoding.UTF8.GetString(this.m_UnityWebRequest.downloadHandler.data);
                        FuXiManager.ManifestVC.OldManifest = FxManifest.Parse(readValue);
                        FuXiManager.ManifestVC.NewManifest = FuXiManager.ManifestVC.OldManifest;
                    }
                    else
                    {
                        FxDebug.LogError($"Load local manifest file failure with error: {this.m_UnityWebRequest.error}!");
                        FuXiManager.ManifestVC.OldManifest = new FxManifest();
                        FuXiManager.ManifestVC.NewManifest = new FxManifest();
                    }
                    FuXiManager.ManifestVC.InitEncrypt();
                    tcs.SetResult(this);
                    this.isDone = true;
                    break;
            }
        }

        protected override void Dispose()
        {
            if (this.m_UnityWebRequest == null) return;
            
            this.m_UnityWebRequest.Dispose();
            this.m_UnityWebRequest = null;
            this.m_AsyncOperation = null;
        }
    }
}