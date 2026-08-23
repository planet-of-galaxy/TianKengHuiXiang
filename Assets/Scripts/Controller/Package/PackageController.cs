using QFramework;
using UnityEngine;

/// <summary>
/// 背包控制器：监听全局快捷键 HotKeyUtility.OpenPackage（默认 Q），
/// 通过 UIKit 打开/关闭背包面板 PackagePanel。
/// </summary>
public class PackageController : MonoBehaviour, IController
{
    private void Update()
    {
        if (!Input.GetKeyDown(HotKeyUtility.OpenPackage))
        {
            return;
        }

        TogglePackage();
    }

    /// <summary>背包面板已打开则关闭，否则通过 UIKit 加载并打开。</summary>
    private void TogglePackage()
    {
        if (UIKit.GetPanel<PackagePanel>() != null)
        {
            UIKit.ClosePanel<PackagePanel>();
        }
        else
        {
            UIKit.OpenPanel<PackagePanel>(prefabName: "resources://UI/Panel/packagepanel");
        }
    }

    public IArchitecture GetArchitecture()
    {
        return TianArchitecture.Interface;
    }
}
