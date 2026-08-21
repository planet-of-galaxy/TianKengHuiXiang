using System.IO;
using QFramework;
using UnityEngine;

/// <summary>
/// Json 数据持久化工具接口
/// 通过 QFramework 的 Utility 机制暴露给 System / Model / Controller / Command 使用
/// </summary>
public interface IJsonStorage : IPersistStorage
{

}

/// <summary>
/// IJsonStorage 的默认实现，底层复用 JsonMgr（LitJson / JsonUtility 双方案）
/// </summary>
public class JsonStorage : IJsonStorage
{
    private const string Extension = ".json";

    public void Save<T>(T data, string fileName)
    {
        if (data == null)
        {
            Debug.LogWarning($"[JsonStorage] Save 被调用时 data 为 null，fileName={fileName}，已忽略。");
            return;
        }

        JsonMgr.Instance.SaveData(data, fileName, JsonType.LitJson);
    }

    public T Load<T>(string fileName) where T : new()
    {
        return JsonMgr.Instance.LoadData<T>(fileName, JsonType.LitJson);
    }

    public bool HasData(string fileName)
    {
        string persistentPath = Path.Combine(Application.persistentDataPath, fileName + Extension);
        string streamingPath = Path.Combine(Application.streamingAssetsPath, fileName + Extension);
        return File.Exists(persistentPath) || File.Exists(streamingPath);
    }

    public void DeleteData(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName + Extension);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public void DeleteAll()
    {
        if (!Directory.Exists(Application.persistentDataPath)) return;

        foreach (var file in Directory.GetFiles(Application.persistentDataPath, "*" + Extension))
        {
            File.Delete(file);
        }
    }
}
