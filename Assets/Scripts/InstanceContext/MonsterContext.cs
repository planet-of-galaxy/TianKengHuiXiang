using UnityEngine;

public class MonsterContext : MonoBehaviour
{
    [SerializeField] private int configId;
    [SerializeField] private MonsterStateMachine monsterStateMachine;
    [SerializeField] private MonsterAnimatorController monsterAnimatorController;

    public int ConfigId => configId;
    public MonsterStateMachine MonsterStateMachine => monsterStateMachine != null
        ? monsterStateMachine
        : monsterStateMachine = GetComponent<MonsterStateMachine>();
    public MonsterAnimatorController MonsterAnimatorController => monsterAnimatorController;
}
