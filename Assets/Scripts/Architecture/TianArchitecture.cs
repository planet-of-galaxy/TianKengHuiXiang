using QFramework;
using UnityEngine;

public class TianArchitecture : Architecture<TianArchitecture>
{
    protected override void Init()
    {
        Debug.Log("Tian Keng architecture initializing.");

        // 注册Utility
        this.RegisterUtility<IJsonStorage>(new JsonStorage());
        this.RegisterUtility<IResourceStorage>(new ResourceStorage());

        // 注册Model
        this.RegisterModel(new PlayerDataModel());

        // 注册System
        this.RegisterSystem<IGameProcedureSystem>(new GameProcedureSystem());
        this.RegisterSystem<ICinemaChineCameraSystem>(new CinemaChineCameraSystem());
        this.RegisterSystem<IRoleSystem>(new RoleSystem());
        this.RegisterSystem<IPlayerSystem>(new PlayerSystem());

        Debug.Log("Tian Keng architecture initialized.");
    }
}
