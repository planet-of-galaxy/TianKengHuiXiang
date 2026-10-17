using UnityEngine;

/// <summary>武器攻击基类，提供攻击生命周期与伤害交付。</summary>
[RequireComponent(typeof(WeaponContext))]
public abstract class WeaponAttackBase : MonoBehaviour, IAttack
{
    protected WeaponContext weaponContext;

    protected virtual void Awake()
    {
        weaponContext = GetComponent<WeaponContext>();
    }

    public abstract void StartAttack(AttackType attackType);
    public abstract void DoAttackHitFrame();
    public abstract void DoAttackInterrupt();
    public abstract void EndAttack();

    /// <summary>向目标或其父节点上的受伤组件交付伤害。</summary>
    protected static bool TryDeliverDamage(GameObject target, DamageInfo damageInfo)
    {
        if (target == null)
        {
            return false;
        }

        var receiver = target.GetComponentInParent<IGetDamage>();
        if (receiver == null || receiver.IsDisabled)
        {
            return false;
        }

        receiver.GetDamage(damageInfo);
        return true;
    }
}
