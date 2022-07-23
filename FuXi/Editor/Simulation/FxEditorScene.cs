using System;
using System.Threading.Tasks;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// ReSharper disable once CheckNamespace
namespace FuXi.Editor
{
    public class FxEditorScene : FxScene
    {
        internal static FxEditorScene CreateEditorScene(string path, bool addition, bool immediate, Action<float> callback)
        { return new FxEditorScene(path, addition, immediate, callback); }
        
        FxEditorScene(string path, bool additive, bool immediate, Action<float> callback) : base(path, additive, immediate, callback) { }
        internal override FTask<FxAsyncTask> Execute()
        {
            base.Execute();
            if (null != FuXiManager.ManifestVC && !FuXiManager.ManifestVC.TryGetAssetManifest(this.m_ScenePath, out _))
            {
                this.tcs.SetResult(this);
                this.isDone = true;
            }
            else
            {
                RefreshRef(this);
                if (this.m_Immediate)
                {
                    EditorSceneManager.LoadSceneInPlayMode(this.m_ScenePath, new LoadSceneParameters(this.m_LoadMode));
                    this.tcs.SetResult(this);
                    this.isDone = true;
                }
                else
                    this.m_Operation = EditorSceneManager.LoadSceneAsyncInPlayMode(this.m_ScenePath,
                        new LoadSceneParameters(this.m_LoadMode));
            }
            return this.tcs;
        }

        protected override void Update()
        {
            if (this.isDone) return;
            if (this.m_Operation != null)
            {
                this.m_LoadUpdate?.Invoke(this.m_Operation.progress);
                if (!this.m_Operation.isDone) return;
            }

            this.tcs.SetResult(this);
            this.isDone = true;
        }
    }
}