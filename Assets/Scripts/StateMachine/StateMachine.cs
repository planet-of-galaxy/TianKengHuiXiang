using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 泛型有限状态机，TOwner 为持有者类型（如角色控制器）
/// </summary>
public class StateMachine<TOwner> where TOwner : class
{
    public TOwner Owner { get; private set; }
    public StateBase<TOwner> CurrentState { get; private set; }
    public StateBase<TOwner> PreviousState { get; private set; }

    private readonly Dictionary<Type, StateBase<TOwner>> _states = new();

    public StateMachine(TOwner owner)
    {
        Owner = owner;
    }

    /// <summary>
    /// 注册一个状态实例
    /// </summary>
    public void AddState<TState>(TState state) where TState : StateBase<TOwner>
    
    {
        var type = typeof(TState);
        if (_states.ContainsKey(type))
        {
            Debug.LogWarning($"[StateMachine] 状态 {type.Name} 已存在，将被覆盖");
        }

        state.StateMachine = this;
        _states[type] = state;
    }

    /// <summary>
    /// 设置初始状态（不触发 OnExit）
    /// </summary>
    public void Start<TState>() where TState : StateBase<TOwner>
    {
        CurrentState = GetState<TState>();
        CurrentState.OnEnter();
    }

    /// <summary>
    /// 切换到目标状态
    /// </summary>
    public void ChangeState<TState>() where TState : StateBase<TOwner>
    {
        var next = GetState<TState>();
        if (next == CurrentState) return;

        PreviousState = CurrentState;
        CurrentState?.OnExit();
        CurrentState = next;
        CurrentState.OnEnter();
    }

    public void Update(float deltaTime)
    {
        CurrentState?.OnUpdate(deltaTime);
    }

    public void FixedUpdate(float fixedDeltaTime)
    {
        CurrentState?.OnFixedUpdate(fixedDeltaTime);
    }

    /// <summary>
    /// 回到上一个状态
    /// </summary>
    public void RevertState()
    {
        if (PreviousState == null)
        {
            Debug.LogWarning("[StateMachine] 没有可回退的状态");
            return;
        }

        var prev = PreviousState;
        PreviousState = CurrentState;
        CurrentState?.OnExit();
        CurrentState = prev;
        CurrentState.OnEnter();
    }

    /// <summary>
    /// 当前是否处于指定状态
    /// </summary>
    public bool IsInState<TState>() where TState : StateBase<TOwner>
    {
        return CurrentState is TState;
    }

    private StateBase<TOwner> GetState<TState>() where TState : StateBase<TOwner>
    {
        if (_states.TryGetValue(typeof(TState), out var state))
            return state;

        throw new KeyNotFoundException($"[StateMachine] 未注册状态: {typeof(TState).Name}，请先调用 AddState");
    }
}
