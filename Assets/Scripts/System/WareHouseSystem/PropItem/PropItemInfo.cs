using QFramework;

/// <summary>
/// 道具运行时信息基类（仓库 / 背包通用模块）。
/// 道具的具体类型由子类表达（如武器 WeaponItemInfo），运行时不再保存 ItemType 字段；
/// 载入存档时由 PropItemMapper 依据 PropItemData.ItemType 实例化对应的子类（PackageSystem / WareHouseSystem 共用）。
/// 新增一种道具类型时，需同时新增一个 PropItemInfo 子类，并在 PropItemMapper 处登记。
/// </summary>
public abstract class PropItemInfo
{
    /// <summary>
    /// 槽位号：道具在所属背包 / 仓库中的栏位下标，由容器在放入时分配。
    /// UI 一律按本字段定位栏位，不要用列表下标——两者只有在「槽位号连续且无空洞」时才恰好相同。
    /// </summary>
    public int index;

    /// <summary>
    /// 道具配置 id，配合 <see cref="Type"/> 唯一决定用哪个 ConfigProvider 解析配置。
    /// 注意不同道具类型之间的 configId 不保证互不相同，必须先按 Type 分发再取配置。
    /// </summary>
    public int configId;

    public BindableProperty<int> num;

    /// <summary>
    /// 道具类型。由子类声明而不是字段，避免运行时数据与持久化数据两处各存一份导致不一致；
    /// 调用方据此分发到对应类型的配置与渲染逻辑，不要用 is 判断子类。
    /// </summary>
    public abstract ItemType Type { get; }
}
