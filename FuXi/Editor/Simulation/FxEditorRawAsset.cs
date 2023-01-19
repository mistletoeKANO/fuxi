using System.IO;

namespace FuXi.Editor
{
    public class FxEditorRawAsset : FxRawAsset
    {
        internal static FxEditorRawAsset CreateEditorRawAsset(string path)
        { return new FxEditorRawAsset(path); }

        private FxEditorRawAsset(string path) : base(path) { }
        protected override FTask<FxRawAsset> Execute()
        {
            var tcs = FTask<FxRawAsset>.Create(true);
            this.m_TcsList.Add(tcs);
            this.Data = File.ReadAllBytes(this.m_PathOrURL);
            this.m_LoadStep = LoadStep.Download;
            return tcs;
        }

        protected override void Update()
        {
            if (this.isDone) return;
            switch (m_LoadStep)
            {
                case LoadStep.Download:
                    this.m_LoadStep = LoadStep.LoadFile;
                    break;
                case LoadStep.LoadFile:
                    this.LoadFinished();
                    break;
            }
        }
    }
}