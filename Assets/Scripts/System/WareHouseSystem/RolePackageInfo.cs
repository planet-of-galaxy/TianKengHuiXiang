using System;
using System.Collections.Generic;
using QFramework;

/// <summary>
/// 单个角色的背包运行时信息，roleRuntimeId 对应 RoleRuntimeModel 中的角色运行时实例 id。
/// 由 PackageSystem 初始化时写入 PackageModel。
///
/// 物品列表对外只读：所有增删都必须走本类的方法，以便在变更后触发 OnPackageUpdate 通知 UI。
/// 直接改 Items 里的内容是无效的（拿不到可写视图），这样设计是为了让「变更即通知」不被绕过。
/// </summary>
public class RolePackageInfo
{
    /// <summary>
    /// 新建背包的默认容量：无存档（或存档容量非法）时该角色背包的可用栏位数。
    /// 定义在背包自己身上，而不是 PackageSystem 上：PackageModel 创建背包时要用它，
    /// Model 引用 System 是分层倒挂，而「背包的默认容量」本就是背包域的常量。
    /// </summary>
    public const int DefaultCapacity = 6;

    /// <summary>
    /// 角色运行时实例 id，唯一，与 RoleRuntimeInfo.runtimeIndex 对应。
    /// </summary>
    public int roleRuntimeId;

    /// <summary>
    /// 背包内物品的存储列表，只通过 AddItem / AddRestoredItem 写入。
    /// </summary>
    private readonly List<PropItemInfo> packageItems = new();

    /// <summary>
    /// 背包内物品的只读视图，供 UI 遍历渲染。
    /// 需要增删请调用 <see cref="AddItem"/> / <see cref="AddRestoredItem"/>，以便触发 <see cref="OnPackageUpdate"/>。
    /// </summary>
    public IReadOnlyList<PropItemInfo> Items => packageItems;

    /// <summary>
    /// 背包物品发生变化（增删）时触发，供 UI 订阅刷新。
    /// 订阅方必须在失效时退订：本对象存活于 PackageModel 中，比 UI 面板活得久，
    /// 忘记退订会让已销毁的面板继续被回调。
    /// </summary>
    public event Action OnPackageUpdate;

    /// <summary>
    /// 该角色的背包容量，取值 &gt;= 1；-1 表示尚未初始化，属异常状态（正常流程由
    /// PackageModel.GetOrCreatePackage 在创建背包时补齐默认容量），UI 读到应报错。
    /// 用于 UI 响应容量变化。
    /// </summary>
    public BindableProperty<int> capacity { get; } = new BindableProperty<int>(-1);

    /// <summary>
    /// 该角色当前手持/选中的物品槽位 index，-1 表示未手持，用于 UI 响应切换。
    /// </summary>
    public BindableProperty<int> heldIndex { get; } = new BindableProperty<int>(-1);

    /// <summary>清空物品并取消手持，保留背包对象、容量及订阅。</summary>
    public void ClearItems()
    {
        packageItems.Clear();
        heldIndex.Value = -1;
        OnPackageUpdate?.Invoke();
    }

    /// <summary>
    /// 加入一件道具，自动分配槽位号（当前最大槽位号 + 1），加入后触发 <see cref="OnPackageUpdate"/>。
    /// 容量是否已满由调用方（PackageSystem）校验，本方法不做校验。
    /// </summary>
    public void AddItem(PropItemInfo item)
    {
        if (item == null)
        {
            return;
        }

        item.index = AllocateSlotIndex();
        packageItems.Add(item);
        OnPackageUpdate?.Invoke();
    }

    /// <summary>
    /// 按 item.index 原样放入一件道具（读档用），不重新分配槽位号，放入后触发 <see cref="OnPackageUpdate"/>。
    /// 存档里的槽位号必须保留，否则道具位置会与存档时不一致。
    /// </summary>
    public void AddRestoredItem(PropItemInfo item)
    {
        if (item == null)
        {
            return;
        }

        packageItems.Add(item);
        OnPackageUpdate?.Invoke();
    }

    /// <summary>
    /// 分配下一个槽位号：当前最大槽位号的后继（空背包从 0 开始）。
    /// 只保证唯一、不回填空洞——将来支持移除道具后，被空出的槽位号不会复用。
    /// </summary>
    private int AllocateSlotIndex()
    {
        int maxIndex = -1;
        foreach (var item in packageItems)
        {
            if (item != null && item.index > maxIndex)
            {
                maxIndex = item.index;
            }
        }

        return maxIndex + 1;
    }
}
