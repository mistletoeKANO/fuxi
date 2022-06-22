using System.Collections.Generic;
using UnityEditor;

// ReSharper disable once CheckNamespace
namespace FuXi.Editor
{
    public class DoCreateFuXiBuildPackage : UnityEditor.ProjectWindowCallback.EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(pathName);
            var mainAsset = AssetDatabase.LoadAssetAtPath<Fx_BuildAsset>(resourceFile);
            var addPackage = UnityEngine.ScriptableObject.CreateInstance<Fx_BuildPackage>();
            {
                addPackage.name = this.GetUniquePackageName(resourceFile, fileName);
            }
            AssetDatabase.AddObjectToAsset(addPackage, mainAsset);
            AssetDatabase.SaveAssets();
        }

        private string GetUniquePackageName(string resourceFile, string curName)
        {
            var existPackages = AssetDatabase.LoadAllAssetsAtPath(resourceFile);
            List<string> names = new List<string>();
            foreach (var p in existPackages)
            {
                if (p is Fx_BuildPackage package) { names.Add(package.name); }
            }
            int index = 0;
            string resName = curName;
            while (names.Contains(resName))
            {
                resName = $"{curName} {index}";
                index ++;
            }
            return resName;
        }
    }
}