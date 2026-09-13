/// <summary>
/// 攻击接口：可执行轻攻击或重攻击并返回本次造成的伤害信息。
/// </summary>
public interface IAttack
{
    DamageInfo ILightAttack();
    DamageInfo IHeavyAttack();
}
