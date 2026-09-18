using QFramework;
using UnityEngine;

/// <summary>普通怪物受伤组件：将收到的伤害直接结算到怪物运行血量。</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(MonsterContext))]
public class MonsterNormalDamageReceiver : DamageReceiverBase
{
    private MonsterContext monsterContext;
    private IMonsterRuntimeSystem monsterRuntimeSystem;

    private void Awake()
    {
        monsterContext = GetComponent<MonsterContext>();
        monsterRuntimeSystem = this.GetSystem<IMonsterRuntimeSystem>();
    }

    public override void GetDamage(DamageInfo damageInfo)
    {
        // 普通怪物不做减伤，直接按伤害段数和每段数值扣血。
        monsterRuntimeSystem.ChangeCurHealth(monsterContext, -damageInfo.hintCount * damageInfo.hintValue);
    }
}
