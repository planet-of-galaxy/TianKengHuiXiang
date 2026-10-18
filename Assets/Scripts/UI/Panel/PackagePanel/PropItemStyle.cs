using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>控制道具栏位的悬停、准心和装备图标。</summary>
public class PropItemStyle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("鼠标悬停时显示的图案节点。")]
    [SerializeField] private GameObject Hover;

    [Tooltip("用于判断栏位是否包含道具的控制器。")]
    public PropItemController PropItemController;

    [Tooltip("空栏位准星，仅在栏位为空且未锁定时显示。")]
    [SerializeField] private Transform emptyCrosshair;

    [Tooltip("当前栏位为手持物时显示的装备图标。")]
    [SerializeField] private GameObject equipped;

    private PropItemPanel panel;
    private bool isHovered;

    private void Awake()
    {
        panel = GetComponent<PropItemPanel>();
        if (PropItemController == null)
        {
            PropItemController = GetComponent<PropItemController>();
        }
    }

    private void OnEnable()
    {
        isHovered = false;
        SetHoverVisible(false);
        RefreshCrosshair();
        RefreshEquipped();
    }

    private void OnDisable()
    {
        isHovered = false;
        SetHoverVisible(false);
        if (equipped != null) equipped.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        SetHoverVisible(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        SetHoverVisible(false);
    }

    private void LateUpdate()
    {
        SetHoverVisible(isHovered);
        RefreshCrosshair();
        RefreshEquipped();
    }

    private void RefreshEquipped()
    {
        if (equipped == null) return;

        bool shouldShow = PropItemController != null && PropItemController.IsEquipped;
        if (equipped.activeSelf != shouldShow)
        {
            equipped.SetActive(shouldShow);
        }
    }

    private void RefreshCrosshair()
    {
        if (emptyCrosshair == null) return;

        bool shouldShow = panel != null && !panel.IsLocked
            && PropItemController != null && PropItemController.index == -1;
        if (emptyCrosshair.gameObject.activeSelf != shouldShow)
        {
            emptyCrosshair.gameObject.SetActive(shouldShow);
        }
    }

    private void SetHoverVisible(bool visible)
    {
        if (Hover != null)
        {
            bool shouldShow = visible && panel != null && !panel.IsLocked;
            if (Hover.activeSelf != shouldShow)
            {
                Hover.SetActive(shouldShow);
            }
        }
    }
}
