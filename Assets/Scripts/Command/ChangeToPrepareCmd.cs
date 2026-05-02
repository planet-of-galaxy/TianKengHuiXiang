using QFramework;

public class ChangeToPrepareCmd : AbstractCommand
{
    protected override void OnExecute()
    {
        this.GetSystem<IGameProcedureSystem>().ChangeState<PrepareState>();
    }
}