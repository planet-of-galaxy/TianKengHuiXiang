using UnityEngine;

/// <summary>
/// 攻击输入监听：挂在具有 IAttackA 实现组件的物体上，
/// 持有该组件，并在鼠标左键单击时调用攻击。
/// </summary>
public class AttackListener : MonoBehaviour
{
    private IAttackA _attack;

    private void Awake()
    {
        _attack = GetComponent<IAttackA>();
        if (_attack == null)
        {
            Debug.LogError("AttackListener 必须挂载在具有 IAttackA 实现组件的物体上。", this);
            enabled = false;
        }
    }

    private void Update()
    {
        // 左键单击（按下的那一帧触发一次）
        if (Input.GetMouseButtonDown(0))
        {
            _attack?.AttackA();
        }
    }
}
