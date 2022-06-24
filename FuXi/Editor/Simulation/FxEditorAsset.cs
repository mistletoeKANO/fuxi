using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace FuXi.Editor
{
    public class FxEditorAsset : FxAsset
    {
        internal static FxEditorAsset CreateEditorAsset(string path, Type type, bool immediate, Action<FxAsset> callback)
        { return new FxEditorAsset(path, type, immediate, callback); }
        
        FxEditorAsset(string path, Type type, bool loadImmediate, Action<FxAsset> callback) : base(path, type, loadImmediate, callback) { }
        internal override Task<FxAsyncTask> Execute()
        {
            base.Execute();
            this.stackInfo = StackTraceUtility.ExtractStackTrace();
            if (FxManager.ManifestVC == null)
                this.asset = AssetDatabase.LoadAssetAtPath(this.m_FilePath, this.m_Type);
            else
            {
                if (FxManager.ManifestVC.TryGetAssetManifest(this.m_FilePath, out var _))
                    this.asset = AssetDatabase.LoadAssetAtPath(this.m_FilePath, this.m_Type);
            }
            manifest = new AssetManifest() {Path = this.m_FilePath, HoldBundle = -1};
            this.tcs.SetResult(this);
            this.isDone = true;
            return this.tcs.Task;
        }
    }
}