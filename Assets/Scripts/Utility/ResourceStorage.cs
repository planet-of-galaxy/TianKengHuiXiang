using QFramework;
using UnityEngine;

public interface IResourceStorage : IUtility
{
    T Load<T>(string path) where T : Object;
    T[] LoadAll<T>(string path) where T : Object;
}

public class ResourceStorage : IResourceStorage
{
    public T Load<T>(string path) where T : Object
    {
        var resource = Resources.Load<T>(path);
        if (resource == null)
        {
            Debug.LogError($"Failed to load resource: {path} as type {typeof(T).Name}");
        }
        return resource;
    }

    public T[] LoadAll<T>(string path) where T : Object
    {
        var resources = Resources.LoadAll<T>(path);
        if (resources == null || resources.Length == 0)
        {
            Debug.LogError($"Failed to load resources: {path} as type {typeof(T).Name}");
        }
        return resources;
    }
}
