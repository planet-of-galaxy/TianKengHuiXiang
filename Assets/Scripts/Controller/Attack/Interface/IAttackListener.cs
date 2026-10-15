public enum AttackType
{
    Light,
    Heavy
}

public interface IAttackListener
{
    bool IsAttacking { get; }
    AttackType attackType { get; }
}
