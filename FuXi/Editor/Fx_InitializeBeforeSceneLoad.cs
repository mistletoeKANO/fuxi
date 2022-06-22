using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FuXi.Editor
{
    public static class Fx_InitializeBeforeSceneLoad
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitFxAssetBeforeSceneLoad()
        {
            FxScene.FxSceneCreate = FxEditorScene.CreateEditorScene;
            FxAsset.FxAssetCreate = FxEditorAsset.CreateEditorAsset;
            FxRawAsset.FxRawAssetCreate = FxEditorRawAsset.CreateEditorRawAsset;
            FxManager.ParseManifestCallback = CreateManifest;
        }

        private static FxManifest CreateManifest()
        {
            var manifest = new FxManifest();
            BuildPlateForm buildPlateForm = RunPlatform2BuildPlatform();
            Fx_BuildAsset buildAsset = BuildHelper.GetBuildAsset(buildPlateForm);
            
            if (buildAsset == null)
            {
                FxDebug.LogError("build asset is not found for this platform!");
                return null;
            }
            
            List<AssetManifest> assetManifests = new List<AssetManifest>();

            foreach (var folder in buildAsset.fx_Objects)
            {
                if (folder.folder == null)
                {
                    FxDebug.LogError($"{buildAsset.name} asset is null or missing!");
                    continue;
                }
                var folderPath = AssetDatabase.GetAssetPath(folder.folder);
                if (AssetDatabase.IsValidFolder(folderPath))
                {
                    var assets = AssetDatabase.FindAssets("*", new[] {folderPath});
                    foreach (var asset in assets)
                    {
                        var p = AssetDatabase.GUIDToAssetPath(asset);
                        if (AssetDatabase.IsValidFolder(p)) continue;
                        assetManifests.Add(new AssetManifest{Path = p, IsRawFile = folder.bundleMode == BundleMode.PackByRaw});
                    }
                }
                else
                {
                    assetManifests.Add(new AssetManifest {Path = folderPath});
                }
            }

            foreach (var asManifest in assetManifests)
            {
                if (manifest.Path2AssetManifest.ContainsKey(asManifest.Path)) continue;
                manifest.Path2AssetManifest.Add(asManifest.Path, asManifest);
            }
            FxDebug.ColorLog(FxDebug.ColorStyle.Green, "Load editor manifest {0}.", buildAsset.name);
            return manifest;
        }

        private static BuildPlateForm RunPlatform2BuildPlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return BuildPlateForm.Android;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return BuildPlateForm.Window;
                case RuntimePlatform.IPhonePlayer:
                    return BuildPlateForm.IOS;
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return BuildPlateForm.MacOS;
                case RuntimePlatform.WebGLPlayer:
                    return BuildPlateForm.WebGL;
                default:
                    return BuildPlateForm.Window;
            }
        }
    }
}