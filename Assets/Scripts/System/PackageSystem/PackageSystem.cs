using System.Collections.Generic;
using QFramework;

public class PackageSystem : AbstractSystem, IPackageSystem
{
    /// <summary>
    /// 默认背包容量：无存档时初始化的可用栏位数。
    /// </summary>
    public const int defaultCapacity = 20;

    /// <summary>
    /// 背包容量上限：可通过升级解锁的最大栏位数。
    /// capacity 与 maxCapacity 之间的栏位在 UI 中以灰色锁定显示。
    /// </summary>
    public const int maxCapacity = 40;

    /// <summary>
    /// 背包运行时数据模型，由 TianArchitecture 注册。
    /// </summary>
    private PackageModel packageModel;

    /// <summary>
    /// JSON 持久化工具，用于读取 "Package" 存档。
    /// </summary>
    private IJsonStorage storage;

    /// <summary>
    /// 系统初始化：获取模型与存储工具，并加载存档写入模型。
    /// </summary>
    protected override void OnInit()
    {
        packageModel = this.GetModel<PackageModel>();
        storage = this.GetUtility<IJsonStorage>();

        InitPackageModel();
    }

    /// <summary>
    /// 读取存档，将数据直接写入 PackageModel。
    /// 若无存档则使用 defaultCapacity 作为初始容量，heldIndex 保持 -1，背包为空。
    /// 系统初始化时由 OnInit 调用；需要重新载入存档时也可手动调用。
    /// </summary>
    public void InitPackageModel()
    {
        var save = storage.Load<PackageSaveData>("Package");

        // 背包列表按存档重新构建（由 PackageSystem 初始化时使用）
        packageModel.packageItems ??= new List<PackageItemInfo>();
        packageModel.packageItems.Clear();

        if (save?.packageItems != null)
        {
            foreach (var data in save.packageItems)
            {
                if (data == null) continue;
                packageModel.packageItems.Add(ToPackageItemInfo(data));
            }
        }

        // capacity 是运行时可变的：有存档则随存档恢复（但 capacity < 1 视为非法，回退默认容量）；
        // 无存档时使用默认容量 defaultCapacity；heldIndex 无存档时保持 -1（表示未手持任何物品）
        if (save != null)
        {
            packageModel.capacity.Value = save.capacity < 1 ? defaultCapacity : save.capacity;
            packageModel.heldIndex.Value = save.heldIndex;
        }
        else
        {
            packageModel.capacity.Value = defaultCapacity;
            packageModel.heldIndex.Value = -1;
        }
    }

    /// <summary>
    /// 将持久化数据转换为运行时信息并写入模型。
    /// </summary>
    private PackageItemInfo ToPackageItemInfo(PackageItemData data)
    {
        return new PackageItemInfo
        {
            index = data.index,
            configId = data.configId,
            type = data.type,
            num = new BindableProperty<int>(data.num),
        };
    }
}
