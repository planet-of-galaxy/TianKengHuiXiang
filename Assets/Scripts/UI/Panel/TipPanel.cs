using System.Collections;
using QFramework;
using TMPro;
using UnityEngine;

public class TipPanel : UIPanel
{
    [SerializeField] private TMP_Text _tipText;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField, Min(0f)] private float _fadeInDuration = 0.2f;
    [SerializeField, Min(0f)] private float _fadeOutDuration = 0.25f;

    private Coroutine _hideCoroutine;

    protected override void OnInit(IUIData uiData = null)
    {
        ClearTip();
    }

    /// <summary>在底部字幕区显示提示，重复调用会替换内容并重新计时。</summary>
    public void ShowTip(string tip, float duringTime = 2f)
    {
        ClearTip();

        if (string.IsNullOrEmpty(tip) || duringTime <= 0f)
        {
            Hide();
            return;
        }

        _tipText.text = tip;
        Show();
        _hideCoroutine = StartCoroutine(PlayTip(duringTime));
    }

    private IEnumerator PlayTip(float duringTime)
    {
        // 显示时长包含淡入淡出；短提示按比例缩短动画。
        float fadeIn = Mathf.Max(0f, _fadeInDuration);
        float fadeOut = Mathf.Max(0f, _fadeOutDuration);
        float fadeTotal = fadeIn + fadeOut;
        if (fadeTotal > duringTime)
        {
            float scale = duringTime / fadeTotal;
            fadeIn *= scale;
            fadeOut *= scale;
        }

        yield return Fade(0f, 1f, fadeIn);

        float holdDuration = duringTime - fadeIn - fadeOut;
        if (holdDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(holdDuration);
        }

        yield return Fade(1f, 0f, fadeOut);
        _hideCoroutine = null;
        Hide();
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            _canvasGroup.alpha = Mathf.Lerp(from, to, progress);
            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }

        _canvasGroup.alpha = to;
    }

    private void OnDisable()
    {
        ClearTip();
    }

    protected override void OnClose()
    {
        ClearTip();
    }

    private void ClearTip()
    {
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }

        _tipText.text = string.Empty;
        _canvasGroup.alpha = 0f;
    }
}
