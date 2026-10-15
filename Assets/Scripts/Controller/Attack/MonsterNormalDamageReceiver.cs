using QFramework;
using UnityEngine;

/// <summary>普通怪物受伤组件：将收到的伤害直接结算到怪物运行血量。</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(MonsterContext))]
public class MonsterNormalDamageReceiver : DamageReceiverBase
{
    private MonsterContext monsterContext;
    private IMonsterRuntimeSystem monsterRuntimeSystem;
    private MonsterStateMachine monsterStateMachine;
    private MonsterStunnedState stunnedState;

    private void Awake()
    {
        monsterContext = GetComponent<MonsterContext>();
        monsterRuntimeSystem = this.GetSystem<IMonsterRuntimeSystem>();
        monsterStateMachine = monsterContext.MonsterStateMachine;
        if (monsterStateMachine != null)
        {
            stunnedState = new MonsterStunnedState();
            monsterStateMachine.RegisterState(stunnedState);
        }
    }

    private void OnDestroy()
    {
        if (monsterStateMachine != null && stunnedState != null)
            monsterStateMachine.StateMachine.RemoveState(stunnedState);
    }

    public override void GetDamage(DamageInfo damageInfo)
    {
        if (monsterStateMachine == null || monsterStateMachine.StateMachine.IsInState<MonsterDeathState>()) return;
        if (!monsterRuntimeSystem.TryGetMonsterRuntime(monsterContext, out var runtimeInfo)) return;

        // 普通怪物不做减伤，直接按伤害段数和每段数值扣血。
        float damage = damageInfo.hintCount * damageInfo.hintValue;
        monsterRuntimeSystem.ChangeCurHealth(monsterContext, -damage);
        if (runtimeInfo.CurHealth.Value <= 0f)
            monsterStateMachine.ChangeState<MonsterDeathState>();
        else if (damage > 0f)
            monsterStateMachine.ChangeState<MonsterStunnedState>(0.2f);
    }
}
