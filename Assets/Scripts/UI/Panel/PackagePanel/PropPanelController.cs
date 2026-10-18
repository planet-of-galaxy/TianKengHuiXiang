using System.Collections.Generic;
using QFramework;
using TMPro;
using UnityEngine;

/// <summary>
/// 背包页（PropPanel）的控制器：只管自己这一页的栏位与容量文本，不关心背包数据从哪来。
/// 职责：
///   1. 生成栏位——把栏位容器（View/Content）下的第一个 Item 当作模板并隐藏，按 PackageSystem.maxCapacity 实例化固定数量的栏位；
///   2. 渲染单个栏位的三种状态——道具 / 锁定（未解锁，显示 Item 下的 LockedIcon 遮罩）/ 空；
///   3. 仅向栏位传入 index，由栏位自行查询道具与配置；
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

    /// <summary>栏位数量：永远等于 maxCapacity。</summary>
    private static int TotalSlots => Mathf.Max(1, PackageSystem.maxCapacity);

    /// <summary>
    /// 准备栏位：取引用、捕获模板、按最大容量生成全部栏位。
    /// 重复调用是幂等的（先清掉上一次生成的栏位），面板被 UIKit 复用时也安全。
    /// </summary>
    public void Init()
    {



        CacheCellTemplate();
        BuildCells();
    }

    // ==================== 刷新 ====================

    /// <summary>
    /// 按背包数据重绘全部栏位。items 为背包道具列表（可为 null），capacity 为当前角色背包容量：
    ///   槽位号 &lt; capacity 且该槽位有道具 → 道具栏位；
    ///   槽位号 &lt; capacity 但该槽位为空   → 空栏位；
    ///   槽位号 &gt;= capacity               → 锁定栏位。
    /// 道具按 PropItemInfo.index（槽位号）定位，不用列表下标——两者只有在槽位号连续且无空洞时才恰好相同，
    /// 一旦支持移除道具就会分叉，所以以槽位号为准。
    /// 先整体重置再逐件放入：槽位号之间的空洞、以及两次刷新之间道具换了槽位，都只能靠「先重置」才不会残留旧显示，
    /// 所以不提供逐槽设置接口。
    /// </summary>
    public void Refresh(IReadOnlyList<PropItemInfo> items, int capacity)
    {
        RefreshCapacityText(items, capacity);

        // 第一遍：把所有栏位重置成「空栏位」或「锁定」，不保留上一次的任何显示
        for (int i = 0; i < cells.Count; i++)
        {
            if (i >= capacity)
            {
                SetLocked(i);
            }
            else
            {
                cells[i]?.Clear();
            }
        }

        if (items == null)
        {
            return;
        }

        // 第二遍：按槽位号把每件道具画进对应栏位
        foreach (var item in items)
        {
            if (item == null)
            {
                continue;
            }

            if (item.index < 0 || item.index >= cells.Count)
            {
                Debug.LogError($"[PropPanelController] 道具槽位号越界（index={item.index}，共 {cells.Count} 个栏位），已跳过 configId={item.configId}");
                continue;
            }

            if (item.index >= capacity)
            {
                // 入包时已校验未满，正常不会出现；容量被下调时才会走到这里，栏位保持第一遍的锁定外观
                Debug.LogWarning($"[PropPanelController] 道具槽位号 {item.index} 超出当前容量 {capacity}，未显示 configId={item.configId}");
                continue;
            }

            GetCell(item.index)?.SetIndex(item.index);
        }
    }

    /// <summary>
    /// 容量文本：背包内道具数量 / 当前角色背包容量，如 "6 / 6"。
    /// 道具数量取 items 的元素个数——PackageSystem 只在未满时允许入包（AddItemToRolePackage），
    /// 所以正常运行状态下它不会超过 capacity，与画面上的道具栏位数一致。
    /// </summary>
    private void RefreshCapacityText(IReadOnlyList<PropItemInfo> items, int capacity)
    {
        if (capacityText == null)
        {
            return;
        }

        capacityText.text = $"{items?.Count ?? 0} / {capacity}";
    }

    private void SetLocked(int index)
    {
        var cell = GetCell(index);
        if (cell == null)
        {
            return;
        }

        cell.ShowLock();
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

    public IArchitecture GetArchitecture()
    {
        return TianArchitecture.Interface;
    }
}
