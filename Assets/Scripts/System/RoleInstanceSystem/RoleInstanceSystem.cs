using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
using Object = UnityEngine.Object;

public interface IRoleInstanceSystem : ISystem
{
    /// <summary>当前受玩家控制的角色，无控制对象时为 null。</summary>
    RoleContext ControllingRole { get; }

    /// <summary>角色实例创建完成（RoleContext 已初始化）。</summary>
    event Action<RoleContext> OnRoleInstanceCreated;
    /// <summary>角色实例已销毁，主动销毁与外部销毁都会触发一次。</summary>
    event Action<RoleContext> OnRoleInstanceDestroyed;
    /// <summary>控制对象发生变化；失去控制时参数为 null。</summary>
    event Action<RoleContext> OnControllingInstanceChanged;

    /// <summary>创建角色实例；该运行时 id 已有存活实例时直接复用，创建失败返回 null。</summary>
    RoleContext CreateRoleInstance(int roleRuntimeId, Vector3 position, Quaternion rotation);

    /// <summary>销毁角色实例；若它正受控制会先解除控制。</summary>
    void DestroyRoleInstance(RoleContext roleContext);

    /// <summary>接管角色：挂载 PlayerMoveController 并成为当前控制对象，原控制对象自动失去控制。</summary>
    void AddPlayerMoveController(RoleContext roleContext);

    /// <summary>解除对指定角色的控制并移除其 PlayerMoveController。</summary>
    void RemovePlayerMoveController(RoleContext roleContext);

    /// <summary>为角色添加并启用 AttackController 和 AttackListener，已有组件直接复用。</summary>
    void AddAttackController(RoleContext roleContext);

    /// <summary>移除角色的 AttackController 和 AttackListener。</summary>
    void RemoveAttackController(RoleContext roleContext);

    /// <summary>按运行时 id 获取已创建的角色实例；不存在或已销毁时返回 null。</summary>
    RoleContext TryGetRoleInstance(int roleRuntimeId);
}

/// <summary>统一管理角色实例的登记与生命周期，以及唯一的 PlayerMoveController。</summary>
public class RoleInstanceSystem : AbstractSystem, IRoleInstanceSystem
{
    /// <summary>已创建的角色实例，key 为运行时 id。</summary>
    private readonly Dictionary<int, RoleContext> roleInstances = new();

    private RoleRuntimeModel runtimeModel;
    private IResourceStorage resourceStorage;
    private PlayerMoveController currentController;

    public RoleContext ControllingRole { get; private set; }

    public event Action<RoleContext> OnRoleInstanceCreated;
    public event Action<RoleContext> OnRoleInstanceDestroyed;
    public event Action<RoleContext> OnControllingInstanceChanged;

    protected override void OnInit()
    {
        runtimeModel = this.GetModel<RoleRuntimeModel>();
        resourceStorage = this.GetUtility<IResourceStorage>();
    }

    protected override void OnDeinit()
    {
        // 架构销毁时角色 GameObject 通常也在同批销毁，这里只清理自身引用，不主动销毁实例。
        ReleaseController();
        ControllingRole = null;
        foreach (var context in roleInstances.Values)
        {
            context.OnDestroyed -= HandleRoleInstanceDestroyed;
        }
        roleInstances.Clear();
        OnRoleInstanceCreated = null;
        OnRoleInstanceDestroyed = null;
        OnControllingInstanceChanged = null;
    }

    public RoleContext CreateRoleInstance(int roleRuntimeId, Vector3 position, Quaternion rotation)
    {
        if (roleInstances.TryGetValue(roleRuntimeId, out var existing) && existing != null) return existing;

        var context = RoleViewFactory.CreateRoleInstance(
            runtimeModel, resourceStorage, roleRuntimeId, position, rotation);
        if (context == null) return null;

        // 实例系统统一挂载角色重力，不依赖是否接管为玩家控制对象。
        if (context.GetComponent<RoleGravityController>() == null)
        {
            context.gameObject.AddComponent<RoleGravityController>();
        }

        roleInstances[roleRuntimeId] = context;
        context.OnDestroyed += HandleRoleInstanceDestroyed;
        OnRoleInstanceCreated?.Invoke(context);
        return context;
    }

    public void DestroyRoleInstance(RoleContext roleContext)
    {
        if (roleContext == null)
        {
            Debug.LogError("[RoleInstanceSystem] DestroyRoleInstance 收到 null。");
            return;
        }

        // 主动销毁：在 GameObject 进入销毁流程前先移除控制器。
        if (ReferenceEquals(ControllingRole, roleContext)) RemovePlayerMoveController(roleContext);

        RoleViewFactory.DestroyRoleInstance(roleContext);
    }

