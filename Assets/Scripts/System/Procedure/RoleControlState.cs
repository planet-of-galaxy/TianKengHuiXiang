using QFramework;
using UnityEngine;

/// <summary>
/// Prepare 流程的"控制角色"子状态：角色已在场景中生成（由 CurrentRoleSetListener 处理），
/// 进入此状态代表选中完成，可在此做控制阶段的初始化
/// </summary>
public class RoleControlState : GameProcedureStateBase
{
    public override void OnEnter()
    {
        Debug.Log("[GameProcedure] 进入 RoleControlState");
        this.GetSystem<IPackageSystem>().AddPackageListener();
    }

    public override void OnExit()
    {
        Debug.Log("[GameProcedure] 退出 RoleControlState");
        this.GetSystem<IPackageSystem>().RemovePackageListener();
    }
}
