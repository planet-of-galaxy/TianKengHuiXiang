using System.Collections.Generic;
using QFramework;

/// <summary>
/// 单个角色的背包运行时信息，roleRuntimeId 对应 RoleRuntimeModel 中的角色运行时实例 id。
/// 由 PackageSystem 初始化时写入 PackageModel。
/// </summary>
public class RolePackageInfo
{
    /// <summary>
    /// 角色运行时实例 id，唯一，与 RoleRuntimeInfo.id 对应。
    /// </summary>
    public int roleRuntimeId;

    /// <summary>
    /// 该角色背包中的物品运行时信息列表。
    /// </summary>
    public List<PackageItemInfo> packageItems = new();

    /// <summary>
    /// 该角色的背包容量上限，-1 表示无限/未设置，用于 UI 响应容量变化。
    /// </summary>
    public BindableProperty<int> capacity { get; } = new BindableProperty<int>(-1);

    /// <summary>
    /// 该角色当前手持/选中的物品槽位 index，-1 表示未手持，用于 UI 响应切换。
    /// </summary>
    public BindableProperty<int> heldIndex { get; } = new BindableProperty<int>(-1);
}
