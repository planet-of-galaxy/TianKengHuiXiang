using QFramework;

public class KilledCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        this.GetModel<GameInfo>().killNum.Value++;
    }
}