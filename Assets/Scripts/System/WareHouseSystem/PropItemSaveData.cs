using System.Collections.Generic;

/// <summary>
/// 仓库道具的持久化容器（存档 key 为 "WareHouse"，由 WareHouseSystem 读写）。
/// 列表项 PropItemData.index 存放仓库实例 id，与运行时字典 key 一一对应。
/// </summary>
public class PropItemSaveData
{
    public List<PropItemData> propItems;
}
