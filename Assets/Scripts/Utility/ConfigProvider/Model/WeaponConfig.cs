[System.Serializable]
public class WeaponConfig
{
    public int weaponId;
    public string name;

    /// <summary>
    /// 道具图标的 Resources 路径（不含扩展名），如 "UI/PropItem/Weapon/icon_golf"。
    /// 留空表示该武器没有图标，UI 不显示图标。
    /// </summary>
    public string icon;

    public float durability;

    public WeaponAttackConfig lightAttack;
    public WeaponAttackConfig heavyAttack;
}

/// <summary>
/// 单种攻击的范围、时序、伤害、暴击和眩晕配置；概率取值为 0～1，时间单位为秒。
/// </summary>
[System.Serializable]
public class WeaponAttackConfig
{
    public DamageType damageType;
    public float attackPower;
    public float attackSpeed;

    /// <summary>攻击范围，单位为 Unity 世界单位。</summary>
    public float attackRange;

    /// <summary>前摇结束后的攻击持续时长，单位为秒，不包含前摇。</summary>
    public float attackDuration;

    /// <summary>攻击开始到生效前的等待时长，单位为秒。</summary>
    public float attackWindup;

    public float criticalRate;
    public float criticalMultiplier;

    public float stunRate;
    public float stunDuration;
}
