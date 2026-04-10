using System;
using QFramework;
using UnityEngine;

public abstract class GameProcedureStateBase : StateBase<GameProcedureSystem>
{
    protected IArchitecture Architecture => Owner.GetArchitecture();
}

public interface IGameProcedureSystem : ISystem
{
    void ChangeState<TState>() where TState : GameProcedureStateBase;
    void RevertState();
    bool IsInState<TState>() where TState : GameProcedureStateBase;
    Type CurrentStateType { get; }
}

public class GameProcedureSystem : AbstractSystem, IGameProcedureSystem
{
    private StateMachine<GameProcedureSystem> _fsm;
    private GameProcedureUpdater _updater;

    public Type CurrentStateType => _fsm.CurrentState?.GetType();

    protected override void OnInit()
    {
        _fsm = new StateMachine<GameProcedureSystem>(this);

        // 注册流程状态
        AddState(new StartState());

        // 创建 MonoBehaviour 驱动器
        var go = new GameObject("[GameProcedureUpdater]");
        UnityEngine.Object.DontDestroyOnLoad(go);
        _updater = go.AddComponent<GameProcedureUpdater>();
        _updater.Init(this);
    }

    protected override void OnDeinit()
    {
        if (_updater != null)
        {
            UnityEngine.Object.Destroy(_updater.gameObject);
            _updater = null;
        }
    }

    public void AddState<TState>(TState state) where TState : GameProcedureStateBase
    {
        _fsm.AddState(state);
    }

    public void StartProcedure<TState>() where TState : GameProcedureStateBase
    {
        _fsm.Start<TState>();
        Debug.Log($"[GameProcedure] 启动流程: {typeof(TState).Name}");
    }

    public void ChangeState<TState>() where TState : GameProcedureStateBase
    {
        Debug.Log($"[GameProcedure] 切换流程: {_fsm.CurrentState?.GetType().Name} → {typeof(TState).Name}");
        _fsm.ChangeState<TState>();
    }

    public void RevertState()
    {
        Debug.Log($"[GameProcedure] 回退流程: {_fsm.CurrentState?.GetType().Name}");
        _fsm.RevertState();
    }

    public bool IsInState<TState>() where TState : GameProcedureStateBase
    {
        return _fsm.IsInState<TState>();
    }

    internal void Update(float deltaTime)
    {
        _fsm.Update(deltaTime);
    }

    internal void FixedUpdate(float fixedDeltaTime)
    {
        _fsm.FixedUpdate(fixedDeltaTime);
    }

    public IArchitecture GetArchitecture()
    {
        return ((IBelongToArchitecture)this).GetArchitecture();
    }
}

public class GameProcedureUpdater : MonoBehaviour
{
    private GameProcedureSystem _system;

    public void Init(GameProcedureSystem system)
    {
        _system = system;
    }

    private void Update()
    {
        _system?.Update(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        _system?.FixedUpdate(Time.fixedDeltaTime);
    }
}
