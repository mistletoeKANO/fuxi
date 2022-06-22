using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace FuXi.Editor
{
    [CustomEditor(typeof(Fx_BuildSetting), true)]
    public class Fx_BuildSettingInspector : UnityEditor.Editor
    {
        private VisualElement m_Root = null;
        static class Style
        {
            public static readonly GUIContent BundleRootPath = EditorGUIUtility.TrTextContent("资源根路径","AssetBundle 资源 根路径");
            public static readonly GUIContent ExtensionName = EditorGUIUtility.TrTextContent("AB包拓展名");
            public static readonly GUIContent FxPlatform = EditorGUIUtility.TrTextContent("配置所属平台");
            public static readonly GUIContent EncryptType = EditorGUIUtility.TrTextContent("加密类型");
            public static readonly GUIContent CopyAllBundle2Player = EditorGUIUtility.TrTextContent("拷贝所有Bundle到安装包");
            public static readonly GUIContent ExcludeExtensions = EditorGUIUtility.TrTextContent("忽略打包文件后缀");
            public static readonly GUIContent BuiltinPackages = EditorGUIUtility.TrTextContent("首包包含的分包");
        }
        
        SerializedProperty m_BundleRootPath;
        SerializedProperty m_ExtensionName;
        SerializedProperty m_FxPlatform;
        SerializedProperty m_EncryptType;
        SerializedProperty m_CopyAllBundle2Player;
        SerializedProperty m_ExcludeExtensions;
        SerializedProperty m_BuiltinPackages;

        private string[] encryptOptions;
        private int encryptSelectIndex = 0;

        private void OnEnable()
        {
            this.m_BundleRootPath = serializedObject.FindProperty("BundleRootPath");
            this.m_ExtensionName = serializedObject.FindProperty("ExtensionName");
            this.m_FxPlatform = serializedObject.FindProperty("FxPlatform");
            this.m_EncryptType = serializedObject.FindProperty("EncryptType");
            this.m_CopyAllBundle2Player = serializedObject.FindProperty("CopyAllBundle2Player");
            this.m_ExcludeExtensions = serializedObject.FindProperty("ExcludeExtensions");
            this.m_BuiltinPackages = serializedObject.FindProperty("BuiltinPackages");

            this.InitEncrypt();
        }

        private void InitEncrypt()
        {
            this.encryptOptions = BuildHelper.GetEncryptOptions();
            for (int i = 0; i < this.encryptOptions.Length; i++)
            {
                if (this.encryptOptions[i] == this.m_EncryptType.stringValue)
                {
                    this.encryptSelectIndex = i;
                }
            }
        }
        public override bool UseDefaultMargins() { return false; }
        public override VisualElement CreateInspectorGUI()
        {
            this.m_Root = new VisualElement();
            var commonStyle = Resources.Load<StyleSheet>(Fx_Style.Fx_CommonInspector_Uss);
            if (commonStyle != null)
            {
                this.m_Root.styleSheets.Add(commonStyle);
            }
            this.m_Root.AddToClassList(Fx_Style.Fx_Inspector_Margins);
            
            IMGUIContainer imguiContainer = new IMGUIContainer(this.OnGUI);
            this.m_Root.Add(imguiContainer);

            IMGUIContainer footerContainer = new IMGUIContainer(this.OnFooterGUI);
            this.m_Root.Add(footerContainer);

            return this.m_Root;
        }

        private void OnGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(this.m_BundleRootPath, Style.BundleRootPath);
            EditorGUILayout.PropertyField(this.m_ExtensionName, Style.ExtensionName);
            EditorGUILayout.PropertyField(this.m_FxPlatform, Style.FxPlatform);

            int selectIndex = EditorGUILayout.Popup(Style.EncryptType, this.encryptSelectIndex, this.encryptOptions);
            if (selectIndex != this.encryptSelectIndex)
            {
                this.m_EncryptType.stringValue = this.encryptOptions[selectIndex];
                this.encryptSelectIndex = selectIndex;
            }
            EditorGUILayout.PropertyField(this.m_CopyAllBundle2Player, Style.CopyAllBundle2Player);
            EditorGUILayout.PropertyField(this.m_ExcludeExtensions, Style.ExcludeExtensions);
            EditorGUILayout.PropertyField(this.m_BuiltinPackages, Style.BuiltinPackages);
            serializedObject.ApplyModifiedProperties();
        }
        
        private void OnFooterGUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy Bundle", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Copy Bundle", "Are you sure Copy bundle to StreamingAssets?",
                    "YES", "NO"))
                {
                    EditorApplication.delayCall += this.DelayCopyBundle;
                }
            }
            if (GUILayout.Button("Clear Bundle", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Clear Bundle", "Are you sure Clear bundle from StreamingAssets?",
                    "YES", "NO"))
                {
                    EditorApplication.delayCall += this.DelayClearBundle;
                }
            }

            GUILayout.EndHorizontal();
        }
        
        private void DelayCopyBundle()
        {
            var fxAsset = AssetDatabase.LoadAssetAtPath<Fx_BuildAsset>(AssetDatabase.GetAssetPath(this.target));
            new BuildPlayerProcess(fxAsset).BeginCopyBundle();
        }
        
        private void DelayClearBundle()
        {
            new BuildPlayerProcess().BeginClearStreamingAssets();
        }
    }
}