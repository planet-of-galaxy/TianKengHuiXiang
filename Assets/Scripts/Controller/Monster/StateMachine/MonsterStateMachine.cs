using UnityEngine;

/// <summary>
/// 持有并驱动怪物状态机。
/// </summary>
[RequireComponent(typeof(MonsterContext))]
public class MonsterStateMachine : MonoBehaviour
{
    private StateMachine<MonsterStateMachine> _stateMachine;
    private MonsterContext monsterContext;

    public MonsterAnimatorController MonsterAnimatorController
    {
        get
        {
            if (monsterContext == null)
                monsterContext = GetComponent<MonsterContext>();

            return monsterContext.MonsterAnimatorController;
        }
    }

    public StateMachine<MonsterStateMachine> StateMachine
    {
        get
        {
            if (_stateMachine == null)
            {
                _stateMachine = new StateMachine<MonsterStateMachine>(this);
                RegisterState(new MonsterDefualtState());
                RegisterState(new MonsterDeathState());
            }

            return _stateMachine;
        }
    }

    private void Awake()
    {
        // 延迟初始化也允许其他组件在 Awake 中提前注册或切换状态。
        if (StateMachine.CurrentState == null)
            StateMachine.Start<MonsterDefualtState>();
    }

    public void RegisterState<T>(T state) where T : StateBase<MonsterStateMachine>
    {
        StateMachine.AddState(state);
    }

    public void ChangeState<T>() where T : StateBase<MonsterStateMachine>
    {
        StateMachine.ChangeState<T>();
    }

    public void UnRegisterState<T>() where T : StateBase<MonsterStateMachine>
    {
        _stateMachine?.RemoveState<T>();
    }

    public void ChangeState<T>(float parameter) where T : StateBase<MonsterStateMachine>
    {
        StateMachine.ChangeState<T>(parameter);
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
