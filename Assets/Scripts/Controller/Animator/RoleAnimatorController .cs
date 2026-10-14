using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RoleAnimatorController : MonoBehaviour
{
    private Animator animator;
    private StateMachine<PlayerStateMachine> stateMachine;
    private PlayerMoveState moveState;

    public Action OnAttackHitFrame;
    public Action OnAttackEnd;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        var roleContext = GetComponentInParent<RoleContext>();
        if (roleContext == null || roleContext.PlayerStateMachine == null)
            return;

        stateMachine = roleContext.PlayerStateMachine.StateMachine;
        stateMachine.OnStateChanged += HandleStateChanged;
        HandleStateChanged(stateMachine.CurrentState);
    }

    private void OnDisable()
    {
        if (stateMachine != null)
            stateMachine.OnStateChanged -= HandleStateChanged;

        stateMachine = null;
        moveState = null;
        animator.SetBool("IsRunning", false);
        PlayMoveAnimation(0f, 0f);
    }

    private void HandleStateChanged(StateBase<PlayerStateMachine> state)
    {
        moveState = state as PlayerMoveState;
        animator.SetBool("IsRunning", moveState?.IsRunning ?? false);
        PlayMoveAnimation(moveState?.Vertical ?? 0f, moveState?.Horizontal ?? 0f);
    }

    private void LateUpdate()
    {
        if (moveState != null)
        {
            animator.SetBool("IsRunning", moveState.IsRunning);
            PlayMoveAnimation(moveState.Vertical, moveState.Horizontal);
        }
    }

    public void PlayMoveAnimation(float vertical, float horizontal)
    {
        animator.SetFloat("MoveVertical", vertical);
        animator.SetFloat("MoveHorizontal", horizontal);
    }

    public void PlayLightAttackAnimation()
    {
        animator.SetTrigger("LightAttack");
    }

    public void PlayHeavyAttackAnimation()
    {
        animator.SetTrigger("HeavyAttack");
    }

    public void PlayAttackInterruptAnimation()
    {
        animator.SetTrigger("AttackInterrupt");
    }

    private void AttackHitFrame()
    {
        OnAttackHitFrame?.Invoke();
    }

    private void AttackEnd()
    {
        OnAttackEnd?.Invoke();
    }
}
