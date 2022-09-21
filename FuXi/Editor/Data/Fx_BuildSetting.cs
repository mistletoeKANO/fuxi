using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace FuXi.Editor
{
    public enum BuildPlateForm
    {
        Window,
        Android,
        IOS,
        MacOS,
        WebGL,
    }
    
    public class Fx_BuildSetting : ScriptableObject
    {
        public string BundleRootPath;
        public string ExtensionName;
        public BuildPlateForm FxPlatform;
        public string EncryptType = "None";
        public bool OpenBreakResume;
        public bool CopyAllBundle2Player;
        public List<string> ExcludeExtensions;
        public List<Fx_BuildPackage> BuiltinPackages;
    }
}