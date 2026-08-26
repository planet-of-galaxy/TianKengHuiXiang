using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrepareState : GameProcedureStateBase
{
    public override void OnEnter()
    {
        Debug.Log("[GameProcedure] 进入 PrepareState");

        UIKit.OpenPanel<RoleSelectPanel>(prefabName: "resources://UI/Panel/roleselectpanel");

        SceneManager.sceneLoaded += OnPrepareLoaded;
        SceneManager.LoadScene("PrepareScene");
    }

    public override void OnExit()
    {
        Debug.Log("[GameProcedure] 退出 PrepareState");
        UIKit.ClosePanel<RoleSelectPanel>();

        this.GetSystem<IPackageSystem>().RemovePackageListener();
    }

    private void OnPrepareLoaded(Scene scene, LoadSceneMode mode)
    {

        if (scene.name == "PrepareScene")
        {
            Debug.Log("[GameProcedure] Prepare场景加载完成");
            this.GetSystem<IPackageSystem>().AddPackageListener();
        }
    }
}