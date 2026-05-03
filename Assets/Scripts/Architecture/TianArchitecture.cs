using QFramework;
using UnityEngine;

public class TianArchitecture : Architecture<TianArchitecture>
{
    protected override void Init()
    {
        Debug.Log("Tian Keng architecture initializing.");

        // 注册Utility
        this.RegisterUtility<IJsonStorage>(new JsonStorage());

        // 注册Model

        // 注册System
        this.RegisterSystem<IGameProcedureSystem>(new GameProcedureSystem());
        this.RegisterSystem<ICinemaChineCameraSystem>(new CinemaChineCameraSystem());

        Debug.Log("Tian Keng architecture initialized.");
    }
}
