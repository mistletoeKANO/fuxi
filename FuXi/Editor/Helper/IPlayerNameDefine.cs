namespace FuXi.Editor
{
    public class PlayerNamePriorityAttribute : System.Attribute
    {
        public int priority;
        public PlayerNamePriorityAttribute(int priority) { this.priority = priority; }
    }

    /// <summary>
    /// 自定义包名
    /// </summary>
    public interface IPlayerNameDefine
    {
        /// <summary>
        /// 获取 自定义 包名
        /// </summary>
        /// <param name="version">unity 设置 安装包 版本</param>
        /// <returns></returns>
        public string GetPlayerName(string version);
    }
}

