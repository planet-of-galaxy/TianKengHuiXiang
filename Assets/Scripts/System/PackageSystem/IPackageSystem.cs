using QFramework;

public interface IPackageSystem : ISystem
{
    /// <summary>
    /// 读取存档，初始化 PackageModel；无存档时使用默认空背包。
    /// </summary>
    void InitPackageModel();
}
