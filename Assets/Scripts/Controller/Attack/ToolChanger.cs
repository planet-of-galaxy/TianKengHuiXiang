using QFramework;
using UnityEngine;

/// <summary>根据角色背包的手持物类型切换实例，未手持时恢复徒手。</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RoleContext))]
public class ToolChanger : MonoBehaviour, IController
{
    private RoleContext roleContext;
    private IPackageSystem packageSystem;
    private IWeaponInstanceSystem weaponInstanceSystem;
    private IResourceStorage resourceStorage;
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
        resourceStorage = this.GetUtility<IResourceStorage>();
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
        if (package != null) package.OnItemListChanged -= RefreshHeldItem;
        package = null;
        heldItem = null;
    }

    private void SubscribePackage()
    {
        package = this.GetModel<IPackageModel>().GetOrCreatePackage(roleContext.RoleRuntimeIndex);

        heldIndexRegistration = package.heldIndex.Register(_ => RefreshHeldItem());
        package.OnItemListChanged += RefreshHeldItem;
        heldItem = packageSystem.GetHeldItem(roleContext.RoleRuntimeIndex);
        ChangeTool(heldItem);
    }

    private void RefreshHeldItem()
    {
        var item = packageSystem.GetHeldItem(roleContext.RoleRuntimeIndex);
        if (ReferenceEquals(heldItem, item)) return;
        heldItem = item;
        ChangeTool(item);
    }

    private void ChangeTool(PropItemInfo item)
    {
        // 配置 id 在不同类型之间可能相同，必须先按类型分发。
        int configId = item?.Type switch
        {
            ItemType.Weapon => item.configId,
            _ => -1
        };
        var config = this.GetUtility<IWeaponConfigProvider>().GetWeaponConfig(configId);
        string path = config?.animationOverrideController;
        AnimatorOverrideController overrideController = null;
        if (!string.IsNullOrWhiteSpace(path))
        {
            overrideController = resourceStorage.Load<AnimatorOverrideController>(path);
        }

        if (overrideController != null)
        {
            roleContext.RoleAnimatorController?.ReplaceAnimation(overrideController);
        }

        switch (item?.Type)
        {
            case ItemType.Weapon:
                // 实例系统负责停用并销毁旧武器，并通知攻击控制器更新引用。
                weaponInstanceSystem.CreateWeapon(roleContext, item.configId);
                break;
            default:
                // 未手持或尚未支持的类型恢复徒手。
                weaponInstanceSystem.RemoveWeapon(roleContext);
                break;
        }
    }

}
