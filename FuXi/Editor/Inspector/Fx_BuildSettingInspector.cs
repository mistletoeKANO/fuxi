using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace FuXi.Editor
{
    [CustomEditor(typeof(Fx_BuildSetting), true)]
    public class Fx_BuildSettingInspector : UnityEditor.Editor
    {
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
        private bool IsCopyAllValid = true;

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
            this.IsCopyAllValid = this.CheckCopyAllValidate();
        }
        public override bool UseDefaultMargins() { return false; }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(this.m_BundleRootPath, Style.BundleRootPath);
            EditorGUILayout.PropertyField(this.m_ExtensionName, Style.ExtensionName);
            EditorGUILayout.PropertyField(this.m_FxPlatform, Style.FxPlatform);

            EditorGUI.BeginChangeCheck();
            int selectIndex = EditorGUILayout.Popup(Style.EncryptType, this.encryptSelectIndex, this.encryptOptions);
            if (selectIndex != this.encryptSelectIndex)
            {
                this.m_EncryptType.stringValue = this.encryptOptions[selectIndex];
                this.encryptSelectIndex = selectIndex;
            }
            EditorGUILayout.PropertyField(this.m_CopyAllBundle2Player, Style.CopyAllBundle2Player);
            if (EditorGUI.EndChangeCheck())
            {
                this.IsCopyAllValid = this.CheckCopyAllValidate();
            }

            if (!this.IsCopyAllValid)
            {
                EditorGUILayout.HelpBox("仅未加密或者OFFSET加密支持全拷贝! 详情查看相关说明!", MessageType.Warning);
            }

            EditorGUILayout.PropertyField(this.m_ExcludeExtensions, Style.ExcludeExtensions);
            EditorGUILayout.PropertyField(this.m_BuiltinPackages, Style.BuiltinPackages);

            this.OnFooterGUI();
            serializedObject.ApplyModifiedProperties();
        }

        private bool CheckCopyAllValidate()
        {
            if (this.encryptSelectIndex == 0)
                return true;
            var encryptFullName = this.encryptOptions[encryptSelectIndex];
            var encryptType = BuildHelper.LoadEncryptObject(encryptFullName);
            if (encryptType == null)
                return true;
            if (this.m_CopyAllBundle2Player.boolValue)
                return encryptType.EncryptMode == EncryptMode.OFFSET;
            return true;
        }

        private void OnFooterGUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy Bundle", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Copy Bundle", "Are you sure Copy bundle to StreamingAssets?",
                    "YES", "NO"))
                {
                    EditorExtension.CallDelay(this.DelayCopyBundle, 0.1f);
                    return;
                }
            }
            if (GUILayout.Button("Clear Bundle", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Clear Bundle", "Are you sure Clear bundle from StreamingAssets?",
                    "YES", "NO"))
                {
                    EditorExtension.CallDelay(this.DelayClearBundle, 0.1f);
                    return;
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