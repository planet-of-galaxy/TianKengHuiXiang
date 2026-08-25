using System.Collections.Generic;
using QFramework;
using UnityEngine;

/// <summary>
/// 管理场景中的角色生成点。
/// </summary>
public class RolePointController : MonoBehaviour, IController
{
    [SerializeField] private List<RoleCreator> roleCreators = new();

    private void Awake()
    {
        var roleRuntimeModel = this.GetModel<RoleRuntimeModel>();
        var creatorIndex = 0;

        foreach (var roleRuntime in roleRuntimeModel.GetAllRoleRuntimes())
        {
            while (creatorIndex < roleCreators.Count &&
                   (roleCreators[creatorIndex] == null || !roleCreators[creatorIndex].IsIdle))
            {
                creatorIndex++;
            }

            if (creatorIndex >= roleCreators.Count)
            {
                Debug.LogWarning(
                    $"[RolePointController] Not enough idle RoleCreator instances for runtime role {roleRuntime.runtimeIndex}.",
                    this);
                break;
            }

            roleCreators[creatorIndex].CreateRole(roleRuntime.runtimeIndex);
            creatorIndex++;
        }
    }

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;
}
