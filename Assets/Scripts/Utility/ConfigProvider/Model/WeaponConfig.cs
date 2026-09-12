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
/// 单种攻击的伤害、暴击和眩晕配置；概率取值为 0～1，眩晕时长单位为秒。
/// </summary>
[System.Serializable]
public class WeaponAttackConfig
{
    public DamageType damageType;
    public float attackPower;
    public float attackSpeed;

    public float criticalRate;
    public float criticalMultiplier;

    public float stunRate;
    public float stunDuration;
}
