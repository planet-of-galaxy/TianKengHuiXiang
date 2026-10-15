using QFramework;
using UnityEngine;

public class PlayerStunnedState : StateBase<PlayerStateMachine>, IController
{
    private float remainingTime;
    private IGamePauseSystem pauseSystem;

    public override bool CanExit => remainingTime <= 0f;

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    public override void OnEnter(float duration)
    {
        remainingTime = Mathf.Max(0f, duration);
        pauseSystem = this.GetSystem<IGamePauseSystem>();
        Owner.RoleAnimatorController?.PlayHitAnimation();
    }

    public override void OnUpdate(float deltaTime)
    {
        if (pauseSystem != null && pauseSystem.IsPaused) return;

        remainingTime -= deltaTime;
        if (!CanExit) return;

        if (StateMachine.HasState<PlayerMoveState>())
            Owner.ChangeState<PlayerMoveState>();
        else
            Owner.ChangeState<PlayerDefualtState>();
    }

    public override void OnExit()
    {
        remainingTime = 0f;
    }
}
