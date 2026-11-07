using UnityEngine;

/// <summary>
/// 持有并驱动玩家状态机。
/// </summary>
[RequireComponent(typeof(RoleContext))]
public class PlayerStateMachine : MonoBehaviour
{
    private StateMachine<PlayerStateMachine> _stateMachine;
    private RoleContext roleContext;

    public RoleAnimatorController RoleAnimatorController
    {
        get
        {
            if (roleContext == null)
                roleContext = GetComponent<RoleContext>();

            return roleContext.RoleAnimatorController;
        }
    }

    public StateMachine<PlayerStateMachine> StateMachine
    {
        get
        {
            if (_stateMachine == null)
            {
                _stateMachine = new StateMachine<PlayerStateMachine>(this);
                RegisterState(new PlayerDefualtState());
                RegisterState(new PlayerMineState());
            }

            return _stateMachine;
        }
    }

    private void Awake()
    {
        // 延迟初始化也允许其他组件在 Awake 中提前注册或切换状态。
        if (StateMachine.CurrentState == null)
            StateMachine.Start<PlayerDefualtState>();
    }

    public void RegisterState<T>(T state) where T : StateBase<PlayerStateMachine>
    {
        StateMachine.AddState(state);
    }

    public void ChangeState<T>() where T : StateBase<PlayerStateMachine>
    {
        StateMachine.ChangeState<T>();
    }

    public void UnRegisterState<T>() where T : StateBase<PlayerStateMachine>
    {
        _stateMachine?.RemoveState<T>();
    }

    public void ChangeState<T>(float parameter) where T : StateBase<PlayerStateMachine>
    {
        StateMachine.ChangeState<T>(parameter);
    }

    public void ChangeState<T>(MineContext context)
        where T : StateBase<PlayerStateMachine>, IStateWithParameter<MineContext>
    {
        if (context == null) throw new System.ArgumentNullException(nameof(context));
        StateMachine.ChangeState<T, MineContext>(context);
    }

    private void Update()
    {
        StateMachine.Update(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        StateMachine.FixedUpdate(Time.fixedDeltaTime);
    }
}
