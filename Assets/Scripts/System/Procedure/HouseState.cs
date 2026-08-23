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
        this.GetSystem<IPackageSystem>().RemovePackageListener();
        Debug.Log("[GameProcedure] 退出 HouseState");
    }

    private void OnHouseLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "House")
        {
            Debug.Log("[GameProcedure] House场景加载完成");
            // 背包快捷键监听挂载在 House 场景的空物体上；OnEnter 时场景尚未切换，需等场景加载完成再创建
            this.GetSystem<IPackageSystem>().AddPackageListener();
        }
    }
}
