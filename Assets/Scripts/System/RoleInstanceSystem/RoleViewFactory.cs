using System;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 角色视图工厂：只按运行时 id 实例化和销毁角色预制体，自身不保存任何状态。
/// 实例登记、控制器挂载与生命周期通知由调用方（RoleInstanceSystem）负责。
/// </summary>
public static class RoleViewFactory
{
    /// <summary>
    /// 实例化角色预制体并返回其 RoleContext。任何一步失败都直接报错并返回 null。
    /// </summary>
    /// <param name="onDestroyed">实例被销毁时回调；用于外部销毁（非 DestroyRoleInstance）时通知调用方。</param>
    public static RoleContext CreateRoleInstance(
        RoleRuntimeModel runtimeModel,
        IResourceStorage resourceStorage,
        int roleRuntimeId,
        Vector3 position,
        Quaternion rotation,
        Action<RoleContext> onDestroyed)
    {
        if (!runtimeModel.TryGetRoleRuntime(roleRuntimeId, out var info) || string.IsNullOrEmpty(info.name))
        {
            Debug.LogError($"[RoleViewFactory] 无效的角色运行时 id：{roleRuntimeId}。");
            return null;
        }

        var path = $"Prefabe/Role/{info.name}";
        var prefab = resourceStorage.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"[RoleViewFactory] 未找到角色预制体：{path}（角色运行时 id：{roleRuntimeId}）。");
            return null;
        }

        var instance = Object.Instantiate(prefab, position, rotation);
        // 约定角色预制体自带 RoleContext；拿不到说明预制体配置有误，直接报错而不是静默补组件。
        var context = instance.GetComponent<RoleContext>();
        if (context == null)
        {
            Debug.LogError($"[RoleViewFactory] 角色预制体 {path} 上缺少 RoleContext，已中止创建。", instance);
            Object.Destroy(instance);
            return null;
        }

        context.Initialize(roleRuntimeId);

        var lifecycle = instance.GetComponent<RoleRuntimeLifecycle>();
        if (lifecycle == null) lifecycle = instance.AddComponent<RoleRuntimeLifecycle>();
        lifecycle.Init(_ => onDestroyed?.Invoke(context));
        return context;
    }

    /// <summary>销毁角色实例所在的 GameObject。</summary>
    public static void DestroyRoleInstance(RoleContext roleContext)
    {
        if (roleContext == null) return;
        var instance = roleContext.gameObject;
        instance.SetActive(false);
        Object.Destroy(instance);
    }
}
