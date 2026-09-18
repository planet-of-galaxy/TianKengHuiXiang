using QFramework;

public class MonsterRuntimeInfo
{
    public BindableProperty<float> CurHealth { get; } = new();
    public BindableProperty<float> MaxHealth { get; } = new();
}
