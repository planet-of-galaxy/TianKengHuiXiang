using System.Collections.Generic;
using QFramework;
using UnityEngine;

/// <summary>
/// 怪物视图工厂：根据 MonsterRuntimeModel 中的运行时信息与 MonsterConfig 的 Name，
/// 经 IResourceStorage 加载预制体并实例化为表现层。
/// 持有每个运行时 id 对应的实例引用，RemoveMonster 时经 DestroyMonsterView 销毁。
/// 作为 MonsterRuntimeSystem 的字段被持有，不对外暴露；对外接口由 MonsterRuntimeSystem 提供。
/// </summary>
public class MonsterViewFactory
{
    private readonly MonsterRuntimeModel runtimeModel;
    private readonly IMonsterConfigProvider configProvider;
    private readonly IResourceStorage resourceStorage;
    private readonly Dictionary<int, GameObject> spawnedMonsters = new();

    public MonsterViewFactory(MonsterRuntimeModel runtimeModel, IMonsterConfigProvider configProvider, IResourceStorage resourceStorage)
    {
        this.runtimeModel = runtimeModel;
        this.configProvider = configProvider;
        this.resourceStorage = resourceStorage;
    }

    public GameObject SpawnMonster(int runtimeId, Vector3 position, Quaternion rotation)
    {
        if (!runtimeModel.TryGetMonsterRuntime(runtimeId, out var info))
        {
            Debug.LogError($"[MonsterViewFactory] MonsterRuntimeInfo not found for id: {runtimeId}");
            return null;
        }

        var config = configProvider.GetMonster(info.configId);
        if (config == null)
        {
            Debug.LogError($"[MonsterViewFactory] MonsterConfig not found for id: {info.configId}");
            return null;
        }

        if (string.IsNullOrEmpty(config.name))
        {
            Debug.LogError($"[MonsterViewFactory] MonsterConfig {info.configId} has empty name");
            return null;
        }

        var prefab = resourceStorage.Load<GameObject>($"Prefabe/Monster/{config.name}");
        if (prefab == null)
        {
            return null;
        }

        var instance = Object.Instantiate(prefab, position, rotation);
        spawnedMonsters[runtimeId] = instance;
        return instance;
    }

    /// <summary>
    /// 销毁指定运行时 id 的表现层实例。
    /// </summary>
    public void DestroyMonsterView(int runtimeId)
    {
        if (!spawnedMonsters.TryGetValue(runtimeId, out var instance))
        {
            return;
        }

        if (instance != null)
        {
            Object.Destroy(instance);
        }

        spawnedMonsters.Remove(runtimeId);
    }

    /// <summary>
    /// 获取指定运行时 id 的表现层实例；未生成或已销毁返回 null。
    /// </summary>
    public GameObject GetMonsterView(int runtimeId)
    {
        spawnedMonsters.TryGetValue(runtimeId, out var instance);
        return instance;
    }
}
