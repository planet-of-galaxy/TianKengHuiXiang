/// <summary>
/// 伤害信息：描述一次攻击造成的伤害。
/// Passed from an attacker to IGetDamage.GetDamage.
/// </summary>
public struct DamageInfo
{
    /// <summary>伤害类型。</summary>
    public DamageType type;

    /// <summary>伤害提示段数。</summary>
    public int hintCount;

    /// <summary>伤害提示数值。</summary>
    public float hintValue;
}
