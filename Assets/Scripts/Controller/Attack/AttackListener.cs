using QFramework;
using UnityEngine;

/// <summary>Listens for attack input and requests player state transitions.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RoleContext))]
public class AttackListener : MonoBehaviour, IController, IAttackListener
{
    [SerializeField, Min(0.01f)] private float heavyAttackHoldTime = 0.5f;

    public bool IsAttacking => playerStateMachine != null && attackState != null
        && playerStateMachine.StateMachine.CurrentState == attackState;
    public AttackType attackType { get; private set; }

    private PlayerStateMachine playerStateMachine;
    private PlayerAttackState attackState;

    private float _pressTime;
    private bool _isPressing;
    private IGamePauseSystem pauseSystem;

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    private void Awake()
    {
        pauseSystem = this.GetSystem<IGamePauseSystem>();
        var roleContext = GetComponent<RoleContext>();
        playerStateMachine = roleContext.PlayerStateMachine;
        if (playerStateMachine == null)
        {
            Debug.LogError("[AttackListener] RoleContext requires a PlayerStateMachine.", this);
            enabled = false;
            return;
        }

        attackState = new PlayerAttackState(this);
        playerStateMachine.RegisterState(attackState);
    }

    private void Update()
    {
        // 暂停时丢弃进行中的按键，恢复后需要重新按下才能攻击。
        if (pauseSystem != null && pauseSystem.IsPaused)
        {
            ResetPress();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            _pressTime = Time.time;
            _isPressing = true;
        }

        if (_isPressing && Time.time - _pressTime >= heavyAttackHoldTime)
        {
            _isPressing = false;
            BeginAttack(AttackType.Heavy);
        }

        if (Input.GetMouseButtonUp(0) && _isPressing)
        {
            _isPressing = false;
            BeginAttack(AttackType.Light);
        }
    }

    private void OnDisable()
    {
        ResetPress();
        EndAttack();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            ResetPress();
        }
    }

    private void BeginAttack(AttackType type)
    {
        if (IsAttacking || playerStateMachine == null) return;

        // Ensure an initial state exists even if input is driven before the owner's Awake.
        if (playerStateMachine.StateMachine.CurrentState == null)
            playerStateMachine.StateMachine.Start<PlayerDefualtState>();

        attackType = type;
        playerStateMachine.ChangeState<PlayerAttackState>();
        ResetPress();
    }

    private void EndAttack()
    {
        if (IsAttacking)
            playerStateMachine.StateMachine.RevertState();
    }

    private void OnDestroy()
    {
        EndAttack();
        if (playerStateMachine != null && attackState != null)
            playerStateMachine.StateMachine.RemoveState(attackState);
    }

    private void ResetPress()
    {
        _isPressing = false;
    }
}
