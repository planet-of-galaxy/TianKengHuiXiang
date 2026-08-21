using QFramework;
using UnityEngine;

public class TestState : GameProcedureStateBase
{
    public override void OnEnter()
    {
        Debug.Log("[GameProcedure] 进入 TestState");
    }

    public override void OnExit()
    {
        Debug.Log("[GameProcedure] 退出 TestState");
    }
}
