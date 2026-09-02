using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class MonsterConfigProvider : IMonsterConfigProvider
{
    private readonly Dictionary<int, MonsterConfig> _monsterConfigs = new();
    private IPersistStorage _storage;

    public MonsterConfigProvider(IPersistStorage storage)
    {
        this._storage = storage;

        Init();
    }

    #region IMonsterConfigProvider
    public void Init()
    {
        _monsterConfigs.Clear();

        var data = _storage.Load<MonsterConfigData>("MonsterConfig");

        if (data?.monsters == null || data.monsters.Length == 0)
        {
            Debug.LogWarning("[IMonsterConfigProvider] No monster configs loaded from MonsterConfig.json");
            return;
        }

        foreach (var config in data.monsters)
        {
            if (config == null) continue;

            if (_monsterConfigs.ContainsKey(config.monsterId))
            {
                Debug.LogWarning($"[IMonsterConfigProvider] Duplicate monsterId {config.monsterId}, overwriting");
            }

            _monsterConfigs[config.monsterId] = config;
        }

        Debug.Log($"[IMonsterConfigProvider] Loaded {_monsterConfigs.Count} monster configs");
    }

    public MonsterConfig GetMonster(int monsterId)
    {
        return _monsterConfigs.TryGetValue(monsterId, out var config) ? config : null;
    }

    public bool HasMonster(int monsterId)
    {
        return _monsterConfigs.ContainsKey(monsterId);
    }

    public int GetMonsterCount()
    {
        return _monsterConfigs.Count;
    }
    #endregion
}
