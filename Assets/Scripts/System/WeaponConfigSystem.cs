using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class WeaponConfigSystem : AbstractSystem, IWeaponConfigSystem
{
    private readonly Dictionary<int, WeaponConfig> _weaponConfigs = new();

    protected override void OnInit()
    {
        LoadWeaponConfigs();
    }

    protected override void OnDeinit()
    {
        _weaponConfigs.Clear();
    }

    private void LoadWeaponConfigs()
    {
        var jsonStorage = this.GetUtility<IJsonStorage>();
        var data = jsonStorage.Load<WeaponConfigData>("WeaponConfig");

        if (data?.weapons == null || data.weapons.Length == 0)
        {
            Debug.LogWarning("[WeaponConfigSystem] No weapon configs loaded from WeaponConfig.json");
            return;
        }

        foreach (var config in data.weapons)
        {
            if (config == null) continue;

            if (_weaponConfigs.ContainsKey(config.weaponId))
            {
                Debug.LogWarning($"[WeaponConfigSystem] Duplicate weaponId {config.weaponId}, overwriting");
            }

            _weaponConfigs[config.weaponId] = config;
        }

        Debug.Log($"[WeaponConfigSystem] Loaded {_weaponConfigs.Count} weapon configs");
    }

    public WeaponConfig GetWeaponConfig(int weaponId)
    {
        return _weaponConfigs.TryGetValue(weaponId, out var config) ? config : null;
    }
}
