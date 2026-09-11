using QFramework;
using TMPro;
using UnityEngine;

/// <summary>
/// 背包面板（UIKit 管理）：只持有 UI 引用，不含任何逻辑。
/// UI 逻辑集中在同物体上的 <see cref="PackageController"/>，本类在生命周期回调里转发：
/// OnInit → Controller.Init，OnShow / OnHide / OnClose 同理。
/// KeyContent、FileContent 目前还没有对应逻辑，只作为引用提供给 Controller（留空待接入）。
/// </summary>
public class PackagePanel : UIPanel
{
    [Header("子节点")]
    [Tooltip("背包页控制器（挂在 PropPanel 上），自己负责栏位生成、渲染与容量文本。")]
    public PropPanelController PropPanelController;

    [Tooltip("键位页（PanelGroup/KeyPanel/Scroll View/Viewport/Content）的列表容器，暂未接入逻辑。")]
    public Transform KeyContent;

    [Tooltip("文件页（PanelGroup/FilePanel/Scroll View/Viewport/Content）的列表容器，暂未接入逻辑。")]
    public Transform FileContent;

    [Tooltip("标题文本（TextMeshProUGUI），显示为「背包 - 当前角色名称」。")]
    public TextMeshProUGUI Title;

    /// <summary>
    /// 同物体上的 UI 逻辑控制器，OnInit 时取一次。
    /// 这里不加 [RequireComponent]：PackageController 已经声明了依赖 PackagePanel，
    /// 两边互相 RequireComponent 会让 Unity 在缺组件时无限互相添加。
    /// </summary>
    private PackageController controller;

    protected override void OnInit(IUIData uiData = null)
    {
        controller = GetComponent<PackageController>();
        if (controller == null)
        {
            Debug.LogError("[PackagePanel] 同物体上缺少 PackageController，面板不会刷新");
            return;
        }

        controller.Init(this);
    }

    protected override void OnShow()
    {
        controller?.OnShow();
    }

    protected override void OnHide()
    {
        controller?.OnHide();
    }

    protected override void OnClose()
    {
        controller?.OnClose();
    }
}
