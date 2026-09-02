using System.Collections.Generic;
using QFramework;

public class MonsterRuntimeModel : AbstractModel
{
    /// <summary>
    /// 所有怪物运行时信息的集合，key 为运行时实例 id（唯一，同一怪物配置可有多份实例）。
    /// </summary>
    private readonly Dictionary<int, MonsterRuntimeInfo> monsterRuntimeInfo = new();

    public int Count => monsterRuntimeInfo.Count;

    public bool TryGetMonsterRuntime(int id, out MonsterRuntimeInfo info)
    {
        return monsterRuntimeInfo.TryGetValue(id, out info);
    }

    /// <summary>
    /// 遍历所有怪物运行时信息（只读）。
    /// </summary>
    public IEnumerable<MonsterRuntimeInfo> GetAllMonsterRuntimes()
    {
        return monsterRuntimeInfo.Values;
    }

    /// <summary>
    /// 添加或覆盖一个怪物运行时信息（由 MonsterRuntimeSystem 写入）。
    /// </summary>
    public void AddMonsterRuntime(MonsterRuntimeInfo info)
    {
        if (info == null) return;
        monsterRuntimeInfo[info.runtimeId] = info;
    }

    /// <summary>
    /// 移除指定 id 的怪物运行时信息（由 MonsterRuntimeSystem 删除怪物时使用）。
    /// </summary>
    public void RemoveMonsterRuntime(int id)
    {
        monsterRuntimeInfo.Remove(id);
    }

    /// <summary>
    /// 清空所有怪物运行时信息（由 MonsterRuntimeSystem 初始化时使用）。
    /// </summary>
    public void ClearMonsterRuntimes()
    {
        monsterRuntimeInfo.Clear();
    }

    protected override void OnInit()
    {
    }
}
