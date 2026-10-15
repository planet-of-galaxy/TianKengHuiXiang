using QFramework;
using UnityEngine;

public class PlayerAttackState : StateBase<PlayerStateMachine>, IController
{
    private readonly IAttackListener attackListener;
    private IGamePauseSystem pauseSystem;
    private float remainingAttackTime;
    private bool attackStarted;

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    public PlayerAttackState(IAttackListener attackListener)
    {
        this.attackListener = attackListener;
    }

    public AttackType attackType => attackListener.attackType;

    public override void OnEnter()
    {
        pauseSystem = this.GetSystem<IGamePauseSystem>();
        var roleContext = Owner.GetComponent<RoleContext>();
        var weapon = this.GetSystem<IWeaponInstanceSystem>().TryGetWeapon(roleContext);
        var config = this.GetUtility<IWeaponConfigProvider>()
            .GetWeaponConfig(weapon != null ? weapon.WeaponConfigId : -1);
        var attackConfig = attackType == AttackType.Light ? config?.lightAttack : config?.heavyAttack;
        if (attackConfig == null)
        {
            StateMachine.RevertState();
            return;
        }

        remainingAttackTime = Mathf.Max(0f, attackConfig.attackDuration);
        attackStarted = true;
        Owner.RoleAnimatorController?.StartAttack(attackType);
    }

    public override void OnUpdate(float deltaTime)
    {
        if (pauseSystem != null && pauseSystem.IsPaused) return;

        remainingAttackTime -= deltaTime;
        if (remainingAttackTime <= 0f)
            StateMachine.RevertState();
    }

    public override void OnExit()
    {
        remainingAttackTime = 0f;
        if (attackStarted)
        {
            attackStarted = false;
            Owner.RoleAnimatorController?.EndAttack();
        }
    }
}
