public class PlayerMoveState : StateBase<PlayerStateMachine>
{
    private readonly PlayerMoveController playerMoveController;

    public PlayerMoveState(PlayerMoveController playerMoveController)
    {
        this.playerMoveController = playerMoveController;
    }

    public float Horizontal => playerMoveController.Horizontal;
    public float Vertical => playerMoveController.Vertical;
    public bool IsRunning => playerMoveController.IsRunning;
}
