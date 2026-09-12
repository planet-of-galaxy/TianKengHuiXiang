using QFramework;
using UnityEngine;

/// <summary>
/// 角色创建器：挂载到出生点（BornPoint）上。
/// Awake 时选中第一个运行时角色，委托 RoleInstanceSystem 实例化并托管其生命周期，创建完成后启用第一人称虚拟相机，然后销毁自身。
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

        roleInstanceSystem.SetCurrentRole(roleRuntimeIds[0]);
        var instance = roleInstanceSystem.SpawnCurrentRole(transform.position, transform.rotation);
        if (instance == null)
        {
            Destroy(gameObject);
            return;
        }

        EnableFirstViewCinema(instance);
        Destroy(gameObject);
    }

    /// <summary>
    /// 启用角色实例身上的第一人称虚拟相机（FirstViewCinema），让新实例立即获得玩家视野。
    /// 与 CurrentRoleSetListener 在选中角色后的处理保持一致。
    /// </summary>
    private void EnableFirstViewCinema(GameObject roleInstance)
    {
        var roleContext = roleInstance.GetComponent<RoleContext>();
        if (roleContext == null || roleContext.FirstViewCinema == null)
        {
            return;
        }

        this.GetSystem<ICinemaChineCameraSystem>().SetCinemaChineCamera(roleContext.FirstViewCinema);
    }
}
