using QFramework;
using UnityEngine;

public interface IRoleInstanceSystem : ISystem
{
    /// <summary>选择运行时角色；尚未生成时释放控制，-1 表示取消选择。</summary>
    void SetCurrentRole(int roleRuntimeId);
    GameObject CurrentRoleInstance { get; }
    RoleContext GetCurrentRoleContext();

    /// <summary>生成或复用当前选中角色，并接管控制；未选择或生成失败时返回 null。</summary>
    GameObject SpawnCurrentRole(Vector3 position, Quaternion rotation);

    /// <summary>接管已登记的角色；-1 取消控制。无效索引不改变当前控制。</summary>
    GameObject ControlRole(int runtimeIndex);

    /// <summary>生成无控制器角色；已有实例直接复用，不改变其控制状态。</summary>
    GameObject SpawnRoleWithoutController(int runtimeId, Vector3 position, Quaternion rotation);
    GameObject TryGetRoleInstance(int roleRuntimeIndex);
    void DestroyRoleInstance(int roleRuntimeIndex);
}

/// <summary>统一管理角色实例，以及唯一的 PlayerMoveController。</summary>
public class RoleInstanceSystem : AbstractSystem, IRoleInstanceSystem
{
    private RoleRuntimeModel runtimeModel;
    private RoleInstanceModel instanceModel;
    private RoleViewFactory viewFactory;
    private GameObject currentRoleInstance;
    private PlayerMoveController currentController;
    private IUnRegister currentRoleSubscription;

    protected override void OnInit()
    {
        runtimeModel = this.GetModel<RoleRuntimeModel>();
        instanceModel = this.GetModel<RoleInstanceModel>();
        viewFactory = new RoleViewFactory(runtimeModel, this.GetUtility<IResourceStorage>());
        viewFactory.RoleInstanceDestroyed += OnRoleInstanceDestroyed;
        currentRoleSubscription = instanceModel.curRole.Register(SynchronizeControl);
        SynchronizeControl(instanceModel.curRole.Value);
    }

    protected override void OnDeinit()
    {
        currentRoleSubscription?.UnRegister();
        viewFactory.RoleInstanceDestroyed -= OnRoleInstanceDestroyed;
        ReleaseControl();
        instanceModel.curRole.Value = -1;
    }

    public void SetCurrentRole(int roleRuntimeId)
    {
        if (roleRuntimeId != -1 && !runtimeModel.TryGetRoleRuntime(roleRuntimeId, out _))
        {
            Debug.LogWarning($"[RoleInstanceSystem] Invalid role runtime index: {roleRuntimeId}");
            return;
        }
        instanceModel.curRole.Value = roleRuntimeId;
        SynchronizeControl(roleRuntimeId);
    }

    public GameObject CurrentRoleInstance => currentRoleInstance != null ? currentRoleInstance : null;

    public RoleContext GetCurrentRoleContext()
    {
        return CurrentRoleInstance != null ? CurrentRoleInstance.GetComponent<RoleContext>() : null;
    }

    public GameObject SpawnCurrentRole(Vector3 position, Quaternion rotation)
    {
        var runtimeIndex = instanceModel.curRole.Value;
        if (runtimeIndex == -1)
        {
            Debug.LogError($"[RoleInstanceSystem] 当前未选中任何角色");
            return null;
        }
        var instance = viewFactory.SpawnRoleWithoutController(runtimeIndex, position, rotation);
        return instance != null ? ControlRole(runtimeIndex) : null;
    }

    public GameObject ControlRole(int runtimeIndex)
    {
        if (runtimeIndex == -1)
        {
            SetCurrentRole(-1);
            return null;
        }
        var instance = TryGetRoleInstance(runtimeIndex);
        if (instance == null || !runtimeModel.TryGetRoleRuntime(runtimeIndex, out _))
        {
            Debug.LogWarning($"[RoleInstanceSystem] Role instance not found: {runtimeIndex}");
            return null;
        }
        SetCurrentRole(runtimeIndex);
        return instance;
    }

    private void SynchronizeControl(int runtimeIndex)
    {
        var nextInstance = runtimeIndex == -1 ? null : TryGetRoleInstance(runtimeIndex);
        if (nextInstance != null && nextInstance == currentRoleInstance && currentController != null) return;

        ReleaseControl();
        currentRoleInstance = nextInstance;
        if (currentRoleInstance != null)
        {
            currentController = currentRoleInstance.AddComponent<PlayerMoveController>();
        }
    }

    private void ReleaseControl()
    {
        var controller = currentController;
        // 先清空引用，避免组件生命周期回调重入时再次销毁同一组件。
        currentController = null;
        currentRoleInstance = null;
        if (controller != null)
        {
            controller.enabled = false;
            // 必须立即移除：Destroy 会延迟到帧末，同帧切换时旧角色仍会保留组件。
            Object.DestroyImmediate(controller);
        }
    }

    public GameObject SpawnRoleWithoutController(int runtimeId, Vector3 position, Quaternion rotation)
    {
        return viewFactory.SpawnRoleWithoutController(runtimeId, position, rotation);
    }

    public GameObject TryGetRoleInstance(int roleRuntimeIndex) => viewFactory.TryGetRoleInstance(roleRuntimeIndex);

    public void DestroyRoleInstance(int roleRuntimeIndex)
    {
        var instance = TryGetRoleInstance(roleRuntimeIndex);
        if (instance != null && ReferenceEquals(currentRoleInstance, instance))
        {
            // 主动销毁时，在 GameObject 进入销毁流程前移除控制器。
            ReleaseControl();
        }
        viewFactory.DestroyRoleInstance(roleRuntimeIndex);
    }

    private void OnRoleInstanceDestroyed(int runtimeIndex, GameObject instance)
    {
        if (ReferenceEquals(currentRoleInstance, instance))
        {
            // GameObject 已在销毁，组件由 Unity 一并销毁；这里只清理引用。
            // 必须先清空，再更新 curRole，避免订阅回调再次释放控制器。
            currentController = null;
            currentRoleInstance = null;
        }
        if (instanceModel.curRole.Value == runtimeIndex) instanceModel.curRole.Value = -1;
    }
}
