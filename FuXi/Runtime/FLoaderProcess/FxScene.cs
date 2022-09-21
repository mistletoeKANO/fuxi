using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FuXi
{
    public partial class FxScene : FxAsyncTask
    {
        private enum LoadSceneSteps
        {
            LoadBundle,
            LoadScene
        }
        internal readonly string m_ScenePath;
        internal readonly LoadSceneMode m_LoadMode;
        internal readonly bool m_Immediate;
        internal AsyncOperation m_Operation;
        internal readonly Action<float> m_LoadUpdate;
        
        private readonly List<FxScene> m_SubScenes = new List<FxScene>();
        private readonly BundleLoader m_BundleLoader;

        private LoadSceneSteps m_LoadStep;
        private FxScene m_Parent;

        internal FxScene(string path, bool additive, bool immediate, Action<float> callback)
        {
            this.m_ScenePath = FuXiManager.ManifestVC.CombineAssetPath(path);
            this.m_LoadMode = additive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            this.m_Immediate = immediate;
            this.m_BundleLoader = new BundleLoader();
            this.m_LoadUpdate = callback;
        }
        
        internal override FTask<FxAsyncTask> Execute()
        {
            base.Execute();
            if (FuXiManager.RuntimeMode == RuntimeMode.Editor) return null;
            if (!FuXiManager.ManifestVC.TryGetAssetManifest(this.m_ScenePath, out var manifest))
            {
                this.tcs.SetResult(this);
                this.isDone = true;
                return this.tcs;
            }
            RefreshRef(this);
            this.m_BundleLoader.StartLoad(manifest, this.m_Immediate);
            if (this.m_Immediate)
            {
                if (this.m_BundleLoader.mainLoader.assetBundle != null)
                    SceneManager.LoadScene(this.m_ScenePath, this.m_LoadMode);
                this.tcs.SetResult(this);
                this.isDone = true;
            }
            this.m_LoadStep = LoadSceneSteps.LoadBundle;
            return this.tcs;
        }

        protected override void Update()
        {
            if (this.isDone) return;
            switch (this.m_LoadStep)
            {
                case LoadSceneSteps.LoadBundle:
                    this.m_LoadUpdate?.Invoke(0.5f * this.m_BundleLoader.progress);
                    if (!this.m_BundleLoader.isDone)
                    {
                        this.m_BundleLoader.Update();
                        return;
                    }
                    this.m_Operation = SceneManager.LoadSceneAsync(this.m_ScenePath, this.m_LoadMode);
                    this.m_LoadStep = LoadSceneSteps.LoadScene;
                    break;
                case LoadSceneSteps.LoadScene:
                    this.m_LoadUpdate?.Invoke(0.5f + 0.5f * this.m_Operation.progress);
                    if (this.m_Operation.allowSceneActivation)
                        if (!this.m_Operation.isDone) return;
                    else
                        if (this.m_Operation.progress < 0.9f) return;
                    this.isDone = true;
                    this.tcs.SetResult(this);
                    break;
            }
        }

        private void Release()
        {
            UnUsed.Enqueue(this);
        }

        private void UnLoad()
        {
            if (this.m_Parent != null)
            {
                SceneManager.UnloadSceneAsync(this.m_ScenePath);
                FxDebug.ColorLog(FX_LOG_CONTROL.LightCyan, "Unload scene {0}", this.m_ScenePath);
            }
            this.m_BundleLoader?.Release();
            foreach (var fxScene in this.m_SubScenes)
            {
                fxScene.Release();
            }
            this.m_SubScenes.Clear();
            this.Dispose();
        }

        protected override void Dispose()
        {
            this.m_BundleLoader?.Dispose();
        }
    }
}