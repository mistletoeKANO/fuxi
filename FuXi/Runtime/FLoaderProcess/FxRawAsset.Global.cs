using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FuXi
{
    public partial class FxRawAsset
    {
        private static readonly Dictionary<string, FxRawAsset> RawAssetCache = new Dictionary<string, FxRawAsset>();
        internal static Func<string, FxRawAsset> FxRawAssetCreate;
        
        internal static FxRawAsset CreateRawAsset(string path)
        { return new FxRawAsset(path); }
        
        private static FxRawAsset ReferenceAsset(string path)
        {
            if (!RawAssetCache.TryGetValue(path, out var fxAsset))
            {
                fxAsset = FxRawAssetCreate.Invoke(path);
                RawAssetCache.Add(path, fxAsset);
            }
            return fxAsset;
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
        /// 同步加载
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FxRawAsset LoadSync(string path)
        {
            var res= ReferenceAsset(path)
                .Execute()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            return (FxRawAsset) res;
        }
        
        /// <summary>
        /// 异步加载
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<FxRawAsset> LoadAsync(string path)
        {
            var res= await ReferenceAsset(path).Execute();
            return (FxRawAsset) res;
        }

        /// <summary>
        /// 异步加载
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FxRawAsset LoadCo(string path)
        {
            var rawAsset = ReferenceAsset(path);
            rawAsset.Execute();
            return rawAsset;
        }
    }
}