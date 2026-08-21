using QFramework;

/// <summary>
/// 所有持久化工具接口的父接口。
/// </summary>
public interface IPersistStorage : IUtility
{
        /// <summary>
    /// 将对象序列化为持久化数据并存储到可读写目录（persistentDataPath）
    /// </summary>
    void Save<T>(T data, string fileName);

    /// <summary>
    /// 从磁盘读取数据并反序列化为对象
    /// 优先读取 streamingAssetsPath（默认数据），回退到 persistentDataPath（玩家存档）
    /// 若均不存在，返回一个默认的 new T()
    /// </summary>
    T Load<T>(string fileName) where T : new();

    /// <summary>
    /// 判断指定名称的存档是否存在（默认目录或可读写目录任一存在即可）
    /// </summary>
    bool HasData(string fileName);

    /// <summary>
    /// 删除可读写目录中指定名称的存档
    /// </summary>
    void DeleteData(string fileName);

    /// <summary>
    /// 清空可读写目录中的所有存档（慎用）
    /// </summary>
    void DeleteAll();
}
