using System.Collections.Generic;
using QFramework;
using UnityEngine;

public interface IPackageSystem : ISystem
{
    /// <summary>
    /// 读取存档，初始化 PackageModel；无存档时为每个角色实例创建默认空背包。
    /// </summary>
    void InitPackageModel();

    /// <summary>
    /// 将 PackageModel 中所有角色的背包数据落盘。
    /// </summary>
    void SavePackage();

    /// <summary>
    /// 依据 ItemType + configId 生成一件道具并加入指定角色的背包（仅内存，不落盘，需手动调用 SavePackage）。
    /// 背包已满、角色不存在或 configId 无法解析时返回 false。
    /// </summary>
    bool AddItemToRolePackage(int roleRuntimeId, ItemType itemType, int configId);

    /// <summary>
    /// 在场景中创建一个空物体并挂载 PackageController，使其监听全局快捷键。
    /// 已存在有效实例时忽略（幂等）。
    /// </summary>
    void AddPackageListener();

    /// <summary>
    /// 销毁 AddPackageListener 创建的空物体与 PackageController。
    /// </summary>
    void RemovePackageListener();
}

public class PackageSystem : AbstractSystem, IPackageSystem
{
    /// <summary>
    /// 默认背包容量：无存档（或存档容量非法）时每个角色背包的可用栏位数。
    /// </summary>
    public const int defaultCapacity = 20;

    /// <summary>
    /// 背包容量上限：可通过升级解锁的最大栏位数。
    /// capacity 与 maxCapacity 之间的栏位在 UI 中以灰色锁定显示。
    /// </summary>
    public const int maxCapacity = 40;

    /// <summary>
    /// 背包运行时数据模型，由 TianArchitecture 注册。
    /// </summary>
    private PackageModel packageModel;

    /// <summary>
    /// 角色运行时数据模型：背包按角色运行时实例 id 划分，
    /// 初始化时需要为每个角色实例建立对应的背包。
    /// </summary>
    private RoleRuntimeModel roleRuntimeModel;

    /// <summary>
    /// AddPackageListener 创建的空物体，用于挂载 PackageController。
    /// 切换场景时该物体可能被 Unity 销毁，此时与 null 相等，AddPackageListener 会重新创建。
    /// </summary>
    private GameObject packageListenerGO;

    /// <summary>
    /// JSON 持久化工具，用于读取 "Package" 存档。
    /// </summary>
    private IJsonStorage storage;

    /// <summary>
    /// 系统初始化：获取模型与存储工具，并加载存档写入模型。
    /// 依赖 RoleRuntimeSystem 先注册（TianArchitecture 中顺序保证），
    /// 以便按 RoleRuntimeModel 中已有的角色实例补齐背包。
    /// </summary>
    protected override void OnInit()
    {
        packageModel = this.GetModel<PackageModel>();
        roleRuntimeModel = this.GetModel<RoleRuntimeModel>();
        storage = this.GetUtility<IJsonStorage>();

        InitPackageModel();
    }

    /// <summary>
    /// 读取存档，将数据直接写入 PackageModel：
    /// 1. 按存档中的 rolePackages 重建各角色背包（物品、容量、手持槽位）；
    ///    其中每个物品按 PropItemData.ItemType 实例化为对应的 PropItemInfo 子类
    ///    （如 ItemType.Weapon -> WeaponItemInfo）；
    /// 2. 为 RoleRuntimeModel 中每个角色实例补齐背包，保证运行时 id 与背包一一对应；
    ///    新补齐的背包使用 defaultCapacity，heldIndex 保持 -1（表示未手持任何物品）。
    /// 系统初始化时由 OnInit 调用；需要重新载入存档时也可手动调用。
    /// </summary>
    public void InitPackageModel()
    {
        var save = storage.Load<PackageSaveData>("Package");

        // 背包字典按存档重新构建（由 PackageSystem 初始化时使用）
        packageModel.ClearPackages();

        if (save?.rolePackages != null)
        {
            foreach (var data in save.rolePackages)
            {
                if (data == null) continue;
                packageModel.AddPackage(ToRolePackageInfo(data));
            }
        }

        // 为每个角色运行时实例补齐背包,保证 id 与背包一一对应；
        // 新补齐的背包容量非法（< 1）时回退默认容量
        foreach (var info in roleRuntimeModel.GetAllRoleRuntimes())
        {
            var package = packageModel.GetOrCreatePackage(info.runtimeIndex);
            if (package.capacity.Value < 1)
            {
                package.capacity.Value = defaultCapacity;
            }
        }
    }

    /// <summary>
    /// 从 PackageModel 中读取所有角色的背包数据并保存（新格式，不再写出旧版全局字段）。
    /// </summary>
    public void SavePackage()
    {
        var save = new PackageSaveData
        {
            rolePackages = new List<RolePackageData>(),
        };

        foreach (var package in packageModel.GetAllPackages())
        {
            save.rolePackages.Add(ToRolePackageData(package));
        }

        storage.Save(save, "Package");
    }

