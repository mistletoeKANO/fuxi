using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace FuXi.Editor {
    internal class Fx_BuildPackageWindow : EditorWindow, IHasCustomMenu {
        private Fx_BuildPackage target;

        private void OnEnable () {
            this.titleContent = Style.PackageObjects;
        }

        public void SetAsset (Fx_BuildPackage asset) {
            this.target = asset;
            OnRefresh ();
        }

        static class Style {
            public static readonly GUIContent PackageObjects = EditorGUIUtility.TrTextContent ("分包资产");
        }
        public void AddItemsToMenu (GenericMenu menu) { }

        SerializedProperty m_PackageObjects;
        bool open;
        string m_Name;
        public GUIContent rename = EditorGUIUtility.TrTextContent ("", "修改分包名");

        SerializedObject serializedObject;

        private void OnRefresh () {
            if (this.target == null) {
                this.enabled = false;
                return;
            }
            serializedObject = new SerializedObject (this.target);
            this.m_PackageObjects = serializedObject.FindProperty ("PackageObjects");
            m_Name = target.name;
            rename.text = target.name;
            this.enabled = true;
        }
        private bool enabled = false;

        Vector2 scrollPos;

        public void OnGUI () {
            if (!this.enabled){OnRefresh (); return;};
            serializedObject.Update ();

            EditorGUILayout.BeginHorizontal ();
            if (open) {
                m_Name = EditorGUILayout.TextField (m_Name);
                if (GUILayout.Button ("修改")) {
                    if (target.name != m_Name) {
                        target.name = m_Name;
                        AssetDatabase.SaveAssets ();
                    }
                    rename.text = m_Name;
                    open = false;
                };
            } else {
                if (GUILayout.Button (rename)) {
                    open = true;
                }
            }

            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.Space (2);
            scrollPos = EditorGUILayout.BeginScrollView (scrollPos, GUILayout.Width (position.width - 10), GUILayout.MaxHeight (position.height - 50));
            EditorGUILayout.PropertyField (this.m_PackageObjects, Style.PackageObjects);

            EditorGUILayout.EndScrollView ();
            serializedObject.ApplyModifiedProperties ();
        }
    }
}