using QFramework;

public class MonsterRuntimeInfo
{
    public int runtimeId;   // 运行时实例 id，唯一
    public int configId;    // 对应的怪物配置 id
    public BindableProperty<float> CurHealth { get; } = new();
    public BindableProperty<float> MaxHealth { get; } = new();
}
