using System;
using UnityEngine;

/// <summary>
/// 攻击输入监听：左键短按松开时触发轻击回调，右键长按达到阈值时触发一次重击回调。
/// </summary>
[DisallowMultipleComponent]
public class AttackListener : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float heavyAttackHoldTime = 0.5f;

    public event Action OnLightAttackKeyPressed;
    public event Action OnHeavyAttackKeyPressed;

    private float _lightPressTime;
    private bool _isLightPressing;
    private float _heavyPressTime;
    private bool _isHeavyPressing;
    private bool _heavyAttackTriggered;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _lightPressTime = Time.time;
            _isLightPressing = true;
        }

        if (Input.GetMouseButtonUp(0) && _isLightPressing)
        {
            _isLightPressing = false;
            if (Time.time - _lightPressTime < heavyAttackHoldTime)
            {
                OnLightAttackKeyPressed?.Invoke();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            _heavyPressTime = Time.time;
            _isHeavyPressing = true;
            _heavyAttackTriggered = false;
        }

        if (!_isHeavyPressing)
        {
            return;
        }

        if (!_heavyAttackTriggered && Time.time - _heavyPressTime >= heavyAttackHoldTime)
        {
            _heavyAttackTriggered = true;
            OnHeavyAttackKeyPressed?.Invoke();
        }

        if (Input.GetMouseButtonUp(1))
        {
            _isHeavyPressing = false;
            _heavyAttackTriggered = false;
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
        _isLightPressing = false;
        _isHeavyPressing = false;
        _heavyAttackTriggered = false;
    }
}
