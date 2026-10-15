public class PlayerDeathState : StateBase<PlayerStateMachine>
{
    public override bool CanExit => false;

    public override void OnEnter()
    {
        Owner.RoleAnimatorController?.PlayDeathAnimation();
    }
}
