using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace FuXi.Editor
{
    [CustomEditor(typeof(Fx_BuildPackage), true)]
    [CanEditMultipleObjects]
    public class Fx_BuildPackageInspector : UnityEditor.Editor
    {
        static class Style
        {
            public static readonly GUIContent PackageObjects = EditorGUIUtility.TrTextContent("分包资产");
        }
        
        SerializedProperty m_PackageObjects;

        private void OnEnable()
        {
            this.m_PackageObjects = serializedObject.FindProperty("PackageObjects");
        }

        public override bool UseDefaultMargins() { return false; }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(this.m_PackageObjects, Style.PackageObjects);
            serializedObject.ApplyModifiedProperties();
        }
    }
}