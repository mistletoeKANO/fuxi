namespace FuXi.Editor
{
    [PlayerNamePriority(0)]
    public class Fx_PlayerName : IPlayerNameDefine
    {
        public string GetPlayerName(string version)
        {
            var targetName = $"/fx-v{version}-{System.DateTime.Now:yyyyMMdd-HHmmss}";
            switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
            {
                case UnityEditor.BuildTarget.Android:
                    return targetName + ".apk";
                case UnityEditor.BuildTarget.StandaloneWindows:
                case UnityEditor.BuildTarget.StandaloneWindows64:
                    return targetName + ".exe";
                case UnityEditor.BuildTarget.StandaloneOSX:
                    return targetName + ".app";
                default:
                    return targetName;
            }
        }
    }
}