using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class RoleConfigProvider : IRoleConfigProvider
{
    private readonly Dictionary<int, RoleConfig> _roleConfigs = new();
    private IJsonStorage _storage;

    public RoleConfigProvider(IJsonStorage storage)
    {
        this._storage = storage;

        LoadRoleConfigs();
    }

    #region IRoleConfigProvider
    private void LoadRoleConfigs()
    {
        var data = _storage.Load<RoleConfigData>("RoleConfig");

        if (data?.roles == null || data.roles.Length == 0)
        {
            Debug.LogWarning("[IRoleConfigProvider] No role configs loaded from RoleConfig.json");
            return;
        }

        foreach (var config in data.roles)
        {
            if (config == null) continue;

            if (_roleConfigs.ContainsKey(config.roleId))
            {
                Debug.LogWarning($"[IRoleConfigProvider] Duplicate roleId {config.roleId}, overwriting");
            }

            _roleConfigs[config.roleId] = config;
        }

        Debug.Log($"[IRoleConfigProvider] Loaded {_roleConfigs.Count} role configs");
    }

    public RoleConfig GetRoleConfig(int roleId)
    {
        return _roleConfigs.TryGetValue(roleId, out var config) ? config : null;
    }

    public bool HasRole(int roleId)
    {
        return _roleConfigs.ContainsKey(roleId);
    }

    public int GetRoleCount()
    {
        return _roleConfigs.Count;
    }
    #endregion
}
