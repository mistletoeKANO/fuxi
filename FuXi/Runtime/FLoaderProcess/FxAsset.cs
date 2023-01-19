using System;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace FuXi
{
    public partial class FxAsset : FxAsyncTask
    {
        protected enum LoadSteps
        {
            LoadBundle,
            LoadAsset
        }
        protected bool m_LoadImmediate;
        protected LoadSteps m_LoadStep;
        protected readonly List<FTask<FxAsset>> m_TcsList;
        
        private readonly BundleLoader m_BundleLoader;
        private AssetBundleRequest m_BundleRequest;
        private Action<FxAsset> m_Completed;

        internal readonly FxReference fxReference;
        internal AssetManifest manifest;
        
#if UNITY_EDITOR
        internal string stackInfo;
#endif
        
        protected readonly string m_FilePath;
        protected readonly Type m_Type;
        public UnityEngine.Object asset;
        
        internal FxAsset(string path, Type type, bool loadImmediate, Action<FxAsset> callback) : base(loadImmediate)
        {
            this.m_FilePath = path;
            this.m_Type = type;
            this.m_LoadImmediate = loadImmediate;
            this.m_Completed = callback;
            this.m_BundleLoader = new BundleLoader();
            this.fxReference = new FxReference();
            this.m_TcsList = new List<FTask<FxAsset>>();
        }

        private FTask<FxAsset> ReLoad(bool loadImmediate, Action<FxAsset> callback)
        {
            var tcs = FTask<FxAsset>.Create(true);
            this.m_TcsList.Add(tcs);
            this.m_LoadImmediate = loadImmediate;
            if (callback != null)
            {
                if (this.m_Completed != null)
                    this.m_Completed += callback;
                else
                    this.m_Completed = callback;
            }
            if (this.isDone)
                this.LoadFinished();
            return tcs;
        }
        
        protected virtual FTask<FxAsset> Execute()
        {
#if UNITY_EDITOR
            this.stackInfo = StackTraceUtility.ExtractStackTrace();
#endif
            var tcs = FTask<FxAsset>.Create(true);
            this.m_TcsList.Add(tcs);
            if (!FuXiManager.ManifestVC.TryGetAssetManifest(this.m_FilePath, out this.manifest))
            {
                this.LoadFinished();
                AssetCache.Remove(this.m_FilePath);
            }
            this.m_BundleLoader.StartLoad(manifest, this.m_LoadImmediate);
            if (this.m_LoadImmediate)
            {
                if (this.m_BundleLoader.mainLoader.assetBundle != null)
                    this.asset = this.m_BundleLoader.mainLoader.assetBundle.LoadAsset(this.m_FilePath, this.m_Type);
                this.LoadFinished();
            }
            this.m_LoadStep = LoadSteps.LoadBundle;
            return tcs;
        }

        protected override void Update()
        {
            if (this.isDone) return;
            switch (this.m_LoadStep)
            {
                case LoadSteps.LoadBundle:
                    if (!this.m_BundleLoader.isDone)
                    {
                        this.m_BundleLoader.Update();
                        return;
                    }
                    if (this.m_BundleLoader.mainLoader.assetBundle == null)
                    {
                        this.LoadFinished();
                        return;
                    }
                    this.m_BundleRequest = this.m_BundleLoader.mainLoader.assetBundle.LoadAssetAsync(this.m_FilePath, this.m_Type);
                    this.m_LoadStep = LoadSteps.LoadAsset;
                    break;
                case LoadSteps.LoadAsset:
                    if (!this.m_BundleRequest.isDone) 
                        return;
                    this.asset = this.m_BundleRequest.asset;
                    this.LoadFinished();
                    break;
            }
        }

        protected void LoadFinished()
        {
            this.isDone = true;
            this.m_Completed?.Invoke(this);
            this.m_Completed = default;
            foreach (var task in this.m_TcsList)
            {
                task.SetResult(this);
            }
            this.m_TcsList.Clear();
        }

        private void AddReference()
        {
            this.fxReference.AddRef();
        }

        public void Release()
        {
            if (!this.isDone)
            {
                FxDebug.LogWarning($"Asset {this.manifest.Path} is in loading! can't release now.");
                return;
            }
            this.m_BundleLoader?.Release();
            if (!this.fxReference.SubRef()) return;

            AssetCache.Remove(this.m_FilePath);
            this.Dispose();
        }

        protected override void Dispose()
        {
            this.m_BundleLoader?.Dispose();
        }
    }
}