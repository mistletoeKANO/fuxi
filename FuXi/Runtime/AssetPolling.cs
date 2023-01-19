using UnityEngine;

// ReSharper disable once CheckNamespace
namespace FuXi
{
    public class AssetPolling : MonoBehaviour
    {
        public static float TimeSlice
        {
            get => timeSlice;
            set => timeSlice = value;
        }
        private static float timeSlice = 0.04f; // s
        private static float lastCheckTime = 0;
        internal static bool IsTimeOut
        {
            get
            {
                var curTime = Time.realtimeSinceStartup;
                if (curTime - lastCheckTime >= timeSlice)
                {
                    lastCheckTime = curTime;
                    return true;
                }
                lastCheckTime = curTime;
                return false;
            }
        }

        private void Update()
        {
            FxAsyncTask.UpdateProcess();
            FxScene.UpdateUnused();
            DependBundleLoader.UpdateUnUsed();
        }

        private void OnApplicationQuit()
        {
            FxAsyncTask.ClearProcess();
            DependBundleLoader.ClearBundleCache();
            FxAsset.ClearAssetCache();
        }
    }
}