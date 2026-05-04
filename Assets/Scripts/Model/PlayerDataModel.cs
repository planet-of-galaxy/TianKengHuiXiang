using QFramework;

public class PlayerDataModel : AbstractModel
{
    public int RoleId { get; }
    public BindableProperty<PlayerInfo> MaxInfo { get; } = new BindableProperty<PlayerInfo>();
    public BindableProperty<PlayerInfo> CurInfo { get; } = new BindableProperty<PlayerInfo>();

    protected override void OnInit()
    {
    }
}
