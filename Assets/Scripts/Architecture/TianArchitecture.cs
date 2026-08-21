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
        this.RegisterUtility<IRoleConfigProvider>(new RoleConfigProvider(this.GetUtility<IJsonStorage>()));

        // 注册Model
        this.RegisterModel(new RoleRuntimeModel());

        // 注册System
        this.RegisterSystem<IGameProcedureSystem>(new GameProcedureSystem());
        this.RegisterSystem<ICinemaChineCameraSystem>(new CinemaChineCameraSystem());
        this.RegisterSystem<IWeaponConfigSystem>(new WeaponConfigSystem());
        this.RegisterSystem<IRoleRuntimeSystem>(new RoleRuntimeSystem());

        Debug.Log("Tian Keng architecture initialized.");
    }
}
