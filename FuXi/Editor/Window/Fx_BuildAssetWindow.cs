using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace FuXi.Editor {
    internal class Fx_BuildAssetWindow : EditorWindow, IHasCustomMenu {
        private Fx_BuildAsset target;

        private void OnEnable () {
            this.titleContent = Style.buildAssetInfo;
        }

        public void SetAsset (Fx_BuildAsset asset) {
            this.target = asset;
            UnityEngine.Debug.Log(asset);
            OnRefresh ();
        }

        public void AddItemsToMenu (GenericMenu menu) { }

        private void OnDestroy () { }

        static class Style {
            public static readonly Vector2 IconSize = new Vector2 (16, 16);
            public static readonly Vector2 IconBGSize = new Vector2 (36, 16);

            public static readonly GUIStyle SMenuBG = new GUIStyle ("PreToolbar2");
            public static readonly GUIContent buildAssetInfo =
                EditorGUIUtility.TrTextContent ("打包资产主配置", "包含版本文件列表, 包含 需要 动态加载的 所有 需要热更新的 资源. 被依赖资源 可选择添加, 未添加资源 会被自动打包", Fx_Style.Fx_Asset);

            public static readonly GUIContent buildAssetInfo2 = EditorGUIUtility.TrTextContent ("打包资产主配置");
            public static readonly GUIContent CAboutFuXiButton =
                EditorGUIUtility.TrTextContent ("", "关于伏羲", Fx_Style.Fx_About);
            public static readonly GUIContent CFxPathMenu =
                EditorGUIUtility.TrTextContent ("", "菜单", Fx_Style.Fx_Asset);

            public static readonly GUIContent FFxPathMenu =
                EditorGUIUtility.TrTextContent ("", "引用分析", Fx_Style.Fx_AssetPackage);
            public static readonly GUIContent CBundleVersion = EditorGUIUtility.TrTextContent ("Bundle版本");
            public static readonly GUIContent CPlayerVersion = EditorGUIUtility.TrTextContent ("安装包版本");

            public static readonly GUIContent CBuildAssetBundleOptions =
                EditorGUIUtility.TrTextContent ("构建选项", "AssetBundle包构建选项");

            public static readonly GUIContent CPackageList =
                EditorGUIUtility.TrTextContent ("版本文件列表", "项目所有动态加载的资源文件, 依赖资源可以不包含, 后面自动打包.");
        }

        SerializedProperty m_BVersion;
        SerializedProperty m_PVersion;
        SerializedProperty m_BuildAssetBundleOptions;
        SerializedProperty m_FxObjectList;
        SerializedObject serializedObject;
        private GUIContent m_Name;

        private void OnRefresh () {
            if (this.target == null) {
                this.enabled = false;
                return;
            }
            serializedObject = new SerializedObject (this.target);
            this.m_Name = EditorGUIUtility.TrTextContent (this.target.name, "包含版本文件列表, 包含 需要 动态加载的 所有 需要热更新的 资源. 被依赖资源 可选择添加, 未添加资源 会被自动打包");
            this.m_BVersion = serializedObject.FindProperty ("bundleVersion");
            this.m_PVersion = serializedObject.FindProperty ("playerVersion");
            this.m_BuildAssetBundleOptions = serializedObject.FindProperty ("buildAssetBundleOptions");
            this.m_FxObjectList = serializedObject.FindProperty ("fx_Objects");
            this.enabled = true;
        }

        protected void OnGUIHeader () {
            GUILayout.Label (Style.buildAssetInfo2);
            Rect lastRect = GUILayoutUtility.GetLastRect ();
            GUI.Box (new Rect (new Vector2 (lastRect.width - 84, 5), Style.IconBGSize), Texture2D.blackTexture, Style.SMenuBG);
            var destRect = new Rect (new Vector2 (lastRect.width - 64, 5), Style.IconSize);
            if (GUI.Button (destRect, Style.CAboutFuXiButton, EditorExtension.EditorStyle.IconButton))
                Application.OpenURL (Fx_EditorConfigs.AboutURL);
            destRect = new Rect (new Vector2 (lastRect.width - 80, 5), Style.IconSize);
            if (GUI.Button (destRect, Style.CFxPathMenu, EditorExtension.EditorStyle.IconButton))
                this.DrawPathMenu ();
                
            destRect = new Rect (new Vector2 (lastRect.width - 48, 5), Style.IconSize);
            if (GUI.Button (destRect, Style.FFxPathMenu, EditorExtension.EditorStyle.IconButton))
                EditorWindow.GetWindow<Fx_BundleReferenceWindow> ();
        }

        private void DrawPathMenu () {
            GenericMenu genericMenu = new GenericMenu ();
            genericMenu.AddItem (new GUIContent ("P AB包目录"), false, () => {
                EditorUtility.OpenWithDefaultApp (FxBuildPath.BundleRootPath ());
            });
            genericMenu.AddItem (new GUIContent ("P 安装包目录"), false, () => {
                EditorUtility.OpenWithDefaultApp (FxBuildPath.PlayerRootPath ());
            });
            genericMenu.AddItem (new GUIContent ("P 下载缓存目录"), false, () => {
                EditorUtility.OpenWithDefaultApp (Application.persistentDataPath);
            });
            genericMenu.AddItem (new GUIContent ("P 项目资源目录"), false, () => {
                EditorUtility.OpenWithDefaultApp (Application.dataPath);
            });
            genericMenu.AddItem (new GUIContent ("P 游戏临时缓存目录"), false, () => {
                EditorUtility.OpenWithDefaultApp (Application.temporaryCachePath);
            });
            genericMenu.AddSeparator ("");
            genericMenu.AddItem (new GUIContent ("D 清除下载缓存"), false, this.DeleteDownloadCache);
            genericMenu.AddItem (new GUIContent ("D 清除AB包"), false, this.DeleteAssetBundle);
            genericMenu.AddItem (new GUIContent ("D 清除安装包"), false, this.DeletePlayer);
            genericMenu.AddSeparator ("");
            genericMenu.AddItem (new GUIContent ("V 配置版本管理"), false, this.OpenVerWindow);
            genericMenu.ShowAsContext ();
        }

        private void DeleteDownloadCache () {
            if (EditorUtility.DisplayDialog ("清除游戏下载缓存", "Are you sure clear all download cache for this rule?",
                    "YES", "NO")) {
                FxCacheHelper.ClearBundleCache ();
            }
        }

        private void DeleteAssetBundle () {
            if (EditorUtility.DisplayDialog ("清除AB包", "Are you sure clear all AB for this rule?",
                    "YES", "NO")) {
                Fx_BuildSetting setting = AssetDatabase.LoadAssetAtPath<Fx_BuildSetting> (AssetDatabase.GetAssetPath (target));
                FxBuildCache.DeleteAssetBundle (setting.FxPlatform);
            }
        }

        private void DeletePlayer () {
            if (EditorUtility.DisplayDialog ("清除安装包", "Are you sure clear all application for this rule?",
                    "YES", "NO")) {
                Fx_BuildSetting setting = AssetDatabase.LoadAssetAtPath<Fx_BuildSetting> (AssetDatabase.GetAssetPath (target));
                FxBuildCache.DeletePlayer (setting.FxPlatform);
            }
        }

        private void OpenVerWindow () {
            Fx_VersionManagerWindow[] windows = Resources.FindObjectsOfTypeAll<Fx_VersionManagerWindow> ();
            var window = windows.Length != 0 ? windows[0] : default;
            if (window != default) {
                window.OpenWindow ((Fx_BuildAsset) this.target);
                window.Focus ();
                return;
            }
            window = CreateInstance<Fx_VersionManagerWindow> ();
            window.OpenWindow ((Fx_BuildAsset) this.target);
            window.Show ();
        }
        private bool enabled = false;
        Vector2 scrollPos;
        public void OnGUI () {
            if (!this.enabled){OnRefresh (); return;};

            serializedObject.Update ();
            OnGUIHeader ();
            EditorGUILayout.Space (2);
            EditorGUIUtility.labelWidth = 120;

            EditorGUILayout.BeginHorizontal ();

            GUI.enabled = false;
            EditorGUILayout.PropertyField (this.m_BVersion, Style.CBundleVersion);
            this.VersionContext ();
            EditorGUILayout.PropertyField (this.m_PVersion, Style.CPlayerVersion);
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.PropertyField (this.m_BuildAssetBundleOptions, Style.CBuildAssetBundleOptions);
            scrollPos = EditorGUILayout.BeginScrollView (scrollPos, GUILayout.Width (position.width - 10), GUILayout.MaxHeight (position.height - 50));
            EditorGUILayout.PropertyField (this.m_FxObjectList, Style.CPackageList);
            this.CheckFxObjectList ();
            this.OnFooterGUI ();
            EditorGUILayout.EndScrollView ();
            serializedObject.ApplyModifiedProperties ();
        }

        private void VersionContext () {
            var lastRect = GUILayoutUtility.GetLastRect ();
            if (!lastRect.Contains (Event.current.mousePosition)) return;

            if (Event.current.button != 1) return;
            GenericMenu genericMenu = new GenericMenu ();
            genericMenu.AddItem (new GUIContent ("版本号>>1"), false, () => {
                Fx_BuildAsset asset = (Fx_BuildAsset) this.target;
                asset.bundleVersion++;
                EditorUtility.SetDirty (asset);
                AssetDatabase.SaveAssets ();
            });
            genericMenu.AddItem (new GUIContent ("版本号<<1"), false, () => {
                Fx_BuildAsset asset = (Fx_BuildAsset) this.target;
                asset.bundleVersion = Mathf.Max (0, --asset.bundleVersion);
                EditorUtility.SetDirty (asset);
                AssetDatabase.SaveAssets ();
            });
            genericMenu.AddItem (new GUIContent ("重置Bundle版本号"), false, () => {
                Fx_BuildAsset asset = (Fx_BuildAsset) this.target;
                asset.bundleVersion = 0;
                EditorUtility.SetDirty (asset);
                AssetDatabase.SaveAssets ();
            });

            genericMenu.ShowAsContext ();
        }

        private void CheckFxObjectList () {
            var fxObj = ((Fx_BuildAsset) this.target).fx_Objects;
            if (fxObj == null) return;
            string elements = string.Empty;
            for (int i = 0; i < fxObj.Count; i++) {
                if (fxObj[i].folder != null) continue;
                elements = string.Concat (elements, $" {i}");
            }
            if (string.IsNullOrEmpty (elements)) return;
            EditorGUILayout.HelpBox ($"Element{elements} folder is null or missing!!!", MessageType.Error);
        }

        private void OnFooterGUI () {
            GUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Build Bundle", GUILayout.Height (30))) {
                if (EditorUtility.DisplayDialog ("Build Asset Bundle", "Are you sure build asset bundle for this rule?",
                        "YES", "NO")) {
                    EditorExtension.CallDelay (this.DelayBuildBundle, 0.1f);
                    return;
                }
            }

            if (GUILayout.Button ("Build Player", GUILayout.Height (30))) {
                if (EditorUtility.DisplayDialog ("Build Player", "Are you sure build player for this rule?",
                        "YES", "NO")) {
                    EditorExtension.CallDelay (this.DelayBuildPlayer, 0.1f);
                    return;
                }
            }
            GUILayout.EndHorizontal ();
        }

        private void DelayBuildBundle () {
            new BuildBundleProcess ((Fx_BuildAsset) this.target).BeginBuild ();
        }

        private void DelayBuildPlayer () {
            new BuildPlayerProcess ((Fx_BuildAsset) this.target).BeginBuild ();
        }
    }
}