using QFramework;
using UnityEngine;

public interface IRoleRuntimeSystem : ISystem
{
    int CreateRole(int roleId);
    void SetCurrentRole(int roleRuntimeId);

    /// <summary>
    /// 当前角色实例；未生成或已销毁时为 null。
    /// </summary>
    GameObject CurrentRoleInstance { get; }

    /// <summary>
    /// 实例化当前选中的角色，返回实例；失败返回 null。
    /// </summary>
    GameObject SpawnCurrentRole(Vector3 position, Quaternion rotation);
}
