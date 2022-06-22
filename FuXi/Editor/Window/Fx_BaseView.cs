using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace FuXi.Editor
{
    public abstract class Fx_BaseView : IDisposable
    {
        protected MultiColumnHeader m_MultiColumnHeader;
        protected MultiColumnHeaderState m_MultiColumnHeaderState;
        protected MultiColumnHeaderState.Column[] m_Columns;
        protected int columnHeight;
        
        protected float m_ColumnHeadWidth = 0f;
        protected Vector2 m_ScrollPos = Vector2.zero;
        protected Rect m_LastRect;
        protected bool m_IsDrawHeader = false;
        
        public abstract void Dispose();
    }
}