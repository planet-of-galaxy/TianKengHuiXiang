using QFramework;

public class TransToRoleObserveCmd : AbstractCommand
{
    private readonly int _runtimeIndex;

    public TransToRoleObserveCmd(int runtimeIndex)
    {
        _runtimeIndex = runtimeIndex;
    }

    protected override void OnExecute()
    {
        this.SendEvent(new RoleObserveEvent { RuntimeIndex = _runtimeIndex });
    }
}

public struct RoleObserveEvent
{
    public int RuntimeIndex;
}
