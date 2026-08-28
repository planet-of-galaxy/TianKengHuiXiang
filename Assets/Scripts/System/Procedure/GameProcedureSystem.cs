using System;
using QFramework;
using UnityEngine;

public abstract class GameProcedureStateBase : StateBase<GameProcedureSystem>, IController
{
	public IArchitecture GetArchitecture()
	{
		return ((IBelongToArchitecture)Owner).GetArchitecture();
	}

}

/// <summary>
/// 流程复合状态基类：需要携带子状态机的流程状态（如 PrepareState）继承此类；
/// 其子状态（如 RoleSelectState / RoleControlState）继承 GameProcedureStateBase
/// </summary>
public abstract class GameProcedureCompositeStateBase : CompositeStateBase<GameProcedureSystem>, IController
{
	public IArchitecture GetArchitecture()
	{
		return ((IBelongToArchitecture)Owner).GetArchitecture();
	}

}

public interface IGameProcedureSystem : ISystem
{
    void ChangeState<TState>() where TState : StateBase<GameProcedureSystem>;
    void RevertState();
    bool IsInState<TState>() where TState : StateBase<GameProcedureSystem>;
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
        AddState(new PrepareState());
        AddState(new MineCaveState());
        AddState(new HouseState());
        AddState(new TestState());

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

    public void AddState<TState>(TState state) where TState : StateBase<GameProcedureSystem>
    {
        _fsm.AddState(state);
    }

    public void StartProcedure<TState>() where TState : StateBase<GameProcedureSystem>
    {
        _fsm.Start<TState>();
        Debug.Log($"[GameProcedure] 启动流程: {typeof(TState).Name}");
    }

    public void ChangeState<TState>() where TState : StateBase<GameProcedureSystem>
    {
        Debug.Log($"[GameProcedure] 切换流程: {_fsm.CurrentState?.GetType().Name} → {typeof(TState).Name}");
        _fsm.ChangeState<TState>();
    }

    public void RevertState()
    {
        Debug.Log($"[GameProcedure] 回退流程: {_fsm.CurrentState?.GetType().Name}");
        _fsm.RevertState();
    }

    public bool IsInState<TState>() where TState : StateBase<GameProcedureSystem>
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
