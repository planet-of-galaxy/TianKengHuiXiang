public class PlayerMoveState : StateBase<PlayerStateMachine>
{
    private readonly IMove move;

    public PlayerMoveState(IMove move)
    {
        this.move = move;
    }

    public float Horizontal => move.Horizontal;
    public float Vertical => move.Vertical;
    public bool IsRunning => move.IsRunning;
    public bool IsMoveDisabled => move.IsMoveDisabled;

    public override void OnEnter()
    {
        move.IsMoveDisabled = false;
    }

    public override void OnExit()
    {
        move.IsMoveDisabled = true;
    }
}