    public void AddPlayerMoveController(RoleContext roleContext)
    {
        if (roleContext == null)
        {
            Debug.LogError("[RoleInstanceSystem] AddPlayerMoveController 收到 null。");
            return;
        }
        if (ReferenceEquals(ControllingRole, roleContext) && currentController != null) return;

        // 同一时刻只允许一个角色被控制，先释放旧角色。
        ReleaseController();

        var controller = roleContext.GetComponent<PlayerMoveController>();
        if (controller == null) controller = roleContext.gameObject.AddComponent<PlayerMoveController>();
        currentController = controller;
        SetControllingRole(roleContext);
    }

    public void RemovePlayerMoveController(RoleContext roleContext)
    {
        if (roleContext == null)
        {
            Debug.LogError("[RoleInstanceSystem] RemovePlayerMoveController 收到 null。");
            return;
        }
        if (!ReferenceEquals(ControllingRole, roleContext))
        {
            Debug.LogWarning($"[RoleInstanceSystem] 角色 {roleContext.RoleRuntimeIndex} 当前未受控制，无需移除控制器。");
            return;
        }

        ReleaseController();
        SetControllingRole(null);
    }

    public void AddAttackController(RoleContext roleContext)
    {
        if (roleContext == null)
        {
            Debug.LogError("[RoleInstanceSystem] AddAttackController 收到 null。");
            return;
        }

        // 先准备输入监听器，供控制器 Awake 时关联；未激活的角色也会拥有两个组件。
        var listener = roleContext.GetComponent<AttackListener>();
        if (listener == null) listener = roleContext.gameObject.AddComponent<AttackListener>();
        listener.enabled = true;

        var controller = roleContext.GetComponent<AttackController>();
        if (controller == null) controller = roleContext.gameObject.AddComponent<AttackController>();
        controller.enabled = true;
    }

    public void RemoveAttackController(RoleContext roleContext)
    {
        if (roleContext == null)
        {
            Debug.LogError("[RoleInstanceSystem] RemoveAttackController 收到 null。");
            return;
        }

        var controller = roleContext.GetComponent<AttackController>();
        if (controller != null)
        {
            // 先禁用控制器以退订事件，再移除监听器。
            controller.enabled = false;
            // 与移动控制器保持一致，立即移除以支持同帧重新添加。
            Object.DestroyImmediate(controller);
        }

        var listener = roleContext.GetComponent<AttackListener>();
        if (listener != null)
        {
            listener.enabled = false;
            Object.DestroyImmediate(listener);
        }
    }

    public RoleContext TryGetRoleInstance(int roleRuntimeId)
    {
        // context != null 走的是 Unity 重载的 ==：已被销毁但引用尚存的实例在这里会返回 null。
        return roleInstances.TryGetValue(roleRuntimeId, out var context) && context != null ? context : null;
    }

    /// <summary>Handles RoleContext destruction and releases instance references.</summary>
    private void HandleRoleInstanceDestroyed(RoleContext roleContext)
    {
        if (!UnregisterRoleInstance(roleContext)) return;

        if (ReferenceEquals(ControllingRole, roleContext))
        {
            // GameObject 已在销毁，控制器组件由 Unity 一并销毁；这里只清理引用，不能 DestroyImmediate。
            currentController = null;
            SetControllingRole(null);
        }

        OnRoleInstanceDestroyed?.Invoke(roleContext);
    }

    private void SetControllingRole(RoleContext roleContext)
    {
        if (ReferenceEquals(ControllingRole, roleContext)) return;
        ControllingRole = roleContext;
        OnControllingInstanceChanged?.Invoke(roleContext);
    }

    /// <summary>销毁当前控制器；控制角色引用由调用方更新，以保留变更检测。</summary>
    private void ReleaseController()
    {
        var controller = currentController;
        // 先清空引用，避免组件生命周期回调重入时再次销毁同一组件。
        currentController = null;
        if (controller == null) return;

        controller.enabled = false;
        // 必须立即移除：Destroy 会延迟到帧末，同帧切换时旧角色仍会保留组件。
        Object.DestroyImmediate(controller);
    }

    /// <summary>注销角色实例；实例不存在或已被注销返回 false。</summary>
    private bool UnregisterRoleInstance(RoleContext roleContext)
    {
        if (ReferenceEquals(roleContext, null)) return false;
        var runtimeIndex = roleContext.RoleRuntimeIndex;
        if (!roleInstances.TryGetValue(runtimeIndex, out var registered)) return false;
        if (!ReferenceEquals(registered, roleContext)) return false;
        roleContext.OnDestroyed -= HandleRoleInstanceDestroyed;
        roleInstances.Remove(runtimeIndex);
        return true;
    }
}
