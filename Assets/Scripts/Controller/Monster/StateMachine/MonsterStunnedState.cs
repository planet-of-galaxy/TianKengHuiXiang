using QFramework;
using UnityEngine;

public class MonsterStunnedState : StateBase<MonsterStateMachine>, IController
{
    private float remainingTime;
    private IGamePauseSystem pauseSystem;
    private MonsterRuntimeInfo runtimeInfo;

    public override bool CanExit => remainingTime <= 0f || (runtimeInfo != null && runtimeInfo.CurHealth.Value <= 0f);

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    public override void OnEnter(float duration)
    {
        remainingTime = Mathf.Max(0f, duration);
        pauseSystem = this.GetSystem<IGamePauseSystem>();
        runtimeInfo = this.GetSystem<IMonsterRuntimeSystem>().GetMonsterRuntime(Owner.GetComponent<MonsterContext>());
        Owner.MonsterAnimatorController?.PlayHitAnimation();
    }

    public override void OnUpdate(float deltaTime)
    {
        if (pauseSystem != null && pauseSystem.IsPaused) return;

        remainingTime -= deltaTime;
        if (!CanExit) return;

        Owner.ChangeState<MonsterDefualtState>();
    }

    public override void OnExit()
    {
        remainingTime = 0f;
        runtimeInfo = null;
    }
}
