public class MonsterDeathState : StateBase<MonsterStateMachine>
{
    public override bool CanExit => false;

    public override void OnEnter()
    {
        Owner.MonsterAnimatorController?.PlayDeathAnimation();
    }
}
