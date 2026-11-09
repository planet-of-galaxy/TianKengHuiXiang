using QFramework;

/// <summary>
/// 普通道具运行时信息，对应 ItemType.Item。
/// 与武器不同，普通道具没有耐久，只有堆叠数量（num，继承自 PropItemInfo）。
/// </summary>
public class ItemItemInfo : PropItemInfo
{
    /// <summary>道具类型标识，供按类型分发的调用方使用。</summary>
    public override ItemType Type => ItemType.Item;
}
