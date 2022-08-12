using UnityEngine;
// ReSharper disable once CheckNamespace
namespace UnityEditor
{
    public static class EditorExtension
    {
        public static class EditorStyle
        {
            public static readonly GUIStyle IconButton = EditorStyles.iconButton;
        }

        public static void CallDelay(UnityEditor.EditorApplication.CallbackFunction action, float time)
        {
            EditorApplication.CallDelayed(action, time);
        }
        
        public static void ClearConsole()
        {
            LogEntries.Clear();
        }
    }
}