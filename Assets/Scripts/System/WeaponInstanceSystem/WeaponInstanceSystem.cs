using System.Collections.Generic;
using System;
using QFramework;
using UnityEngine;
using Object = UnityEngine.Object;

public interface IWeaponInstanceSystem : ISystem
{
    /// <summary>武器替换完成后触发；无武器时 newWeaponConfig.weaponId 为 -1。</summary>
    event Action<RoleContext, WeaponConfig> OnWeaponChanged;

    /// <summary>创建角色的武器，成功后替换已有武器。</summary>
    void CreateWeapon(RoleContext roleContext, int weaponConfigId);

    /// <summary>将角色武器替换为配置 -1 的无武器对象；已是无武器对象时不做处理。</summary>
    void RemoveWeapon(RoleContext roleContext);

    /// <summary>获取角色的武器，不存在或已销毁时返回 null。</summary>
    WeaponContext TryGetWeapon(RoleContext roleContext);
}

/// <summary>由调用方驱动角色武器的创建、移除和查询。</summary>
public class WeaponInstanceSystem : AbstractSystem, IWeaponInstanceSystem
{
    private const int UnarmedWeaponConfigId = -1;

    public event Action<RoleContext, WeaponConfig> OnWeaponChanged;

    private readonly Dictionary<RoleContext, WeaponContext> weapons = new();
    private IWeaponConfigProvider weaponConfigs;
    private IResourceStorage resources;

    protected override void OnInit()
    {
        weaponConfigs = this.GetUtility<IWeaponConfigProvider>();
        resources = this.GetUtility<IResourceStorage>();
    }

    protected override void OnDeinit()
    {
        foreach (var context in weapons.Keys)
        {
            context.OnDestroyed -= HandleRoleDestroyed;
        }
        foreach (var weapon in weapons.Values)
        {
            DestroyWeapon(weapon);
        }
        weapons.Clear();
        OnWeaponChanged = null;
    }

    public void CreateWeapon(RoleContext roleContext, int weaponConfigId)
    {
        if (roleContext == null)
        {
            Debug.LogError("[WeaponInstanceSystem] CreateWeapon 收到无效的 RoleContext。");
            return;
        }

        if (roleContext.WeaponPosition == null)
        {
            Debug.LogError($"[WeaponInstanceSystem] 角色 {roleContext.RoleRuntimeIndex} 未设置武器挂点。", roleContext);
            return;
        }

        var config = weaponConfigs.GetWeaponConfig(weaponConfigId);
        if (config == null || string.IsNullOrEmpty(config.name))
        {
            Debug.LogError($"[WeaponInstanceSystem] 武器配置 {weaponConfigId} 不存在或缺少预制体名称。", roleContext);
            return;
        }

        var path = $"Prefabe/Weapon/{config.name}";
        var prefab = resources.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"[WeaponInstanceSystem] 找不到武器预制体：{path}。", roleContext);
            return;
        }

        var prefabContext = prefab.GetComponent<WeaponContext>();
        if (prefabContext == null)
        {
            Debug.LogError($"[WeaponInstanceSystem] 武器预制体根节点缺少 WeaponContext：{path}。", prefab);
            return;
        }

        var weapon = Object.Instantiate(prefabContext, roleContext.WeaponPosition, false);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;

        if (!weapons.TryGetValue(roleContext, out var previousWeapon))
        {
            roleContext.OnDestroyed += HandleRoleDestroyed;
        }
        weapons[roleContext] = weapon;
        DestroyWeapon(previousWeapon);
        OnWeaponChanged?.Invoke(roleContext, config);
    }

    public void RemoveWeapon(RoleContext roleContext)
    {
        // 已销毁的 Unity 对象仍可作为字典键，用于清理残留登记。
        if (ReferenceEquals(roleContext, null)) return;
        weapons.TryGetValue(roleContext, out var weapon);

        if (roleContext == null)
        {
            roleContext.OnDestroyed -= HandleRoleDestroyed;
            weapons.Remove(roleContext);
            DestroyWeapon(weapon);
            return;
        }

        if (weapon != null && weapon.WeaponConfigId == UnarmedWeaponConfigId) return;

        CreateWeapon(roleContext, UnarmedWeaponConfigId);
    }

    public WeaponContext TryGetWeapon(RoleContext roleContext)
    {
        if (ReferenceEquals(roleContext, null)) return null;
        if (!weapons.TryGetValue(roleContext, out var weapon)) return null;
        if (roleContext != null && weapon != null) return weapon;

        roleContext.OnDestroyed -= HandleRoleDestroyed;
        weapons.Remove(roleContext);
        DestroyWeapon(weapon);
        return null;
    }

    private void HandleRoleDestroyed(RoleContext roleContext)
    {
        roleContext.OnDestroyed -= HandleRoleDestroyed;
        // Child weapons are destroyed with the role; only release the reference here.
        weapons.Remove(roleContext);
    }

    private static void DestroyWeapon(WeaponContext weapon)
    {
        if (weapon == null) return;
        weapon.gameObject.SetActive(false);
        Object.Destroy(weapon.gameObject);
    }
}
