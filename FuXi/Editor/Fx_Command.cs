using System;
using UnityEditor;

namespace FuXi.Editor
{
    public static class Fx_Command
    {
        public static void BuildBundle(string platform)
        {
            try
            {
                var buildPlatform = Name2BuildTarget(platform);
                var buildAsset = BuildHelper.GetBuildAsset(buildPlatform.PlateForm);
                new BuildBundleProcess(buildAsset).BeginBuild();
            }
            catch (Exception e)
            {
                FxDebug.ColorError(FX_LOG_CONTROL.Red, "Build bundle failure in platform {0} with error {1}",
                    platform, e.Message);
            }
        }

        public static void BuildPlayer(string platform)
        {
            try
            {
                var buildPlatform = Name2BuildTarget(platform);
                SwitchBuildPlatform(buildPlatform.TargetGroup, buildPlatform.Target);
                var buildAsset = BuildHelper.GetBuildAsset(buildPlatform.PlateForm);
                new BuildPlayerProcess(buildAsset).BeginBuild();
            }
            catch (Exception e)
            {
                FxDebug.ColorError(FX_LOG_CONTROL.Red, "Build player failure in platform {0} with error {1}",
                    platform, e.Message);
            }
            
        }

        public static void BuildOneTime(string platform)
        {
            try
            {
                var buildPlatform = Name2BuildTarget(platform);
                SwitchBuildPlatform(buildPlatform.TargetGroup, buildPlatform.Target);
                var buildAsset = BuildHelper.GetBuildAsset(buildPlatform.PlateForm);
                new BuildBundleProcess(buildAsset).BeginBuild();
                new BuildPlayerProcess(buildAsset).BeginBuild();
            }
            catch (Exception e)
            {
                FxDebug.ColorError(FX_LOG_CONTROL.Red, "Build failure in platform {0} with error {1}",
                    platform, e.Message);
            }
        }
        
        private static void BuildBundleInternal()
        {
            BuildBundle("StandaloneWindows64");
        }

        private static void BuildInternal()
        {
            BuildOneTime("");
        }

        private static void SwitchBuildPlatform(BuildTargetGroup targetGroup, BuildTarget target)
        {
            var switchRes = EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, target);
            if (!switchRes)
                throw new Exception("Switch platform failure!");
            
        }

        private static BuildTargetInfo Name2BuildTarget(string platform)
        {
            BuildTargetInfo info = new BuildTargetInfo();
            switch (platform)
            {
                case "Android":
                    info.Target = BuildTarget.Android;
                    info.TargetGroup = BuildTargetGroup.Android;
                    info.PlateForm = BuildPlateForm.Android;
                    break;
                case "StandaloneWindows":
                    info.Target = BuildTarget.StandaloneWindows;
                    info.TargetGroup = BuildTargetGroup.Standalone;
                    info.PlateForm = BuildPlateForm.Window;
                    break;
                case "StandaloneWindows64":
                    info.Target = BuildTarget.StandaloneWindows64;
                    info.TargetGroup = BuildTargetGroup.Standalone;
                    info.PlateForm = BuildPlateForm.Window;
                    break;
                case "IOS":
                    info.Target = BuildTarget.iOS;
                    info.TargetGroup = BuildTargetGroup.iOS;
                    info.PlateForm = BuildPlateForm.IOS;
                    break;
                case "StandaloneOSX":
                    info.Target = BuildTarget.StandaloneOSX;
                    info.TargetGroup = BuildTargetGroup.iOS;
                    info.PlateForm = BuildPlateForm.MacOS;
                    break;
                case "WebGL":
                    info.Target = BuildTarget.WebGL;
                    info.TargetGroup = BuildTargetGroup.WebGL;
                    info.PlateForm = BuildPlateForm.WebGL;
                    break;
                default:
                    throw new Exception($"Platform {platform} is invalid!");
            }
            return info;
        }
        
        private struct BuildTargetInfo
        {
            internal BuildTarget Target;
            internal BuildTargetGroup TargetGroup;
            internal BuildPlateForm PlateForm;
        }
    }
}