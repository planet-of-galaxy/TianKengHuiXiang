using UnityEngine;

/// <summary>
/// 加载并实例化已挂载 MonsterContext 的怪物预制体，不持有实例数据。
/// </summary>
public class MonsterViewFactory
{
    private readonly IMonsterConfigProvider configProvider;
    private readonly IResourceStorage resourceStorage;

    public MonsterViewFactory(IMonsterConfigProvider configProvider, IResourceStorage resourceStorage)
    {
        this.configProvider = configProvider;
        this.resourceStorage = resourceStorage;
    }

    public MonsterContext SpawnMonster(int configId, Vector3 position, Quaternion rotation)
    {
        var config = configProvider.GetMonster(configId);
        if (config == null)
        {
            Debug.LogError($"[MonsterViewFactory] MonsterConfig not found for id: {configId}");
            return null;
        }

        if (string.IsNullOrEmpty(config.name))
        {
            Debug.LogError($"[MonsterViewFactory] MonsterConfig {configId} has empty name");
            return null;
        }

        var prefab = resourceStorage.Load<GameObject>($"Prefabe/Monster/{config.name}");
        if (prefab == null) return null;

        var prefabContext = prefab.GetComponent<MonsterContext>();
        if (prefabContext == null || prefabContext.ConfigId != configId)
        {
            Debug.LogError($"[MonsterViewFactory] Prefab {config.name} must have a MonsterContext with configId {configId}");
            return null;
        }

        return Object.Instantiate(prefab, position, rotation).GetComponent<MonsterContext>();
    }
}
