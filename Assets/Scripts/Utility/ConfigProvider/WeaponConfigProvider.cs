using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class WeaponConfigProvider : IWeaponConfigProvider
{
    private readonly Dictionary<int, WeaponConfig> _weaponConfigs = new();
    private IPersistStorage _storage;

    public WeaponConfigProvider(IPersistStorage storage)
    {
        this._storage = storage;

        LoadWeaponConfigs();
    }

    #region IWeaponConfigProvider
    private void LoadWeaponConfigs()
    {
        var data = _storage.Load<WeaponConfigData>("WeaponConfig");

        if (data?.weapons == null || data.weapons.Length == 0)
        {
            Debug.LogWarning("[IWeaponConfigProvider] No weapon configs loaded from WeaponConfig.json");
            return;
        }

        foreach (var config in data.weapons)
        {
            if (config == null) continue;

            if (_weaponConfigs.ContainsKey(config.weaponId))
            {
                Debug.LogWarning($"[IWeaponConfigProvider] Duplicate weaponId {config.weaponId}, overwriting");
            }

            _weaponConfigs[config.weaponId] = config;
        }

        Debug.Log($"[IWeaponConfigProvider] Loaded {_weaponConfigs.Count} weapon configs");
    }

    public WeaponConfig GetWeaponConfig(int weaponId)
    {
        return _weaponConfigs.TryGetValue(weaponId, out var config) ? config : null;
    }

    public bool HasWeapon(int weaponId)
    {
        return _weaponConfigs.ContainsKey(weaponId);
    }

    public int GetWeaponCount()
    {
        return _weaponConfigs.Count;
    }
    #endregion
}
