using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrepareState : GameProcedureCompositeStateBase
{
    private IRoleInstanceSystem _roleInstanceSystem;

    protected override void OnSubStateEnter()
    {
        Debug.Log("[GameProcedure] 进入 PrepareState");

        SceneManager.sceneLoaded += OnPrepareLoaded;
        SceneManager.LoadScene("PrepareScene");

        // 有没有受控角色决定处于选择阶段还是控制阶段
        _roleInstanceSystem = this.GetSystem<IRoleInstanceSystem>();
        _roleInstanceSystem.OnControllingInstanceChanged += OnControllingInstanceChanged;

        AddSubState(new RoleSelectState());
        AddSubState(new RoleControlState());
        StartSubState<RoleSelectState>();
    }

    protected override void OnSubStateExit()
    {
        if (_roleInstanceSystem != null)
        {
            _roleInstanceSystem.OnControllingInstanceChanged -= OnControllingInstanceChanged;
            _roleInstanceSystem = null;
        }

        SceneManager.sceneLoaded -= OnPrepareLoaded;

        Debug.Log("[GameProcedure] 退出 PrepareState");
    }

    /// <summary>
    /// 接管角色后进入控制阶段，失去控制则退回选择阶段（面板会重新打开）。
    /// 目标就是当前子状态时 StateMachine 会直接返回，重复触发同一方向是空操作。
    /// </summary>
    private void OnControllingInstanceChanged(RoleContext roleContext)
    {
        if (roleContext == null)
        {
            ChangeSubState<RoleSelectState>();
        }
        else
        {
            ChangeSubState<RoleControlState>();
        }
    }

    private void OnPrepareLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "PrepareScene")
        {
            Debug.Log("[GameProcedure] Prepare场景加载完成");
        }
    }
}
