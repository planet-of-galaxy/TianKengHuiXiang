using UnityEngine;

/// <summary>
/// 全局快捷键工具类：集中记录游戏核心操作的按键映射，
/// 供 InteractableBaseA / PackageListener / WareHouseListener 等统一读取。
/// 如需调整按键，只需改动此处，避免硬编码散落各处。
/// </summary>
public static class HotKeyUtility
{
    /// <summary>跑步（左 Shift）</summary>
    public static readonly KeyCode Run = KeyCode.LeftShift;

    /// <summary>跑步（右 Shift）</summary>
    public static readonly KeyCode RunAlternate = KeyCode.RightShift;

    /// <summary>交互A</summary>
    public static readonly KeyCode InteractA = KeyCode.E;

    /// <summary>打开背包</summary>
    public static readonly KeyCode OpenPackage = KeyCode.Q;
}
