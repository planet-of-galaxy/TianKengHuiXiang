using QFramework;
using UnityEngine;

/// <summary>
/// 角色选择面板（UIKit 管理）：只持有 UI 引用，不含任何逻辑。
/// UI 逻辑集中在同物体上的 <see cref="RoleSelectController"/>，本类在生命周期回调里转发：
/// OnInit → Controller.Init，OnClose → Controller.OnClose。
/// </summary>
public class RoleSelectPanel : UIPanel
{
    [SerializeField] private Transform _itemContainer;
    [SerializeField] private RoleSelectItem _itemExample;

    /// <summary>角色选项的列表容器（RoleList）。</summary>
    public Transform ItemContainer => _itemContainer;

    /// <summary>
    /// 角色选项模板：常驻容器内、自身禁用，每次新增选项时由它克隆。
    /// </summary>
    public RoleSelectItem ItemExample => _itemExample;

    /// <summary>
    /// 同物体上的 UI 逻辑控制器，OnInit 时取一次。
    /// 这里不加 [RequireComponent]：RoleSelectController 已经声明了依赖 RoleSelectPanel，
    /// 两边互相 RequireComponent 会让 Unity 在缺组件时无限互相添加。
    /// </summary>
    private RoleSelectController _controller;

    protected override void OnInit(IUIData uiData = null)
    {
        _controller = GetComponent<RoleSelectController>();
        if (_controller == null)
        {
            Debug.LogError("[RoleSelectPanel] 同物体上缺少 RoleSelectController，面板不会生成角色选项");
            return;
        }

        _controller.Init(this);
    }

    protected override void OnClose()
    {
        if (_controller != null)
        {
            _controller.OnClose();
        }
    }
}
