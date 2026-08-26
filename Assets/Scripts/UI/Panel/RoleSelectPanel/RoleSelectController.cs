using QFramework;
using UnityEngine;

[RequireComponent(typeof(RoleSelectPanel))]
public class RoleSelectController : MonoBehaviour, IController
{
    private RoleSelectPanel _panel;

    void Awake()
    {
        _panel = GetComponent<RoleSelectPanel>();
        _panel.OnRoleSelectItemHover += OnItemHover;
        _panel.OnRoleSelectItemClicked += OnItemClicked;
    }

    void OnDestroy()
    {
        if (_panel != null)
        {
            _panel.OnRoleSelectItemHover -= OnItemHover;
            _panel.OnRoleSelectItemClicked -= OnItemClicked;
        }
    }

    private void OnItemHover(int runtimeIndex)
    {
        this.SendCommand(new TransToRoleObserveCmd(runtimeIndex));
    }

    private void OnItemClicked(int runtimeIndex)
    {
        this.SendCommand(new SetCurrentRoleCmd(runtimeIndex));
    }

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;
}
