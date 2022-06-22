using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FuXi
{
    public partial class FxAsset
    {
        internal static Dictionary<string, FxAsset> AssetCache = new Dictionary<string, FxAsset>();
        internal static Func<string, Type, bool, Action<FxAsset>, FxAsset> FxAssetCreate;
        
        internal static FxAsset CreateAsset(string path, Type type, bool immediate, Action<FxAsset> callback)
        { return new FxAsset(path, type, immediate, callback); }

        private static FxAsset ReferenceAsset(string path, Type type, bool immediate, Action<FxAsset> callback)
        {
            if (!AssetCache.TryGetValue(path, out var fxAsset))
            {
                fxAsset = FxAssetCreate.Invoke(path, type, immediate, callback);
                AssetCache.Add(path, fxAsset);
            }
            fxAsset.AddReference();
            return fxAsset;
        }

        internal static void GameQuit()
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
            var res = ReferenceAsset(path, typeof(T), true, null).Execute();
            return (FxAsset) res.Result;
        }
        
        /// <summary>
        /// 同步加载
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static FxAsset Load(string path, Type type)
        {
            var res = ReferenceAsset(path, type, true, null);
            res.Execute();
            return res;
        }
        
        /// <summary>
        /// 异步转同步加载
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static FxAsset LoadSync<T>(string path)
        {
            var res = ReferenceAsset(path, typeof(T), false, null)
                .Execute()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            return (FxAsset) res;
        }

        /// <summary>
        /// 异步加载
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<FxAsset> LoadAsync<T>(string path)
        {
            var res = await ReferenceAsset(path, typeof(T), false, null).Execute();
            return (FxAsset) res;
        }

        /// <summary>
        /// 异步回调加载
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        public static void LoadAsync(string path, Type type, Action<FxAsset> callback)
        {
            ReferenceAsset(path, type, false, callback).Execute();
        }
        
        /// <summary>
        /// 协程异步加载
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static FxAsset LoadCo(string path, Type type)
        {
            var res = ReferenceAsset(path, type, false, null);
            res.Execute();
            return res;
        }
    }
}