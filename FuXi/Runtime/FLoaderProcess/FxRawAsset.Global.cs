using System;
using System.Collections.Generic;

namespace FuXi
{
    public partial class FxRawAsset
    {
        private static readonly Dictionary<string, FxRawAsset> RawAssetCache = new Dictionary<string, FxRawAsset>();
        internal static Func<string, FxRawAsset> FxRawAssetCreate;
        
        internal static FxRawAsset CreateRawAsset(string path)
        { return new FxRawAsset(path); }
        
        private static FTask<FxRawAsset> ReferenceAsset(string path)
        {
            path = FuXiManager.ManifestVC.CombineAssetPath(path);
            FTask<FxRawAsset> tcs;
            if (!RawAssetCache.TryGetValue(path, out var fxAsset))
            {
                fxAsset = FxRawAssetCreate.Invoke(path);
                tcs = fxAsset.Execute();
                RawAssetCache.Add(path, fxAsset);
            }
            else
                tcs = fxAsset.GetRawAsset();
            return tcs;
        }

        public static void Release(FxRawAsset rawAsset)
        {
            if (!RawAssetCache.ContainsValue(rawAsset)) return;
            rawAsset.Dispose();
            var key = string.Empty;
            foreach (var item in RawAssetCache)
            {
                if (item.Value != rawAsset) continue;
                key = item.Key;
                break;
            }
            RawAssetCache.Remove(key);
        }

        public static void Release(string path)
        {
            if (!RawAssetCache.TryGetValue(path, out var rawAsset)) return;
            rawAsset.Dispose();
            RawAssetCache.Remove(path);
        }

        public static void ReleaseAll()
        {
            foreach (var rawAsset in RawAssetCache)
                rawAsset.Value.Dispose();
            RawAssetCache.Clear();
        }

        /// <summary>
        /// 异步加载
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FTask<FxRawAsset> LoadAsync(string path)
        {
            return ReferenceAsset(path);
        }
    }
}