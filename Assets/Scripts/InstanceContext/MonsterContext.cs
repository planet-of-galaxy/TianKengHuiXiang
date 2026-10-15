using UnityEngine;

public class MonsterContext : MonoBehaviour
{
    [SerializeField] private int configId;
    [SerializeField] private MonsterAnimatorController monsterAnimatorController;

    public int ConfigId => configId;
    public MonsterAnimatorController MonsterAnimatorController => monsterAnimatorController;
}
