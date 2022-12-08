using System.Collections.Generic;

namespace FuXi.Editor
{
    /// <summary>
    /// 构建Bundle 预处理 后处理 操作 接口
    /// </summary>
    public interface IBuildBundlePreprocess
    {
        /// <summary>
        /// 构建Bundle 包 预处理
        /// </summary>
        void BuildBundlePre();
        /// <summary>
        /// 构建Bundle 包 后处理
        /// </summary>
        void BuildBundlePost(List<string> diffFiles);
    }
    /// <summary>
    /// 构建 安装包 预处理 后处理 操作 接口
    /// </summary>
    public interface IBuildPlayerPreprocess
    {
        /// <summary>
        /// 构建Bundle 包 预处理
        /// </summary>
        void BuildPlayerPre();
        /// <summary>
        /// 构建Bundle 包 后处理
        /// </summary>
        void BuildPlayerPost();
    }
}