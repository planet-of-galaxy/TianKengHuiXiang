using QFramework;
using UnityEngine;

/// <summary>
/// 接管指定角色：挂载玩家控制器并切到该角色的第一人称相机。
/// 角色选择面板点击某个选项时执行；进入角色控制阶段由控制权变化驱动（见 PrepareState）。
/// </summary>
public class ControlRoleCmd : AbstractCommand
{
    private readonly RoleContext _roleContext;

    public ControlRoleCmd(RoleContext roleContext)
    {
        _roleContext = roleContext;
    }

    protected override void OnExecute()
    {
        if (_roleContext == null)
        {
            Debug.LogError($"{nameof(ControlRoleCmd)}: 收到 null 的角色实例");
            return;
        }

        // 先接管控制再切相机：控制器与武器攻击监听都挂在「受控角色」上，顺序反了会漏掉这一帧的绑定。
        this.GetSystem<IRoleInstanceSystem>().AddPlayerMoveController(_roleContext);

        if (_roleContext.FirstViewCinema == null)
        {
            Debug.LogError($"{nameof(ControlRoleCmd)}: roleRuntimeIndex={_roleContext.RoleRuntimeIndex} 的 firstViewCinema 未配置");
            return;
        }

        this.GetSystem<ICinemaChineCameraSystem>().TransitionTo(_roleContext.FirstViewCinema);
    }
}
