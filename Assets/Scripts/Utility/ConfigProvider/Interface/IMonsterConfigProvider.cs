using QFramework;

public interface IMonsterConfigProvider : IUtility
{
    void Init();
    MonsterConfig GetMonster(int monsterId);
    bool HasMonster(int monsterId);
    int GetMonsterCount();
}
