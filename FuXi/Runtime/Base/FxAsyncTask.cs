using System.Collections;
// ReSharper disable once CheckNamespace
namespace FuXi
{
    public partial class FxAsyncTask : IEnumerator
    {
        public bool isDone;
        public float progress;
        public string error;
        
        public bool MoveNext() => !isDone;
        public void Reset() { }
        public object Current => null;

        internal FTask<FxAsyncTask> tcs;

        internal virtual FTask<FxAsyncTask> Execute()
        {
            this.isDone = false;
            this.progress = 0;
            tcs = FTask<FxAsyncTask>.Create(true);
            Processes.Add(this);
            return null;
        }

        protected virtual void Update()
        {
            this.isDone = true;
            tcs.SetResult(default);
        }
        
        protected virtual void Dispose(){}
    }
}