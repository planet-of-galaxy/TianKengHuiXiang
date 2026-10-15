using QFramework;
using UnityEngine;

/// <summary>监听当前怪物血量，死亡时移除运行数据并销毁怪物。</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(MonsterContext))]
public class MonsterHealthController : MonoBehaviour
{
    private MonsterContext monsterContext;
    private IMonsterRuntimeSystem monsterRuntimeSystem;
    private IUnRegister healthSubscription;
    private bool isDead;

    // Instantiate 时运行数据尚未登记，由创建流程在登记后初始化。
    internal void Initialize(MonsterRuntimeInfo runtimeInfo, IMonsterRuntimeSystem runtimeSystem)
    {
        healthSubscription?.UnRegister();
        monsterContext = GetComponent<MonsterContext>();
        monsterRuntimeSystem = runtimeSystem;
        isDead = false;
        healthSubscription = runtimeInfo.CurHealth.Register(OnCurHealthChanged);
        OnCurHealthChanged(runtimeInfo.CurHealth.Value);
    }

    private void OnCurHealthChanged(float health)
    {
        if (isDead || health > 0f) return;

        isDead = true;
        healthSubscription?.UnRegister();
        healthSubscription = null;
        monsterRuntimeSystem.RemoveMonster(monsterContext);
    }

    private void OnDestroy()
    {
        healthSubscription?.UnRegister();
        healthSubscription = null;
    }
}
