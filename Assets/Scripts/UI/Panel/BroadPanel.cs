using System.Collections.Generic;
using QFramework;
using TMPro;
using UnityEngine;

/// <summary>左下角广播列表，每条独立淡入淡出，新广播将旧广播向上推。</summary>
public class BroadPanel : UIPanel
{
    private const int MaxMessages = 5;
    private const float DisplayDuration = 5f;

    [SerializeField] private TMP_Text _messageTemplate;
    [SerializeField, Min(0f)] private float _spacing = 8f;
    [SerializeField, Min(0f)] private float _fadeInDuration = 0.2f;
    [SerializeField, Min(0f)] private float _fadeOutDuration = 0.25f;

    private readonly List<BroadcastEntry> _messages = new(MaxMessages);
    private readonly Stack<BroadcastEntry> _availableMessages = new(MaxMessages);

    private sealed class BroadcastEntry
    {
        public TMP_Text Text;
        public CanvasGroup CanvasGroup;
        public float Elapsed;
    }

    protected override void OnInit(IUIData uiData = null)
    {
        _messageTemplate.gameObject.SetActive(false);
        // 固定复用五个条目，持续广播时不反复创建和销毁 UI 对象。
        for (int i = 0; i < MaxMessages; i++)
        {
            var text = Instantiate(_messageTemplate, _messageTemplate.transform.parent);
            text.name = $"BroadcastMessage{i + 1}";
            text.text = string.Empty;
            text.raycastTarget = false;
            var canvasGroup = text.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            _availableMessages.Push(new BroadcastEntry { Text = text, CanvasGroup = canvasGroup });
        }
    }

    /// <summary>发布一条广播，显示两秒（包含淡入淡出），最多同时显示五条。</summary>
    public void Broadcast(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return;

        Show();
        if (_messages.Count == MaxMessages)
        {
            RemoveMessage(0);
        }

        var message = _availableMessages.Pop();
        message.Text.text = content;
        message.Elapsed = 0f;
        message.CanvasGroup.alpha = _fadeInDuration > 0f ? 0f : 1f;
        message.Text.gameObject.SetActive(true);
        _messages.Add(message);
        LayoutMessages();
    }

    private void Update()
    {
        if (_messages.Count == 0) return;

        float fadeIn = Mathf.Clamp(_fadeInDuration, 0f, DisplayDuration);
        float fadeOut = Mathf.Clamp(_fadeOutDuration, 0f, DisplayDuration - fadeIn);
        bool layoutChanged = false;
        for (int i = _messages.Count - 1; i >= 0; i--)
        {
            var message = _messages[i];
            message.Elapsed += Time.unscaledDeltaTime;
            if (message.Elapsed >= DisplayDuration)
            {
                RemoveMessage(i);
                layoutChanged = true;
                continue;
            }

            float alpha = 1f;
            if (fadeIn > 0f && message.Elapsed < fadeIn)
            {
                alpha = Mathf.SmoothStep(0f, 1f, message.Elapsed / fadeIn);
            }
            else if (fadeOut > 0f && message.Elapsed > DisplayDuration - fadeOut)
            {
                alpha = Mathf.SmoothStep(1f, 0f,
                    (message.Elapsed - (DisplayDuration - fadeOut)) / fadeOut);
            }
            message.CanvasGroup.alpha = alpha;
        }

        if (_messages.Count == 0)
        {
            Hide();
        }
        else if (layoutChanged)
        {
            LayoutMessages();
        }
    }

    private void LayoutMessages()
    {
        var origin = _messageTemplate.rectTransform.anchoredPosition;
        float offset = 0f;
        // 列表末尾是最新广播，固定放在最下方；长文本按实际换行高度排列。
        for (int i = _messages.Count - 1; i >= 0; i--)
        {
            var text = _messages[i].Text;
            var rect = text.rectTransform;
            float height = Mathf.Max(_messageTemplate.rectTransform.rect.height,
                text.GetPreferredValues(text.text, rect.rect.width, Mathf.Infinity).y);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            rect.anchoredPosition = origin + new Vector2(0f, offset);
            offset += height + Mathf.Max(0f, _spacing);
        }
    }

    private void RemoveMessage(int index)
    {
        var message = _messages[index];
        _messages.RemoveAt(index);
        message.Text.text = string.Empty;
        message.CanvasGroup.alpha = 0f;
        message.Elapsed = 0f;
        message.Text.gameObject.SetActive(false);
        _availableMessages.Push(message);
    }

    private void ClearBroadcasts()
    {
        while (_messages.Count > 0)
        {
            RemoveMessage(_messages.Count - 1);
        }
    }

    private void OnDisable()
    {
        ClearBroadcasts();
    }

    protected override void OnClose()
    {
        ClearBroadcasts();
    }
}
