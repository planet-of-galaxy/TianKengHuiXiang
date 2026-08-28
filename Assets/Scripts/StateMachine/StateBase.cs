/// <summary>
/// 状态基类，TOwner 为状态机持有者类型
/// </summary>
public abstract class StateBase<TOwner> where TOwner : class
{
    public StateMachine<TOwner> StateMachine { get; internal set; }
    public TOwner Owner => StateMachine.Owner;

    /// <summary>
    /// 当前激活的子状态机（无子状态时为 null），供外层状态机进行层级查询
    /// </summary>
    internal virtual StateMachine<TOwner> ActiveSubStateMachine => null;

    /// <summary>
    /// 进入状态时调用
    /// </summary>
    public virtual void OnEnter() { }

    /// <summary>
    /// 每帧更新
    /// </summary>
    public virtual void OnUpdate(float deltaTime) { }

    /// <summary>
    /// 固定时间步更新（物理）
    /// </summary>
    public virtual void OnFixedUpdate(float fixedDeltaTime) { }

    /// <summary>
    /// 退出状态时调用
    /// </summary>
    public virtual void OnExit() { }
}
