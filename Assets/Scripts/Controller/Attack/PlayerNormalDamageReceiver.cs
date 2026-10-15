using QFramework;
using UnityEngine;

/// <summary>玩家受伤组件：将收到的伤害直接结算到角色运行血量。</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RoleContext))]
public class PlayerNormalDamageReceiver : DamageReceiverBase
{
    private RoleContext roleContext;
    private IRoleRuntimeSystem roleRuntimeSystem;
    private PlayerStateMachine playerStateMachine;
    private PlayerStunnedState stunnedState;

    private void Awake()
    {
        roleContext = GetComponent<RoleContext>();
        roleRuntimeSystem = this.GetSystem<IRoleRuntimeSystem>();
        playerStateMachine = roleContext.PlayerStateMachine;
        if (playerStateMachine != null)
        {
            stunnedState = new PlayerStunnedState();
            playerStateMachine.RegisterState(stunnedState);
        }
    }

    private void OnDestroy()
    {
        if (playerStateMachine != null && stunnedState != null)
            playerStateMachine.StateMachine.RemoveState(stunnedState);
    }

    public override void GetDamage(DamageInfo damageInfo)
    {
        if (playerStateMachine == null || playerStateMachine.StateMachine.IsInState<PlayerDeathState>()) return;
        if (!this.GetModel<IRoleRuntimeModel>().TryGetRoleRuntime(roleContext.RoleRuntimeIndex, out var runtimeInfo)) return;

        // 不做减伤，直接按伤害段数和每段数值扣血。
        float damage = damageInfo.hintCount * damageInfo.hintValue;
        roleRuntimeSystem.ChangeCurHealth(roleContext, -damage);
        if (runtimeInfo.CurHealth.Value <= 0f)
            playerStateMachine.ChangeState<PlayerDeathState>();
        else if (damage > 0f)
            playerStateMachine.ChangeState<PlayerStunnedState>(0.2f);
    }
}
