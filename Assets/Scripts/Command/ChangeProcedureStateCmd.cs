using QFramework;

public class ChangeProcedureStateCmd<TState> : AbstractCommand where TState : StateBase<GameProcedureSystem>
{
    protected override void OnExecute()
    {
        this.GetSystem<IGameProcedureSystem>().ChangeState<TState>();
    }
}
