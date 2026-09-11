using System.Collections.Generic;
using QFramework;
using UnityEngine;

/// <summary>
/// 背包面板的 UI 逻辑，挂在与 PackagePanel 相同的物体上，由 PackagePanel 的生命周期回调驱动。
/// 职责：
///   1. 标题：显示「背包 - 当前角色名称」；
///   2. 槽位语义：按当前角色背包容量，决定每个槽位是道具、空栏位还是锁定；
///   3. 订阅：当前角色（RoleRuntimeModel.curRole）、其背包容量、以及背包内物品增减，变化时自动刷新。
/// 栏位本身由 PropPanelController 生成与渲染，本类只在刷新时把道具列表与容量交给它；
/// KeyContent / FileContent 暂未接入逻辑。
/// </summary>
[RequireComponent(typeof(PackagePanel))]
public class PackageController : MonoBehaviour, IController
{
    private PackagePanel panel;
    private PackageModel packageModel;
    private RoleRuntimeModel roleRuntimeModel;
    private PropPanelController propPanel;

    private IUnRegister curRoleUnRegister;
    private IUnRegister capacityUnRegister;

    /// <summary>
    /// 当前已订阅物品变化的背包。背包存活于 PackageModel 中、比面板活得久，
    /// 必须记住它才能在关闭 / 换角色时退订 C# 事件。
    /// </summary>
    private RolePackageInfo subscribedPackage;

    /// <summary>
    /// 当前可用容量：取当前角色背包的容量；该角色还没有背包时按默认容量显示。
    /// 容量小于 1 说明背包未被初始化（PackageModel 在创建背包时会补默认容量，走到这里说明那条路被绕过了）。
    /// 这里必须报错并回退默认容量——容量 -1 会被 UI 当成「0 个栏位」，把整屏栏位全锁死。
    /// </summary>
    private int Capacity
    {
        get
        {
            if (packageModel == null
                || !packageModel.TryGetPackage(roleRuntimeModel.curRole.Value, out var package))
            {
                return RolePackageInfo.DefaultCapacity;
            }

            if (package.capacity.Value < 1)
            {
                Debug.LogError($"[PackageController] 角色 {roleRuntimeModel.curRole.Value} 的背包容量未初始化（{package.capacity.Value}），已按默认容量 {RolePackageInfo.DefaultCapacity} 显示");
                return RolePackageInfo.DefaultCapacity;
            }

            return package.capacity.Value;
        }
    }

    // ==================== 生命周期（由 PackagePanel 转发） ====================

    /// <summary>面板初始化：准备栏位，并订阅角色与容量变化。</summary>
    public void Init(PackagePanel packagePanel)
    {
        panel = packagePanel;
        packageModel = this.GetModel<PackageModel>();
        roleRuntimeModel = this.GetModel<RoleRuntimeModel>();

        propPanel = panel.PropPanelController;
        if (propPanel == null)
        {
            Debug.LogError("[PackageController] PackagePanel.PropPanelController 未配置，背包栏位不会生成");
        }
        else
        {
            propPanel.Init();
        }

        // 角色切换时重新订阅新角色背包的容量与物品变化并刷新栏位
        curRoleUnRegister = roleRuntimeModel.curRole.Register(OnCurRoleChanged);
        RegisterPackage(roleRuntimeModel.curRole.Value);
    }

    /// <summary>面板打开：进入游戏暂停（时间停止、显示并解锁鼠标），并刷新一次显示。</summary>
    public void OnShow()
    {
        this.GetSystem<IGamePauseSystem>().Pause();
        RefreshAll();
    }

    /// <summary>面板关闭：退出暂停，还原暂停前的时间缩放与光标状态。</summary>
    public void OnHide()
    {
        this.GetSystem<IGamePauseSystem>().Resume();
    }

    /// <summary>面板关闭：取消订阅。</summary>
    public void OnClose()
    {
        curRoleUnRegister?.UnRegister();
        curRoleUnRegister = null;
        UnRegisterPackage();
    }

    // ==================== 刷新 ====================

    /// <summary>刷新标题与全部栏位。</summary>
    private void RefreshAll()
    {
        RefreshTitle();
        RefreshCells();
    }

    /// <summary>标题：背包 - 当前角色名称；取不到当前角色时只显示「背包」。</summary>
    private void RefreshTitle()
    {
        if (panel.Title == null)
        {
            return;
        }

        panel.Title.text = roleRuntimeModel.TryGetRoleRuntime(roleRuntimeModel.curRole.Value, out var role)
            ? $"背包 - {role.name}"
            : "背包";
    }

    /// <summary>
    /// 刷新全部栏位：把当前角色的背包道具列表与容量交给容器，由它决定每个栏位画成
    /// 道具 / 空 / 锁定（见 PropPanelController.Refresh）。
    /// 道具按各自的槽位号（PropItemInfo.index）落位，不是按列表顺序。
    /// </summary>
    private void RefreshCells()
    {
        if (propPanel == null)
        {
            return;
        }

        propPanel.Refresh(GetPackageItems(), Capacity);
    }

    /// <summary>当前角色背包的道具列表；无背包时返回 null。</summary>
    private IReadOnlyList<PropItemInfo> GetPackageItems()
    {
        return packageModel.TryGetPackage(roleRuntimeModel.curRole.Value, out var package)
            ? package.Items
            : null;
    }

    // ==================== 订阅回调 ====================

    private void OnCurRoleChanged(int roleRuntimeId)
    {
        RegisterPackage(roleRuntimeId);
        RefreshAll();
    }

    /// <summary>
    /// 改为订阅指定角色背包的容量与物品变化；该角色无背包时仅取消旧订阅。
    /// 物品变化是 C# 事件（RolePackageInfo.OnPackageUpdate），退订必须交回同一个委托目标，
    /// 所以这里用 subscribedPackage 记住订阅的是哪个背包。
    /// </summary>
    private void RegisterPackage(int roleRuntimeId)
    {
        UnRegisterPackage();

        if (!packageModel.TryGetPackage(roleRuntimeId, out var package))
        {
            return;
        }

        subscribedPackage = package;
        capacityUnRegister = package.capacity.Register(OnCapacityChanged);
        subscribedPackage.OnPackageUpdate += OnPackageItemsChanged;
    }

    /// <summary>退掉当前背包的容量与物品订阅；未订阅时是空操作。</summary>
    private void UnRegisterPackage()
    {
        capacityUnRegister?.UnRegister();
        capacityUnRegister = null;

        if (subscribedPackage != null)
        {
            subscribedPackage.OnPackageUpdate -= OnPackageItemsChanged;
            subscribedPackage = null;
        }
    }

    private void OnCapacityChanged(int capacity)
    {
        RefreshCells();
    }

    /// <summary>背包内物品增减时刷新。标题不含物品信息，只重画栏位与容量文本。</summary>
    private void OnPackageItemsChanged()
    {
        RefreshCells();
    }

    public IArchitecture GetArchitecture()
    {
        return TianArchitecture.Interface;
    }
}
