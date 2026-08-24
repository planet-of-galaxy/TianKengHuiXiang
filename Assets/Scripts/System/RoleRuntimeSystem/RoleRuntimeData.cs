using System.Collections.Generic;

[System.Serializable]
public class RoleRuntimeData
{
    public int runtimeIndex;        // 运行时实例 id，唯一（同一角色配置可有多份实例）
    public int configId;  // 对应的角色配置 id
    public float curHealth;
    public float maxHealth;
}

/// <summary>
/// 角色运行时数据的持久化容器。
/// </summary>
[System.Serializable]
public class RoleRuntimeSaveData
{
    public List<RoleRuntimeData> roleRuntimeDatas;
    public int curRole;
}
