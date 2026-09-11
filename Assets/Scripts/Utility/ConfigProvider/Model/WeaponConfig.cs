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

    public float attackSpeed;
    public float attackPower;
    public float durability;
}
