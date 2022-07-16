using System.IO;
using System.Threading.Tasks;

namespace FuXi.Editor
{
    public class FxEditorRawAsset : FxRawAsset
    {
        internal static FxEditorRawAsset CreateEditorRawAsset(string path)
        { return new FxEditorRawAsset(path); }

        private FxEditorRawAsset(string path) : base(path) { }
        internal override FTask<FxAsyncTask> Execute()
        {
            base.Execute();
            this.Data = File.ReadAllBytes(this.m_PathOrURL);
            this.isDone = true;
            this.tcs.SetResult(this);
            return this.tcs;
        }
    }
}