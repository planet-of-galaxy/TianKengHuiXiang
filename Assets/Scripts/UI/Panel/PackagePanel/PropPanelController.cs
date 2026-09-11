using System.Collections.Generic;
using QFramework;
using TMPro;
using UnityEngine;

/// <summary>
/// 背包页（PropPanel）的控制器：只管自己这一页的栏位与容量文本，不关心背包数据从哪来。
/// 职责：
///   1. 生成栏位——把栏位容器（View/Content）下的第一个 Item 当作模板并隐藏，按 PackageSystem.maxCapacity 实例化固定数量的栏位；
///   2. 渲染单个栏位的三种状态——道具 / 锁定（未解锁，显示 Item 下的 LockedIcon 遮罩）/ 空；
///   3. 道具到显示的映射——名称、图标（按 WeaponConfig.icon 从 Resources 加载并缓存）、数量、耐久；
///   4. 容量文本——背包内道具数量 / 当前角色背包容量。
/// 对外只有 Init + Refresh：调用方给出「背包道具列表 + 当前容量」，本类负责把每个栏位画成对应状态。
/// 「每个栏位恰好处于三种状态之一」由本类自己在 Refresh 里保证，不拆成逐槽接口交给调用方拼装。
/// </summary>
public class PropPanelController : MonoBehaviour, IController
{
    [Header("子节点")]
    [Tooltip("栏位容器（PropPanel/View/Content）：生成的栏位都挂在它下面，它的第一个子物体是栏位模板。")]
    [SerializeField] private RectTransform slotContainer;

    [Header("容量")]
    [Tooltip("容量文本（PropPanel/Capacity），显示「背包内道具数量 / 当前角色背包容量」，如 6 / 6。")]
    [SerializeField] private TextMeshProUGUI capacityText;

    /// <summary>栏位模板：栏位容器下的第一个子物体，仅作模板用，生成栏位前会被隐藏。</summary>
    private Transform cellTemplate;

    /// <summary>已生成的栏位，下标与槽位一一对应。</summary>
    private readonly List<PropItemController> cells = new List<PropItemController>();

    /// <summary>图标缓存（key 为 Resources 路径），避免每次刷新都重新加载。</summary>
    private readonly Dictionary<string, Sprite> iconCache = new Dictionary<string, Sprite>();

    private IWeaponConfigProvider weaponConfigProvider;
    private IResourceStorage resourceStorage;

    /// <summary>栏位数量：永远等于 maxCapacity。</summary>
    private static int TotalSlots => Mathf.Max(1, PackageSystem.maxCapacity);

    /// <summary>
    /// 准备栏位：取引用、捕获模板、按最大容量生成全部栏位。
    /// 重复调用是幂等的（先清掉上一次生成的栏位），面板被 UIKit 复用时也安全。
    /// </summary>
    public void Init()
    {
        weaponConfigProvider = this.GetUtility<IWeaponConfigProvider>();
        resourceStorage = this.GetUtility<IResourceStorage>();

        CacheCellTemplate();
        BuildCells();
    }

    // ==================== 刷新 ====================

    /// <summary>
    /// 按背包数据重绘全部栏位。items 为背包道具列表（可为 null），capacity 为当前角色背包容量：
    ///   i &lt; capacity 且该槽有道具 → 道具栏位；
    ///   i &lt; capacity 但该槽为空   → 空栏位；
    ///   i &gt;= capacity             → 锁定栏位。
    /// 每次都重建全部栏位，不保留上一次的任何显示，所以不提供逐槽设置接口。
    /// </summary>
    public void Refresh(IList<PropItemInfo> items, int capacity)
    {
        RefreshCapacityText(items, capacity);

        for (int i = 0; i < cells.Count; i++)
        {
            if (i >= capacity)
            {
                SetLocked(i);
                continue;
            }

            var item = items != null && i < items.Count ? items[i] : null;
            if (item == null)
            {
                cells[i]?.Clear();
                continue;
            }

            SetItem(i, item);
        }
    }

    /// <summary>
    /// 容量文本：背包内道具数量 / 当前角色背包容量，如 "6 / 6"。
    /// 道具数量取 items 的元素个数——PackageSystem 只在未满时允许入包（AddItemToRolePackage），
    /// 所以正常运行状态下它不会超过 capacity，与画面上的道具栏位数一致。
    /// </summary>
    private void RefreshCapacityText(IList<PropItemInfo> items, int capacity)
    {
        if (capacityText == null)
        {
            return;
        }

        capacityText.text = $"{items?.Count ?? 0} / {capacity}";
    }

