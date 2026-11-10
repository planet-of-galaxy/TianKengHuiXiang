using QFramework;

/// <summary>广播入口：按需打开并复用左下角 BroadPanel。</summary>
public static class BroadUtility
{
    private const string BroadPanelPrefab = "resources://UI/Panel/BroadPanel";

    /// <summary>发布一条广播，后来的广播向上推动旧广播，最多同时显示五条。</summary>
    public static void Broadcast(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return;

        var panel = UIKit.GetPanel<BroadPanel>();
        if (panel == null)
        {
            panel = UIKit.OpenPanel<BroadPanel>(UILevel.PopUI, prefabName: BroadPanelPrefab);
        }

        if (panel != null)
        {
            panel.Broadcast(content);
        }
    }
}
