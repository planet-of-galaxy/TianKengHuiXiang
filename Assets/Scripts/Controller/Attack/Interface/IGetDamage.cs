/// <summary>
/// 伤害接收接口：可接收一次伤害的对象（角色、怪物等）。
/// </summary>
public interface IGetDamage
{
    void GetDamage(DamageInfo damageInfo);
}
