public class MonsterDeathState : StateBase<MonsterStateMachine>
{
    private readonly DamageReceiverBase damageReceiver;

    public MonsterDeathState(DamageReceiverBase damageReceiver)
    {
        this.damageReceiver = damageReceiver;
    }

    public override bool CanExit => false;

    public override void OnEnter()
    {
        damageReceiver.IsDisabled = true;
        Owner.MonsterAnimatorController?.PlayDeathAnimation();
    }
}
