using UnityEditor;

// ReSharper disable once CheckNamespace
namespace FuXi.Editor
{
    public class DoCreateFuXiBuildAsset : UnityEditor.ProjectWindowCallback.EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            var mainAsset = UnityEngine.ScriptableObject.CreateInstance<Fx_BuildAsset>();
            AssetDatabase.CreateAsset(mainAsset, pathName);
            
            var settings = UnityEngine.ScriptableObject.CreateInstance<Fx_BuildSetting>();
            {
                settings.name = Fx_EditorConfigs.SettingName;
            }
            var firstPackage = UnityEngine.ScriptableObject.CreateInstance<Fx_BuildPackage>();
            {
                firstPackage.name = Fx_EditorConfigs.FirstPackageName;
            }
            AssetDatabase.AddObjectToAsset(settings, mainAsset);
            AssetDatabase.AddObjectToAsset(firstPackage, mainAsset);
            AssetDatabase.SaveAssets();
            ProjectWindowUtil.ShowCreatedAsset((UnityEngine.Object) mainAsset);
        }
    }
}