using System.Globalization;
using System.Reflection;
using UnityEditor.IMGUI.Controls;

// ReSharper disable once CheckNamespace
namespace UnityEditor
{
    public static class ProjectBrowserExtension
    {
        public static void RenameSelectAsset()
        {
            if (Selection.activeObject == null) return;
            var browser = ProjectBrowser.s_LastInteractedProjectBrowser;
            TreeViewController vc = null;
            if (!browser.IsTwoColumns())
            {
                var ts = typeof(ProjectBrowser).GetField("m_AssetTree", 
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                vc = ts?.GetValue(browser) as TreeViewController;
                if (vc == null) return;
                TreeViewItem t1 = vc.data.FindItem(vc.state.lastClickedID);
                vc.gui.BeginRename(t1, 0.0f);
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
                { if (vc.state.renameOverlay.IsRenaming()) return; }
                else
                { if (browser.ListArea.GetRenameOverlay().IsRenaming()) return; }
                AssetDatabase.SaveAssets();
                EditorApplication.CallDelayed(() =>
                {
                    var method = typeof(ProjectBrowser).GetMethod("ResetViews", BindingFlags.Instance | BindingFlags.NonPublic);
                    method?.Invoke(browser, BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);
                },0.01);
                EditorApplication.update -= RenameCheck;
            }
            EditorApplication.update += RenameCheck;
        }
    }
}