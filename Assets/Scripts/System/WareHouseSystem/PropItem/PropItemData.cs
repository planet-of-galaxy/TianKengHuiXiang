/// <summary>
/// 道具持久化数据（仓库 / 背包通用模块）。
/// type 记录道具类型，载入时据此实例化对应的 PropItemInfo 子类；
/// durability 为道具耐久，当前仅武器使用，对应 WeaponItemInfo.durability 的持久化值。
/// </summary>
public class PropItemData
{
    public int index;
    public int configId;
    public ItemType type;
    public int num;
    public float durability;
}
