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
        Debug.Log("[GameProcedure] 退出 HouseState");
    }

    private void OnHouseLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "House")
        {
            Debug.Log("[GameProcedure] House场景加载完成");
        }
    }
}
