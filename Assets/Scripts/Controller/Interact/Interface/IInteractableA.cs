using UnityEngine;

public interface IInteractableA
{
    /// <summary>触发交互A的快捷键，默认由 HotKeyUtility.InteractA 提供。</summary>
    KeyCode InteractKey { get; }
    void InteractA();
    float GetWeightA();
}