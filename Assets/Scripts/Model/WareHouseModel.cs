using System.Collections.Generic;
using QFramework;

public interface IWareHouseModel : IModel
{
    int Count { get; }
    IReadOnlyDictionary<int, PropItemInfo> wareHouseItems { get; }
    bool TryGetItem(int instanceId, out PropItemInfo item);
    IEnumerable<PropItemInfo> GetAllItems();
}

public class WareHouseModel : AbstractModel, IWareHouseModel
{
    /// <summary>
    /// 仓库道具集合，key 为道具实例 id（唯一；逐件道具独占一个条目并各自携带独立耐久，
    /// 与按角色划分的背包 RolePackageInfo 不同）。由 WareHouseSystem 初始化时写入。
    /// </summary>
    private readonly Dictionary<int, PropItemInfo> items = new();

    /// <summary>
    /// 下一个可分配的实例 id，随 AddItem 递增；载入存档时由 AddItem 抬升到 max(index)+1。
    /// </summary>
    private int nextInstanceId = 0;

    /// <summary>
    /// 仓库道具数量。
    /// </summary>
    public int Count => items.Count;

    /// <summary>
    /// 仓库道具集合的只读视图，key 为实例 id。
    /// </summary>
    public IReadOnlyDictionary<int, PropItemInfo> wareHouseItems => items;

    /// <summary>
    /// 尝试获取指定实例 id 的道具。
    /// </summary>
    public bool TryGetItem(int instanceId, out PropItemInfo item)
    {
        return items.TryGetValue(instanceId, out item);
    }

    /// <summary>
    /// 分配一个新的仓库道具实例 id（随 nextInstanceId 自增）。
    /// </summary>
    public int AllocateInstanceId()
    {
        return nextInstanceId++;
    }

    /// <summary>
    /// 添加一件道具（由 WareHouseSystem 写入），以 item.index 为实例 id。
    /// 同时将 nextInstanceId 抬升到 max(index)+1，保证之后分配的 id 不与已有条目冲突。
    /// </summary>
    public void AddItem(PropItemInfo item)
    {
        if (item == null) return;
        items[item.index] = item;
        if (item.index >= nextInstanceId)
        {
            nextInstanceId = item.index + 1;
        }
    }

    /// <summary>
    /// 遍历仓库所有道具（只读）。
    /// </summary>
    public IEnumerable<PropItemInfo> GetAllItems()
    {
        return items.Values;
    }

    /// <summary>
    /// 清空所有道具并重置实例 id 分配（由 WareHouseSystem 初始化时使用）。
    /// </summary>
    public void ClearItems()
    {
        items.Clear();
        nextInstanceId = 0;
    }

    protected override void OnInit()
    {
    }
}