    /// <summary>
    /// 把指定栏位渲染成道具：名称、图标、数量、耐久。
    /// 下标越界（没有该栏位）时忽略。
    /// </summary>
    private void SetItem(int index, PropItemInfo item)
    {
        if (item == null)
        {
            return;
        }

        var cell = GetCell(index);
        if (cell == null)
        {
            return;
        }

        var config = weaponConfigProvider.GetWeaponConfig(item.configId);

        // Refresh 会复用栏位，道具栏位必须显式摘掉锁定遮罩，否则会残留上一次的锁定外观
        cell.HideLock();
        cell.SetName(config != null ? config.name : $"道具{item.configId}");
        cell.SetIcon(LoadIcon(config));

        // 数量不大于 1 时 PropItemController 会自动隐藏（武器不可叠加，不显示数量）
        cell.SetNum(item.num?.Value ?? 1);

        if (item is WeaponItemInfo weapon && config != null)
        {
            cell.SetDurability(weapon.durability?.Value ?? 0f, config.durability);
        }
        else
        {
            cell.HideDurability();
        }
    }

    /// <summary>
    /// 把指定栏位渲染成锁定状态：显示 Item 下的 LockedIcon 遮罩，图标 / 名称 / 数量 / 耐久一律隐藏。
    /// 图标必须显式隐藏——Refresh 复用栏位，道具栏位变锁定时不清掉就会残留上一次的道具图标。
    /// </summary>
    private void SetLocked(int index)
    {
        var cell = GetCell(index);
        if (cell == null)
        {
            return;
        }

        cell.HideIcon();
        cell.ShowLock();
        cell.HideName();
        cell.HideNum();
        cell.HideDurability();
    }

    // ==================== 栏位生成 ====================

    /// <summary>
    /// 取栏位容器下的第一个子物体作为栏位模板并隐藏，同时清掉容器里可能残留的旧栏位。
    /// 锚是 slotContainer 而不是 transform：本组件挂在 PropPanel 上，它下面的第一个子物体是背景 Bg，不是栏位模板。
    /// </summary>
    private void CacheCellTemplate()
    {
        if (slotContainer == null)
        {
            Debug.LogError("[PropPanelController] slotContainer（PropPanel/View/Content）未配置，无法生成道具栏位");
            return;
        }

        if (slotContainer.childCount > 0)
        {
            cellTemplate = slotContainer.GetChild(0);
            cellTemplate.gameObject.SetActive(false);
        }

        if (cellTemplate == null)
        {
            Debug.LogError("[PropPanelController] Content 下面没有 Item 模板，无法生成道具栏位");
            return;
        }

        // 保留模板，其余子物体是上一次生成的栏位（面板被复用时才会出现），一并清掉
        for (int i = slotContainer.childCount - 1; i >= 0; i--)
        {
            var child = slotContainer.GetChild(i);
            if (child != cellTemplate)
            {
                Destroy(child.gameObject);
            }
        }
    }

    /// <summary>按最大容量生成全部栏位；模板或容器缺失时不生成。</summary>
    private void BuildCells()
    {
        cells.Clear();

        if (cellTemplate == null || slotContainer == null)
        {
            return;
        }

        int total = TotalSlots;

        for (int i = 0; i < total; i++)
        {
            // 模板本身是隐藏的，实例化结果同样处于隐藏状态，需要手动激活
            var clone = Instantiate(cellTemplate, slotContainer);
            clone.name = "Item_" + i;
            clone.gameObject.SetActive(true);

            var cell = clone.GetComponent<PropItemController>();
            if (cell == null)
            {
                Debug.LogError("[PropPanelController] Item 模板上缺少 PropItemController，栏位无法刷新");
                continue;
            }

            cells.Add(cell);
        }
    }

    /// <summary>取指定下标的栏位；越界或栏位为空时返回 null。</summary>
    private PropItemController GetCell(int index)
    {
        if (index < 0 || index >= cells.Count)
        {
            return null;
        }

        return cells[index];
    }

    /// <summary>按配置里的 Resources 路径加载图标并缓存；未配置路径时返回 null（栏位不显示图标）。</summary>
    private Sprite LoadIcon(WeaponConfig config)
    {
        if (config == null || string.IsNullOrEmpty(config.icon))
        {
            return null;
        }

        if (!iconCache.TryGetValue(config.icon, out var sprite))
        {
            sprite = resourceStorage.Load<Sprite>(config.icon);
            iconCache[config.icon] = sprite;
        }

        return sprite;
    }

    public IArchitecture GetArchitecture()
    {
        return TianArchitecture.Interface;
    }
}
