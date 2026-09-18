using System.Collections.Generic;
using QFramework;
using UnityEngine;

public interface IWareHouseSystem : ISystem
{
    /// <summary>
    /// 读取存档，初始化 WareHouseModel；无存档（或存档为空）时得到空仓库。
    /// 每个存档项按其 PropItemData.ItemType 实例化为对应的 PropItemInfo 子类，
    /// 并以 PropItemData.index 作为仓库实例 id 写入模型。
    /// </summary>
    void InitWareHouseModel();

    /// <summary>
    /// 将一把 configId 对应的新武器存入仓库：分配新的实例 id，
    /// 耐久初始化为 WeaponConfig.durability（满耐久）。仅修改内存，需调用 SaveWareHouse() 落盘。
    /// </summary>
    void AddWeapon(int configId);

    /// <summary>
    /// 将仓库中所有道具落盘为 PropItemSaveData（存档 key "WareHouse"）。
    /// </summary>
    void SaveWareHouse();

    /// <summary>
    /// 在场景中创建一个空物体并挂载 WareHouseListener，使其监听全局快捷键。
    /// </summary>
    void AddWareHouseListener();

    /// <summary>
    /// 销毁 AddWareHouseListener 创建的空物体与 WareHouseListener。
    /// </summary>
    void RemoveWareHouseListener();
}

public class WareHouseSystem : AbstractSystem, IWareHouseSystem
{
    /// <summary>
    /// 仓库存档文件名（IJsonStorage 的 Save/Load key）。
    /// </summary>
    private const string SaveFileName = "WareHouse";

    /// <summary>
    /// 仓库运行时数据模型，由 TianArchitecture 注册。
    /// </summary>
    private WareHouseModel wareHouseModel;

    /// <summary>
    /// 武器配置提供者：AddWeapon 时取 configId 对应的满耐久值。
    /// </summary>
    private IWeaponConfigProvider weaponConfigProvider;

    /// <summary>
    /// JSON 持久化工具，用于读写 "WareHouse" 存档。
    /// </summary>
    private IJsonStorage storage;

    /// <summary>
    /// AddWareHouseListener 创建的空物体，用于挂载 WareHouseListener。
    /// 切换场景时该物体可能被 Unity 销毁，此时与 null 相等，AddWareHouseListener 会重新创建。
    /// </summary>
    private GameObject wareHouseListenerGO;

    /// <summary>
    /// 系统初始化：获取模型与工具，并载入存档写入模型。
    /// </summary>
    protected override void OnInit()
    {
        wareHouseModel = (WareHouseModel)this.GetModel<IWareHouseModel>();
        weaponConfigProvider = this.GetUtility<IWeaponConfigProvider>();
        storage = this.GetUtility<IJsonStorage>();

        InitWareHouseModel();
    }

    /// <summary>
    /// 读取存档重建仓库：按 PropItemData.index 恢复各道具实例，实例 id 继续自增分配；
    /// 无法识别的类型（PropItemMapper 返回 null）直接跳过。
    /// </summary>
    public void InitWareHouseModel()
    {
        wareHouseModel.ClearItems();

        var save = storage.Load<PropItemSaveData>(SaveFileName);
        if (save?.propItems == null)
        {
            return;
        }

        foreach (var data in save.propItems)
        {
            if (data == null) continue;

            var item = PropItemMapper.ToPropItemInfo(data);
            if (item != null)
            {
                wareHouseModel.AddItem(item);
            }
        }
    }

    /// <summary>
    /// 校验 configId 后创建一把新武器并存入仓库（实例 id 由模型分配）。
    /// 武器数量固定为单件（num = 1），耐久取配置的满耐久。
    /// </summary>
    public void AddWeapon(int configId)
    {
        var weaponConfig = weaponConfigProvider.GetWeaponConfig(configId);
        if (weaponConfig == null)
        {
            Debug.LogWarning($"[WareHouseSystem] 未知的武器 configId={configId}，已忽略");
            return;
        }

        var data = new PropItemData
        {
            index = wareHouseModel.AllocateInstanceId(),
            configId = configId,
            type = ItemType.Weapon,
            num = 1,
            durability = weaponConfig.durability,
        };

        wareHouseModel.AddItem(PropItemMapper.ToPropItemInfo(data));
    }

    /// <summary>
    /// 将模型中所有仓库道具写回 "WareHouse" 存档。
    /// </summary>
    public void SaveWareHouse()
    {
        var save = new PropItemSaveData
        {
            propItems = new List<PropItemData>(),
        };

        foreach (var item in wareHouseModel.GetAllItems())
        {
            if (item == null) continue;
            save.propItems.Add(PropItemMapper.ToPropItemData(item));
        }

        storage.Save(save, SaveFileName);
    }

    /// <summary>
    /// 在场景中创建空物体并挂载 WareHouseListener，使其监听全局快捷键。
    /// 已有有效实例（未被销毁）时忽略，保证幂等。
    /// </summary>
    public void AddWareHouseListener()
    {
        if (wareHouseListenerGO != null)
        {
            return;
        }

        wareHouseListenerGO = new GameObject("WareHouseListener");
        wareHouseListenerGO.AddComponent<WareHouseListener>();
    }

    /// <summary>
    /// 销毁 AddWareHouseListener 创建的空物体与 WareHouseListener。
    /// </summary>
    public void RemoveWareHouseListener()
    {
        if (wareHouseListenerGO == null)
        {
            return;
        }

        Object.Destroy(wareHouseListenerGO);
        wareHouseListenerGO = null;
    }
}
