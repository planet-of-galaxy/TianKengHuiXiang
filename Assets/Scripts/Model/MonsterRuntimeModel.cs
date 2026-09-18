using System.Collections.Generic;
using QFramework;

public interface IMonsterRuntimeModel : IModel
{
    int Count { get; }
    bool TryGetMonsterRuntime(MonsterContext context, out MonsterRuntimeInfo info);
    IEnumerable<MonsterRuntimeInfo> GetAllMonsterRuntimes();
}

public class MonsterRuntimeModel : AbstractModel, IMonsterRuntimeModel
{
    // 每个 Context 对应一个怪物实例的运行数据。
    private readonly Dictionary<MonsterContext, MonsterRuntimeInfo> monsterRuntimeInfo = new();

    public int Count => monsterRuntimeInfo.Count;

    public bool TryGetMonsterRuntime(MonsterContext context, out MonsterRuntimeInfo info)
    {
        info = null;
        return !ReferenceEquals(context, null) && monsterRuntimeInfo.TryGetValue(context, out info);
    }

    public IEnumerable<MonsterRuntimeInfo> GetAllMonsterRuntimes()
    {
        return monsterRuntimeInfo.Values;
    }

    public void AddMonsterRuntime(MonsterContext context, MonsterRuntimeInfo info)
    {
        if (context == null || info == null) return;
        monsterRuntimeInfo[context] = info;
    }

    public void RemoveMonsterRuntime(MonsterContext context)
    {
        if (!ReferenceEquals(context, null)) monsterRuntimeInfo.Remove(context);
    }

    public void ClearMonsterRuntimes()
    {
        monsterRuntimeInfo.Clear();
    }

    protected override void OnInit()
    {
    }
}
