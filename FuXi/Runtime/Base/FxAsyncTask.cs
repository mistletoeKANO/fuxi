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

        protected FxAsyncTask(bool immediate)
        {
            this.isDone = false;
            this.progress = 0;
            if (!immediate)
                Processes.Add(this);
        }
        
        protected virtual void Update()
        {
            this.isDone = true;
        }
        protected virtual void Dispose(){}
    }
}