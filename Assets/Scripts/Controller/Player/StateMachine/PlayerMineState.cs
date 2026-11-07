using QFramework;
using UnityEngine;

/// <summary>持续挖矿，检测移动输入后退出并恢复移动。</summary>
public class PlayerMineState : StateBase<PlayerStateMachine>, IStateWithParameter<MineContext>, IController
{
    public MineContext Context { get; private set; }
    internal int LastExitFrame { get; private set; } = -1;

    private int enteredFrame;
    private IGamePauseSystem pauseSystem;

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    public void OnEnter(MineContext context)
    {
        Context = context ?? throw new System.ArgumentNullException(nameof(context));
        OnEnter();
    }

    public override void OnEnter()
    {
        // 回退到挖矿状态时继续使用上次目标。
        if (Context == null)
            throw new System.InvalidOperationException("进入挖矿状态需要 MineContext。");

        enteredFrame = Time.frameCount;
        pauseSystem = this.GetSystem<IGamePauseSystem>();

        var direction = Context.Position - Owner.transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.0001f)
            Owner.transform.rotation = UnityEngine.Quaternion.LookRotation(direction);

        Owner.RoleAnimatorController?.SetRunning(false);
        Owner.RoleAnimatorController?.PlayMoveAnimation(0f, 0f);
        Owner.RoleAnimatorController?.SetMining(true);
    }

    public override void OnExit()
    {
        LastExitFrame = Time.frameCount;
        Owner.RoleAnimatorController?.SetMining(false);
    }

    public override void OnUpdate(float deltaTime)
    {
        // 忽略进入当帧的输入，暂停期间不响应退出。
        if (Time.frameCount == enteredFrame || (pauseSystem != null && pauseSystem.IsPaused)) return;
        // 使用原始输入，避免平滑轴的残余值在松开移动键后取消挖矿。
        var moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (moveInput.sqrMagnitude <= 0.001f) return;

        if (StateMachine.HasState<PlayerMoveState>())
            Owner.ChangeState<PlayerMoveState>();
        else
            Owner.ChangeState<PlayerDefualtState>();
    }
}
