using QFramework;
using UnityEngine;

public class TestState : GameProcedureStateBase
{
    public override void OnEnter()
    {
        Debug.Log("[GameProcedure] 进入 TestState");

        this.GetSystem<IPlayerSystem>().InitPlayer(0);

        var bornPoint = GameObject.Find("BornPoint");
        if (bornPoint != null)
        {
            this.GetSystem<IPlayerSystem>().CreatePlayer(bornPoint.transform);
            // 切换到Player虚拟相机
            this.SendCommand(new TransitionCameraCmd("Player", 2f));
        }
        else
        {
            Debug.LogError("[GameProcedure] BornPoint not found in House scene");
        }
    }

    public override void OnExit()
    {
        this.GetSystem<IPlayerSystem>().DestroyPlayer();
        Debug.Log("[GameProcedure] 退出 TestState");
    }
}
