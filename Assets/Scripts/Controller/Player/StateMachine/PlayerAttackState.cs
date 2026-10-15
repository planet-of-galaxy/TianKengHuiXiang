public class PlayerAttackState : StateBase<PlayerStateMachine>
{
    private readonly IAttackListener attackListener;

    public PlayerAttackState(IAttackListener attackListener)
    {
        this.attackListener = attackListener;
    }

    public AttackType attackType => attackListener.attackType;
}
