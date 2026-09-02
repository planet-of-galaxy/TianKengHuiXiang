using QFramework;
using UnityEngine;

/// <summary>
/// 武器攻击基类：挂在武器物体上。
/// 监听 <see cref="PlayerAttackEvent"/>（由 AttackListener 在玩家左键单击时发出），
/// 事件发生时执行一次 <see cref="AttackA"/>。
/// </summary>
public abstract class WeaponAttackBase : MonoBehaviour, IAttackA, IController
{
    private IUnRegister _playerAttackUnRegister;

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    private void OnEnable()
    {
        _playerAttackUnRegister = this.RegisterEvent<PlayerAttackEvent>(OnPlayerAttackEvent);
    }

    private void OnDisable()
    {
        _playerAttackUnRegister?.UnRegister();
        _playerAttackUnRegister = null;
    }

    private void OnPlayerAttackEvent(PlayerAttackEvent e)
    {
        AttackA();
    }

    /// <summary>
    /// 执行一次 A 型攻击，返回本次造成的伤害信息。
    /// 具体命中判定与伤害结算由子类实现；命中后可调用 <see cref="TryDeliverDamage"/> 把伤害交给目标。
    /// </summary>
    public abstract DamageInfo AttackA();

    /// <summary>
    /// 把伤害结算给命中物体：当该物体自身或其任一父节点实现 <see cref="IGetDamage"/> 时，
    /// 调用其 GetDamage 接收本次伤害。返回是否成功结算。
    /// </summary>
    protected static bool TryDeliverDamage(GameObject target, DamageInfo damageInfo)
    {
        if (target == null)
        {
            return false;
        }

        var receiver = target.GetComponentInParent<IGetDamage>();
        if (receiver == null)
        {
            return false;
        }

        receiver.GetDamage(damageInfo);
        return true;
    }
}
