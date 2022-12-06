// ReSharper disable once CheckNamespace

using UnityEngine;

namespace FuXi
{
    public struct FX_LOG_CONTROL
    {
        public static FX_LOG_TYPE LogLevel = FX_LOG_TYPE.LOG | FX_LOG_TYPE.WARNING | FX_LOG_TYPE.ERROR;
        
        public static UnityEngine.Color Red = UnityEngine.Color.red;
        public static UnityEngine.Color Green = new Color(0.01f, 0.38f, 0f);
        public static UnityEngine.Color Cyan = new Color(0f, 0.57f, 0.57f);
        public static UnityEngine.Color LightCyan = new Color(0.22f, 0.32f, 0.3f);
        public static UnityEngine.Color Orange = new UnityEngine.Color(1f, 0.35f, 0f);
    }

    [System.Flags]
    public enum FX_LOG_TYPE : byte
    {
        LOG       = 1 << 0,
        WARNING   = 1 << 1,
        ERROR     = 1 << 2,
    }
    
    internal static class FxDebug
    {
        private static System.Diagnostics.Stopwatch mStopWatch;
        [System.Diagnostics.DebuggerHidden]
        internal static void Log(string message, params object[] args)
        {
            if ((FX_LOG_CONTROL.LogLevel & FX_LOG_TYPE.LOG) == 0) return;
            var msg = args.Length == 0 ? message : string.Format(message, args);
            UnityEngine.Debug.Log($"Frame {UnityEngine.Time.frameCount} -- {msg}");
        }
        
        [System.Diagnostics.DebuggerHidden]
        internal static void LogWarning(string message, params object[] args)
        {
            if ((FX_LOG_CONTROL.LogLevel & FX_LOG_TYPE.WARNING) == 0) return;
            var msg = args.Length == 0 ? message : string.Format(message, args);
            UnityEngine.Debug.LogWarning($"Frame {UnityEngine.Time.frameCount} -- {msg}");
        }
        [System.Diagnostics.DebuggerHidden]
        internal static void LogError(string message, params object[] args)
        {
            if ((FX_LOG_CONTROL.LogLevel & FX_LOG_TYPE.ERROR) == 0) return;
            var msg = args.Length == 0 ? message : string.Format(message, args);
            UnityEngine.Debug.LogError($"Frame {UnityEngine.Time.frameCount} -- {msg}");
        }
        [System.Diagnostics.DebuggerHidden]
        internal static void ColorLog(UnityEngine.Color color, string message, params object[] args)
        {
            if ((FX_LOG_CONTROL.LogLevel & FX_LOG_TYPE.LOG) == 0) return;
            var colorFormat = UnityEngine.ColorUtility.ToHtmlStringRGBA(color);
            if (args.Length == 0)
            {
                Log($"<color=#{colorFormat}>{message}</color>");
                return;
            }
            for (int i = 0; i < args.Length; i++)
                args[i] = $"<color=#{colorFormat}>{args[i]}</color>";
            Log(message, args);
        }
        [System.Diagnostics.DebuggerHidden]
        internal static void ColorWarning(UnityEngine.Color color, string message, params object[] args)
        {
            if ((FX_LOG_CONTROL.LogLevel & FX_LOG_TYPE.WARNING) == 0) return;
            var colorFormat = UnityEngine.ColorUtility.ToHtmlStringRGBA(color);
            if (args.Length == 0)
            {
                LogWarning($"<color=#{colorFormat}>{message}</color>");
                return;
            }
            for (int i = 0; i < args.Length; i++)
                args[i] = $"<color=#{colorFormat}>{args[i]}</color>";
            LogWarning(message, args);
        }
        [System.Diagnostics.DebuggerHidden]
        internal static void ColorError(UnityEngine.Color color, string message, params object[] args)
        {
            if ((FX_LOG_CONTROL.LogLevel & FX_LOG_TYPE.ERROR) == 0) return;
            var colorFormat = UnityEngine.ColorUtility.ToHtmlStringRGBA(color);
            if (args.Length == 0)
            {
                LogError($"<color=#{colorFormat}>{message}</color>");
                return;
            }
            for (int i = 0; i < args.Length; i++)
                args[i] = $"<color=#{colorFormat}>{args[i]}</color>";
            LogError(message, args);
        }
        [System.Diagnostics.DebuggerHidden]
        internal static void StartWatch()
        {
            if (null == mStopWatch)
                mStopWatch = System.Diagnostics.Stopwatch.StartNew();
            mStopWatch.Restart();
        }
        [System.Diagnostics.DebuggerHidden]
        internal static void Watch(string title)
        {
            ColorLog(FX_LOG_CONTROL.Orange,$"{title}耗时: {0} ms", mStopWatch.ElapsedMilliseconds);
        }
        [System.Diagnostics.DebuggerHidden]
        internal static void StopWatch(string title)
        {
            mStopWatch.Stop();
            ColorLog(FX_LOG_CONTROL.Orange,$"{title}耗时: {0} ms", mStopWatch.ElapsedMilliseconds);
        }
    }
}