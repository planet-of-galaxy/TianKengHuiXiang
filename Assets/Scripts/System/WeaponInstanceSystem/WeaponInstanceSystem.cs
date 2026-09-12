using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
using Object = UnityEngine.Object;

public interface IWeaponInstanceSystem : ISystem
{
    GameObject TryGetWeaponInstance(int roleRuntimeIndex);
}

/// <summary>同步所有已实例角色的手持武器表现，武器生命周期隶属于角色实例。</summary>
public class WeaponInstanceSystem : AbstractSystem, IWeaponInstanceSystem
{
    private sealed class RoleBinding
    {
        public GameObject Role;
        public RoleContext Context;
        public RolePackageInfo Package;
        public IUnRegister HeldSubscription;
        public Action PackageUpdated;
        public PropItemInfo HeldItem;
        public GameObject Weapon;
    }

    private readonly Dictionary<int, RoleBinding> bindings = new();
    private IRoleInstanceSystem roleSystem;
    private PackageModel packageModel;
    private IWeaponConfigProvider weaponConfigs;
    private IResourceStorage resources;

    protected override void OnInit()
    {
        roleSystem = this.GetSystem<IRoleInstanceSystem>();
        packageModel = this.GetModel<PackageModel>();
        weaponConfigs = this.GetUtility<IWeaponConfigProvider>();
        resources = this.GetUtility<IResourceStorage>();
        roleSystem.RoleInstanceCreated += OnRoleCreated;
        roleSystem.RoleInstanceDestroyed += OnRoleDestroyed;
        packageModel.PackageChanged += BindPackage;

        foreach (var role in this.GetModel<RoleRuntimeModel>().GetAllRoleRuntimes())
        {
            var instance = roleSystem.TryGetRoleInstance(role.runtimeIndex);
            if (instance != null) OnRoleCreated(role.runtimeIndex, instance);
        }
    }

    protected override void OnDeinit()
    {
        roleSystem.RoleInstanceCreated -= OnRoleCreated;
        roleSystem.RoleInstanceDestroyed -= OnRoleDestroyed;
        packageModel.PackageChanged -= BindPackage;
        foreach (var binding in bindings.Values)
        {
            UnbindPackage(binding);
            DestroyWeapon(binding);
        }
        bindings.Clear();
    }

    public GameObject TryGetWeaponInstance(int roleRuntimeIndex)
    {
        return bindings.TryGetValue(roleRuntimeIndex, out var binding)
            && binding.Role != null && binding.Weapon != null ? binding.Weapon : null;
    }

    private void OnRoleCreated(int runtimeIndex, GameObject instance)
    {
        if (bindings.TryGetValue(runtimeIndex, out var previous))
        {
            if (ReferenceEquals(previous.Role, instance)) return;
            UnbindPackage(previous);
            DestroyWeapon(previous);
        }

        // 在登记绑定前补齐背包，避免 PackageChanged 回调重复初始化。
        packageModel.GetOrCreatePackage(runtimeIndex);
        bindings[runtimeIndex] = new RoleBinding
        {
            Role = instance,
            Context = instance.GetComponent<RoleContext>()
        };
        BindPackage(runtimeIndex);
    }

    private void OnRoleDestroyed(int runtimeIndex, GameObject instance)
    {
        if (!bindings.TryGetValue(runtimeIndex, out var binding)
            || !ReferenceEquals(binding.Role, instance)) return;

        bindings.Remove(runtimeIndex);
        UnbindPackage(binding);
        // 子物体由 Unity 随角色销毁，不在销毁回调中再次操作它。
        binding.Weapon = null;
        binding.HeldItem = null;
        binding.Context = null;
        binding.Role = null;
    }

    private void BindPackage(int runtimeIndex)
    {
        if (!bindings.TryGetValue(runtimeIndex, out var binding)) return;
        UnbindPackage(binding);
        if (packageModel.TryGetPackage(runtimeIndex, out var package))
        {
            binding.Package = package;
            binding.HeldSubscription = package.heldIndex.Register(_ => RefreshWeapon(binding));
            binding.PackageUpdated = () => RefreshWeapon(binding);
            package.OnPackageUpdate += binding.PackageUpdated;
        }
        RefreshWeapon(binding);
    }

    private static void UnbindPackage(RoleBinding binding)
    {
        binding.HeldSubscription?.UnRegister();
        binding.HeldSubscription = null;
        if (binding.Package != null) binding.Package.OnPackageUpdate -= binding.PackageUpdated;
        binding.PackageUpdated = null;
        binding.Package = null;
    }

    private void RefreshWeapon(RoleBinding binding)
    {
        PropItemInfo heldItem = null;
        if (binding.Package != null && binding.Package.heldIndex.Value >= 0)
        {
            foreach (var item in binding.Package.Items)
            {
                if (item == null || item.index != binding.Package.heldIndex.Value) continue;
                if (item.Type == ItemType.Weapon) heldItem = item;
                break;
            }
        }

        // 背包中其他物品变化不应重新生成当前武器。
        if (heldItem != null && ReferenceEquals(binding.HeldItem, heldItem)
            && binding.Weapon != null) return;

        DestroyWeapon(binding);
        if (heldItem == null || binding.Role == null) return;
        var config = weaponConfigs.GetWeaponConfig(heldItem.configId);
        if (config == null || string.IsNullOrEmpty(config.name)) return;
        if (binding.Context == null || binding.Context.WeaponPosition == null)
        {
            Debug.LogWarning($"[WeaponInstanceSystem] Role {binding.Package.roleRuntimeId} has no weapon position.");
            return;
        }

        var path = $"Prefabe/Weapon/{config.name}";
        var prefab = resources.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogWarning($"[WeaponInstanceSystem] Weapon prefab not found: {path}");
            return;
        }

        binding.Weapon = Object.Instantiate(prefab, binding.Context.WeaponPosition, false);
        binding.Weapon.transform.localPosition = Vector3.zero;
        binding.Weapon.transform.localRotation = Quaternion.identity;
        binding.HeldItem = heldItem;
    }

    private static void DestroyWeapon(RoleBinding binding)
    {
        var weapon = binding.Weapon;
        binding.Weapon = null;
        binding.HeldItem = null;
        if (weapon == null) return;
        weapon.SetActive(false);
        Object.Destroy(weapon);
    }
}
