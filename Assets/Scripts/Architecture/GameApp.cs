using QFramework;
using UnityEngine;

public class GameApp : Architecture<GameApp>
{
    protected override void Init()
    {
        // init model
        this.RegisterModel<GameInfo>(new GameInfo());

        // init system
        this.RegisterSystem<AchievementSystem>(new AchievementSystem());
    }
}
