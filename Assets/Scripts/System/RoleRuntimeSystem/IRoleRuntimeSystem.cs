using QFramework;
using UnityEngine;

public interface IRoleRuntimeSystem : ISystem
{
    int CreateRole(int roleId);
    void SetCurrentRole(int roleRuntimeId);

    /// <summary>
    /// 实例化当前选中的角色并托管其生命周期，返回实例；失败返回 null。
    /// </summary>
    GameObject SpawnCurrentRole(Vector3 position, Quaternion rotation);
}
