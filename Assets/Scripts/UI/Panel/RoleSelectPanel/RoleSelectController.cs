using System.Collections.Generic;
using QFramework;
using UnityEngine;

/// <summary>
/// 角色选择面板的 UI 逻辑，挂在与 RoleSelectPanel 相同的物体上，由面板的生命周期回调驱动。
/// 职责：
///   1. 为每个角色实例生成一个选项，并随实例的创建 / 销毁增删选项；
///   2. 悬停选项时观察该角色，点击选项时接管该角色。
/// 这里直接持有 RoleContext，所以不需要再靠事件广播让角色实例「认领」自己。
/// </summary>
[RequireComponent(typeof(RoleSelectPanel))]
public class RoleSelectController : MonoBehaviour, IController
{
    private RoleSelectPanel panel;
    private IRoleInstanceSystem roleInstanceSystem;
    private RoleRuntimeModel roleRuntimeModel;

    /// <summary>角色实例 → 它对应的选项；实例销毁时用它在列表里反查出要销毁的选项。</summary>
    private readonly Dictionary<RoleContext, RoleSelectItem> items = new Dictionary<RoleContext, RoleSelectItem>();

    // ==================== 生命周期（由 RoleSelectPanel 转发） ====================

    /// <summary>
    /// 面板初始化：先为已经存在的角色实例补一遍选项，再订阅实例的创建与销毁。
    /// 面板打开时角色实例早已由 RoleCreator 建好，只订阅事件的话会一个选项都不显示。
    /// </summary>
    public void Init(RoleSelectPanel roleSelectPanel)
    {
        panel = roleSelectPanel;
        roleInstanceSystem = this.GetSystem<IRoleInstanceSystem>();
        roleRuntimeModel = this.GetModel<RoleRuntimeModel>();

        roleInstanceSystem.OnRoleInstanceCreated += OnRoleInstanceCreated;
        roleInstanceSystem.OnRoleInstanceDestroyed += OnRoleInstanceDestroyed;

        foreach (var info in roleRuntimeModel.GetAllRoleRuntimes())
        {
            var roleContext = roleInstanceSystem.TryGetRoleInstance(info.runtimeIndex);
            if (roleContext == null)
            {
                Debug.LogWarning($"[RoleSelectController] 角色运行时 {info.runtimeIndex} 没有对应的角色实例，已跳过");
                continue;
            }

            AddItem(roleContext);
        }
    }

    /// <summary>面板关闭：退订并销毁所有选项。</summary>
    public void OnClose()
    {
        if (roleInstanceSystem != null)
        {
            roleInstanceSystem.OnRoleInstanceCreated -= OnRoleInstanceCreated;
            roleInstanceSystem.OnRoleInstanceDestroyed -= OnRoleInstanceDestroyed;
        }

        foreach (var item in items.Values)
        {
            DestroyItem(item);
        }

        items.Clear();
    }

    // ==================== 订阅回调 ====================

    private void OnRoleInstanceCreated(RoleContext roleContext)
    {
        AddItem(roleContext);
    }

    private void OnRoleInstanceDestroyed(RoleContext roleContext)
    {
        RemoveItem(roleContext);
    }

    private void OnItemHovered(RoleContext roleContext)
    {
        this.SendCommand(new ObserveRoleCmd(roleContext));
    }

    private void OnItemClicked(RoleContext roleContext)
    {
        this.SendCommand(new ControlRoleCmd(roleContext));
    }

    // ==================== 选项 ====================

    /// <summary>按角色实例克隆一个选项；该实例已有选项时是空操作。</summary>
    private void AddItem(RoleContext roleContext)
    {
        if (roleContext == null || items.ContainsKey(roleContext))
        {
            return;
        }

        if (panel == null || panel.ItemExample == null || panel.ItemContainer == null)
        {
            Debug.LogError($"[RoleSelectController] 面板的 ItemExample / ItemContainer 未配置，角色 {roleContext.RoleRuntimeIndex} 的选项不会生成");
            return;
        }

        // 示例自身是禁用的，克隆出来后要显式启用。
        var go = Instantiate(panel.ItemExample.gameObject, panel.ItemContainer);
        go.name = $"RoleSelectItem_{roleContext.RoleRuntimeIndex}";
        go.SetActive(true);

        var item = go.GetComponent<RoleSelectItem>();
        item.Setup(roleContext, GetRoleName(roleContext));
        item.Hovered += OnItemHovered;
        item.Clicked += OnItemClicked;

        items[roleContext] = item;
    }

    /// <summary>销毁角色实例对应的选项；该实例没有选项时是空操作。</summary>
    private void RemoveItem(RoleContext roleContext)
    {
        if (roleContext == null || !items.TryGetValue(roleContext, out var item))
        {
            return;
        }

        items.Remove(roleContext);
        DestroyItem(item);
    }

    private void DestroyItem(RoleSelectItem item)
    {
        if (item == null)
        {
            return;
        }

        item.Hovered -= OnItemHovered;
        item.Clicked -= OnItemClicked;
        Destroy(item.gameObject);
    }

    /// <summary>选项上显示的角色名；RoleViewFactory 已保证存活实例的 name 非空，取不到时退化成实例 id。</summary>
    private string GetRoleName(RoleContext roleContext)
    {
        return roleRuntimeModel.TryGetRoleRuntime(roleContext.RoleRuntimeIndex, out var info) && !string.IsNullOrEmpty(info.name)
            ? info.name
            : roleContext.RoleRuntimeIndex.ToString();
    }

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;
}
