using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
using Object = UnityEngine.Object;

public interface IWeaponInstanceSystem : ISystem
{
    GameObject TryGetWeaponInstance(int roleRuntimeIndex);
    /// <summary>启用当前角色武器的攻击监听，并在切换角色或武器时自动同步。</summary>
    void AddAttackListener();
    /// <summary>移除攻击监听，直到再次调用 AddAttackListener。</summary>
    void RemoveAttackListener();
}

/// <summary>同步所有已实例角色的手持武器表现，武器生命周期隶属于角色实例。</summary>
public class WeaponInstanceSystem : AbstractSystem, IWeaponInstanceSystem
{
    private sealed class RoleBinding
    {
        public int RuntimeIndex;
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
    private RoleInstanceModel instanceModel;
    private IUnRegister currentRoleSubscription;
    private bool attackListenerEnabled = true;

    protected override void OnInit()
    {
        roleSystem = this.GetSystem<IRoleInstanceSystem>();
        packageModel = this.GetModel<PackageModel>();
        weaponConfigs = this.GetUtility<IWeaponConfigProvider>();
        resources = this.GetUtility<IResourceStorage>();
        instanceModel = this.GetModel<RoleInstanceModel>();
        currentRoleSubscription = instanceModel.curRole.Register(_ => SynchronizeAttackListeners());
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
        currentRoleSubscription?.UnRegister();
        currentRoleSubscription = null;
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

    public void AddAttackListener()
    {
        attackListenerEnabled = true;
        SynchronizeAttackListeners();
    }

    public void RemoveAttackListener()
    {
        attackListenerEnabled = false;
        SynchronizeAttackListeners();
    }

    private void SynchronizeAttackListeners()
    {
        // 先移除旧角色的监听，再为当前角色添加，确保只有当前角色接收输入。
        foreach (var binding in bindings.Values)
        {
            if (!attackListenerEnabled || binding.RuntimeIndex != instanceModel.curRole.Value)
                RemoveWeaponAttackListeners(binding.Weapon);
        }

        if (attackListenerEnabled && bindings.TryGetValue(instanceModel.curRole.Value, out var current))
            AddWeaponAttackListener(current.Weapon);
    }

    private static void AddWeaponAttackListener(GameObject weapon)
    {
        if (weapon == null) return;
        if (weapon.GetComponent<IAttack>() == null)
        {
            RemoveWeaponAttackListeners(weapon);
            Debug.LogWarning("[WeaponInstanceSystem] 武器根节点缺少 IAttack 实现，无法添加攻击监听。", weapon);
            return;
        }

        var listener = weapon.GetComponent<AttackListener>();
        if (listener == null) weapon.AddComponent<AttackListener>();
        else listener.enabled = true;
    }

    private static void RemoveWeaponAttackListeners(GameObject weapon)
    {
        if (weapon == null) return;
        foreach (var listener in weapon.GetComponents<AttackListener>())
        {
            listener.enabled = false;
            // 与角色控制器一致，立即移除，避免同帧切回时复用已等待销毁的组件。
            Object.DestroyImmediate(listener);
        }
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
            RuntimeIndex = runtimeIndex,
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
        if (attackListenerEnabled && binding.RuntimeIndex == instanceModel.curRole.Value)
            AddWeaponAttackListener(binding.Weapon);
        else
            RemoveWeaponAttackListeners(binding.Weapon);
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
