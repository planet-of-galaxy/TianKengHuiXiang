using UnityEngine;

/// <summary>
/// 攻击输入监听：挂在具有 IAttack 实现组件的物体上，
/// 持有该组件，左键短按松开时调用轻击，长按达到阈值时调用一次重击。
/// </summary>
public class AttackListener : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float heavyAttackHoldTime = 0.5f;

    private IAttack _attack;
    private float _pressTime;
    private bool _isPressing;
    private bool _heavyAttackTriggered;

    private void Awake()
    {
        _attack = GetComponent<IAttack>();
        if (_attack == null)
        {
            Debug.LogError("AttackListener 必须挂载在具有 IAttack 实现组件的物体上。", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _pressTime = Time.time;
            _isPressing = true;
            _heavyAttackTriggered = false;
        }

        if (!_isPressing)
        {
            return;
        }

        if (!_heavyAttackTriggered && Time.time - _pressTime >= heavyAttackHoldTime)
        {
            _heavyAttackTriggered = true;
            _attack?.IHeavyAttack();
        }

        if (Input.GetMouseButtonUp(0))
        {
            bool shouldLightAttack = !_heavyAttackTriggered;
            ResetPress();
            if (shouldLightAttack)
            {
                _attack?.ILightAttack();
            }
        }
    }

    private void OnDisable()
    {
        ResetPress();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            ResetPress();
        }
    }

    private void ResetPress()
    {
        _isPressing = false;
        _heavyAttackTriggered = false;
    }
}
