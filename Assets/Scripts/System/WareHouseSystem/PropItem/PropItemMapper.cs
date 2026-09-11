using QFramework;
using UnityEngine;

/// <summary>
/// PropItem 持久化数据与运行时信息的双向转换（背包 / 仓库通用）。
/// 道具类型依据 PropItemData.ItemType 与运行时子类一一对应：
/// 每种 ItemType 需要一个 PropItemInfo 子类并在此登记，新增类型时需同步两个方法。
/// </summary>
public static class PropItemMapper
{
    /// <summary>
    /// 持久化数据 -> 运行时信息：依据 PropItemData.ItemType 实例化对应的子类，
    /// 例如 ItemType.Weapon -> WeaponItemInfo（并恢复其耐久）。未登记的类型打印警告并返回 null。
    /// </summary>
    public static PropItemInfo ToPropItemInfo(PropItemData data)
    {
        switch (data.type)
        {
            case ItemType.Weapon:
                return new WeaponItemInfo
                {
                    index = data.index,
                    configId = data.configId,
                    num = new BindableProperty<int>(data.num),
                    durability = new BindableProperty<float>(data.durability),
                };
            default:
                Debug.LogWarning($"[PropItem] 未知的道具类型 {data.type}，已跳过 configId={data.configId}");
                return null;
        }
    }

    /// <summary>
    /// 运行时信息 -> 持久化数据：道具类型由运行时子类声明的 <see cref="PropItemInfo.Type"/> 决定，
    /// 并落盘子类携带的字段（如武器耐久）。
    /// 未登记的运行时子类返回 null（并打印错误）——不能让它带着 PropItemData.type 的默认值 0
    /// （= ItemType.Weapon）落盘，那会把新类型的道具静默变成武器。
    /// </summary>
    public static PropItemData ToPropItemData(PropItemInfo info)
    {
        if (info == null)
        {
            return null;
        }

        var data = new PropItemData
        {
            index = info.index,
            configId = info.configId,
            num = info.num?.Value ?? 0,
            type = info.Type,
        };

        switch (info.Type)
        {
            case ItemType.Weapon:
                // Type 由子类声明，理论上 ItemType.Weapon 一定对应 WeaponItemInfo
                if (info is WeaponItemInfo weapon)
                {
                    data.durability = weapon.durability?.Value ?? 0f;
                    break;
                }

                Debug.LogError($"[PropItem] {info.GetType().Name} 声明为 {ItemType.Weapon} 但不是 WeaponItemInfo，无法落盘（configId={info.configId}）");
                return null;

            default:
                Debug.LogError($"[PropItem] 未登记的道具类型 {info.Type}（{info.GetType().Name}，configId={info.configId}），无法落盘，已跳过");
                return null;
        }

        return data;
    }
}
