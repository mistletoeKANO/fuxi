using System.Collections.Generic;

namespace FuXi
{
    public partial class DependBundleLoader
    {
        internal static Dictionary<string, DependBundleLoader> UsedBundleDic = new Dictionary<string, DependBundleLoader>();
        private static readonly List<DependBundleLoader> UnUsedBundle = new List<DependBundleLoader>();

        internal static void UpdateUnUsed()
        {
            if (UnUsedBundle.Count == 0) return;
            for (int i = 0; i < UnUsedBundle.Count; i++)
            {
                var bundle = UnUsedBundle[i];
                if (bundle.assetBundle == null)
                    continue;
                bundle.assetBundle.Unload(true);
                bundle.assetBundle = null;
                FxDebug.ColorLog(FX_LOG_CONTROL.LightCyan, "Unload bundle {0}", bundle.m_BundleManifest.BundleHashName);
                UnUsedBundle.RemoveAt(i);
                i--;
                if (AssetPolling.IsTimeOut) 
                    break;
            }
        }

        private static DependBundleLoader GetFromUnUsedCache(BundleManifest manifest)
        {
            DependBundleLoader res = default;
            foreach (var bundle in UnUsedBundle)
            {
                if (bundle.m_BundleManifest.BundleHashName != manifest.BundleHashName)
                    continue;
                res = bundle;
                break;
            }
            UnUsedBundle.Remove(res);
            return res;
        }

        internal static void ReferenceBundle(BundleManifest manifest, out DependBundleLoader bundleLoader)
        {
            if (!UsedBundleDic.TryGetValue(manifest.BundleHashName, out bundleLoader))
            {
                bundleLoader = GetFromUnUsedCache(manifest) ?? new DependBundleLoader(manifest);
                UsedBundleDic.Add(manifest.BundleHashName, bundleLoader);
            }
            bundleLoader.AddReference();
        }
        
        private static void ReleaseBundleLoader(BundleManifest manifest)
        {
            if (!UsedBundleDic.ContainsKey(manifest.BundleHashName)) 
                return;
            var bundleLoader = UsedBundleDic[manifest.BundleHashName];
            UnUsedBundle.Add(bundleLoader);
            UsedBundleDic.Remove(manifest.BundleHashName);
        }

        internal static void ClearBundleCache()
        {
            UsedBundleDic.Clear();
            UnUsedBundle.Clear();
        }
    }
}