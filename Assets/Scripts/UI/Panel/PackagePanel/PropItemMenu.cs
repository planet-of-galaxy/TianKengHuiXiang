using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>预制体道具操作菜单，点击外部关闭。</summary>
public class PropItemMenu : MonoBehaviour
{
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private Button dismissButton;
    private static PropItemMenu activeMenu;
    private Action onAction;

    public void Initialize(string label, TMP_FontAsset font, Action action)
    {
        if (activeMenu != null && activeMenu != this) activeMenu.Close();
        activeMenu = this;
        onAction = action;
        actionText.text = label;
        if (font != null) actionText.font = font;
        actionButton.onClick.AddListener(ExecuteAction);
        dismissButton.onClick.AddListener(Close);
        var parentCanvas = transform.parent.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            var canvas = parentCanvas.rootCanvas;
            var menuCanvas = GetComponent<Canvas>();
            menuCanvas.sortingLayerID = canvas.sortingLayerID;
            menuCanvas.sortingOrder = canvas.sortingOrder + 1;
            var corners = new Vector3[4];
            ((RectTransform)canvas.transform).GetWorldCorners(corners);
            var min = transform.InverseTransformPoint(corners[0]);
            var max = transform.InverseTransformPoint(corners[2]);
            var blocker = (RectTransform)dismissButton.transform;
            blocker.pivot = Vector2.zero;
            blocker.localPosition = min;
            blocker.sizeDelta = new Vector2(max.x - min.x, max.y - min.y);
        }
    }

    private void ExecuteAction()
    {
        var callback = onAction;
        Close();
        callback?.Invoke();
    }

    private void OnDestroy()
    {
        if (activeMenu == this) activeMenu = null;
        actionButton.onClick.RemoveListener(ExecuteAction);
        dismissButton.onClick.RemoveListener(Close);
    }

    public void Close()
    {
        if (activeMenu == this) activeMenu = null;
        onAction = null;
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
