using QFramework;
using UnityEngine;

/// <summary>
/// 仓库监听器：监听全局快捷键 HotKeyUtility.OpenPackage（默认 Q，与背包共用快捷键互斥打开），
/// 通过 UIKit 打开/关闭仓库面板 WareHousePanel。
/// </summary>
public class WareHouseListener : MonoBehaviour, IController
{
    private void Update()
    {
        if (!Input.GetKeyDown(HotKeyUtility.OpenPackage))
        {
            return;
        }

        ToggleWareHouse();
    }

    /// <summary>仓库面板已打开则关闭，否则通过 UIKit 加载并打开。</summary>
    private void ToggleWareHouse()
    {
        if (UIKit.GetPanel<WareHousePanel>() != null)
        {
            UIKit.ClosePanel<WareHousePanel>();
        }
        else
        {
            UIKit.OpenPanel<WareHousePanel>(prefabName: "resources://UI/Panel/warehousepanel");
        }
    }

    public IArchitecture GetArchitecture()
    {
        return TianArchitecture.Interface;
    }
}
