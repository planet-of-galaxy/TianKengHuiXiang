using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrepareState : GameProcedureStateBase
{
    private GameObject _cameraRoot;

    public override void OnEnter()
    {
        Debug.Log("[GameProcedure] 进入 PrepareState");
        SceneManager.LoadScene("PrepareScene");

        var prefab = this.GetUtility<IResourceStorage>().Load<GameObject>("Prefabe/CinemaChineCamera/PrepareCinemaChineCamera");
        _cameraRoot = Object.Instantiate(prefab);
        Object.DontDestroyOnLoad(_cameraRoot);

        this.GetSystem<ICinemaChineCameraSystem>().SetCinemaChineCamera("LetSGo");

        UIKit.OpenPanel<PreparePanel>(prefabName: "resources://UI/Panel/preparepanel");
    }

    public override void OnExit()
    {
        Debug.Log("[GameProcedure] 退出 PrepareState");
        UIKit.ClosePanel<PreparePanel>();

        if (_cameraRoot != null)
            Object.Destroy(_cameraRoot);
    }
}