using QFramework;
using UnityEngine;

public class PlayerStunnedState : StateBase<PlayerStateMachine>, IController
{
    private float remainingTime;
    private IGamePauseSystem pauseSystem;
    private RoleRuntimeInfo runtimeInfo;

    public override bool CanExit => remainingTime <= 0f || (runtimeInfo != null && runtimeInfo.CurHealth.Value <= 0f);

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    public override void OnEnter(float duration)
    {
        remainingTime = Mathf.Max(0f, duration);
        pauseSystem = this.GetSystem<IGamePauseSystem>();
        this.GetModel<IRoleRuntimeModel>().TryGetRoleRuntime(Owner.GetComponent<RoleContext>().RoleRuntimeIndex, out runtimeInfo);
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
        runtimeInfo = null;
    }
}
