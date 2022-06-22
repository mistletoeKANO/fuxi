using UnityEditor;
using UnityEngine;

namespace FuXi.Editor
{
    public static class Fx_Style
    {
        public static readonly Texture2D Fx_Asset = Resources.Load<Texture2D>("Gizmos/Fx_Asset");
        public static readonly Texture2D Fx_About = Resources.Load<Texture2D>("Gizmos/Fx_About");
        public static readonly Texture2D Fx_PathMenu = Resources.Load<Texture2D>("Gizmos/Fx_PathMenu");
        public static readonly Texture2D Fx_AssetBlack = Resources.Load<Texture2D>("Gizmos/Fx_Asset Black");
        public static readonly Texture2D Fx_AssetPackage = Resources.Load<Texture2D>("Gizmos/Fx_AssetPackage");

        public static readonly string Fx_CommonInspector_Uss = "Uss/Fx_CommonInspector";
        public static readonly string Fx_BuildAsset_Uss = "Uss/Fx_BuildAsset";
        
        public static readonly string Fx_Inspector_Margins = "fx-inspector-margins";
        
        public static readonly string Fx_BuildAsset_Root_Class = "fx-buildAsset-root";
        public static readonly string Fx_BuildAsset_Main_Class = "fx-buildAsset-main";
        public static readonly string Fx_BuildAsset_Foot_Class = "fx-buildAsset-foot";
        
        internal static readonly GUIContent Title = new GUIContent("引用分析", Resources.Load<Texture2D>("Gizmos/Fx_Asset Black"));
        internal static readonly GUIContent PrefabIcon = EditorGUIUtility.IconContent("d_Prefab Icon");
        internal static readonly GUIContent BundleIcon = EditorGUIUtility.IconContent("d_ScriptableObject Icon");
        internal static readonly GUIContent PinButton = EditorGUIUtility.IconContent("d__Help@2x");
        
        internal static readonly GUIStyle ByteStyle = new GUIStyle
            {alignment = TextAnchor.MiddleRight, normal = {textColor = new Color(1f, 0.42f, 0.1f)}};

        internal static readonly GUIStyle FooterLabelInfo = new GUIStyle
        {
            margin = new RectOffset(18,0,0,0),
            normal = new GUIStyleState{textColor = Color.gray}
        };

        internal static readonly string CName_BF_Toolbar = "bf-toolbar";
        internal static readonly string CName_BF_Toolbar_Enum = "bf-toolbar-enum";
        internal static readonly string CName_BF_MainView_Header = "bf-main-view-header";
        internal static readonly string CName_BF_MainView = "bf-main-view";
        internal static readonly string CName_BF_MainView_BG = "bf-main-view-bg";
        internal static readonly string CName_BF_MainView_ScrollView = "bf-main-view-scroll-view";
        internal static readonly string CName_BF_Footer = "bf-footer";
        internal static readonly string CName_BF_Footer_Header = "bf-footer-header";
        internal static readonly string CName_BF_Footer_Label = "bf-footer-label";
        internal static readonly string CName_BF_Footer_View = "bf-footer-view";

        internal static readonly Color C_ColumnDark = new Color(0.16f, 0.16f, 0.16f, 0.6f);
        internal static readonly Color C_ColumnLight = new Color(0.25f, 0.25f, 0.25f, 0.6f);
    }
}