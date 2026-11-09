using QFramework;

/// <summary>
/// 底部字幕提示入口：按需打开并复用 TipPanel 显示一条提示。
/// 重复调用只替换文本并重新计时，调用方无需关心面板的加载与生命周期。
/// </summary>
public static class TipUtility
{
    private const string TipPanelPrefab = "resources://UI/Panel/TipPanel";

    /// <summary>在底部字幕区显示提示，同一时间只保留最后一条。</summary>
    public static void ShowTip(string tip, float duringTime = 2f)
    {
        // 面板被关闭后会从 UIKit 表中移除，此时重新加载；否则复用已有面板。
        var panel = UIKit.GetPanel<TipPanel>();
        if (panel == null)
        {
            panel = UIKit.OpenPanel<TipPanel>(UILevel.PopUI, prefabName: TipPanelPrefab);
        }

        if (panel != null)
        {
            panel.ShowTip(tip, duringTime);
        }
    }
}
