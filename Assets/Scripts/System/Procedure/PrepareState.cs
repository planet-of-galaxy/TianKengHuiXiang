using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrepareState : GameProcedureStateBase
{
    private GameObject _cameraRoot;

    public override void OnEnter()
    {
        Debug.Log("[GameProcedure] 进入 PrepareState");

        UIKit.OpenPanel<RuntimePanel>(prefabName: "resources://UI/Panel/runtimepanel");

        SceneManager.sceneLoaded += OnPrepareLoaded;
        SceneManager.LoadScene("PrepareScene");
    }

    public override void OnExit()
    {
        Debug.Log("[GameProcedure] 退出 PrepareState");
        UIKit.ClosePanel<PreparePanel>();
        UIKit.ClosePanel<RuntimePanel>();

        if (_cameraRoot != null)
            Object.Destroy(_cameraRoot);
    }

    private void OnPrepareLoaded(Scene scene, LoadSceneMode mode)
    {

        if (scene.name == "PrepareScene")
        {
            Debug.Log("[GameProcedure] Prepare场景加载完成");
        }
    }
}