using QFramework;
using UnityEngine;

public class AchievementSystem : AbstractSystem
{
    private int _killNum; // 击杀数
    protected override void OnInit()
    {
        this.GetModel<GameInfo>()
            .killNum
            .Register(OnKillNumChanged);
    }

    #region Listener
    private void OnKillNumChanged(int value)
    {
        if (value == 3)
        {
            Debug.Log("达成成就： 击杀达人");
        }
    }
    #endregion
}
