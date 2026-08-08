using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HouseState : GameProcedureStateBase
{
    public override void OnEnter()
    {
        Debug.Log("[GameProcedure] 进入 HouseState");
        SceneManager.sceneLoaded += OnHouseLoaded;
        SceneManager.LoadScene("House");
    }

    public override void OnExit()
    {
        SceneManager.sceneLoaded -= OnHouseLoaded;
        this.GetSystem<IPlayerSystem>().DestroyPlayer();
        Debug.Log("[GameProcedure] 退出 HouseState");
    }

    private void OnHouseLoaded(Scene scene, LoadSceneMode mode)
    {
        this.GetSystem<IPlayerSystem>().InitPlayer(0);
        if (scene.name == "House")
        {
            Debug.Log("[GameProcedure] House场景加载完成");

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
