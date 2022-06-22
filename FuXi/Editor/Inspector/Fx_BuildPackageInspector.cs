using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace FuXi.Editor
{
    [CustomEditor(typeof(Fx_BuildPackage), true)]
    [CanEditMultipleObjects]
    public class Fx_BuildPackageInspector : UnityEditor.Editor
    {
        private VisualElement m_Root = null;
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
        public override VisualElement CreateInspectorGUI()
        {
            this.m_Root = new VisualElement();
            var mainStyle = Resources.Load<StyleSheet>(Fx_Style.Fx_CommonInspector_Uss);
            if (mainStyle != null)
            {
                this.m_Root.styleSheets.Add(mainStyle);
            }
            this.m_Root.AddToClassList(Fx_Style.Fx_Inspector_Margins);
            
            IMGUIContainer imguiContainer = new IMGUIContainer(this.OnGUI);
            this.m_Root.Add(imguiContainer);

            return this.m_Root;
        }

        private void OnGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(this.m_PackageObjects, Style.PackageObjects);
            serializedObject.ApplyModifiedProperties();
        }
    }
}