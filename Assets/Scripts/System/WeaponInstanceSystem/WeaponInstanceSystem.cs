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
    private bool attackListenerEnabled = true;

    protected override void OnInit()
    {
        roleSystem = this.GetSystem<IRoleInstanceSystem>();
        packageModel = this.GetModel<PackageModel>();
        weaponConfigs = this.GetUtility<IWeaponConfigProvider>();
        resources = this.GetUtility<IResourceStorage>();
        roleSystem.OnControllingInstanceChanged += OnControllingInstanceChanged;
        roleSystem.OnRoleInstanceCreated += OnRoleCreated;
        roleSystem.OnRoleInstanceDestroyed += OnRoleDestroyed;
        packageModel.PackageChanged += BindPackage;

        // 本系统可能晚于 RoleInstanceSystem 初始化，补登记此间已创建的角色实例。
        foreach (var role in this.GetModel<RoleRuntimeModel>().GetAllRoleRuntimes())
        {
            var context = roleSystem.TryGetRoleInstance(role.runtimeIndex);
            if (context != null) OnRoleCreated(context);
        }
    }

    protected override void OnDeinit()
    {
        roleSystem.OnControllingInstanceChanged -= OnControllingInstanceChanged;
        roleSystem.OnRoleInstanceCreated -= OnRoleCreated;
        roleSystem.OnRoleInstanceDestroyed -= OnRoleDestroyed;
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
            && binding.Context != null && binding.Weapon != null ? binding.Weapon : null;
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

    private void OnControllingInstanceChanged(RoleContext roleContext)
    {
        SynchronizeAttackListeners();
    }

    private void SynchronizeAttackListeners()
    {
        // 先移除旧角色的监听，再为当前角色添加，确保只有当前角色接收输入。
        var controllingRole = roleSystem.ControllingRole;
        foreach (var binding in bindings.Values)
        {
            // 显式判 controllingRole 非空：只写成 binding.Context != controllingRole 的话，
            // 双方都为 null（角色已销毁 + 已无控制对象）会被 Unity 的 == 判成相等，监听反而留着。
            var isControlling = attackListenerEnabled && controllingRole != null && binding.Context == controllingRole;
            if (!isControlling) RemoveWeaponAttackListeners(binding.Weapon);
        }

        if (!attackListenerEnabled || controllingRole == null) return;
        if (bindings.TryGetValue(controllingRole.RoleRuntimeIndex, out var current))
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

    private void OnRoleCreated(RoleContext context)
    {
        if (context == null) return;
        var runtimeIndex = context.RoleRuntimeIndex;

        if (bindings.TryGetValue(runtimeIndex, out var previous))
        {
            // 同一个实例重复登记（如 OnInit 补登记）直接跳过，换了实例才需要清理旧绑定。
            if (previous.Context == context) return;
            UnbindPackage(previous);
            DestroyWeapon(previous);
        }

        // 在登记绑定前补齐背包，避免 PackageChanged 回调重复初始化。
        packageModel.GetOrCreatePackage(runtimeIndex);
        bindings[runtimeIndex] = new RoleBinding
        {
            RuntimeIndex = runtimeIndex,
            Context = context,
        };
        BindPackage(runtimeIndex);
    }

    private void OnRoleDestroyed(RoleContext context)
    {
        if (context == null) return;
        var runtimeIndex = context.RoleRuntimeIndex;
        if (!bindings.TryGetValue(runtimeIndex, out var binding) || binding.Context != context) return;

        bindings.Remove(runtimeIndex);
        UnbindPackage(binding);
        // 子物体由 Unity 随角色销毁，不在销毁回调中再次操作它。
        binding.Weapon = null;
        binding.HeldItem = null;
        binding.Context = null;
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
        if (heldItem == null || binding.Context == null) return;
        var config = weaponConfigs.GetWeaponConfig(heldItem.configId);
        if (config == null || string.IsNullOrEmpty(config.name)) return;
        if (binding.Context.WeaponPosition == null)
        {
            Debug.LogWarning($"[WeaponInstanceSystem] Role {binding.RuntimeIndex} has no weapon position.");
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
        if (attackListenerEnabled && binding.Context == roleSystem.ControllingRole)
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
