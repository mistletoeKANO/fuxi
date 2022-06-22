using System;
using UnityEditor.IMGUI.Controls;

// ReSharper disable once CheckNamespace
namespace UnityEditor
{
    public static class ProjectBrowserExtension
    {
        public static void RenameSelectAsset(Action<string> renameEndCallBack = null)
        {
            if (Selection.activeObject == null) return;
            var browser = ProjectBrowser.s_LastInteractedProjectBrowser;
            TreeViewController at = null;
            if (!browser.IsTwoColumns())
            {
                var ts = typeof(ProjectBrowser).GetField("m_AssetTree", 
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                at = ts?.GetValue(browser) as TreeViewController;
                if (at == null) return;
                TreeViewItem t1 = at.data.FindItem(at.state.lastClickedID);
                at.gui.BeginRename(t1, 0.0f);
            }
            else
            {
                browser.ListArea.Frame(Selection.activeInstanceID, true, false);
                browser.ListArea.GetRenameOverlay()
                    .BeginRename(Selection.activeObject.name, Selection.activeInstanceID, 0.0f);
                browser.ListArea.repaintCallback?.Invoke();
            }
            void RenameCheck()
            {
                if (!browser.IsTwoColumns())
                { if (at.state.renameOverlay.IsRenaming()) return; }
                else
                { if (browser.ListArea.GetRenameOverlay().IsRenaming()) return; }
                EditorApplication.update -= RenameCheck;
                AssetDatabase.SaveAssets();
                renameEndCallBack?.Invoke(Selection.activeObject.name);
            }
            EditorApplication.update += RenameCheck;
        }
    }
}