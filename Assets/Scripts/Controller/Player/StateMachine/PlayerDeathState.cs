public class PlayerDeathState : StateBase<PlayerStateMachine>
{
    private readonly DamageReceiverBase damageReceiver;

    public PlayerDeathState(DamageReceiverBase damageReceiver)
    {
        this.damageReceiver = damageReceiver;
    }

    public override bool CanExit => false;

    public override void OnEnter()
    {
        damageReceiver.IsDisabled = true;
        Owner.RoleAnimatorController?.PlayDeathAnimation();
    }
}
