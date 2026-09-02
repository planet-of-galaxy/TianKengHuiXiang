using QFramework;
using UnityEngine;

/// <summary>
/// 怪物创建器：挂载到场景中的生成点上，通过 monsterId 指定要生成的怪物。
/// Awake 时经 MonsterRuntimeSystem 创建对应的怪物运行时并实例化表现层，完成后销毁自身。
/// </summary>
public class MonsterCreator : MonoBehaviour, IController
{
    [SerializeField] private int monsterId;

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    private void Awake()
    {
        var monsterRuntimeSystem = this.GetSystem<IMonsterRuntimeSystem>();

        var runtimeId = monsterRuntimeSystem.CreateMonster(monsterId);
        if (runtimeId < 0)
        {
            // CreateMonster 内部已对配置不存在的情况输出错误日志
            Destroy(gameObject);
            return;
        }

        var instance = monsterRuntimeSystem.SpawnMonster(runtimeId, transform.position, transform.rotation);
        if (instance == null)
        {
            // 表现层生成失败时回滚运行时，避免留下无表现的怪物运行时
            monsterRuntimeSystem.RemoveMonster(runtimeId);
            Debug.LogError($"[MonsterCreator] monsterId={monsterId} 表现层生成失败，请检查 Resources/Prefabe/Monster 下是否存在对应预制体");
        }

        Destroy(gameObject);
    }
}
