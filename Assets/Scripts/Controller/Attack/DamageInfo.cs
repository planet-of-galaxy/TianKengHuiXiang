/// <summary>
/// 伤害信息：描述一次攻击造成的伤害。
/// 作为 IAttackA.AttackA 的返回值，也作为 IGetDamage.GetDamage 的入参，在攻击方与受伤方之间传递。
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
