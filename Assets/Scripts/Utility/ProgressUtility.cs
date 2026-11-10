using QFramework;

/// <summary>进度提示入口：按需打开并复用 ProgressPanel，统一更新和关闭。</summary>
public static class ProgressUtility
{
    private const string ProgressPanelPrefab = "resources://UI/Panel/ProgressPanel";

    /// <summary>显示进度描述和进度，重复调用会更新当前面板。</summary>
    public static void ShowProgress(string description, float progress = 0f)
    {
        var panel = UIKit.GetPanel<ProgressPanel>();
        if (panel == null)
        {
            panel = UIKit.OpenPanel<ProgressPanel>(UILevel.Common, prefabName: ProgressPanelPrefab);
        }

        if (panel != null)
        {
            panel.SetDescription(description);
            panel.SetProgress(progress);
            panel.Show();
        }
    }

    /// <summary>更新已打开面板的进度，取值范围为 0～1。</summary>
    public static void SetProgress(float progress)
    {
        var panel = UIKit.GetPanel<ProgressPanel>();
        if (panel != null)
        {
            panel.SetProgress(progress);
        }
    }

    /// <summary>关闭进度面板并清空内容，面板未打开时忽略。</summary>
    public static void CloseProgress()
    {
        if (UIKit.GetPanel<ProgressPanel>() != null)
        {
            UIKit.ClosePanel<ProgressPanel>();
        }
    }
}
