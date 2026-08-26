using System.Collections.Generic;
using QFramework;
using UnityEngine;

/// <summary>
/// 角色创建器：挂载到场景中，持有一组 Transform 作为角色生成点。
/// Awake 时从 RoleRuntimeSystem 获取所有角色，为每个角色在对应 Transform 位置实例化一个展示用角色（不挂载 PlayerController）。
/// </summary>
public class RoleCreator : MonoBehaviour, IController
{
    [SerializeField] private List<Transform> roleSpawnPoints = new List<Transform>();

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    private void Awake()
    {
        var roleRuntimeSystem = this.GetSystem<IRoleRuntimeSystem>();
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

            var instance = roleRuntimeSystem.SpawnRoleWithoutController(
                roleInfo.runtimeIndex,
                spawnPoint.position,
                spawnPoint.rotation
            );

            if (instance != null)
            {
                var roleContext = instance.GetComponent<RoleContext>();
                if (roleContext == null)
                {
                    roleContext = instance.AddComponent<RoleContext>();
                }
                roleContext.roleRuntimeIndex = roleInfo.runtimeIndex;

                var listener = instance.GetComponent<RoleObserveListener>();
                if (listener == null)
                {
                    instance.AddComponent<RoleObserveListener>();
                }
            }

            index++;
        }
    }
}
