// ReSharper disable once CheckNamespace
namespace FuXi
{
    internal static class FxDebug
    {
        internal static readonly int LogLevel = 0B0111;
        private static System.Diagnostics.Stopwatch mStopWatch;
        internal static void Log(string message, params object[] args)
        {
            if ((LogLevel & 1) == 0) return;
            var msg = args.Length == 0 ? message : string.Format(message, args);
            UnityEngine.Debug.Log($"Frame {UnityEngine.Time.frameCount} -- {msg}");
        }
        
        internal static void LogWarning(string message, params object[] args)
        {
            if ((LogLevel & 2) == 0) return;
            var msg = args.Length == 0 ? message : string.Format(message, args);
            UnityEngine.Debug.LogWarning($"Frame {UnityEngine.Time.frameCount} -- {msg}");
        }
        
        internal static void LogError(string message, params object[] args)
        {
            if ((LogLevel & 4) == 0) return;
            var msg = args.Length == 0 ? message : string.Format(message, args);
            UnityEngine.Debug.LogError($"Frame {UnityEngine.Time.frameCount} -- {msg}");
        }
        
        internal static void ColorLog(UnityEngine.Color color, string message, params object[] args)
        {
            if ((LogLevel & 1) == 0) return;
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
        
        internal static void ColorWarning(UnityEngine.Color color, string message, params object[] args)
        {
            if ((LogLevel & 2) == 0) return;
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
        
        internal static void ColorError(UnityEngine.Color color, string message, params object[] args)
        {
            if ((LogLevel & 4) == 0) return;
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

        internal static void StartWatch()
        {
            if (null == mStopWatch)
                mStopWatch = System.Diagnostics.Stopwatch.StartNew();
            mStopWatch.Restart();
        }

        internal static void Watch(string title)
        {
            ColorLog(ColorStyle.Orange,$"{title}耗时: {0} ms", mStopWatch.ElapsedMilliseconds);
        }

        internal static void StopWatch(string title)
        {
            mStopWatch.Stop();
            ColorLog(ColorStyle.Orange,$"{title}耗时: {0} ms", mStopWatch.ElapsedMilliseconds);
        }
        
        internal static class ColorStyle
        {
            internal static readonly UnityEngine.Color Red = UnityEngine.Color.red;
            internal static readonly UnityEngine.Color Green = UnityEngine.Color.green;
            internal static readonly UnityEngine.Color Cyan = UnityEngine.Color.cyan;
            internal static readonly UnityEngine.Color Cyan2 = new UnityEngine.Color(0.69f, 1f, 0.93f);
            internal static readonly UnityEngine.Color Orange = new UnityEngine.Color(1f, 0.35f, 0f);
        }
    }
}