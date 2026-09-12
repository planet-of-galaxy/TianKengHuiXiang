using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>只负责生成、登记和销毁角色，控制器由 RoleInstanceSystem 管理。</summary>
public class RoleViewFactory
{
    private readonly RoleRuntimeModel runtimeModel;
    private readonly IResourceStorage resourceStorage;
    private readonly Dictionary<int, GameObject> instances = new();

    public event Action<int, GameObject> RoleInstanceCreated;
    public event Action<int, GameObject> RoleInstanceDestroyed;

    public RoleViewFactory(RoleRuntimeModel runtimeModel, IResourceStorage resourceStorage)
    {
        this.runtimeModel = runtimeModel;
        this.resourceStorage = resourceStorage;
    }

    public GameObject TryGetRoleInstance(int roleRuntimeIndex)
    {
        return instances.TryGetValue(roleRuntimeIndex, out var instance) && instance != null ? instance : null;
    }

    public GameObject SpawnRoleWithoutController(int runtimeId, Vector3 position, Quaternion rotation)
    {
        var existing = TryGetRoleInstance(runtimeId);
        if (existing != null) return existing;
        if (!runtimeModel.TryGetRoleRuntime(runtimeId, out var info) || string.IsNullOrEmpty(info.name))
        {
            Debug.LogWarning($"[RoleInstanceSystem] Invalid role runtime index: {runtimeId}");
            return null;
        }
        // 约定角色预制体不挂载 PlayerMoveController，只由 RoleInstanceSystem 添加。
        var prefab = resourceStorage.Load<GameObject>($"Prefabe/Role/{info.name}");
        if (prefab == null) return null;
        var instance = Object.Instantiate(prefab, position, rotation);
        var context = instance.GetComponent<RoleContext>();
        if (context == null) context = instance.AddComponent<RoleContext>();
        context.Initialize(runtimeId);
        instances[runtimeId] = instance;
        var lifecycle = instance.GetComponent<RoleRuntimeLifecycle>();
        if (lifecycle == null) lifecycle = instance.AddComponent<RoleRuntimeLifecycle>();
        lifecycle.Init(destroyed => OnRoleInstanceDestroyed(runtimeId, destroyed));
        RoleInstanceCreated?.Invoke(runtimeId, instance);
        return instance;
    }

    public void DestroyRoleInstance(int roleRuntimeIndex)
    {
        var instance = TryGetRoleInstance(roleRuntimeIndex);
        if (instance == null) return;
        OnRoleInstanceDestroyed(roleRuntimeIndex, instance);
        instance.SetActive(false);
        Object.Destroy(instance);
    }

    private void OnRoleInstanceDestroyed(int runtimeId, GameObject instance)
    {
        if (!instances.TryGetValue(runtimeId, out var registered) || !ReferenceEquals(registered, instance)) return;
        instances.Remove(runtimeId);
        RoleInstanceDestroyed?.Invoke(runtimeId, instance);
    }
}
