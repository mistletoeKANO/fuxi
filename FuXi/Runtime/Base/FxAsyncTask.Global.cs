using System.Collections.Generic;

namespace FuXi
{
    public partial class FxAsyncTask
    {
        private static readonly List<FxAsyncTask> Processes = new List<FxAsyncTask>();

        internal static void UpdateProcess()
        {
            if (Processes.Count == 0) return;
            for (int i = 0; i < Processes.Count; i++)
            {
                var p = Processes[i];
                p.Update();
                if (p.isDone) Processes.RemoveAt(i);
                if (AssetPolling.IsTimeOut) break;
            }
        }

        internal static void ProcessQuit()
        {
            if (Processes.Count == 0) return;
            foreach (var p in Processes)
            {
                p.Dispose();
            }
            Processes.Clear();
        }
    }
}