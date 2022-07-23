using UnityEditor;
using UnityEngine;

namespace FuXi.Editor
{
    internal static class Fx_CreateMenu
    {
        [MenuItem("Assets/Create/FuXi/FuXi Asset", false, 106)]
        private static void CreateFuXiAsset()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                 ScriptableObject.CreateInstance<DoCreateFuXiBuildAsset>(), Fx_EditorConfigs.DefaultAssetName, Fx_Style.Fx_Asset, null);
        }
        
        [MenuItem("Assets/Create/FuXi/Add Package", false, 121)]
        private static void AddPackage()
        {
            if (Selection.activeObject.GetType() != typeof(Fx_BuildAsset))
            {
                throw new System.ComponentModel.WarningException("can't add fuXi package to nonFuXiAsset!");
            }
            var resourceFile = AssetDatabase.GetAssetPath(Selection.activeObject);
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<DoCreateFuXiBuildPackage>(), Fx_EditorConfigs.AdditionPackageName, Fx_Style.Fx_AssetPackage, resourceFile);
        }

        [MenuItem("Assets/Create/FuXi/Remove Package", false, 121)]
        private static void RemovePackage()
        {
            var buildPackage = Selection.activeObject as Fx_BuildPackage;
            if (buildPackage == null)
            {
                throw new System.ComponentModel.WarningException($"can't remove {Selection.activeObject.GetType()} from fuXi asset!");
            }
            foreach (var o in Selection.objects)
            {
                AssetDatabase.RemoveObjectFromAsset(o);
            }
            AssetDatabase.SaveAssets();
        }
        
        [MenuItem("Assets/Create/FuXi/Rename Package", false, 201)]
        private static void RenamePackage()
        {
            if (Selection.activeObject.GetType() != typeof(Fx_BuildPackage)) return;
            ProjectBrowserExtension.RenameSelectAsset();
        }

        [UnityEditor.Callbacks.OnOpenAsset(0)]
        private static bool OnMapAssetOpened(int instanceId, int line)
        {
            var selectAsset = Selection.activeObject as Fx_BuildAsset;
            if (selectAsset == null) return false;
            EditorWindow.GetWindow<Fx_BundleReferenceWindow>();
            return true;
        }
    }
}