using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrepareState : GameProcedureStateBase
{
    private GameObject _cameraRoot;

    public override void OnEnter()
    {
        Debug.Log("[GameProcedure] 进入 PrepareState");
 
        SceneManager.sceneLoaded += OnPrepareLoaded;
        SceneManager.LoadScene("PrepareScene");
    }

    public override void OnExit()
    {
        Debug.Log("[GameProcedure] 退出 PrepareState");
        UIKit.ClosePanel<PreparePanel>();

        if (_cameraRoot != null)
            Object.Destroy(_cameraRoot);
    }

    private void OnPrepareLoaded(Scene scene, LoadSceneMode mode)
    {
        this.GetSystem<IPlayerSystem>().InitPlayer(0);

        if (scene.name == "PrepareScene")
        {
            Debug.Log("[GameProcedure] Prepare场景加载完成");

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
    }
}