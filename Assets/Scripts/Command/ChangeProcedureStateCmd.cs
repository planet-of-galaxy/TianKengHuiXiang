using QFramework;

public class ChangeProcedureStateCmd<TState> : AbstractCommand where TState : GameProcedureStateBase
{
    protected override void OnExecute()
    {
        this.GetSystem<IGameProcedureSystem>().ChangeState<TState>();
    }
}
