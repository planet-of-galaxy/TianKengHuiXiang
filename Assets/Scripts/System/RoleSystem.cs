using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class RoleSystem : AbstractSystem, IRoleSystem
{
    private readonly Dictionary<int, RoleConfig> _roleConfigs = new();

    protected override void OnInit()
    {
        LoadRoleConfigs();
    }

    protected override void OnDeinit()
    {
        _roleConfigs.Clear();
    }

    private void LoadRoleConfigs()
    {
        var jsonStorage = this.GetUtility<IJsonStorage>();
        var data = jsonStorage.Load<RoleConfigData>("RoleConfig");

        if (data?.roles == null || data.roles.Length == 0)
        {
            Debug.LogWarning("[RoleSystem] No role configs loaded from RoleConfig.json");
            return;
        }

        foreach (var config in data.roles)
        {
            if (config == null) continue;

            if (_roleConfigs.ContainsKey(config.roleId))
            {
                Debug.LogWarning($"[RoleSystem] Duplicate roleId {config.roleId}, overwriting");
            }

            _roleConfigs[config.roleId] = config;
        }

        Debug.Log($"[RoleSystem] Loaded {_roleConfigs.Count} role configs");
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
}
