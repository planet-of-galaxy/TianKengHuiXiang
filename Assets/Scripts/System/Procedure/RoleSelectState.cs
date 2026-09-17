using QFramework;
using UnityEngine;

/// <summary>
/// Prepare 流程的"选择角色"子状态：展示角色选择面板
/// </summary>
public class RoleSelectState : GameProcedureStateBase
{
    private readonly PrepareSceneContext _sceneContext;

    public RoleSelectState(PrepareSceneContext sceneContext)
    {
        _sceneContext = sceneContext;
    }

    public override void OnEnter()
    {
        Debug.Log("[GameProcedure] 进入 RoleSelectState");

        this.GetSystem<ICinemaChineCameraSystem>().TransitionTo(_sceneContext.previewCinema);

        UIKit.OpenPanel<RoleSelectPanel>(prefabName: "resources://UI/Panel/roleselectpanel");
    }

    public override void OnExit()
    {
        Debug.Log("[GameProcedure] 退出 RoleSelectState");
        UIKit.ClosePanel<RoleSelectPanel>();
    }
}
