using QFramework;

public class PlayerDataModel : AbstractModel
{
    public BindableProperty<int> RoleId { get; } = new BindableProperty<int>(-1);
    public BindableProperty<PlayerInfo> MaxInfo { get; } = new BindableProperty<PlayerInfo>();
    public BindableProperty<PlayerInfo> CurInfo { get; } = new BindableProperty<PlayerInfo>();

    protected override void OnInit()
    {
    }
}
