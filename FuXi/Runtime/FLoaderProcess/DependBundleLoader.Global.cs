using System.Collections.Generic;

namespace FuXi
{
    public partial class DependBundleLoader
    {
        internal static Dictionary<string, DependBundleLoader> UsedBundleDic = new Dictionary<string, DependBundleLoader>();
        private static readonly Queue<DependBundleLoader> UnUsedBundle = new Queue<DependBundleLoader>();

        internal static void UpdateUnUsed()
        {
            if (UnUsedBundle.Count == 0) return;
            while (UnUsedBundle.Count > 0)
            {
                var releaseBundle = UnUsedBundle.Dequeue();
                if (releaseBundle.assetBundle == null) return;
                FxDebug.ColorLog(FxDebug.ColorStyle.Cyan2, "Unload bundle {0}", releaseBundle.m_BundleManifest.BundleHashName);
                releaseBundle.assetBundle.Unload(true);
                releaseBundle.assetBundle = null;
                if (AssetPolling.IsTimeOut) break;
            }
        }

        internal static bool TryReferenceBundle(BundleManifest manifest, out DependBundleLoader bundleLoader)
        {
            if (!UsedBundleDic.TryGetValue(manifest.BundleHashName, out bundleLoader))
            {
                bundleLoader = new DependBundleLoader(manifest);
                UsedBundleDic.Add(manifest.BundleHashName, bundleLoader);
                bundleLoader.AddReference();
                return false;
            }
            bundleLoader.AddReference();
            return true;
        }
        
        private static void ReleaseBundleLoader(BundleManifest manifest)
        {
            if (!UsedBundleDic.ContainsKey(manifest.BundleHashName)) return;
            var bundleLoader = UsedBundleDic[manifest.BundleHashName];
            UnUsedBundle.Enqueue(bundleLoader);
            UsedBundleDic.Remove(manifest.BundleHashName);
        }

        internal static void GameQuit()
        {
            UsedBundleDic.Clear();
            UnUsedBundle.Clear();
        }
    }
}