using QFramework;
using UnityEngine;

/// <summary>
/// 在场景中标记一个生成点，调用 CreateRole 可在此位置和朝向生成不带 Controller 的角色。
/// 最多持有一个角色实例，重复调用时先销毁旧实例再生成新实例。
/// </summary>
public class RoleCreator : MonoBehaviour, IController
{
    private GameObject spawnedRole;

    public bool IsIdle => spawnedRole == null;

    public void CreateRole(int runtimeIndex)
    {
        if (spawnedRole != null)
        {
            Destroy(spawnedRole);
            spawnedRole = null;
        }

        var system = this.GetSystem<IRoleRuntimeSystem>();
        spawnedRole = system.SpawnRoleWithoutController(runtimeIndex, transform.position, transform.rotation);
    }

    private void OnDestroy()
    {
        if (spawnedRole != null)
        {
            Destroy(spawnedRole);
            spawnedRole = null;
        }
    }

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;
}
