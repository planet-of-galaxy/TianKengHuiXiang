using QFramework;

/// <summary>
/// 道具运行时信息基类（仓库 / 背包通用模块）。
/// 道具的具体类型由子类表达（如武器 WeaponItemInfo），运行时不再保存 ItemType 字段；
/// 载入存档时由 PackageSystem 依据 PropItemData.ItemType 实例化对应的子类。
/// 新增一种道具类型时，需同时新增一个 PropItemInfo 子类，并在类型转换处登记。
/// </summary>
public abstract class PropItemInfo
{
    public int index;
    public int configId;
    public BindableProperty<int> num;
}
