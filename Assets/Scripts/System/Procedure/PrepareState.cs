using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrepareState : GameProcedureCompositeStateBase
{
    private IRoleInstanceSystem _roleInstanceSystem;
    private PrepareSceneContext _sceneContext;
    private bool _isQuitting;

    protected override void OnSubStateEnter()
    {
        _isQuitting = false;
        Application.quitting += OnApplicationQuitting;

        Debug.Log("[GameProcedure] 进入 PrepareState");

        _roleInstanceSystem = this.GetSystem<IRoleInstanceSystem>();
        UIKit.OpenPanel<LoadingPanel>(UILevel.PopUI, prefabName: "resources://UI/Panel/LoadingPanel");
        SceneManager.sceneLoaded += OnPrepareLoaded;
        SceneManager.LoadScene("PrepareScene");
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

        _sceneContext = null;
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
        _sceneContext = null;
        Application.quitting -= OnApplicationQuitting;
    }

    private void OnPrepareLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "PrepareScene") return;

        UIKit.HidePanel<LoadingPanel>();
        SceneManager.sceneLoaded -= OnPrepareLoaded;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.TryGetComponent<PrepareSceneContext>(out _sceneContext))
                break;
        }

        if (_sceneContext == null)
        {
            Debug.LogError("[GameProcedure] PrepareScene 缺少 PrepareSceneContext");
            return;
        }

        CreateRoles();
        AddSubState(new RoleSelectState(_sceneContext));
        AddSubState(new RoleControlState());
        _roleInstanceSystem.OnControllingInstanceChanged += OnControllingInstanceChanged;
        StartSubState<RoleSelectState>();
        Debug.Log("[GameProcedure] Prepare场景加载完成");
    }

    private void CreateRoles()
    {
        var runtimeModel = this.GetModel<IRoleRuntimeModel>();
        int index = 0;
        foreach (var roleInfo in runtimeModel.GetAllRoleRuntimes())
        {
            if (index >= _sceneContext.roleList.Count)
            {
                Debug.LogWarning($"[PrepareState] 角色数量 ({runtimeModel.Count}) 超过生成点数量 ({_sceneContext.roleList.Count})，剩余角色未生成");
                break;
            }

            var spawnPoint = _sceneContext.roleList[index++];
            if (spawnPoint == null)
            {
                Debug.LogWarning($"[PrepareState] 生成点 {index - 1} 为 null，跳过角色 {roleInfo.runtimeIndex}");
                continue;
            }

            _roleInstanceSystem.CreateRoleInstance(
                roleInfo.runtimeIndex, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
