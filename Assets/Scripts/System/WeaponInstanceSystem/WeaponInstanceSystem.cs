using System.Collections.Generic;
using QFramework;
using UnityEngine;
using Object = UnityEngine.Object;

public interface IWeaponInstanceSystem : ISystem
{
    /// <summary>创建角色的武器，成功后替换已有武器。</summary>
    void CreateWeapon(RoleContext roleContext, int weaponConfigId);

    /// <summary>移除角色的武器，没有武器时不做处理。</summary>
    void RemoveWeapon(RoleContext roleContext);

    /// <summary>获取角色的武器，不存在或已销毁时返回 null。</summary>
    WeaponContext TryGetWeapon(RoleContext roleContext);
}

/// <summary>由调用方驱动角色武器的创建、移除和查询。</summary>
public class WeaponInstanceSystem : AbstractSystem, IWeaponInstanceSystem
{
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
        foreach (var weapon in weapons.Values)
        {
            DestroyWeapon(weapon);
        }
        weapons.Clear();
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

        RemoveWeapon(roleContext);
        weapons[roleContext] = weapon;
    }

    public void RemoveWeapon(RoleContext roleContext)
    {
        // 已销毁的 Unity 对象仍可作为字典键，用于清理残留登记。
        if (ReferenceEquals(roleContext, null)) return;
        if (!weapons.TryGetValue(roleContext, out var weapon)) return;

        weapons.Remove(roleContext);
        DestroyWeapon(weapon);
    }

    public WeaponContext TryGetWeapon(RoleContext roleContext)
    {
        if (ReferenceEquals(roleContext, null)) return null;
        if (!weapons.TryGetValue(roleContext, out var weapon)) return null;
        if (roleContext != null && weapon != null) return weapon;

        weapons.Remove(roleContext);
        DestroyWeapon(weapon);
        return null;
    }

    private static void DestroyWeapon(WeaponContext weapon)
    {
        if (weapon == null) return;
        weapon.gameObject.SetActive(false);
        Object.Destroy(weapon.gameObject);
    }
}
