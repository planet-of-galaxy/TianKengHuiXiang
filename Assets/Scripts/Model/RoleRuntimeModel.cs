using System.Collections.Generic;
using QFramework;

public interface IRoleRuntimeModel : IModel
{
    int Count { get; }
    bool TryGetRoleRuntime(int id, out RoleRuntimeInfo info);
    IEnumerable<RoleRuntimeInfo> GetAllRoleRuntimes();
    List<int> GetAllRoleRuntimeIds();
}

public class RoleRuntimeModel : AbstractModel, IRoleRuntimeModel
{
    /// <summary>
    /// 所有角色运行时信息的集合，key 为运行时实例 id（唯一，同一角色配置可有多份实例）。
    /// </summary>
    private readonly Dictionary<int, RoleRuntimeInfo> roleRuntimeInfo = new();

    public int Count => roleRuntimeInfo.Count;

    public bool TryGetRoleRuntime(int id, out RoleRuntimeInfo info)
    {
        return roleRuntimeInfo.TryGetValue(id, out info);
    }

    /// <summary>
    /// 遍历所有角色运行时信息（只读）。
    /// </summary>
    public IEnumerable<RoleRuntimeInfo> GetAllRoleRuntimes()
    {
        return roleRuntimeInfo.Values;
    }

    /// <summary>
    /// 返回所有角色运行时实例 id 的列表副本。
    /// </summary>
    public List<int> GetAllRoleRuntimeIds()
    {
        return new List<int>(roleRuntimeInfo.Keys);
    }

    /// <summary>
    /// 添加或覆盖一个角色运行时信息（由 RoleRuntimeSystem 写入）。
    /// </summary>
    public void AddRoleRuntime(RoleRuntimeInfo info)
    {
        if (info == null) return;
        roleRuntimeInfo[info.runtimeIndex] = info;
    }

    /// <summary>
    /// 清空所有角色运行时信息（由 RoleRuntimeSystem 初始化时使用）。
    /// </summary>
    public void ClearRoleRuntimes()
    {
        roleRuntimeInfo.Clear();
    }

    protected override void OnInit()
    {
    }
}
