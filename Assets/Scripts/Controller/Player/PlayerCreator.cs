using QFramework;
using UnityEngine;

/// <summary>
/// 角色创建器：挂载到出生点（BornPoint）上。
/// Awake 时创建第一个运行时角色并接管其控制，创建完成后启用第一人称虚拟相机，然后销毁自身。
/// </summary>
public class PlayerCreator : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    private void Awake()
    {
        var roleInstanceSystem = this.GetSystem<IRoleInstanceSystem>();

        var roleRuntimeIds = this.GetModel<RoleRuntimeModel>().GetAllRoleRuntimeIds();
        if (roleRuntimeIds.Count == 0)
        {
            Debug.LogWarning("[PlayerCreator] 没有可创建的运行时角色。");
            Destroy(gameObject);
            return;
        }

        var roleContext = roleInstanceSystem.CreateRoleInstance(roleRuntimeIds[0], transform.position, transform.rotation);
        if (roleContext == null)
        {
            Destroy(gameObject);
            return;
        }

        // 先接管控制再切相机：控制器与武器攻击监听都挂在「受控角色」上，顺序反了会漏掉这一帧的绑定。
        roleInstanceSystem.AddPlayerMoveController(roleContext);
        EnableFirstViewCinema(roleContext);
        Destroy(gameObject);
    }

    /// <summary>
    /// 启用角色实例身上的第一人称虚拟相机（FirstViewCinema），让新实例立即获得玩家视野。
    /// 与 CurrentRoleSetListener 在选中角色后的处理保持一致。
    /// </summary>
    private void EnableFirstViewCinema(RoleContext roleContext)
    {
        if (roleContext.FirstViewCinema == null)
        {
            return;
        }

        this.GetSystem<ICinemaChineCameraSystem>().SetCinemaChineCamera(roleContext.FirstViewCinema);
    }
}
