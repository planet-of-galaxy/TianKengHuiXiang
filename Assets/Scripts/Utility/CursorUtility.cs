using UnityEngine;

/// <summary>
/// 按引用计数管理光标：加锁与隐藏绑定，解锁与显示绑定，解锁显示优先。
/// 每次请求必须配对释放；没有请求时默认解锁显示。
/// </summary>
public static class CursorUtility
{
    private static int lockRequestCount;
    private static int showRequestCount;

    public static bool IsVisible => Cursor.visible;
    public static bool IsLocked => Cursor.lockState == CursorLockMode.Locked;

    /// <summary>申请加锁隐藏，与 ReleaseLock 配对；显示请求清零后才生效。</summary>
    public static void Lock()
    {
        lockRequestCount++;
        ApplyState();
    }

    /// <summary>释放一次加锁隐藏请求，忽略多余的释放。</summary>
    public static void ReleaseLock()
    {
        if (lockRequestCount == 0) return;
        lockRequestCount--;
        ApplyState();
    }

    /// <summary>申请解锁显示，与 ReleaseShowAndUnlock 配对，优先于所有锁定请求。</summary>
    public static void ShowAndUnlock()
    {
        showRequestCount++;
        ApplyState();
    }

    /// <summary>释放一次解锁显示请求，忽略多余的释放。</summary>
    public static void ReleaseShowAndUnlock()
    {
        if (showRequestCount == 0) return;
        showRequestCount--;
        ApplyState();
    }

    // 禁用 Domain Reload 时，进入运行模式也需要清空静态引用计数。
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        lockRequestCount = 0;
        showRequestCount = 0;
        ApplyState();
    }

    private static void ApplyState()
    {
        bool locked = lockRequestCount > 0 && showRequestCount == 0;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
