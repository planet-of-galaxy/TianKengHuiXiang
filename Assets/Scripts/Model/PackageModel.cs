using System.Collections.Generic;
using QFramework;

public class PackageModel : AbstractModel
{
    /// <summary>
    /// 背包中的物品运行时信息列表（由 PackageSystem 初始化时写入）。
    /// </summary>
    public List<PackageItemInfo> packageItems;

    /// <summary>
    /// 背包容量上限，-1 表示无限/未设置，用于 UI 响应容量变化。
    /// </summary>
    public BindableProperty<int> capacity { get; } = new BindableProperty<int>(-1);

    /// <summary>
    /// 当前手持/选中的物品槽位 index，-1 表示未手持，用于 UI 响应切换。
    /// </summary>
    public BindableProperty<int> heldIndex { get; } = new BindableProperty<int>(-1);

    protected override void OnInit()
    {
    }
}
