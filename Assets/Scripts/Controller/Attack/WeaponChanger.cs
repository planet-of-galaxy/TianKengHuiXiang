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
    private RolePackageInfo package;
    private IUnRegister heldIndexRegistration;
    private PropItemInfo heldItem;
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
        if (started) SubscribePackage();
    }

    private void Start()
    {
        // 角色工厂在 Instantiate 返回后初始化 RoleContext，避免 Awake 时读取未赋值的角色 id。
        started = true;
        SubscribePackage();
    }

    private void OnDisable()
    {
        heldIndexRegistration?.UnRegister();
        heldIndexRegistration = null;
        if (package != null) package.OnPackageUpdate -= RefreshWeapon;
        package = null;
        heldItem = null;
    }

    private void SubscribePackage()
    {
        package = this.GetModel<IPackageModel>().GetOrCreatePackage(roleContext.RoleRuntimeIndex);

        heldIndexRegistration = package.heldIndex.Register(_ => RefreshWeapon());
        package.OnPackageUpdate += RefreshWeapon;
        heldItem = packageSystem.GetHeldItem(roleContext.RoleRuntimeIndex);
        ChangeWeapon(heldItem);
    }

    private void RefreshWeapon()
    {
        var item = packageSystem.GetHeldItem(roleContext.RoleRuntimeIndex);
        if (ReferenceEquals(heldItem, item)) return;
        heldItem = item;
        ChangeWeapon(item);
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
