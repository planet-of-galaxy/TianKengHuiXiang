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
    private AttackListener attackListener;
    private IWeaponInstanceSystem weaponInstanceSystem;
    private WeaponContext weaponContext;
    private IAttack attack;

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    private void Awake()
    {
        roleContext = GetComponent<RoleContext>();
        attackListener = GetComponent<AttackListener>();
        if (attackListener == null)
        {
            attackListener = gameObject.AddComponent<AttackListener>();
        }

        weaponInstanceSystem = this.GetSystem<IWeaponInstanceSystem>();
        RefreshWeapon();
    }

    private void OnEnable()
    {
        attackListener.OnLightAttackKeyPressed += DoLightAttack;
        attackListener.OnHeavyAttackKeyPressed += DoHeavyAttack;
        weaponInstanceSystem.OnWeaponChanged += HandleWeaponChanged;
        LogDebug("已订阅攻击输入和武器切换事件。");
        RefreshWeapon();
    }

    private void OnDisable()
    {
        if (attackListener != null)
        {
            attackListener.OnLightAttackKeyPressed -= DoLightAttack;
            attackListener.OnHeavyAttackKeyPressed -= DoHeavyAttack;
        }

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
        weaponContext = weaponInstanceSystem.TryGetWeapon(roleContext);
        attack = weaponContext != null ? weaponContext.Attack : null;
        LogDebug($"更新武器引用：{(weaponContext != null ? weaponContext.name : "未获取到武器")}，IAttack：{(attack != null ? attack.GetType().Name : "未找到")}。");
    }

    public void DoLightAttack()
    {
        if (weaponContext == null || attack == null)
        {
            LogDebug("跳过轻攻击：当前武器或 IAttack 不存在。");
            return;
        }

        attack.ILightAttack();
    }

    public void DoHeavyAttack()
    {
        if (weaponContext == null || attack == null)
        {
            LogDebug("跳过重攻击：当前武器或 IAttack 不存在。");
            return;
        }

        attack.IHeavyAttack();
    }

    /// <summary>预留攻击打断入口。</summary>
    public void DonInterruptAttack()
    {
        LogDebug("调用 DonInterruptAttack（预留入口，尚未实现）。");
    }

    /// <summary>预留攻击命中帧入口。</summary>
    public void DoAttackHitFrame()
    {
        LogDebug("调用 DoAttackHitFrame（预留入口，尚未实现）。");
    }

    private void LogDebug(string message)
    {
        if (!enableDebugLog) return;

        var roleId = roleContext != null ? roleContext.RoleRuntimeIndex.ToString() : "未知";
        var weaponId = weaponContext != null ? weaponContext.WeaponConfigId.ToString() : "无引用";
        Debug.Log($"[AttackController] 角色={roleId} 武器={weaponId} | {message}", this);
    }
}
