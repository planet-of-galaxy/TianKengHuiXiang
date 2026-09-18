using QFramework;
using UnityEngine;

/// <summary>
/// 在生成点创建指定配置的怪物，完成后销毁生成点自身。
/// </summary>
public class MonsterCreator : MonoBehaviour, IController
{
    [SerializeField] private int monsterId;

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    private void Awake()
    {
        var monsterRuntimeSystem = this.GetSystem<IMonsterRuntimeSystem>();
        var context = monsterRuntimeSystem.CreateMonster(monsterId, transform.position, transform.rotation);
        if (context == null)
        {
            Debug.LogError($"[MonsterCreator] monsterId={monsterId} 创建失败，请检查怪物配置及预制体上的 MonsterContext");
        }

        Destroy(gameObject);
    }
}
