using System;
// ReSharper disable once CheckNamespace
namespace FuXi.Editor
{
    internal interface IBuild : IDisposable
    {
        void BeginBuild();
        void EndBuild();

        void OnAssetValueChanged();
    }
}