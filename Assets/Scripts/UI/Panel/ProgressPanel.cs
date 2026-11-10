using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressPanel : UIPanel
{
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private Image _progressFill;

    protected override void OnInit(IUIData uiData = null)
    {
        _progressFill.type = Image.Type.Filled;
        _progressFill.fillMethod = Image.FillMethod.Horizontal;
        _progressFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        ClearProgress();
    }

    /// <summary>设置进度描述。</summary>
    public void SetDescription(string description)
    {
        _descriptionText.text = description ?? string.Empty;
    }

    /// <summary>设置进度，取值范围为 0～1。</summary>
    public void SetProgress(float progress)
    {
        _progressFill.fillAmount = Mathf.Clamp01(progress);
    }

    protected override void OnClose()
    {
        ClearProgress();
    }

    private void ClearProgress()
    {
        SetDescription(string.Empty);
        SetProgress(0f);
    }
}
