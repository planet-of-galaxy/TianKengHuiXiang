using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>道具栏位的结构与显示接口，不查询业务数据。</summary>
public class PropItemPanel : MonoBehaviour
{
    public event System.Action ItemClicked;

    [Tooltip("菜单生成点，菜单左上角与此 Transform 的位置对齐。")]
    [SerializeField] private Transform menuSpawnPoint;
    public Transform MenuSpawnPoint => menuSpawnPoint;

    private void OnEnable()
    {
        ItemButton.onClick.AddListener(OnItemClicked);
    }

    private void OnDisable()
    {
        if (itemButton != null) itemButton.onClick.RemoveListener(OnItemClicked);
    }

    private void OnItemClicked()
    {
        ItemClicked?.Invoke();
    }

    [Tooltip("点击道具栏位时打开操作菜单。未配置时使用同节点的 Button。")]
    [SerializeField] private Button itemButton;

    public Button ItemButton
    {
        get
        {
            if (itemButton == null)
            {
                itemButton = GetComponent<Button>();
                if (itemButton == null) itemButton = gameObject.AddComponent<Button>();
                itemButton.transition = Selectable.Transition.None;
            }
            return itemButton;
        }
    }

    public TMP_FontAsset MenuFont => nameText != null ? nameText.font : TMP_Settings.defaultFontAsset;

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

    [Tooltip("栏位背景，根据锁定状态切换图片。")]
    [SerializeField] private Image background;

    [Tooltip("未锁定背景：bg_package_grid。")]
    [SerializeField] private Sprite unlockedBackground;

    [Tooltip("锁定背景：bg_package_grid_locked。")]
    [SerializeField] private Sprite lockedBackground;

    /// <summary>栏位当前是否锁定。</summary>
    public bool IsLocked { get; private set; }

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

    // ==================== 锁定背景 ====================

    /// <summary>显示锁定背景（未解锁栏位）。</summary>
    public void ShowLock()
    {
        IsLocked = true;

        if (background == null)
        {
            return;
        }

        background.sprite = lockedBackground;
    }

    /// <summary>显示未锁定背景（道具栏位与空栏位）。</summary>
    public void HideLock()
    {
        IsLocked = false;
        if (background == null)
        {
            return;
        }

        background.sprite = unlockedBackground;
    }

    // ==================== 清空 ====================

    /// <summary>
    /// 清空栏位显示，回到「空栏位」外观：隐藏图标、名称、数量、耐久，背景恢复未锁定状态。
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
