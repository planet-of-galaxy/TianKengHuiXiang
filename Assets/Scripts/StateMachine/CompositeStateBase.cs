using System;

/// <summary>
/// 复合状态基类：作为"容器"状态，自身携带一个子状态机，可把普通状态注册为其子状态（支持多层嵌套）。
/// 如 PrepareState 继承此类，RoleSelectState / RoleControlState 作为其子状态。
/// 生命周期已由模板方法接管，子类通过 OnSubStateEnter / OnSubStateExit 参与。
/// </summary>
public abstract class CompositeStateBase<TOwner> : StateBase<TOwner> where TOwner : class
{
    private StateMachine<TOwner> _subFsm;

    /// <summary>
    /// 当前激活的子状态机（未使用时为 null），供外层状态机进行层级查询
    /// </summary>
    internal override StateMachine<TOwner> ActiveSubStateMachine => _subFsm;

    /// <summary>
    /// 当前激活的子状态
    /// </summary>
    public StateBase<TOwner> CurrentSubState => _subFsm?.CurrentState;

    /// <summary>
    /// 子状态机（首次访问时惰性创建，需在状态注册到 StateMachine 后再访问）
    /// </summary>
    protected StateMachine<TOwner> SubStateMachine
    {
        get
        {
            if (StateMachine == null)
                throw new InvalidOperationException("[SubState] 请先通过 StateMachine.AddState 注册状态，再访问子状态机");
            return _subFsm ??= new StateMachine<TOwner>(Owner);
        }
    }

    /// <summary>
    /// 注册一个子状态
    /// </summary>
    protected void AddSubState<TState>(TState state) where TState : StateBase<TOwner>
    {
        SubStateMachine.AddState(state);
    }

    /// <summary>
    /// 启动初始子状态（不触发 OnExit）
    /// </summary>
    protected void StartSubState<TState>() where TState : StateBase<TOwner>
    {
        SubStateMachine.Start<TState>();
    }

    /// <summary>
    /// 切换到目标子状态
    /// </summary>
    protected void ChangeSubState<TState>() where TState : StateBase<TOwner>
    {
        SubStateMachine.ChangeState<TState>();
    }

    /// <summary>
    /// 当前是否处于指定子状态（含更深层的子状态）
    /// </summary>
    protected bool IsInSubState<TState>() where TState : StateBase<TOwner>
    {
        return _subFsm?.IsInState<TState>() ?? false;
    }

    // ===== 生命周期（模板方法，子类通过下方虚方法参与） =====

    public sealed override void OnEnter()
    {
        OnSubStateEnter();
    }

    public sealed override void OnUpdate(float deltaTime)
    {
        _subFsm?.Update(deltaTime);
    }

    public sealed override void OnFixedUpdate(float fixedDeltaTime)
    {
        _subFsm?.FixedUpdate(fixedDeltaTime);
    }

    public sealed override void OnExit()
    {
        _subFsm?.CurrentState?.OnExit();
        OnSubStateExit();
        _subFsm = null;
    }

    /// <summary>
    /// 进入该状态时调用，在此注册并启动初始子状态
    /// </summary>
    protected virtual void OnSubStateEnter() { }

    /// <summary>
    /// 退出该状态时调用（当前子状态已退出）
    /// </summary>
    protected virtual void OnSubStateExit() { }
}
