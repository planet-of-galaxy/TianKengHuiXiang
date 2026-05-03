using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrepareState : GameProcedureStateBase
{
    public override void OnEnter()
    {
        Debug.Log("[GameProcedure] 进入 PrepareState");
        SceneManager.LoadScene("PrepareScene");
        UIKit.OpenPanel<PreparePanel>(prefabName: "resources://UI/Panel/preparepanel");
    }

    public override void OnExit()
    {
        Debug.Log("[GameProcedure] 退出 PrepareState");
        UIKit.ClosePanel<PreparePanel>();
    }
}