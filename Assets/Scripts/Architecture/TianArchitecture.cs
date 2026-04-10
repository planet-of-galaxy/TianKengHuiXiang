using QFramework;
using UnityEngine;

public class TianArchitecture : Architecture<TianArchitecture>
{
    protected override void Init()
    {
        Debug.Log("Tian Keng architecture initializing.");

        // 注册Utility

        // 注册Model

        // 注册System
        this.RegisterSystem<IGameProcedureSystem>(new GameProcedureSystem());

        Debug.Log("Tian Keng architecture initialized.");
    }
}
