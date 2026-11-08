using UnityEngine;

public class MonsterDeathState : StateBase<MonsterStateMachine>
{
    private readonly DamageReceiverBase damageReceiver;
    private Collider monsterCollider;
    private bool colliderWasEnabled;

    public MonsterDeathState(DamageReceiverBase damageReceiver)
    {
        this.damageReceiver = damageReceiver;
    }

    public override bool CanExit => false;

    public override void OnEnter()
    {
        damageReceiver.IsDisabled = true;
        monsterCollider = Owner.GetComponent<Collider>();
        if (monsterCollider != null)
        {
            colliderWasEnabled = monsterCollider.enabled;
            monsterCollider.enabled = false;
        }
        Owner.MonsterAnimatorController?.PlayDeathAnimation();
    }

    public override void OnExit()
    {
        if (monsterCollider != null) monsterCollider.enabled = colliderWasEnabled;
    }
}
