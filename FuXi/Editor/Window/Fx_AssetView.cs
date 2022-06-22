using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace FuXi.Editor
{
    public class Fx_AssetView : Fx_BaseView
    {
        private readonly Fx_BundleReferenceWindow m_HoldWindow;
        internal Fx_AssetView(Fx_BundleReferenceWindow window, int columnHeight)
        {
            this.m_HoldWindow = window;
            this.columnHeight = columnHeight;
            this.m_Columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent  = new GUIContent("-"),
                    headerTextAlignment = TextAlignment.Center,
                    canSort = false,
                    maxWidth = 60
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Asset Name"),
                    minWidth = 240,
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Ref Count"),
                    maxWidth = 120,
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Ref Bundle"),
                    minWidth = 160,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Analysis"),
                    maxWidth = 120,
                    canSort = false
                },
            };
            this.m_MultiColumnHeaderState = new MultiColumnHeaderState(this.m_Columns);
            this.m_MultiColumnHeader = new MultiColumnHeader(this.m_MultiColumnHeaderState);
            this.m_MultiColumnHeader.visibleColumnsChanged += header => { header.ResizeToFit(); };
            this.m_MultiColumnHeader.sortingChanged += this.SortColumn;
            this.m_MultiColumnHeader.ResizeToFit();
        }
        
        private void SortColumn(MultiColumnHeader header)
        {
            IOrderedEnumerable<KeyValuePair<string, FxAsset>> sortedDic = null;
            if (header.sortedColumnIndex == 1)
            {
                sortedDic = header.IsSortedAscending(header.sortedColumnIndex)
                    ? FuXi.FxAsset.AssetCache.OrderBy(c => c.Key)
                    : FuXi.FxAsset.AssetCache.OrderByDescending(c => c.Key);
            }
            else if (header.sortedColumnIndex == 2)
            {
                sortedDic = header.IsSortedAscending(header.sortedColumnIndex)
                    ? FuXi.FxAsset.AssetCache.OrderBy(c => c.Value.fxReference.RefCount)
                    : FuXi.FxAsset.AssetCache.OrderByDescending(c => c.Key);
            }
            FuXi.FxAsset.AssetCache = sortedDic?
                .ToDictionary(c => c.Key, c => c.Value);
        }

        internal void OnHeader(Rect windowRect)
        {
            Rect posRect = GUILayoutUtility.GetRect(0, float.MaxValue, 0, float.MaxValue);
            this.m_ColumnHeadWidth = Mathf.Max(posRect.width + this.m_ScrollPos.x, this.m_ColumnHeadWidth);
            Rect columnRect = new Rect(posRect) {width = this.m_ColumnHeadWidth, height = columnHeight};
            this.m_MultiColumnHeader.OnGUI(columnRect, this.m_ScrollPos.x);
            this.m_LastRect = new Rect(windowRect);
            this.m_IsDrawHeader = true;
        }

        internal void OnGUI()
        {
            if (!this.m_IsDrawHeader) return;
            
            var assetDic = FuXi.FxAsset.AssetCache;
            Rect posRect = GUILayoutUtility.GetRect(0, float.MaxValue, 0, float.MaxValue);
            Rect viewRect = new Rect(this.m_LastRect)
            {
                xMax = this.m_Columns.Sum(c => c.width),
                yMax = assetDic.Sum(c=> columnHeight)
            };
            Rect columnRect = new Rect(posRect) {width = this.m_ColumnHeadWidth, height = columnHeight};
            this.m_ScrollPos = GUI.BeginScrollView(posRect, this.m_ScrollPos, viewRect, false, false);
            int index = 0;
            int mMaxHeight = 0;
            foreach (var asset in assetDic)
            {
                Rect rowRect = new Rect(columnRect);
                int columnIndex = 0;
                Rect bgRect = new Rect(rowRect) {y = rowRect.y + mMaxHeight, height = columnHeight};
                EditorGUI.DrawRect(bgRect, index % 2 == 0 ? Fx_Style.C_ColumnDark : Fx_Style.C_ColumnLight);
                if (this.m_MultiColumnHeader.IsColumnVisible(columnIndex))
                {
                    int visibleColumnIndex = this.m_MultiColumnHeader.GetVisibleColumnIndex(columnIndex);
                    Rect cRect = this.m_MultiColumnHeader.GetColumnRect(visibleColumnIndex);
                    cRect.y = rowRect.y + mMaxHeight;
                    GUI.Box(cRect, Fx_Style.PrefabIcon);
                }
                columnIndex++;
                if (this.m_MultiColumnHeader.IsColumnVisible(columnIndex))
                {
                    int visibleColumnIndex = this.m_MultiColumnHeader.GetVisibleColumnIndex(columnIndex);
                    Rect cRect = this.m_MultiColumnHeader.GetColumnRect(visibleColumnIndex);
                    cRect.y = rowRect.y + mMaxHeight;
                    GUI.Label(cRect, asset.Key);
                }
                columnIndex++;
                if (this.m_MultiColumnHeader.IsColumnVisible(columnIndex))
                {
                    int visibleColumnIndex = this.m_MultiColumnHeader.GetVisibleColumnIndex(columnIndex);
                    Rect cRect = this.m_MultiColumnHeader.GetColumnRect(visibleColumnIndex);
                    cRect.y = rowRect.y + mMaxHeight;
                    GUI.Label(cRect, asset.Value.fxReference.RefCount.ToString());
                }
                columnIndex++;
                if (this.m_MultiColumnHeader.IsColumnVisible(columnIndex))
                {
                    int visibleColumnIndex = this.m_MultiColumnHeader.GetVisibleColumnIndex(columnIndex);
                    Rect cRect = this.m_MultiColumnHeader.GetColumnRect(visibleColumnIndex);
                    cRect.y = rowRect.y + mMaxHeight;
                    if (FxManager.ManifestVC.TryGetBundleManifest(asset.Value.manifest.HoldBundle, out var bundle))
                    {
                        GUI.Label(cRect, bundle.BundleHashName);
                    }
                }
                columnIndex++;
                if (this.m_MultiColumnHeader.IsColumnVisible(columnIndex))
                {
                    int visibleColumnIndex = this.m_MultiColumnHeader.GetVisibleColumnIndex(columnIndex);
                    Rect cRect = this.m_MultiColumnHeader.GetColumnRect(visibleColumnIndex);
                    cRect.y = rowRect.y + mMaxHeight;
                    cRect.width = cRect.height;
                    GUI.DrawTexture(cRect, Fx_Style.PinButton.image);
                    this.OnAssetInfo(cRect, asset.Value);
                }
                index++;
                mMaxHeight += columnHeight;
            }
            GUI.EndScrollView(true);
        }

        private void OnAssetInfo(Rect rect, FxAsset fxAsset)
        {
            if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint) return;
            if (!rect.Contains(Event.current.mousePosition)) return;
            if (Event.current.type != EventType.MouseUp || Event.current.button != 0) return;
            this.m_HoldWindow.CheckAssetInfo(fxAsset);
            Event.current.Use();
        }

        public override void Dispose() { }
    }
}