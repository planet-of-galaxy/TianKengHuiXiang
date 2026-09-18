using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestState : GameProcedureStateBase
{
    private bool initialized;

    public override void OnEnter()
    {
        Debug.Log("[GameProcedure] 进入 TestState");
        GUITester.EnsureCreated();
        this.GetSystem<IPackageSystem>().AddPackageListener();
        initialized = false;
        SceneManager.sceneLoaded += OnTestSceneLoaded;
        // 支持直接启动测试场景，以及在场景已经加载后进入测试状态。
        for (int i = 0; i < SceneManager.sceneCount && !initialized; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded) OnTestSceneLoaded(scene, LoadSceneMode.Single);
        }
    }

    public override void OnExit()
    {
        SceneManager.sceneLoaded -= OnTestSceneLoaded;
        Debug.Log("[GameProcedure] 退出 TestState");
        this.GetSystem<IPackageSystem>().RemovePackageListener();
    }

    private void OnTestSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (initialized) return;
        foreach (var root in scene.GetRootGameObjects())
        {
            var context = root.GetComponent<TestContext>();
            if (context == null) continue;
            initialized = true;
            SceneManager.sceneLoaded -= OnTestSceneLoaded;
            CreateInstances(context);
            return;
        }
    }

    private void CreateInstances(TestContext context)
    {
        if (context == null || context.PlayerBornpoint == null || context.MonsterBornPoint == null)
        {
            Debug.LogError("[TestState] 请配置 PlayerBornpoint 和 MonsterBornPoint。");
            return;
        }

        CreatePlayer(context);
        var monster = this.GetSystem<IMonsterRuntimeSystem>().CreateMonster(
            0, context.MonsterBornPoint.position, context.MonsterBornPoint.rotation);
        if (monster != null && monster.GetComponent<MonsterNormalDamageReceiver>() == null)
        {
            monster.gameObject.AddComponent<MonsterNormalDamageReceiver>();
        }
    }

    private void CreatePlayer(TestContext context)
    {
        var roleIds = this.GetModel<IRoleRuntimeModel>().GetAllRoleRuntimeIds();
        if (roleIds.Count == 0)
        {
            Debug.LogError("[TestState] 没有可创建的运行时角色。");
            return;
        }

        var roleSystem = this.GetSystem<IRoleInstanceSystem>();
        var player = roleSystem.CreateRoleInstance(
            roleIds[0], context.PlayerBornpoint.position, context.PlayerBornpoint.rotation);
        if (player == null) return;

        roleSystem.AddPlayerMoveController(player);
        roleSystem.AddAttackController(player);
        if (player.GetComponent<WeaponChanger>() == null)
        {
            player.gameObject.AddComponent<WeaponChanger>();
        }

        if (player.FirstViewCinema != null)
        {
            this.GetSystem<ICinemaChineCameraSystem>().SetCinemaChineCamera(player.FirstViewCinema);
        }
    }
}
