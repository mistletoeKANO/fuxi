using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace FuXi.Editor
{
    internal class Fx_VersionManagerWindow : EditorWindow, IHasCustomMenu
    {
        private readonly Vector2 WindowMinSize = new Vector2(600, 320); 
        private string m_CacheRootPath = String.Empty;
        private List<string> m_AllVersionFiles;
        private string m_SelectVersionFile = string.Empty;
        private FxManifest m_Manifest;
        private FxManifest m_LastManifest;
        private BundleManifest[] m_DiffBundleManifest;

        private Toolbar m_ToolBar;
        private VisualElement m_MainView;
        private Label m_Footer;
        
        private Vector2 m_BundleListScrollPos = Vector2.zero;
        private readonly List<string> versionFileOptions = new List<string>{"版本文件列表", "对比上一版本差异"};
        private int m_SelectVersionOption = 0;

        private void OnEnable() 
        { 
            this.titleContent = Fx_Style.VerWindowTitle;
            this.minSize = WindowMinSize;
        }
        internal void OpenWindow(Fx_BuildAsset buildAsset)
        {
            var path = AssetDatabase.GetAssetPath(buildAsset);
            Fx_BuildSetting buildSetting = AssetDatabase.LoadAssetAtPath<Fx_BuildSetting>(path);
            
            this.m_CacheRootPath = FxBuildCache.BundleCachePlatformPath(buildSetting.FxPlatform);
            DirectoryInfo directoryInfo = new DirectoryInfo(this.m_CacheRootPath);
            FileInfo[] verFileInfos = directoryInfo.GetFiles("*_V*.json");
            this.m_AllVersionFiles = verFileInfos.Select(v => v.Name.Replace(v.Extension, "")).ToList();
            if (this.m_AllVersionFiles.Count > 0)
                this.m_SelectVersionFile = this.m_AllVersionFiles[0];

            this.InitElement();
            this.InitVersionView(this.m_SelectVersionFile);
        }

        private void InitElement()
        {
            var style = Resources.Load<StyleSheet>("Uss/Fx_VersionManagerWindow");
            this.rootVisualElement.styleSheets.Add(style);

            this.m_ToolBar = new Toolbar();
            this.m_ToolBar.AddToClassList(Fx_Style.CName_VM_Toolbar);
            this.m_ToolBar.Add(new Label("版本:"));
            var toolbarMenu = new ToolbarMenu {text = this.m_SelectVersionFile};
            toolbarMenu.AddToClassList(Fx_Style.CName_VM_Toolbar_DropMenu);
            foreach (var file in this.m_AllVersionFiles)
            {
                toolbarMenu.menu.AppendAction(file, d =>
                {
                    toolbarMenu.text = file;
                    this.InitVersionView(file);
                });
            }
            this.m_ToolBar.Add(toolbarMenu);

            this.m_ToolBar.Add(new Label("选项:"));
            var toolbarMenu2 = new ToolbarMenu {text = versionFileOptions[this.m_SelectVersionOption]};
            foreach (var option in versionFileOptions)
            {
                toolbarMenu2.menu.AppendAction(option, d =>
                {
                    toolbarMenu2.text = option;
                    this.m_SelectVersionOption = versionFileOptions.IndexOf(option);
                });
            }
            this.m_ToolBar.Add(toolbarMenu2);
            
            this.rootVisualElement.Add(this.m_ToolBar);

            this.m_MainView = new VisualElement();
            this.m_MainView.AddToClassList(Fx_Style.CName_VM_MainView);
            var bg = new Image();
            bg.AddToClassList(Fx_Style.CName_VM_MainView_BG);
            this.m_MainView.Add(bg);
            
            TwoPaneSplitView splitView = new TwoPaneSplitView(0, 240, TwoPaneSplitViewOrientation.Horizontal);
            var versionInfo = new IMGUIContainer(this.OnVersionInfoGUI);
            versionInfo.AddToClassList(Fx_Style.CName_VM_MainView_Info);
            splitView.Add(versionInfo);
            
            var versionBundleList = new IMGUIContainer(this.OnVersionBundleListGUI);
            versionBundleList.AddToClassList(Fx_Style.CName_VM_MainView_BundleList);
            splitView.Add(versionBundleList);
            
            this.m_MainView.Add(splitView);
            this.rootVisualElement.Add(this.m_MainView);

            this.m_Footer = new Label();
            this.m_Footer.AddToClassList(Fx_Style.CName_VM_Footer);
            this.rootVisualElement.Add(this.m_Footer);
        }

        private void InitVersionView(string selectFile)
        {
            this.m_SelectVersionFile = selectFile;
            if (string.IsNullOrEmpty(selectFile)) return;
            var filePath = $"{this.m_CacheRootPath}/{selectFile}.json";
            this.m_Footer.text = filePath;
            if (!File.Exists(filePath)) return;

            var content = File.ReadAllText(filePath, Encoding.UTF8);
            this.m_Manifest = FxManifest.Parse(content);

            var lastFilePath = filePath.Replace($"V{this.m_Manifest.ResVersion}", $"V{this.m_Manifest.ResVersion - 1}");
            if (File.Exists(lastFilePath))
            {
                content = File.ReadAllText(lastFilePath, Encoding.UTF8);
                this.m_LastManifest = FxManifest.Parse(content);
            }
            else
            {
                this.m_LastManifest = null;
                this.m_DiffBundleManifest = null;
            }
            if (this.m_LastManifest == null) return;
            var oldListNames = this.m_LastManifest.Bundles.Select(m => m.BundleHashName);
            this.m_DiffBundleManifest =
                this.m_Manifest.Bundles.Where(m => !oldListNames.Contains(m.BundleHashName)).ToArray();
        }

        private void OnVersionInfoGUI()
        {
            GUILayout.Space(10);
            if (this.m_Manifest == null)
            {
                GUILayout.Label("未发现版本构建记录!", Fx_Style.LabelTitleCyan);
                return;
            }
            GUILayout.Label("版本信息", Fx_Style.LabelTitleCyan);
            GUILayout.Space(10);
            GUILayout.Label($"资源版本号: {this.m_Manifest.ResVersion}", Fx_Style.LabelInfo);
            GUILayout.Label($"版本资源大小: {FxUtility.FormatBytes(this.m_Manifest.Bundles.Sum(b => b.Size))}",
                Fx_Style.LabelInfo);
            GUILayout.Label($"Bundle包数量: {this.m_Manifest.Bundles.Length}", Fx_Style.LabelInfo);
            GUILayout.Label($"Asset资产数量: {this.m_Manifest.Assets.Length}", Fx_Style.LabelInfo);
        }

        private void OnVersionBundleListGUI()
        {
            if (this.m_Manifest == null) return;
            GUILayout.Space(10);
            switch (this.m_SelectVersionOption)
            {
                case 0: this.OnDrawVerBundleList();
                    break;
                case 1: this.OnDrawVerDiffBundleList();
                    break;
            }
        }

        private void OnDrawVerBundleList()
        {
            GUILayout.Label("Bundle文件列表", Fx_Style.LabelTitleCyan);
            GUILayout.Box("", Fx_Style.Space);
            this.m_BundleListScrollPos = GUILayout.BeginScrollView(this.m_BundleListScrollPos, false, false);
            foreach (var bundle in this.m_Manifest.Bundles)
            {
                GUILayout.Label(bundle.BundleHashName, Fx_Style.LabelBG);
            }
            GUILayout.EndScrollView();
        }

        private void OnDrawVerDiffBundleList()
        {
            GUILayout.Label("版本差异信息", Fx_Style.LabelTitleCyan);
            GUILayout.Space(10);
            var size = m_DiffBundleManifest?.Sum(m => m.Size) ?? 0;
            GUILayout.Label($"差异文件大小: {FxUtility.FormatBytes(size)}", Fx_Style.LabelInfoMiddle);
            if (this.m_DiffBundleManifest == null || this.m_DiffBundleManifest.Length == 0) return;
            GUILayout.Box("", Fx_Style.Space);
            this.m_BundleListScrollPos = GUILayout.BeginScrollView(this.m_BundleListScrollPos, false, false);
            foreach (var bundle in this.m_DiffBundleManifest)
            {
                GUILayout.Label(bundle.BundleHashName, Fx_Style.LabelBG);
            }
            GUILayout.EndScrollView();
        }

        public void AddItemsToMenu(GenericMenu menu) { }
    }
}