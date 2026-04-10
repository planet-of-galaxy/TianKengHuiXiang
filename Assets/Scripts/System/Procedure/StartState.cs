using QFramework;
using UnityEngine;

public class StartState : GameProcedureStateBase
{
    public override void OnEnter()
    {
        Debug.Log("[GameProcedure] 进入 StartState");
        UIKit.OpenPanel<StartPanel>(prefabName: "resources://UI/Panel/startpanel");
    }

    public override void OnExit()
    {
        Debug.Log("[GameProcedure] 退出 StartState");
        UIKit.ClosePanel<StartPanel>();
    }
}
