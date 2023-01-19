using System;
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
        protected override FTask<FxAsset> Execute()
        {
#if UNITY_EDITOR
            this.stackInfo = StackTraceUtility.ExtractStackTrace();
#endif
            var tcs = FTask<FxAsset>.Create(true);
            this.m_TcsList.Add(tcs);
            if (FuXiManager.ManifestVC.TryGetAssetManifest(this.m_FilePath, out var _))
                this.asset = AssetDatabase.LoadAssetAtPath(this.m_FilePath, this.m_Type);
            manifest = new AssetManifest() {Path = this.m_FilePath, HoldBundle = -1};
            if (this.m_LoadImmediate)
                this.LoadFinished();
            this.m_LoadStep = LoadSteps.LoadBundle;
            return tcs;
        }

        protected override void Update()
        {
            if (this.isDone) return;
            switch (this.m_LoadStep)
            {
                case LoadSteps.LoadBundle:
                    this.m_LoadStep = LoadSteps.LoadAsset;
                    break;
                case LoadSteps.LoadAsset:
                    this.LoadFinished();
                    break;
            }
        }
    }
}