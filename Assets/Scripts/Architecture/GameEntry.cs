using QFramework;
using UnityEngine;

public class GameEntry : MonoBehaviour, ICanGetSystem
{
    public void Awake()
    {
        // UIKit 的资源管理默认使用的是 ResKit
        ResKit.Init();

        // UIKit 的分辨率设置
        UIKit.Root.SetResolution(1920, 1080, 1);

        TianArchitecture.InitArchitecture();

        this.GetSystem<IGameProcedureSystem>().ChangeState<StartState>();
    }

	public IArchitecture GetArchitecture()
	{
		return TianArchitecture.Interface;
	}
}
