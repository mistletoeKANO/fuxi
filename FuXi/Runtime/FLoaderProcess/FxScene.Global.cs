using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace FuXi
{
    public partial class FxScene
    {
        private static FxScene Main;
        private static FxScene CurrentScene;

        private static readonly Queue<FxScene> UnUsed = new Queue<FxScene>();
        internal static Func<string, bool, bool, FxScene> FxSceneCreate;

        internal static FxScene CreateScene(string path, bool addition, bool immediate)
        { return new FxScene(path, addition, immediate); }

        protected static void RefreshRef(FxScene fxScene)
        {
            if (fxScene.m_LoadMode == LoadSceneMode.Additive)
            {
                Main?.m_SubScenes.Add(fxScene);
                fxScene.m_Parent = Main;
            }
            else
            {
                Main?.Release();
                Main = fxScene;
            }
            CurrentScene = fxScene;
        }

        internal static void UpdateUnused()
        {
            if (CurrentScene == null || !CurrentScene.isDone) return;

            while (UnUsed.Count > 0)
            {
                UnUsed.Dequeue().UnLoad();
                if (AssetPolling.IsTimeOut) break;
            }
        }
        
        /// <summary>
        /// 同步加载场景
        /// </summary>
        /// <param name="path"></param>
        /// <param name="additive"></param>
        /// <returns></returns>
        public static FxScene LoadScene(string path, bool additive = false)
        {
            if (CurrentScene != null && CurrentScene.m_ScenePath == path) return CurrentScene;
            var res = FxSceneCreate.Invoke(path, additive, true).Execute();
            var scene = (FxScene) res.Result;
            return scene;
        }
        
        /// <summary>
        /// 异步转同步加载
        /// </summary>
        /// <param name="path"></param>
        /// <param name="additive"></param>
        /// <returns></returns>
        public static FxScene LoadSync(string path, bool additive = false)
        {
            if (CurrentScene != null && CurrentScene.m_ScenePath == path) return CurrentScene;
            var res = FxSceneCreate.Invoke(path, additive, false)
                .Execute()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            return (FxScene) res;
        }
        
        /// <summary>
        /// 异步加载
        /// </summary>
        /// <param name="path"></param>
        /// <param name="additive"></param>
        /// <returns></returns>
        public static async Task<FxScene> LoadSceneAsync(string path, bool additive = false)
        {
            if (CurrentScene != null && CurrentScene.m_ScenePath == path) return CurrentScene;
            var res = await FxSceneCreate.Invoke(path, additive, false).Execute();
            var scene = (FxScene) res;
            return scene;
        }

        /// <summary>
        /// 协程异步加载
        /// </summary>
        /// <param name="path"></param>
        /// <param name="additive"></param>
        /// <returns></returns>
        public static FxScene LoadSceneCo(string path, bool additive = false)
        {
            if (CurrentScene != null && CurrentScene.m_ScenePath == path) return CurrentScene;
            var scene = FxSceneCreate.Invoke(path, additive, false);
            scene.Execute();
            return scene;
        }
    }
}