// ReSharper disable once CheckNamespace
namespace FuXi
{
    /// <summary>
    /// 运行模式, 仅供编辑器下使用, 打包后 需要走ab包流程
    /// </summary>
    public enum RuntimeMode
    {
        /// <summary>
        /// 不走ab包逻辑
        /// </summary>
        Editor,
        /// <summary>
        /// 离线模式
        /// </summary>
        Offline,
        /// <summary>
        /// 热更新模式
        /// </summary>
        Runtime,
    }
}