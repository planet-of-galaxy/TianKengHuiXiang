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

    private void Update()
    {
        float vertical = (Input.GetKey(KeyCode.W) ? 1f : 0f)
            - (Input.GetKey(KeyCode.S) ? 1f : 0f);
        float horizontal = (Input.GetKey(KeyCode.D) ? 1f : 0f)
            - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        PlayMoveAnimation(vertical, horizontal);

        IsRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Input.GetMouseButtonDown(0))
        {
            PlayLightAttackAnimation();
        }

        if (Input.GetMouseButtonDown(1))
        {
            animator.SetTrigger("Hit");
        }

        if (Input.GetMouseButtonDown(2))
        {
            animator.SetBool("IsDead", true);
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
