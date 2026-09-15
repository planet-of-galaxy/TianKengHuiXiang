using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrepareState : GameProcedureCompositeStateBase
{
    private IRoleInstanceSystem _roleInstanceSystem;
    private bool _isQuitting;

    protected override void OnSubStateEnter()
    {
        _isQuitting = false;
        Application.quitting += OnApplicationQuitting;

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
        Application.quitting -= OnApplicationQuitting;
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
        // 退出时角色销毁只是清理，不能再打开面板并重新创建 UIRoot。
        if (_isQuitting) return;

        if (roleContext == null)
        {
            ChangeSubState<RoleSelectState>();
        }
        else
        {
            ChangeSubState<RoleControlState>();
        }
    }

    private void OnApplicationQuitting()
    {
        _isQuitting = true;
        // 停止 Play Mode 不保证流程状态会先 OnExit，提前解除业务事件订阅。
        if (_roleInstanceSystem != null)
        {
            _roleInstanceSystem.OnControllingInstanceChanged -= OnControllingInstanceChanged;
        }

        SceneManager.sceneLoaded -= OnPrepareLoaded;
        Application.quitting -= OnApplicationQuitting;
    }

    private void OnPrepareLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "PrepareScene")
        {
            Debug.Log("[GameProcedure] Prepare场景加载完成");
        }
    }
}
