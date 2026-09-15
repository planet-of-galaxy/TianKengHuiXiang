using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 角色选择面板里的一个选项，由 <see cref="RoleSelectController"/> 从面板内的示例克隆生成。
/// 自身只把悬停 / 点击连同指向的角色实例抛出去，选中谁由 Controller 决定。
/// </summary>
[RequireComponent(typeof(Button))]
public class RoleSelectItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;

    private RoleContext _roleContext;

    /// <summary>鼠标移入。</summary>
    public event Action<RoleContext> Hovered;

    /// <summary>点击。</summary>
    public event Action<RoleContext> Clicked;

    /// <summary>绑定角色实例并刷新显示；只在克隆出的实例上调用一次。</summary>
    public void Setup(RoleContext roleContext, string roleName)
    {
        _roleContext = roleContext;
        _nameText.text = roleName;

        GetComponent<Button>().onClick.AddListener(() => Clicked?.Invoke(_roleContext));

        if (!gameObject.TryGetComponent(out EventTrigger trigger))
            trigger = gameObject.AddComponent<EventTrigger>();

        var hoverEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        hoverEntry.callback.AddListener(_ => Hovered?.Invoke(_roleContext));
        trigger.triggers.Add(hoverEntry);
    }
}
