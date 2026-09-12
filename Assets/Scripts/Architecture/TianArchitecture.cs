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
        this.RegisterUtility<IWeaponConfigProvider>(new WeaponConfigProvider(this.GetUtility<IJsonStorage>()));
        this.RegisterUtility<IMonsterConfigProvider>(new MonsterConfigProvider(this.GetUtility<IJsonStorage>()));

        // 注册Model
        this.RegisterModel(new RoleRuntimeModel());
        this.RegisterModel(new RoleInstanceModel());
        this.RegisterModel(new PackageModel());
        this.RegisterModel(new WareHouseModel());
        this.RegisterModel(new MonsterRuntimeModel());

        // 注册System
        this.RegisterSystem<ICameraSystem>(new CameraSystem());
        this.RegisterSystem<IGameProcedureSystem>(new GameProcedureSystem());
        this.RegisterSystem<ICinemaChineCameraSystem>(new CinemaChineCameraSystem());
        this.RegisterSystem<IRoleRuntimeSystem>(new RoleRuntimeSystem());
        this.RegisterSystem<IRoleInstanceSystem>(new RoleInstanceSystem());
        this.RegisterSystem<IPackageSystem>(new PackageSystem());
        this.RegisterSystem<IWeaponInstanceSystem>(new WeaponInstanceSystem());
        this.RegisterSystem<IWareHouseSystem>(new WareHouseSystem());
        this.RegisterSystem<IGamePauseSystem>(new GamePauseSystem());
        this.RegisterSystem<IMonsterRuntimeSystem>(new MonsterRuntimeSystem());

        Debug.Log("Tian Keng architecture initialized.");
    }
}
