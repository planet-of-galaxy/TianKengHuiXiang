/// <summary>持续挖矿，由外部逻辑显式切换到其他状态。</summary>
public class PlayerMineState : StateBase<PlayerStateMachine>
{
    public override void OnEnter()
    {
        Owner.RoleAnimatorController?.SetRunning(false);
        Owner.RoleAnimatorController?.PlayMoveAnimation(0f, 0f);
        Owner.RoleAnimatorController?.SetMining(true);
    }

    public override void OnExit()
    {
        Owner.RoleAnimatorController?.SetMining(false);
    }
}
