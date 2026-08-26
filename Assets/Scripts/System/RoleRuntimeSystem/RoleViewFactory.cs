using QFramework;
using UnityEngine;

/// <summary>
/// 角色视图工厂：读取 RoleRuntimeModel 中当前角色，经 IResourceStorage 加载预制体并实例化。
/// 持有当前实例引用，实例销毁时通过 RoleRuntimeLifecycle 置空。
/// 切换当前角色时不销毁上一个角色，只移除其 PlayerController，交还控制权。
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

        var instance = Object.Instantiate(prefab, position, rotation);
        return TakeControlOf(instance);
    }

    /// <summary>
    /// 将场景中已存在的角色设为当前角色：校验其 RoleContext 后挂载 PlayerController，不实例化新对象。
    /// 上一个当前角色只被移除 PlayerController，实例保留在场景中。失败返回 null。
    /// </summary>
    public GameObject SpawnCurrentRole(GameObject roleInstance)
    {
        if (roleInstance == null)
        {
            Debug.LogError("[RoleViewFactory] SpawnCurrentRole 传入的 GameObject 为 null");
            return null;
        }

        var roleContext = roleInstance.GetComponent<RoleContext>();
        if (roleContext == null)
        {
            Debug.LogError($"[RoleViewFactory] {roleInstance.name} 上没有 RoleContext，无法作为当前角色");
            return null;
        }

        if (!runtimeModel.TryGetRoleRuntime(roleContext.roleRuntimeIndex, out _))
        {
            Debug.LogError($"[RoleViewFactory] RoleRuntimeInfo not found for id: {roleContext.roleRuntimeIndex}");
            return null;
        }

        return TakeControlOf(roleInstance);
    }

    /// <summary>
    /// 把实例设为当前角色：先释放上一个角色的控制权，再挂载生命周期与 PlayerController。
    /// </summary>
    private GameObject TakeControlOf(GameObject instance)
    {
        ReleaseCurrentRoleControl(instance);

        currentRoleInstance = instance;

        var lifecycle = currentRoleInstance.GetComponent<RoleRuntimeLifecycle>();
        if (lifecycle == null)
        {
            lifecycle = currentRoleInstance.AddComponent<RoleRuntimeLifecycle>();
        }
        lifecycle.Init(OnRoleInstanceDestroyed);

        if (currentRoleInstance.GetComponent<PlayerController>() == null)
        {
            currentRoleInstance.AddComponent<PlayerController>();
        }

        return currentRoleInstance;
    }

    /// <summary>
    /// 移除上一个当前角色身上的 PlayerController，实例本身不销毁。
    /// nextInstance 与当前实例相同时不做任何处理，避免刚接管就被摘掉控制器。
    /// </summary>
    private void ReleaseCurrentRoleControl(GameObject nextInstance)
    {
        if (currentRoleInstance == null) return;
        if (ReferenceEquals(currentRoleInstance, nextInstance)) return;

        var playerController = currentRoleInstance.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // Destroy 到帧末才生效，先禁用避免同帧内两个 PlayerController 同时响应输入
            playerController.enabled = false;
            Object.Destroy(playerController);
        }

        currentRoleInstance = null;
    }

    /// <summary>
    /// 根据指定角色运行时 id 实例化对应角色，不挂载 PlayerController。
    /// 适用于非玩家控制场景（如 NPC、展示用角色）。
    /// 返回的实例不被 currentRoleInstance 跟踪，生命周期由调用方管理。
    /// </summary>
    public GameObject SpawnRoleWithoutController(int runtimeId, Vector3 position, Quaternion rotation)
    {
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

        var instance = Object.Instantiate(prefab, position, rotation);
        return instance;
    }

    private void OnRoleInstanceDestroyed(GameObject instance)
    {
        if (ReferenceEquals(instance, currentRoleInstance))
        {
            currentRoleInstance = null;
        }
    }
}
