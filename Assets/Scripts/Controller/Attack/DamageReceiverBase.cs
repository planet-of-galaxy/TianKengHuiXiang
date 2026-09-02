using UnityEngine;

/// <summary>
/// 伤害接收基类：挂在可受伤的物体上（玩家、怪物等），作为 <see cref="IGetDamage"/> 的基础实现。
/// 子类负责把 DamageInfo 映射到自身的血量、受伤表现等具体逻辑。
/// </summary>
public abstract class DamageReceiverBase : MonoBehaviour, IGetDamage
{
    /// <summary>接收一次伤害。</summary>
    public abstract void GetDamage(DamageInfo damageInfo);
}
