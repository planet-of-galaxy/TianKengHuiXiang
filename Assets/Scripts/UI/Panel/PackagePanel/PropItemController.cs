using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 背包面板道具栏位的显示控制器，挂载于 PackagePanel/PanelGroup/PropPanel/View/Content/Item。
/// 只负责单个栏位的显示，不持有任何数据：外层列表刷新时逐项调用 SetName / SetIcon / SetNum / SetDurability，
/// 清空栏位时调用 Clear。
/// 四部分显示内容都可缺省：名称/数量为空、图标为 null、无耐久时对应子节点自动隐藏；
/// 也可以直接调用 HideName / HideIcon / HideNum / HideDurability 主动隐藏某一部分，
/// 例如武器不叠加，不显示数量；不消耗耐久的道具不显示耐久条。
/// 未解锁栏位由 ShowLock / HideLock 控制 LockedIcon 节点的显隐，与图标是两套独立显示，换图不影响遮罩。
/// 全部隐藏后栏位只剩 Border / Back 底图，即空栏位外观。
/// </summary>
public class PropItemController : MonoBehaviour
{
    [Header("子节点")]
    [Tooltip("道具图标（Icon）。")]
    [SerializeField] private Image iconImage;

    [Tooltip("道具名称文本（Name）。")]
    [SerializeField] private TextMeshProUGUI nameText;

    [Tooltip("叠加数量文本（Num）。")]
    [SerializeField] private TextMeshProUGUI numText;

    [Tooltip("耐久条整体（Durability 节点），无耐久的道具整体隐藏。")]
    [SerializeField] private GameObject durabilityRoot;

    [Tooltip("耐久条填充（Durability/CurrentValue），按耐久比例设置 fillAmount。")]
    [SerializeField] private Image durabilityFill;

    [Tooltip("锁定遮罩（LockedIcon），未解锁栏位显示，其余状态隐藏。")]
    [SerializeField] private GameObject lockedRoot;

    // ==================== 设置显示内容 ====================

    /// <summary>设置道具名称；name 为空时隐藏名称节点（等同 HideName）。</summary>
    public void SetName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            HideName();
            return;
        }

        if (nameText == null)
        {
            return;
        }

        nameText.text = name;
        nameText.gameObject.SetActive(true);
    }

    /// <summary>设置道具图标；sprite 为空时隐藏图标节点（等同 HideIcon），空栏位不会留下白块。</summary>
    public void SetIcon(Sprite sprite)
    {
        if (sprite == null)
        {
            HideIcon();
            return;
        }

        if (iconImage == null)
        {
            return;
        }

        iconImage.sprite = sprite;
        iconImage.gameObject.SetActive(true);
    }

    /// <summary>设置耐久条：按 current/max 换算填充比例；max 不大于 0（无耐久道具）时整体隐藏。</summary>
    public void SetDurability(float current, float max)
    {
        if (max <= 0f)
        {
            HideDurability();
            return;
        }

        if (durabilityRoot == null)
        {
            return;
        }

        durabilityRoot.SetActive(true);
        if (durabilityFill != null)
        {
            durabilityFill.fillAmount = Mathf.Clamp01(current / max);
        }
    }

    /// <summary>设置叠加数量；按项目约定（同 WareHousePanel）数量不大于 1 时不显示，显示格式为 "×N"。</summary>
    public void SetNum(int num)
    {
        if (num <= 1)
        {
            HideNum();
            return;
        }

        if (numText == null)
        {
            return;
        }

        numText.text = "×" + num;
        numText.gameObject.SetActive(true);
    }

    // ==================== 隐藏显示内容 ====================

    /// <summary>隐藏名称节点，例如只显示图标的紧凑栏位。</summary>
    public void HideName()
    {
        if (nameText == null)
        {
            return;
        }

        nameText.text = string.Empty;
        nameText.gameObject.SetActive(false);
    }

    /// <summary>隐藏图标节点。</summary>
    public void HideIcon()
    {
        if (iconImage == null)
        {
            return;
        }

        iconImage.sprite = null;
        iconImage.gameObject.SetActive(false);
    }

    /// <summary>隐藏数量节点，例如武器不可叠加，不显示数量。</summary>
    public void HideNum()
    {
        if (numText == null)
        {
            return;
        }

        numText.text = string.Empty;
        numText.gameObject.SetActive(false);
    }

    /// <summary>隐藏耐久条，例如不消耗耐久的道具。</summary>
    public void HideDurability()
    {
        if (durabilityRoot == null)
        {
            return;
        }

        durabilityRoot.SetActive(false);
    }

    // ==================== 锁定遮罩 ====================

    /// <summary>显示锁定遮罩（未解锁栏位）。遮罩是 Item 下独立的 LockedIcon 节点，不由图标换图实现。</summary>
    public void ShowLock()
    {
        if (lockedRoot == null)
        {
            return;
        }

        lockedRoot.SetActive(true);
    }

    /// <summary>隐藏锁定遮罩（道具栏位与空栏位）。</summary>
    public void HideLock()
    {
        if (lockedRoot == null)
        {
            return;
        }

        lockedRoot.SetActive(false);
    }

    // ==================== 清空 ====================

    /// <summary>
    /// 清空栏位显示，回到「空栏位」外观：图标、名称、数量、耐久、锁定遮罩全部隐藏。
    /// 刻意不叫 Reset：Reset 是 Unity 的编辑器消息（添加组件、Inspector 右键菜单会自动调用），
    /// 用那个名字会同时被编辑器触发，还得再判断运行环境来兜底；Clear 只由刷新逻辑调用。
    /// 只改显示，不动序列化引用。
    /// </summary>
    public void Clear()
    {
        HideIcon();
        HideName();
        HideNum();
        HideDurability();
        HideLock();
    }
}
