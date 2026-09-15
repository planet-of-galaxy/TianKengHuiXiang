using QFramework;
using UnityEngine;

/// <summary>
/// 观察指定角色：把主相机切到该角色的观察相机。
/// 角色选择面板悬停某个选项时执行。
/// </summary>
public class ObserveRoleCmd : AbstractCommand
{
    private readonly RoleContext _roleContext;

    public ObserveRoleCmd(RoleContext roleContext)
    {
        _roleContext = roleContext;
    }

    protected override void OnExecute()
    {
        if (_roleContext == null)
        {
            Debug.LogError($"{nameof(ObserveRoleCmd)}: 收到 null 的角色实例");
            return;
        }

        if (_roleContext.ObserveCinema == null)
        {
            Debug.LogError($"{nameof(ObserveRoleCmd)}: roleRuntimeIndex={_roleContext.RoleRuntimeIndex} 的 observeCinema 未配置");
            return;
        }

        this.GetSystem<ICinemaChineCameraSystem>().TransitionTo(_roleContext.ObserveCinema);
    }
}
