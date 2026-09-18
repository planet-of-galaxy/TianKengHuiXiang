using QFramework;
using UnityEngine;

/// <summary>
/// 管理怪物实例的创建、运行数据查询、血量修改和删除。
/// 通过实例上的 MonsterContext 标识怪物，运行数据存储在 MonsterRuntimeModel 中。
/// </summary>
public interface IMonsterRuntimeSystem : ISystem
{
    /// <summary>
    /// 根据配置实例化怪物预制体，并以配置中的血量初始化、登记运行数据。
    /// </summary>
    /// <param name="configId">怪物配置 ID</param>
    /// <param name="position">生成位置（世界坐标）。</param>
    /// <param name="rotation">生成朝向（世界旋转）。</param>
    /// <returns>新实例的 MonsterContext；配置或预制体无效时返回 null。</returns>
    MonsterContext CreateMonster(int configId, Vector3 position, Quaternion rotation);

    /// <summary>获取指定怪物实例的运行数据。</summary>
    /// <param name="context">用于标识怪物实例的 Context。</param>
    /// <returns>已登记的运行数据；未找到时返回 null。</returns>
    MonsterRuntimeInfo GetMonsterRuntime(MonsterContext context);

    /// <summary>尝试获取指定怪物实例的运行数据。</summary>
    /// <param name="context">用于标识怪物实例的 Context。</param>
    /// <param name="info">找到的运行数据；未找到时为 null。</param>
    /// <returns>找到运行数据时返回 true，否则返回 false。</returns>
    bool TryGetMonsterRuntime(MonsterContext context, out MonsterRuntimeInfo info);

    /// <summary>移除运行数据并销毁 Context 所在的怪物实例。</summary>
    /// <param name="context">要移除的怪物实例的 Context。</param>
    /// <returns>成功移除时返回 true；未登记运行数据时返回 false，且不销毁实例。</returns>
    bool RemoveMonster(MonsterContext context);

    /// <summary>
    /// 设置当前血量，结果限制在 [0, MaxHealth] 范围内；未找到运行数据时不执行操作。
    /// </summary>
    /// <param name="context">要修改血量的怪物实例的 Context。</param>
    /// <param name="value">目标血量。</param>
    void SetCurHealth(MonsterContext context, float value);

    /// <summary>
    /// 在当前血量上增减指定数值，结果限制在 [0, MaxHealth] 范围内；未找到运行数据时不执行操作。
    /// </summary>
    /// <param name="context">要修改血量的怪物实例的 Context。</param>
    /// <param name="delta">血量变化值，负数表示扣血，正数表示回血。</param>
    void ChangeCurHealth(MonsterContext context, float delta);
}

public class MonsterRuntimeSystem : AbstractSystem, IMonsterRuntimeSystem
{
    private MonsterRuntimeModel runtimeModel;
    private IMonsterConfigProvider monsterConfigProvider;
    private MonsterViewFactory viewFactory;

    protected override void OnInit()
    {
        runtimeModel = (MonsterRuntimeModel)this.GetModel<IMonsterRuntimeModel>();
        monsterConfigProvider = this.GetUtility<IMonsterConfigProvider>();
        viewFactory = new MonsterViewFactory(monsterConfigProvider, this.GetUtility<IResourceStorage>());
    }

    public MonsterContext CreateMonster(int configId, Vector3 position, Quaternion rotation)
    {
        var context = viewFactory.SpawnMonster(configId, position, rotation);
        if (context == null) return null;

        var config = monsterConfigProvider.GetMonster(context.ConfigId);
        var info = new MonsterRuntimeInfo();
        info.CurHealth.Value = config.health;
        info.MaxHealth.Value = config.health;
        runtimeModel.AddMonsterRuntime(context, info);
        return context;
    }

    public MonsterRuntimeInfo GetMonsterRuntime(MonsterContext context)
    {
        runtimeModel.TryGetMonsterRuntime(context, out var info);
        return info;
    }

    public bool TryGetMonsterRuntime(MonsterContext context, out MonsterRuntimeInfo info)
    {
        return runtimeModel.TryGetMonsterRuntime(context, out info);
    }

    public bool RemoveMonster(MonsterContext context)
    {
        if (!runtimeModel.TryGetMonsterRuntime(context, out _)) return false;

        runtimeModel.RemoveMonsterRuntime(context);
        if (context != null) Object.Destroy(context.gameObject);
        return true;
    }

    public void SetCurHealth(MonsterContext context, float value)
    {
        if (!runtimeModel.TryGetMonsterRuntime(context, out var info)) return;
        info.CurHealth.Value = Mathf.Clamp(value, 0f, info.MaxHealth.Value);
    }

    public void ChangeCurHealth(MonsterContext context, float delta)
    {
        if (!runtimeModel.TryGetMonsterRuntime(context, out var info)) return;
        info.CurHealth.Value = Mathf.Clamp(info.CurHealth.Value + delta, 0f, info.MaxHealth.Value);
    }
}
