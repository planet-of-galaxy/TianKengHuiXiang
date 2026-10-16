using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RoleAnimatorController : MonoBehaviour
{
    private Animator animator;
    private Animator Animator => animator != null ? animator : animator = GetComponent<Animator>();

    private const string DefaultLightAttackClipName = "punching";
    private RuntimeAnimatorController originalAnimatorController;
    private AnimatorOverrideController overrideController;
    private readonly Dictionary<string, AnimationClip> defaultClips = new();

    /// <summary>Replace the light attack clip. Null restores its original animation.</summary>
    public bool UpdateAttackAnimation(AnimationClip clip)
    {
        return ReplaceAnimation(DefaultLightAttackClipName, clip);
    }

    /// <summary>Replace a clip by its original name. Null restores the original override or clip.</summary>
    public bool ReplaceAnimation(string originalClipName, AnimationClip clip)
    {
        if (string.IsNullOrEmpty(originalClipName)) return false;
        if (overrideController == null)
        {
            if (Animator.runtimeAnimatorController == null) return false;
            originalAnimatorController = Animator.runtimeAnimatorController;
            overrideController = new AnimatorOverrideController(originalAnimatorController);
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrides);
            foreach (var entry in overrides)
                defaultClips[entry.Key.name] = entry.Value != null ? entry.Value : entry.Key;
            Animator.runtimeAnimatorController = overrideController;
        }

        if (!defaultClips.TryGetValue(originalClipName, out var defaultClip))
        {
            Debug.LogWarning($"[RoleAnimatorController] Animation '{originalClipName}' was not found.", this);
            return false;
        }

        overrideController[originalClipName] = clip != null ? clip : defaultClip;
        return true;
    }

    private void OnDestroy()
    {
        if (overrideController == null) return;
        if (animator != null && animator.runtimeAnimatorController == overrideController)
            animator.runtimeAnimatorController = originalAnimatorController;
        Destroy(overrideController);
    }

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
