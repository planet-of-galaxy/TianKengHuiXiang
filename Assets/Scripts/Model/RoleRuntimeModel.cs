using System.Collections.Generic;
using QFramework;

public class RoleRuntimeModel : AbstractModel
{
    /// <summary>
    /// 所有角色运行时信息的集合，key 为运行时实例 id（唯一，同一角色配置可有多份实例）。
    /// </summary>
    public Dictionary<int, RoleRuntimeInfo> roleRuntimeInfo { get; } = new();

    /// <summary>
    /// 当前选中的角色运行时实例 id，用于 UI 响应角色切换。
    /// </summary>
    public BindableProperty<int> curRole { get; } = new BindableProperty<int>(-1);

    public void AddRoleRuntime(RoleRuntimeInfo info)
    {
        if (info == null) return;
        roleRuntimeInfo[info.id] = info;
    }

    protected override void OnInit()
    {
    }
}
