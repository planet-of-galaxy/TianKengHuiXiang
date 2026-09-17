using QFramework;
using UnityEngine;

/// <summary>根据角色背包的手持物创建并切换武器实例。</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RoleContext))]
public class WeaponChanger : MonoBehaviour, IController
{
    private RoleContext roleContext;
    private IPackageSystem packageSystem;
    private IWeaponInstanceSystem weaponInstanceSystem;
    private bool started;

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    private void Awake()
    {
        roleContext = GetComponent<RoleContext>();
        packageSystem = this.GetSystem<IPackageSystem>();
        weaponInstanceSystem = this.GetSystem<IWeaponInstanceSystem>();
    }

    private void OnEnable()
    {
        packageSystem.OnHeldItemChanged += HandleHeldItemChanged;
        if (started) RefreshWeapon();
    }

    private void Start()
    {
        // 角色工厂在 Instantiate 返回后初始化 RoleContext，避免 Awake 时读取未赋值的角色 id。
        started = true;
        RefreshWeapon();
    }

    private void OnDisable()
    {
        if (packageSystem != null) packageSystem.OnHeldItemChanged -= HandleHeldItemChanged;
    }

    private void HandleHeldItemChanged(int roleRuntimeId, PropItemInfo item)
    {
        if (!started || roleRuntimeId != roleContext.RoleRuntimeIndex) return;
        ChangeWeapon(item);
    }

    private void RefreshWeapon()
    {
        ChangeWeapon(packageSystem.GetHeldItem(roleContext.RoleRuntimeIndex));
    }

    private void ChangeWeapon(PropItemInfo item)
    {
        if (item is WeaponItemInfo weapon)
        {
            // 实例系统负责停用并销毁旧武器，并通知攻击控制器更新引用。
            weaponInstanceSystem.CreateWeapon(roleContext, weapon.configId);
        }
        else
        {
            weaponInstanceSystem.RemoveWeapon(roleContext);
        }
    }
}
