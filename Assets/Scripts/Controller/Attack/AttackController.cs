using QFramework;
using UnityEngine;

/// <summary>将角色攻击输入转发给当前武器，并在武器切换后更新引用。</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RoleContext))]
public class AttackController : MonoBehaviour, IController
{
    [SerializeField, Tooltip("输出攻击调用、武器更新及事件订阅日志。")]
    private bool enableDebugLog = true;

    private RoleContext roleContext;
    private IWeaponInstanceSystem weaponInstanceSystem;
    private WeaponContext weaponContext;
    private IAttack attack;
    private RoleAnimatorController roleAnimatorController;

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    private void Awake()
    {
        roleContext = GetComponent<RoleContext>();
        weaponInstanceSystem = this.GetSystem<IWeaponInstanceSystem>();
        roleAnimatorController = GetComponentInChildren<RoleAnimatorController>(true);
        RefreshWeapon();
    }

    private void OnEnable()
    {
        weaponInstanceSystem.OnWeaponChanged += HandleWeaponChanged;
        if (roleAnimatorController != null)
        {
            roleAnimatorController.OnAttackStart += HandleAttackStart;
            roleAnimatorController.OnAttackHitFrame += DoAttackHitFrame;
            roleAnimatorController.OnAttackEnd += EndAttack;
        }
        else
        {
            Debug.LogWarning("[AttackController] RoleAnimatorController was not found.", this);
        }
        LogDebug("已订阅攻击输入和武器切换事件。");
        RefreshWeapon();
    }

    private void OnDisable()
    {
        if (roleAnimatorController != null)
        {
            roleAnimatorController.OnAttackStart -= HandleAttackStart;
            roleAnimatorController.OnAttackHitFrame -= DoAttackHitFrame;
            roleAnimatorController.OnAttackEnd -= EndAttack;
        }
        DoAttackInterrupt();
        if (weaponInstanceSystem != null)
        {
            weaponInstanceSystem.OnWeaponChanged -= HandleWeaponChanged;
        }
        LogDebug("已退订攻击输入和武器切换事件。");
    }

    private void HandleWeaponChanged(RoleContext changedRoleContext, WeaponConfig newWeaponConfig)
    {
        if (changedRoleContext == roleContext)
        {
            LogDebug($"收到武器切换事件，新配置 ID：{newWeaponConfig?.weaponId}。");
            RefreshWeapon();
        }
    }

    private void RefreshWeapon()
    {
        var nextWeapon = weaponInstanceSystem.TryGetWeapon(roleContext);
        if (weaponContext != nextWeapon)
            DoAttackInterrupt();
        weaponContext = nextWeapon;
        attack = weaponContext != null ? weaponContext.Attack : null;
        LogDebug($"更新武器引用：{(weaponContext != null ? weaponContext.name : "未获取到武器")}，IAttack：{(attack != null ? attack.GetType().Name : "未找到")}。");
    }

    private void HandleAttackStart(AttackType attackType)
    {
        attack?.StartAttack(attackType);
    }

    public void DoAttackInterrupt() => attack?.DoAttackInterrupt();

    public void DoAttackHitFrame() => attack?.DoAttackHitFrame();

    public void EndAttack() => attack?.EndAttack();

    private void LogDebug(string message)
    {
        if (!enableDebugLog) return;

        var roleId = roleContext != null ? roleContext.RoleRuntimeIndex.ToString() : "未知";
        var weaponId = weaponContext != null ? weaponContext.WeaponConfigId.ToString() : "无引用";
        Debug.Log($"[AttackController] 角色={roleId} 武器={weaponId} | {message}", this);
    }
}
