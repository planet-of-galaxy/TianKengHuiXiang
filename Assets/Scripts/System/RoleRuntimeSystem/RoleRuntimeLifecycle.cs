using System;
using UnityEngine;

/// <summary>
/// 挂在角色运行时实例上，实例被销毁时通知 RoleRuntimeSystem 置空引用。
/// </summary>
public class RoleRuntimeLifecycle : MonoBehaviour
{
    private Action<GameObject> onDestroyed;

    public void Init(Action<GameObject> callback)
    {
        onDestroyed = callback;
    }

    private void OnDestroy()
    {
        onDestroyed?.Invoke(gameObject);
    }
}
