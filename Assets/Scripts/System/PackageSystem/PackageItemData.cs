using System.Collections.Generic;

public class PackageItemData
{
    public int index;
    public int configId;
    public ItemType type;
    public int num;
}

/// <summary>
/// 单个角色的背包持久化数据，roleRuntimeId 对应 RoleRuntimeModel 中的角色运行时实例 id。
/// 与运行时的 RolePackageInfo 一一对应。
/// </summary>
public class RolePackageData
{
    public int roleRuntimeId;
    public List<PackageItemData> packageItems;
    public int capacity;
    public int heldIndex;
}

public class PackageSaveData
{
    /// <summary>
    /// 各角色的背包数据。持久化用 List 而非字典：LitJson 对 int key 的字典序列化支持不佳。
    /// </summary>
    public List<RolePackageData> rolePackages;

    /// <summary>
    /// 旧版存档字段（未按角色区分），仅用于兼容迁移：
    /// 读档时若 rolePackages 为空而旧字段有值，则整体迁移给当前选中角色。
    /// </summary>
    public List<PackageItemData> packageItems;
    public int capacity;
    public int heldIndex;
}
