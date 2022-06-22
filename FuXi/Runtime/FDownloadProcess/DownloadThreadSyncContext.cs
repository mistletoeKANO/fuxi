using System;
using System.Collections.Concurrent;
using System.Threading;

namespace FuXi
{
    public class DownloadThreadSyncContext : SynchronizationContext
    {
        private readonly ConcurrentQueue<Action> m_SafeCtBk = new ConcurrentQueue<Action>();

        internal void Update()
        {
            if (!m_SafeCtBk.TryDequeue(out var cb)) return;
            cb.Invoke();
        }
        
        public override void Post(SendOrPostCallback d, object state)
        {
            var action = new Action(() => { d.Invoke(state); });
            this.m_SafeCtBk.Enqueue(action);
        }
    }
}