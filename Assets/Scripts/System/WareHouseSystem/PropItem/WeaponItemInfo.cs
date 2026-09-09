using QFramework;

/// <summary>
/// 武器道具运行时信息，对应 ItemType.Weapon。
/// durability 为武器耐久，随使用消耗，用于 UI 响应变化。
/// </summary>
public class WeaponItemInfo : PropItemInfo
{
    public BindableProperty<float> durability;
}
