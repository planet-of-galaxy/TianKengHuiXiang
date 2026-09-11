using System.Collections.Generic;
using QFramework;

public class PackageModel : AbstractModel
{
    /// <summary>
    /// 各角色的背包运行时信息（物品列表、容量、手持槽位），
    /// key 为 RoleRuntimeModel 中的角色运行时实例 id（由 PackageSystem 初始化时写入）。
    /// </summary>
    private readonly Dictionary<int, RolePackageInfo> rolePackages = new();

    /// <summary>
    /// 拥有背包的角色数量。
    /// </summary>
    public int PackageCount => rolePackages.Count;

    /// <summary>
    /// 尝试获取指定角色运行时实例的背包信息。
    /// </summary>
    public bool TryGetPackage(int roleRuntimeId, out RolePackageInfo package)
    {
        return rolePackages.TryGetValue(roleRuntimeId, out package);
    }

    /// <summary>
    /// 获取指定角色运行时实例的背包信息；不存在时创建一个空背包并返回，
    /// 新背包的容量立即补为 <see cref="RolePackageInfo.DefaultCapacity"/>（heldIndex 保持 -1，表示未手持）。
    /// </summary>
    public RolePackageInfo GetOrCreatePackage(int roleRuntimeId)
    {
        if (!rolePackages.TryGetValue(roleRuntimeId, out var package))
        {
            package = new RolePackageInfo { roleRuntimeId = roleRuntimeId };
            rolePackages[roleRuntimeId] = package;
            EnsureCapacityInitialized(package);
        }

        return package;
    }

    /// <summary>
    /// 添加或覆盖一个角色的背包信息（由 PackageSystem 写入），以 info.roleRuntimeId 为 key。
    /// </summary>
    public void AddPackage(RolePackageInfo info)
    {
        if (info == null) return;

        rolePackages[info.roleRuntimeId] = info;
        EnsureCapacityInitialized(info);
    }

    /// <summary>
    /// 保证背包容量已初始化：capacity 为 -1（RolePackageInfo 的默认值）说明该背包从未被赋过容量，
    /// 此时补默认容量。在这里统一兜底，是为了让「流到 UI 的背包容量一定 &gt;= 1」成立——
    /// PackageSystem 里的容量校验（capacity &gt;= 0 才判满）会把 -1 当成「无限」放行，
    /// 而 UI 会把 -1 当成「0 个栏位」全锁死，两边对同一个值的理解相反，所以不能让 -1 流出去。
    /// </summary>
    private static void EnsureCapacityInitialized(RolePackageInfo info)
    {
        if (info.capacity.Value < 1)
        {
            info.capacity.Value = RolePackageInfo.DefaultCapacity;
        }
    }

    /// <summary>
    /// 移除指定角色运行时实例的背包（角色实例销毁时使用）。
    /// </summary>
    public bool RemovePackage(int roleRuntimeId)
    {
        return rolePackages.Remove(roleRuntimeId);
    }

    /// <summary>
    /// 遍历所有角色的背包信息（只读）。
    /// </summary>
    public IEnumerable<RolePackageInfo> GetAllPackages()
    {
        return rolePackages.Values;
    }

    /// <summary>
    /// 清空所有角色的背包（由 PackageSystem 初始化时使用）。
    /// </summary>
    public void ClearPackages()
    {
        rolePackages.Clear();
    }

    protected override void OnInit()
    {
    }
}
