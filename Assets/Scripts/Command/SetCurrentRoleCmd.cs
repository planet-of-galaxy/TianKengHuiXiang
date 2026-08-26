using QFramework;

public class SetCurrentRoleCmd : AbstractCommand
{
    private readonly int _runtimeIndex;

    public SetCurrentRoleCmd(int runtimeIndex)
    {
        _runtimeIndex = runtimeIndex;
    }

    protected override void OnExecute()
    {
        this.SendEvent(new CurrentRoleSetEvent { RuntimeIndex = _runtimeIndex });
    }
}

public struct CurrentRoleSetEvent
{
    public int RuntimeIndex;
}
