using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MonsterAnimatorController : MonoBehaviour
{
    private Animator animator;
    private Animator Animator => animator != null ? animator : animator = GetComponent<Animator>();

    public Action<AttackType> OnAttackStart;
    public Action OnAttackHitFrame;
    public Action OnAttackEnd;

    public void PlayMoveAnimation(float vertical, float horizontal)
    {
        Animator.SetFloat("MoveVertical", vertical);
        Animator.SetFloat("MoveHorizontal", horizontal);
    }

    public void SetRunning(bool isRunning)
    {
        Animator.SetBool("IsRunning", isRunning);
    }

    public void PlayHitAnimation()
    {
        Animator.SetTrigger("Hit");
    }

    public void PlayDeathAnimation()
    {
        Animator.SetBool("IsDead", true);
    }

    public void StartAttack(AttackType attackType)
    {
        OnAttackStart?.Invoke(attackType);
        Animator.SetTrigger(attackType == AttackType.Light ? "LightAttack" : "HeavyAttack");
    }

    public void EndAttack()
    {
        OnAttackEnd?.Invoke();
    }

    private void AttackHitFrame()
    {
        OnAttackHitFrame?.Invoke();
    }

}
