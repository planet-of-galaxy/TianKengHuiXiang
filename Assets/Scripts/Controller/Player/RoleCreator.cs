using System.Collections.Generic;
using QFramework;
using UnityEngine;

/// <summary>
/// 角色创建器：挂载到场景中，持有一组 Transform 作为角色生成点。
/// Awake 时从 RoleInstanceSystem 获取所有角色，为每个角色在对应 Transform 位置实例化一个展示用角色（不挂载 PlayerMoveController）。
/// </summary>
public class RoleCreator : MonoBehaviour, IController
{
    [SerializeField] private List<Transform> roleSpawnPoints = new List<Transform>();

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    private void Awake()
    {
        var roleInstanceSystem = this.GetSystem<IRoleInstanceSystem>();
        var runtimeModel = this.GetModel<RoleRuntimeModel>();

        var allRoles = runtimeModel.GetAllRoleRuntimes();
        int index = 0;

        foreach (var roleInfo in allRoles)
        {
            if (index >= roleSpawnPoints.Count)
            {
                Debug.LogWarning($"[RoleCreator] 角色数量 ({runtimeModel.Count}) 超过生成点数量 ({roleSpawnPoints.Count})，剩余角色未生成");
                break;
            }

            var spawnPoint = roleSpawnPoints[index];
            if (spawnPoint == null)
            {
                Debug.LogWarning($"[RoleCreator] 生成点 {index} 为 null，跳过角色 {roleInfo.runtimeIndex}");
                index++;
                continue;
            }

            // 只创建实例、不接管控制：CreateRoleInstance 不挂 PlayerMoveController，
            // 角色就停在生成点上供观察，玩家控制权留在别处。
            // 创建失败时工厂已报错，这里不用额外处理。
            roleInstanceSystem.CreateRoleInstance(
                roleInfo.runtimeIndex,
                spawnPoint.position,
                spawnPoint.rotation
            );

            index++;
        }
    }
}
