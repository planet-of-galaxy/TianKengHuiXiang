using QFramework;
using UnityEngine;

/// <summary>
/// 角色创建器：挂载到出生点（BornPoint）上。
/// Awake 时委托 RoleRuntimeSystem 实例化当前角色并托管其生命周期，完成后销毁自身。
/// </summary>
public class PlayerCreator : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    private void Awake()
    {
        var roleRuntimeSystem = this.GetSystem<IRoleRuntimeSystem>();

        var instance = roleRuntimeSystem.SpawnCurrentRole(transform.position, transform.rotation);
        if (instance == null)
        {
            Destroy(gameObject);
            return;
        }

        // 切换到Player虚拟相机
        this.SendCommand(new TransitionCameraCmd("Player", 2f));
        Destroy(gameObject);
    }
}
