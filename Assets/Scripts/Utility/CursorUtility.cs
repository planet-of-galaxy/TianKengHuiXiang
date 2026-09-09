using UnityEngine;

/// <summary>
/// 鼠标光标工具类：集中封装对 Cursor 的加锁、解锁与显隐控制，
/// 供 PlayerMoveController 等统一调用，避免 Cursor.lockState / visible 硬编码散落各处。
/// </summary>
public static class CursorUtility
{
    /// <summary>光标是否可见</summary>
    public static bool IsVisible => Cursor.visible;

    /// <summary>光标是否处于锁定状态</summary>
    public static bool IsLocked => Cursor.lockState == CursorLockMode.Locked;

    /// <summary>锁定并隐藏光标（第一人称视角常用）</summary>
    public static void Lock()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>解锁光标（Lock 的反向操作）</summary>
    public static void UnLock()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>设置光标是否可见</summary>
    public static void SetVisible(bool visible)
    {
        Cursor.visible = visible;
    }

    /// <summary>
    /// 显示并解锁光标：打开背包/仓库等需要鼠标操作 UI 的面板时调用。
    /// 退出时应通过打开时 Capture 的快照 Restore() 还原为进入前状态。
    /// </summary>
    public static void ShowAndUnlock()
    {
        UnLock();
        SetVisible(true);
    }

    /// <summary>暂存当前光标状态，供打开面板前调用；退出面板时对快照调用 Restore() 还原。</summary>
    public static CursorSnapshot Capture()
    {
        return new CursorSnapshot(IsVisible, IsLocked);
    }

    /// <summary>
    /// 光标状态快照：记录打开面板前的可见性/锁定状态，用于退出时还原。
    /// 锁定状态下光标必然隐藏，因此 wasLocked 时只需重新锁定，无需再处理可见性。
    /// </summary>
    public struct CursorSnapshot
    {
        private readonly bool wasVisible;
        private readonly bool wasLocked;

        internal CursorSnapshot(bool visible, bool locked)
        {
            wasVisible = visible;
            wasLocked = locked;
        }

        /// <summary>还原为记录时的光标状态。</summary>
        public void Restore()
        {
            if (wasLocked)
            {
                Lock();
            }
            else
            {
                SetVisible(wasVisible);
            }
        }
    }
}
