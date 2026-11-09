using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class ItemConfigProvider : IItemConfigProvider
{
    private readonly Dictionary<int, ItemConfig> _itemConfigs = new();
    private IPersistStorage _storage;

    public ItemConfigProvider(IPersistStorage storage)
    {
        this._storage = storage;

        LoadItemConfigs();
    }

    #region IItemConfigProvider
    private void LoadItemConfigs()
    {
        var data = _storage.Load<ItemConfigData>("ItemConfig");

        if (data?.items == null || data.items.Length == 0)
        {
            Debug.LogWarning("[IItemConfigProvider] No item configs loaded from ItemConfig.json");
            return;
        }

        foreach (var config in data.items)
        {
            if (config == null) continue;

            if (_itemConfigs.ContainsKey(config.itemId))
            {
                Debug.LogWarning($"[IItemConfigProvider] Duplicate itemId {config.itemId}, overwriting");
            }

            _itemConfigs[config.itemId] = config;
        }

        Debug.Log($"[IItemConfigProvider] Loaded {_itemConfigs.Count} item configs");
    }

    public ItemConfig GetItemConfig(int itemId)
    {
        return _itemConfigs.TryGetValue(itemId, out var config) ? config : null;
    }

    public bool HasItem(int itemId)
    {
        return _itemConfigs.ContainsKey(itemId);
    }

    public int GetItemCount()
    {
        return _itemConfigs.Count;
    }
    #endregion
}
