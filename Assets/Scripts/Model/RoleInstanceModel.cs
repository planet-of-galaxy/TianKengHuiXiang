using QFramework;

public class RoleInstanceModel : AbstractModel
{
    /// <summary>Current role runtime index; -1 means no selection.</summary>
    public BindableProperty<int> curRole { get; } = new BindableProperty<int>(-1);
    protected override void OnInit() { }
}
