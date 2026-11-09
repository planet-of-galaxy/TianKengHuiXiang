/// <summary>单个道具（非武器类）的配置，对应 ItemConfig.json 中的一条记录。</summary>
[System.Serializable]
public class ItemConfig
{
    public int itemId;
    public string name;

    /// <summary>
    /// 道具图标的 Resources 路径（不含扩展名），如 "UI/PropItem/Item/icon_gold_mine"。
    /// 留空表示该道具没有图标，UI 不显示图标。
    /// </summary>
    public string icon;

    /// <summary>单个背包格允许堆叠的最大数量，1 表示不可堆叠。</summary>
    public int maxStack;
}
