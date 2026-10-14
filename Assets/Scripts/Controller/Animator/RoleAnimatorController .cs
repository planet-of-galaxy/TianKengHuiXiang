using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RoleAnimatorController : MonoBehaviour
{
    private Animator animator;
    private bool isRunning;
    private bool IsRunning
    {
        get => isRunning;
        set
        {
            if (isRunning == value) return;
            isRunning = value;
            animator.SetBool("IsRunning", isRunning);
        }
    }

    public Action OnAttackHitFrame;
    public Action OnAttackEnd;

    private void Awake()
    {
        animator = GetComponent<Animator>();
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