    /// <summary>
    /// 依据 ItemType + configId 生成一件道具并加入指定角色的背包（仅内存，不落盘）。
    /// 流程：校验角色存在 -> 依据类型从对应 ConfigProvider 校验并构造持久化数据
    ///        -> PropItemMapper 实例化运行时子类 -> 校验背包未满 -> 分配包内实例 id 后加入。
    /// 新增道具类型时在此按 ItemType 扩展构造逻辑，并同步 PropItemMapper 登记。
    /// </summary>
    public bool AddItemToRolePackage(int roleRuntimeId, ItemType itemType, int configId)
    {
        if (!roleRuntimeModel.TryGetRoleRuntime(roleRuntimeId, out _))
        {
            Debug.LogWarning($"[PackageSystem] 角色运行时不存在 roleRuntimeId={roleRuntimeId}，已忽略");
            return false;
        }

        var data = CreateItemData(itemType, configId);
        if (data == null) return false;

        var item = PropItemMapper.ToPropItemInfo(data);
        if (item == null) return false;

        var package = packageModel.GetOrCreatePackage(roleRuntimeId);
        if (package.capacity.Value >= 0 && package.packageItems.Count >= package.capacity.Value)
        {
            Debug.LogWarning($"[PackageSystem] 角色 {roleRuntimeId} 背包已满（{package.capacity.Value}），无法加入道具");
            return false;
        }

        item.index = AllocatePackageIndex(package.packageItems);
        package.packageItems.Add(item);

        Debug.Log($"[PackageSystem] 已将 {itemType}(configId={configId}) 加入角色 {roleRuntimeId} 的背包（未保存）");
        return true;
    }

    /// <summary>
    /// 依据 ItemType 构造对应道具的持久化数据；未知类型或 configId 无法解析时打印警告并返回 null。
    /// 武器耐久取 WeaponConfig 的满耐久。
    /// </summary>
    private PropItemData CreateItemData(ItemType itemType, int configId)
    {
        switch (itemType)
        {
            case ItemType.Weapon:
                var weaponConfig = this.GetUtility<IWeaponConfigProvider>().GetWeaponConfig(configId);
                if (weaponConfig == null)
                {
                    Debug.LogWarning($"[PackageSystem] 未知的武器 configId={configId}，已忽略");
                    return null;
                }
                return new PropItemData
                {
                    configId = configId,
                    type = ItemType.Weapon,
                    num = 1,
                    durability = weaponConfig.durability,
                };
            default:
                Debug.LogWarning($"[PackageSystem] 暂不支持的道具类型 {itemType}");
                return null;
        }
    }

    /// <summary>
    /// 分配背包内新的实例 id：当前最大 index 的后继（空背包从 0 开始），保证包内唯一。
    /// </summary>
    private static int AllocatePackageIndex(List<PropItemInfo> packageItems)
    {
        int maxIndex = -1;
        foreach (var item in packageItems)
        {
            if (item != null && item.index > maxIndex)
            {
                maxIndex = item.index;
            }
        }
        return maxIndex + 1;
    }

    /// <summary>
    /// 在场景中创建空物体并挂载 PackageController，使其监听全局快捷键。
    /// 已有有效实例（未被销毁）时忽略，保证幂等。
    /// </summary>
    public void AddPackageListener()
    {
        if (packageListenerGO != null)
        {
            return;
        }

        packageListenerGO = new GameObject("PackageController");
        packageListenerGO.AddComponent<PackageController>();
    }

    /// <summary>
    /// 销毁 AddPackageListener 创建的空物体与 PackageController。
    /// </summary>
    public void RemovePackageListener()
    {
        if (packageListenerGO == null)
        {
            return;
        }

        Object.Destroy(packageListenerGO);
        packageListenerGO = null;
    }

    /// <summary>
    /// 将单个角色的持久化背包数据转换为运行时信息并写入模型。
    /// capacity < 1 视为非法，回退默认容量。
    /// </summary>
    private RolePackageInfo ToRolePackageInfo(RolePackageData data)
    {
        var info = new RolePackageInfo
        {
            roleRuntimeId = data.roleRuntimeId,
        };

        if (data.packageItems != null)
        {
            foreach (var item in data.packageItems)
            {
                if (item == null) continue;

                // 依据持久化的 ItemType 实例化对应的 PropItemInfo 子类，未知类型直接跳过
                var prop = PropItemMapper.ToPropItemInfo(item);
                if (prop != null)
                {
                    info.packageItems.Add(prop);
                }
            }
        }

        info.capacity.Value = data.capacity < 1 ? defaultCapacity : data.capacity;
        info.heldIndex.Value = data.heldIndex;
        return info;
    }

    /// <summary>
    /// 将模型中单个角色的背包运行时信息转换为持久化数据。
    /// </summary>
    private RolePackageData ToRolePackageData(RolePackageInfo info)
    {
        var data = new RolePackageData
        {
            roleRuntimeId = info.roleRuntimeId,
            packageItems = new List<PropItemData>(),
            capacity = info.capacity.Value,
            heldIndex = info.heldIndex.Value,
        };

        if (info.packageItems != null)
        {
            foreach (var item in info.packageItems)
            {
                if (item == null) continue;
                data.packageItems.Add(PropItemMapper.ToPropItemData(item));
            }
        }

        return data;
    }
}
