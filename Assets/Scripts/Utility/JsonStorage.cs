using System.IO;
using QFramework;
using UnityEngine;

/// <summary>
/// Json 数据持久化工具接口
/// 通过 QFramework 的 Utility 机制暴露给 System / Model / Controller / Command 使用
/// </summary>
public interface IJsonStorage : IUtility
{
    /// <summary>
    /// 将对象序列化为 Json 并存储到可读写目录（persistentDataPath）
    /// </summary>
    void Save<T>(T data, string fileName, JsonType type = JsonType.LitJson);

    /// <summary>
    /// 从磁盘读取 Json 并反序列化为对象
    /// 优先读取 streamingAssetsPath（默认数据），回退到 persistentDataPath（玩家存档）
    /// 若均不存在，返回一个默认的 new T()
    /// </summary>
    T Load<T>(string fileName, JsonType type = JsonType.LitJson) where T : new();

    /// <summary>
    /// 判断指定名称的存档是否存在（默认目录或可读写目录任一存在即可）
    /// </summary>
    bool HasData(string fileName);

    /// <summary>
    /// 删除可读写目录中指定名称的存档
    /// </summary>
    void DeleteData(string fileName);

    /// <summary>
    /// 清空可读写目录中的所有 Json 存档（慎用）
    /// </summary>
    void DeleteAll();
}

/// <summary>
/// IJsonStorage 的默认实现，底层复用 JsonMgr（LitJson / JsonUtility 双方案）
/// </summary>
public class JsonStorage : IJsonStorage
{
    private const string Extension = ".json";

    public void Save<T>(T data, string fileName, JsonType type = JsonType.LitJson)
    {
        if (data == null)
        {
            Debug.LogWarning($"[JsonStorage] Save 被调用时 data 为 null，fileName={fileName}，已忽略。");
            return;
        }

        JsonMgr.Instance.SaveData(data, fileName, type);
    }

    public T Load<T>(string fileName, JsonType type = JsonType.LitJson) where T : new()
    {
        return JsonMgr.Instance.LoadData<T>(fileName, type);
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
