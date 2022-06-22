using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace FuXi.Editor
{
    public enum BundleMode
    {
        PackByFile,
        PackTogether,
        PackByDirectory,
        PackByTopDirectory,
        PackByRaw,
    }
    [Serializable]
    public class Fx_Object
    {
        public BundleMode bundleMode = BundleMode.PackByFile;
        public UnityEngine.Object folder;
    }
    internal class Fx_BuildAsset : ScriptableObject
    {
        public int bundleVersion;
        public string playerVersion;
        public BuildAssetBundleOptions buildAssetBundleOptions = BuildAssetBundleOptions.ChunkBasedCompression;
        public List<Fx_Object> fx_Objects;
    }
}