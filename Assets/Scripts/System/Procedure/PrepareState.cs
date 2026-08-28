using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrepareState : GameProcedureCompositeStateBase
{
    private IUnRegister _roleSelectUnRegister;

    protected override void OnSubStateEnter()
    {
        Debug.Log("[GameProcedure] 进入 PrepareState");

        SceneManager.sceneLoaded += OnPrepareLoaded;
        SceneManager.LoadScene("PrepareScene");

        // 选中角色后进入控制子状态
        _roleSelectUnRegister = this.RegisterEvent<CurrentRoleSetEvent>(_ => ChangeSubState<RoleControlState>());

        AddSubState(new RoleSelectState());
        AddSubState(new RoleControlState());
        StartSubState<RoleSelectState>();
    }

    protected override void OnSubStateExit()
    {
        _roleSelectUnRegister?.UnRegister();
        _roleSelectUnRegister = null;

        SceneManager.sceneLoaded -= OnPrepareLoaded;

        Debug.Log("[GameProcedure] 退出 PrepareState");
    }

    private void OnPrepareLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "PrepareScene")
        {
            Debug.Log("[GameProcedure] Prepare场景加载完成");
        }
    }
}
