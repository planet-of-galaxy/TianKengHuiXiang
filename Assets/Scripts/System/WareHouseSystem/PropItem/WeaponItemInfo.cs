using QFramework;

/// <summary>
/// 武器道具运行时信息，对应 ItemType.Weapon。
/// durability 为武器耐久，随使用消耗，用于 UI 响应变化。
/// </summary>
public class WeaponItemInfo : PropItemInfo
{
    /// <summary>武器类型标识，供按类型分发的调用方使用。</summary>
    public override ItemType Type => ItemType.Weapon;

    public BindableProperty<float> durability;
}
