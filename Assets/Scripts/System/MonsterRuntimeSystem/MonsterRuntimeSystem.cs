using QFramework;
using UnityEngine;

public interface IMonsterRuntimeSystem : ISystem
{
    /// <summary>
    /// 根据怪物配置 id 创建一个怪物运行时实例，返回其运行时 id；配置不存在返回 -1。
    /// </summary>
    int CreateMonster(int configId);

    /// <summary>
    /// 获取指定运行时 id 的怪物信息；不存在返回 null。
    /// </summary>
    MonsterRuntimeInfo GetMonsterRuntime(int runtimeId);

    /// <summary>
    /// 尝试获取指定运行时 id 的怪物信息。
    /// </summary>
    bool TryGetMonsterRuntime(int runtimeId, out MonsterRuntimeInfo info);

    /// <summary>
    /// 删除指定运行时 id 的怪物：先销毁表现层，再从模型中移除。不存在返回 false。
    /// </summary>
    bool RemoveMonster(int runtimeId);

    /// <summary>
    /// 直接设置怪物当前生命值（限制在 [0, MaxHealth]）。
    /// </summary>
    void SetCurHealth(int runtimeId, float value);

    /// <summary>
    /// 增减怪物当前生命值（delta 为负表示受伤）。
    /// </summary>
    void ChangeCurHealth(int runtimeId, float delta);

    /// <summary>
    /// 为指定运行时 id 的怪物创建表现层 GameObject，返回实例；失败返回 null。
    /// </summary>
    GameObject SpawnMonster(int runtimeId, Vector3 position, Quaternion rotation);
}

public class MonsterRuntimeSystem : AbstractSystem, IMonsterRuntimeSystem
{
    private MonsterRuntimeModel runtimeModel;
    private IMonsterConfigProvider monsterConfigProvider;
    private MonsterViewFactory viewFactory;
    private int nextMonsterRuntimeId = 1;

    protected override void OnInit()
    {
        runtimeModel = this.GetModel<MonsterRuntimeModel>();
        monsterConfigProvider = this.GetUtility<IMonsterConfigProvider>();
        viewFactory = new MonsterViewFactory(runtimeModel, monsterConfigProvider, this.GetUtility<IResourceStorage>());
    }

    private int AllocateRuntimeId()
    {
        return nextMonsterRuntimeId++;
    }

    public int CreateMonster(int configId)
    {
        var config = monsterConfigProvider.GetMonster(configId);
        if (config == null)
        {
            Debug.LogError($"[MonsterRuntimeSystem] MonsterConfig not found for monsterId: {configId}");
            return -1;
        }

        var info = new MonsterRuntimeInfo
        {
            runtimeId = AllocateRuntimeId(),
            configId = configId,
        };
        info.CurHealth.Value = config.health;
        info.MaxHealth.Value = config.health;

        runtimeModel.AddMonsterRuntime(info);

        return info.runtimeId;
    }

    public MonsterRuntimeInfo GetMonsterRuntime(int runtimeId)
    {
        runtimeModel.TryGetMonsterRuntime(runtimeId, out var info);
        return info;
    }

    public bool TryGetMonsterRuntime(int runtimeId, out MonsterRuntimeInfo info)
    {
        return runtimeModel.TryGetMonsterRuntime(runtimeId, out info);
    }

    public bool RemoveMonster(int runtimeId)
    {
        if (!runtimeModel.TryGetMonsterRuntime(runtimeId, out _))
        {
            Debug.LogWarning($"[MonsterRuntimeSystem] MonsterRuntimeInfo not found for id: {runtimeId}");
            return false;
        }

        viewFactory.DestroyMonsterView(runtimeId);
        runtimeModel.RemoveMonsterRuntime(runtimeId);
        return true;
    }

    public void SetCurHealth(int runtimeId, float value)
    {
        if (!runtimeModel.TryGetMonsterRuntime(runtimeId, out var info))
        {
            Debug.LogWarning($"[MonsterRuntimeSystem] MonsterRuntimeInfo not found for id: {runtimeId}");
            return;
        }

        info.CurHealth.Value = Mathf.Clamp(value, 0f, info.MaxHealth.Value);
    }

    public void ChangeCurHealth(int runtimeId, float delta)
    {
        if (!runtimeModel.TryGetMonsterRuntime(runtimeId, out var info))
        {
            Debug.LogWarning($"[MonsterRuntimeSystem] MonsterRuntimeInfo not found for id: {runtimeId}");
            return;
        }

        info.CurHealth.Value = Mathf.Clamp(info.CurHealth.Value + delta, 0f, info.MaxHealth.Value);
    }

    public GameObject SpawnMonster(int runtimeId, Vector3 position, Quaternion rotation)
    {
        return viewFactory.SpawnMonster(runtimeId, position, rotation);
    }
}
