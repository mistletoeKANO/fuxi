using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace FuXi.Editor
{
    internal class Fx_BundleReferenceWindow : EditorWindow, IHasCustomMenu
    {
        private Toolbar m_ToolBar;
        private VisualElement m_MainView;
        private VisualElement m_Footer;

        private Fx_BundleView m_BundleView;
        private Fx_AssetView m_AssetView;
        private const int ColumnHeight = 20;

        private ViewType m_ViewType = ViewType.BundleView;
        
        private void OnEnable()
        {
            this.InitWindow();
            this.InitGUIData();
            this.DrawGUI();
        }
        private void InitWindow() { this.titleContent = Fx_Style.RefWindowTitle; }

        private void InitGUIData()
        {
            this.m_BundleView = new Fx_BundleView(ColumnHeight);
            this.m_AssetView = new Fx_AssetView(this, ColumnHeight);
        }

        private void DrawGUI()
        {
            var style = Resources.Load<StyleSheet>("Uss/Fx_BundleReferenceWindow");
            this.rootVisualElement.styleSheets.Add(style);
            
            this.m_ToolBar = new Toolbar();
            this.DoToolbar();
            this.rootVisualElement.Add(this.m_ToolBar);

            this.m_MainView = new BindableElement();
            this.DoMain();
            this.rootVisualElement.Add(this.m_MainView);

            this.m_Footer = new VisualElement();
            this.DoFooter();
            this.rootVisualElement.Add(this.m_Footer);
        }

        private void DoToolbar()
        {
            this.m_ToolBar.AddToClassList(Fx_Style.CName_BF_Toolbar);

            var menu = new UnityEditor.UIElements.ToolbarMenu {text = this.m_ViewType.ToString()};
            menu.menu.AppendAction(ViewType.AssetView.ToString(), action =>
            {
                this.m_ViewType = ViewType.AssetView;
                menu.text = this.m_ViewType.ToString();
            });
            menu.menu.AppendAction(ViewType.BundleView.ToString(), action =>
            {
                this.m_ViewType = ViewType.BundleView;
                menu.text = this.m_ViewType.ToString();
            });
            menu.AddToClassList(Fx_Style.CName_BF_Toolbar_Enum);
            this.m_ToolBar.Add(menu);
        }

        private void DoMain()
        {
            this.m_MainView.AddToClassList(Fx_Style.CName_BF_MainView);

            var background = new Image();
            background.AddToClassList(Fx_Style.CName_BF_MainView_BG);
            this.m_MainView.Add(background);

            var header = new IMGUIContainer(this.OnDrawHeader);
            header.AddToClassList(Fx_Style.CName_BF_MainView_Header);
            this.m_MainView.Add(header);
            
            var view = new IMGUIContainer(this.OnDrawGUI);
            view.AddToClassList(Fx_Style.CName_BF_MainView_ScrollView);
            this.m_MainView.Add(view);
        }

        private void OnDrawHeader()
        {
            if (this.m_BundleView == null || this.m_AssetView == null) this.InitGUIData();
            GUILayout.Space(0);
            var windowRect = GUILayoutUtility.GetLastRect();
            windowRect.width = this.position.width;
            windowRect.height = this.position.height;
            if (this.m_ViewType == ViewType.BundleView)
                this.m_BundleView.OnHeader(windowRect);
            else if (this.m_ViewType == ViewType.AssetView)
                this.m_AssetView.OnHeader(windowRect);
        }

        private void OnDrawGUI()
        {
            if (this.m_ViewType == ViewType.BundleView)
                this.m_BundleView.OnGUI();
            else if (this.m_ViewType == ViewType.AssetView)
                this.m_AssetView.OnGUI();
        }

        private IMGUIContainer m_FooterHeader;
        private Label m_FootLabel;
        private IMGUIContainer m_FootView;
        private FxAsset m_FxAsset;
        private Vector2 m_FooterScrollPos = Vector2.zero;
        private bool m_FBundleFlag = false;
        private bool m_FBundleFlag2 = false;
        private readonly int kDragHandleControlID = "DragFooterHandle".GetHashCode();
        private float m_StartDragPos = 0;
        private float m_DragDistance = 0;
        private float m_FooterStartHeight = 0;
        
        internal void CheckAssetInfo(FxAsset fxAsset)
        {
            this.m_FxAsset = fxAsset;
            this.m_Footer.style.height = 240;
            this.m_FootLabel.text = fxAsset.manifest.Path;
        }

        private void DoFooter()
        {
            this.m_Footer.AddToClassList(Fx_Style.CName_BF_Footer);
            this.m_Footer.style.height = 20;

            this.m_FootLabel = new Label("Asset Info.");
            this.m_FootLabel.AddToClassList(Fx_Style.CName_BF_Footer_Label);
            
            this.m_FooterHeader = new IMGUIContainer(this.OnDragFooterLabel);
            this.m_FooterHeader.RegisterCallback<ClickEvent>(this.OnClickFooterHeader);
            this.m_FooterHeader.AddToClassList(Fx_Style.CName_BF_Footer_Header);
            
            this.m_FootLabel.Add(this.m_FooterHeader);
            
            this.m_Footer.Add(this.m_FootLabel);

            this.m_FootView = new IMGUIContainer(this.OnDrawFooterView);
            this.m_FootView.AddToClassList(Fx_Style.CName_BF_Footer_View);
            this.m_Footer.Add(this.m_FootView);
        }

        private void OnDragFooterLabel()
        {
            if (this.m_ViewType == ViewType.BundleView) return;
            int controlId = GUIUtility.GetControlID(kDragHandleControlID, FocusType.Passive);
            Event current = Event.current;
            if (current.type == EventType.Layout || current.type == EventType.Repaint) return;
            switch (current.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                    if (current.button != 0 || current.clickCount == 2)
                        break;
                    GUIUtility.hotControl = controlId;
                    this.m_StartDragPos = current.mousePosition.y;
                    this.m_DragDistance = 0f;
                    this.m_FooterStartHeight = this.m_Footer.style.height.value.value;
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl != controlId)
                        break;
                    GUIUtility.hotControl = 0;
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl != controlId)
                        break;
                    float moveY = current.mousePosition.y - this.m_StartDragPos;
                    this.m_DragDistance -= moveY;
                    float newHeight = this.m_FooterStartHeight + this.m_DragDistance;
                    this.m_Footer.style.height = newHeight;
                    if (newHeight > this.rootVisualElement.layout.height * 0.8)
                        this.m_Footer.style.height = this.rootVisualElement.layout.height * 0.8f;
                    if (newHeight < 20)
                        this.m_Footer.style.height = 20;
                    break;
            }
            current.Use();
        }

        private void OnClickFooterHeader(ClickEvent e)
        {
            if (e.button != 0 || e.clickCount < 2) return;
            if (this.m_ViewType == ViewType.BundleView)
            {
                this.m_Footer.style.height = 20;
                return;
            }
            this.m_Footer.style.height = this.m_Footer.style.height == 20 ? 240 : 20;
        }

        private void OnDrawFooterView()
        {
            if (this.m_FxAsset == null) return;
            this.m_FooterScrollPos = GUILayout.BeginScrollView(this.m_FooterScrollPos);
            this.m_FBundleFlag = EditorGUILayout.BeginFoldoutHeaderGroup(this.m_FBundleFlag, "依赖Bundle信息");
            if (this.m_FBundleFlag)
            {
                long totalSize = 0;
                if (FxManager.ManifestVC.TryGetBundleManifest(this.m_FxAsset.manifest.HoldBundle, out var bundle))
                {
                    GUILayout.Label(bundle.BundleHashName, Fx_Style.FooterLabelInfo);
                    totalSize += bundle.Size;
                }
                foreach (var id in this.m_FxAsset.manifest.DependBundles)
                {
                    if (!FxManager.ManifestVC.TryGetBundleManifest(id, out bundle)) continue;
                    GUILayout.Label(bundle.BundleHashName, Fx_Style.FooterLabelInfo);
                    totalSize += bundle.Size;
                }
                GUILayout.Label($"资源依赖Bundle大小: {FxUtility.FormatBytes(totalSize)}", Fx_Style.FooterLabelInfo);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            this.m_FBundleFlag2 = EditorGUILayout.BeginFoldoutHeaderGroup(this.m_FBundleFlag2, "堆栈信息");
            if (this.m_FBundleFlag2)
            {
                if (!string.IsNullOrEmpty(this.m_FxAsset.stackInfo))
                    GUILayout.Label(this.m_FxAsset.stackInfo, Fx_Style.FooterLabelInfo);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.EndScrollView();
        }

        public void AddItemsToMenu(GenericMenu menu) { }

        private void OnDestroy()
        {
            this.m_FxAsset = null; 
        }
    }

    internal enum ViewType
    {
        BundleView,
        AssetView,
    }
}