using QFramework;
using UnityEngine;

/// <summary>玩家受伤组件：将收到的伤害直接结算到角色运行血量。</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RoleContext))]
public class PlayerNormalDamageReceiver : DamageReceiverBase
{
    private RoleContext roleContext;
    private IRoleRuntimeSystem roleRuntimeSystem;

    private void Awake()
    {
        roleContext = GetComponent<RoleContext>();
        roleRuntimeSystem = this.GetSystem<IRoleRuntimeSystem>();
    }

    public override void GetDamage(DamageInfo damageInfo)
    {
        // 不做减伤，直接按伤害段数和每段数值扣血。
        roleRuntimeSystem.ChangeCurHealth(roleContext, -damageInfo.hintCount * damageInfo.hintValue);
    }
}
