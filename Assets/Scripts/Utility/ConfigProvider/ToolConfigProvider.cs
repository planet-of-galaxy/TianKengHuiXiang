using System.Collections.Generic;
using UnityEngine;

public class ToolConfigProvider : IToolConfigProvider
{
    private readonly Dictionary<int, ToolConfig> _toolConfigs = new();
    private readonly IPersistStorage _storage;

    public ToolConfigProvider(IPersistStorage storage)
    {
        _storage = storage;

        LoadToolConfigs();
    }

    #region IToolConfigProvider
    private void LoadToolConfigs()
    {
        var data = _storage.Load<ToolConfigData>("ToolConfig");

        if (data?.tools == null || data.tools.Length == 0)
        {
            Debug.LogWarning("[IToolConfigProvider] No tool configs loaded from ToolConfig.json");
            return;
        }

        foreach (var config in data.tools)
        {
            if (config == null) continue;

            if (_toolConfigs.ContainsKey(config.toolId))
            {
                Debug.LogWarning($"[IToolConfigProvider] Duplicate toolId {config.toolId}, overwriting");
            }

            _toolConfigs[config.toolId] = config;
        }

        Debug.Log($"[IToolConfigProvider] Loaded {_toolConfigs.Count} tool configs");
    }

    public ToolConfig GetToolConfig(int toolId)
    {
        return _toolConfigs.TryGetValue(toolId, out var config) ? config : null;
    }

    public bool HasTool(int toolId)
    {
        return _toolConfigs.ContainsKey(toolId);
    }

    public int GetToolCount()
    {
        return _toolConfigs.Count;
    }
    #endregion
}
