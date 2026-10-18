using System.Collections.Generic;
using QFramework;
using UnityEngine;

/// <summary>查询道具数据，并通过 PropItemPanel 更新显示。</summary>
[RequireComponent(typeof(PropItemPanel))]
public class PropItemController : MonoBehaviour, IController
{
    /// <summary>对应 PropItemInfo.index，空栏位为 -1。</summary>
    public int index { get; private set; } = -1;

    /// <summary>当前栏位的物品是否为受控角色的手持物。</summary>
    public bool IsEquipped
    {
        get
        {
            if (index < 0 || Panel.IsLocked) return false;
            var role = this.GetSystem<IRoleInstanceSystem>().ControllingRole;
            return role != null
                && this.GetModel<IPackageModel>().TryGetPackage(role.RoleRuntimeIndex, out var package)
                && package.heldIndex.Value == index;
        }
    }

    private readonly Dictionary<string, Sprite> iconCache = new Dictionary<string, Sprite>();
    private PropItemMenu menu;

    private void OnEnable()
    {
        Panel.ItemClicked += OpenMenu;
    }

    private void OnDisable()
    {
        Panel.ItemClicked -= OpenMenu;
        CloseMenu();
    }

    private bool TryGetWeapon(out RolePackageInfo package, out WeaponItemInfo weapon)
    {
        package = null;
        weapon = null;
        if (Panel.IsLocked || index < 0) return false;
        var role = this.GetSystem<IRoleInstanceSystem>().ControllingRole;
        if (role == null || !this.GetModel<IPackageModel>().TryGetPackage(role.RoleRuntimeIndex, out package))
            return false;
        foreach (var item in package.Items)
        {
            if (item != null && item.index == index && item.Type == ItemType.Weapon)
            {
                weapon = item as WeaponItemInfo;
                return weapon != null;
            }
        }
        return false;
    }

    private void OpenMenu()
    {
        CloseMenu();
        if (!TryGetWeapon(out var package, out var weapon)) return;
        if (Panel.MenuSpawnPoint == null) return;
        var prefab = this.GetUtility<IResourceStorage>().Load<GameObject>("UI/Panel/PropItemMenu");
        if (prefab == null) return;
        var instance = Instantiate(prefab, Panel.MenuSpawnPoint, false);
        menu = instance.GetComponent<PropItemMenu>();
        if (menu == null)
        {
            Destroy(instance);
            return;
        }
        var menuRect = (RectTransform)menu.transform;
        menuRect.pivot = new Vector2(0f, 1f);
        menuRect.localPosition = Vector3.zero;
        bool equipped = package.heldIndex.Value == index;
        menu.Initialize(equipped ? "卸下" : "装备", Panel.MenuFont, () =>
            {
                // 菜单打开后角色、物品或装备状态可能已发生变化，执行前重新校验。
                if (!TryGetWeapon(out var currentPackage, out var currentWeapon)
                    || !ReferenceEquals(package, currentPackage) || !ReferenceEquals(weapon, currentWeapon)
                    || (currentPackage.heldIndex.Value == index) != equipped) return;
                currentPackage.heldIndex.Value = equipped ? -1 : index;
            });
    }

    private void CloseMenu()
    {
        if (menu != null) menu.Close();
        menu = null;
    }

    /// <summary>按当前受控角色背包中的槽位号查询道具并刷新显示。</summary>
    public void SetIndex(int itemIndex)
    {
        Clear();
        if (itemIndex < 0) return;

        var role = this.GetSystem<IRoleInstanceSystem>().ControllingRole;
        if (role == null || !this.GetModel<IPackageModel>().TryGetPackage(role.RoleRuntimeIndex, out var package))
        {
            return;
        }

        foreach (var item in package.Items)
        {
            if (item == null || item.index != itemIndex) continue;

            switch (item.Type)
            {
                case ItemType.Weapon:
                    if (item is WeaponItemInfo weapon)
                    {
                        index = itemIndex;
                        SetWeapon(weapon);
                        return;
                    }
                    break;
            }

            Debug.LogError($"[PropItemController] 无法显示道具类型 {item.Type}（configId={item.configId}）");
            return;
        }
    }

    private void SetWeapon(WeaponItemInfo weapon)
    {
        var config = this.GetUtility<IWeaponConfigProvider>().GetWeaponConfig(weapon.configId);
        Panel.SetName(config.name);
        Panel.SetIcon(LoadIcon(config));
        Panel.SetNum(weapon.num?.Value ?? 1);
        if (config != null)
        {
            Panel.SetDurability(weapon.durability?.Value ?? 0f, config.durability);
        }
    }

    private Sprite LoadIcon(WeaponConfig config)
    {
        if (config == null || string.IsNullOrEmpty(config.icon)) return null;

        if (!iconCache.TryGetValue(config.icon, out var sprite))
        {
            sprite = this.GetUtility<IResourceStorage>().Load<Sprite>(config.icon);
            iconCache[config.icon] = sprite;
        }

        return sprite;
    }

    public IArchitecture GetArchitecture()
    {
        return TianArchitecture.Interface;
    }

    private PropItemPanel panel;
    private PropItemPanel Panel => panel != null ? panel : (panel = GetComponent<PropItemPanel>());

    /// <summary>清除道具绑定并显示空栏位。</summary>
    public void Clear()
    {
        CloseMenu();
        index = -1;
        Panel.Clear();
    }

    /// <summary>清除道具绑定并显示锁定栏位。</summary>
    public void ShowLock()
    {
        Clear();
        Panel.ShowLock();
    }
}
