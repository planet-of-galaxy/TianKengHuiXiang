/// <summary>由动画事件驱动的攻击生命周期。</summary>
public interface IAttack
{
    void StartAttack(AttackType attackType);
    void DoAttackHitFrame();
    void DoAttackInterrupt();
    void EndAttack();
}
