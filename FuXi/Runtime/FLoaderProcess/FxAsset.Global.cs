using System;
using System.Collections.Generic;

namespace FuXi
{
    public partial class FxAsset
    {
        internal static Dictionary<string, FxAsset> AssetCache = new Dictionary<string, FxAsset>();
        internal static Func<string, Type, bool, Action<FxAsset>, FxAsset> FxAssetCreate;
        
        internal static FxAsset CreateAsset(string path, Type type, bool immediate, Action<FxAsset> callback)
        { return new FxAsset(path, type, immediate, callback); }

        private static FxAsset LoadInternal(string path, Type type, bool immediate)
        {
            path = FuXiManager.ManifestVC.CombineAssetPath(path);
            if (!AssetCache.TryGetValue(path, out var fxAsset))
            {
                fxAsset = FxAssetCreate.Invoke(path, type, immediate, null);
                fxAsset.Execute();
                AssetCache.Add(path, fxAsset);
            }else
                fxAsset.ReLoad(immediate, null);
            fxAsset.AddReference();
            return fxAsset;
        }
        
        private static FTask<FxAsset> LoadAsyncInternal(string path, Type type, Action<FxAsset> callback)
        {
            path = FuXiManager.ManifestVC.CombineAssetPath(path);
            FTask<FxAsset> tcs;
            if (!AssetCache.TryGetValue(path, out var fxAsset))
            {
                fxAsset = FxAssetCreate.Invoke(path, type, false, callback);
                tcs = fxAsset.Execute();
                AssetCache.Add(path, fxAsset);
            }else
                tcs = fxAsset.ReLoad(false, callback);
            fxAsset.AddReference();
            return tcs;
        }

        internal static void ClearAssetCache()
        {
            AssetCache.Clear();
        }

        /// <summary>
        /// 同步加载
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static FxAsset Load<T>(string path)
        {
            return LoadInternal(path, typeof(T), true);
        }
        
        /// <summary>
        /// 同步加载
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static FxAsset Load(string path, Type type)
        {
            return LoadInternal(path, type, true);
        }

        /// <summary>
        /// 异步加载
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static FxAsset LoadAsyncCo<T>(string path)
        {
            return LoadInternal(path, typeof(T), false);
        }

        public static FTask<FxAsset> LoadAsync<T>(string path)
        {
            return LoadAsyncInternal(path, typeof(T), null);
        }

        /// <summary>
        /// 异步回调加载
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        public static void LoadAsync(string path, Type type, Action<FxAsset> callback)
        {
            LoadAsyncInternal(path, type, callback);
        }
    }
}