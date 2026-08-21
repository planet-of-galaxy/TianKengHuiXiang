using QFramework;

public class RoleRuntimeInfo
{
    public int id;        // 运行时实例 id，唯一
    public int configId;  // 对应的角色配置 id
    public string name;
    public BindableProperty<float> CurHealth { get; } = new();
    public BindableProperty<float> MaxHealth { get; } = new();
    public BindableProperty<float> MoveSpeed { get; } = new();
}