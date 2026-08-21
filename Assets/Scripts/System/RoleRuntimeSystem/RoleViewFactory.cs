using UnityEngine;

/// <summary>
/// 角色视图工厂：读取 RoleRuntimeModel 中当前角色，经 IResourceStorage 加载预制体并实例化。
/// 持有当前实例引用，实例销毁时通过 RoleRuntimeLifecycle 置空。
/// 作为 RoleRuntimeSystem 的字段被持有，不对外暴露；对外接口由 RoleRuntimeSystem 提供。
/// </summary>
public class RoleViewFactory
{
    private readonly RoleRuntimeModel runtimeModel;
    private readonly IResourceStorage resourceStorage;
    private GameObject currentRoleInstance;

    public RoleViewFactory(RoleRuntimeModel runtimeModel, IResourceStorage resourceStorage)
    {
        this.runtimeModel = runtimeModel;
        this.resourceStorage = resourceStorage;
    }

    public GameObject CurrentRoleInstance
    {
        get
        {
            // Unity 重载的 == 能识别已销毁对象，避免拿到已销毁实例的残留引用
            if (currentRoleInstance == null) return null;
            return currentRoleInstance;
        }
    }

    public GameObject SpawnCurrentRole(Vector3 position, Quaternion rotation)
    {
        var runtimeId = runtimeModel.curRole.Value;
        if (!runtimeModel.TryGetRoleRuntime(runtimeId, out var info))
        {
            Debug.LogError($"[RoleViewFactory] RoleRuntimeInfo not found for id: {runtimeId}");
            return null;
        }

        if (string.IsNullOrEmpty(info.name))
        {
            Debug.LogError($"[RoleViewFactory] RoleRuntimeInfo {runtimeId} has empty name");
            return null;
        }

        var prefab = resourceStorage.Load<GameObject>($"Prefabe/Role/{info.name}");
        if (prefab == null)
        {
            return null;
        }

        if (currentRoleInstance != null)
        {
            Object.Destroy(currentRoleInstance);
            currentRoleInstance = null;
        }

        currentRoleInstance = Object.Instantiate(prefab, position, rotation);
        var lifecycle = currentRoleInstance.GetComponent<RoleRuntimeLifecycle>();
        if (lifecycle == null)
        {
            lifecycle = currentRoleInstance.AddComponent<RoleRuntimeLifecycle>();
        }
        lifecycle.Init(OnRoleInstanceDestroyed);

        return currentRoleInstance;
    }

    /// <summary>
    /// 角色实例销毁时回调：置空引用。
    /// </summary>
    private void OnRoleInstanceDestroyed(GameObject instance)
    {
        if (ReferenceEquals(instance, currentRoleInstance))
        {
            currentRoleInstance = null;
        }
    }
}
