using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MineCaveState : GameProcedureStateBase
{
    public override void OnEnter()
    {
        Debug.Log("[GameProcedure] 进入 MineCaveState");
        SceneManager.sceneLoaded += OnMineCaveLoaded;
        SceneManager.LoadScene("MineCave");
    }

    public override void OnExit()
    {
        SceneManager.sceneLoaded -= OnMineCaveLoaded;
        Debug.Log("[GameProcedure] 退出 MineCaveState");
    }

    private void OnMineCaveLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MineCave")
        {
            Debug.Log("[GameProcedure] MineCave场景加载完成");
        }
    }
}
