using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RoleSelectItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;

    private int _runtimeIndex;

    public event Action<int> OnRoleSelectItemHover;
    public event Action<int> OnRoleSelectItemClicked;

    public void Setup(int runtimeIndex, string roleName)
    {
        _runtimeIndex = runtimeIndex;
        _nameText.text = roleName;

        GetComponent<Button>().onClick.AddListener(() => OnRoleSelectItemClicked?.Invoke(_runtimeIndex));

        if (!gameObject.TryGetComponent(out EventTrigger trigger))
            trigger = gameObject.AddComponent<EventTrigger>();

        var hoverEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        hoverEntry.callback.AddListener(_ => OnRoleSelectItemHover?.Invoke(_runtimeIndex));
        trigger.triggers.Add(hoverEntry);
    }
}
