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
        return Resources.Load<T>(path);
    }

    public T[] LoadAll<T>(string path) where T : Object
    {
        return Resources.LoadAll<T>(path);
    }
}
